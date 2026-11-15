using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BaseLunar.Nucleo;

/// <summary>
/// Consulta en vivo de NASA DONKI para el Nivel 4 (sección 7, D-09, D-19). Nunca falla:
/// <list type="number">
/// <item>Si hay una consulta válida de menos de 6 h en la caché local, la usa.</item>
/// <item>Si no, pregunta a api.nasa.gov (clave personal) y, si falla, al servicio DONKI del CCMC (sin clave).
/// Cada intento espera como máximo 3 s.</item>
/// <item>De la lista de los últimos 30 días elige la llamarada más intensa (si empatan, la más reciente).</item>
/// <item>Ante cualquier problema (sin red, 403, 404, 429, 5xx, JSON inválido o lista vacía) devuelve el
/// respaldo histórico real (la X8.7 del 14-05-2024).</item>
/// </list>
/// Solo envía las fechas y la clave de la API: ningún dato del niño sale del equipo.
/// </summary>
public sealed class ClienteDonki : IDisposable
{
    private readonly HttpClient http;
    private readonly string? rutaCache;
    private readonly Func<DateTimeOffset> reloj;
    private readonly TimeSpan espera;
    private readonly Action<string>? registro;

    /// <param name="manejador">Para las pruebas: respuestas simuladas. Por defecto, la red de verdad.</param>
    /// <param name="rutaCache">Archivo de la caché (null: sin caché).</param>
    /// <param name="reloj">Hora actual (las pruebas la fijan).</param>
    /// <param name="espera">Espera máxima por intento (por defecto, 3 s).</param>
    /// <param name="registro">Errores técnicos: nunca se muestran al niño (sección 7).</param>
    public ClienteDonki(HttpMessageHandler? manejador = null, string? rutaCache = null, Func<DateTimeOffset>? reloj = null,
        TimeSpan? espera = null, Action<string>? registro = null)
    {
        http = manejador is null ? new HttpClient() : new HttpClient(manejador, disposeHandler: false);
        http.Timeout = Timeout.InfiniteTimeSpan;
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BaseLunar", "1.0"));
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        this.rutaCache = rutaCache;
        this.reloj = reloj ?? (() => DateTimeOffset.UtcNow);
        this.espera = espera ?? TimeSpan.FromMilliseconds(Configuracion.Nasa.TimeoutMs);
        this.registro = registro;
    }

    /// <summary>Las dos direcciones de la consulta, en orden (D-19), para el rango de los últimos 30 días.</summary>
    public static IReadOnlyList<Uri> Direcciones(DateTimeOffset ahora)
    {
        var fin = ahora.UtcDateTime.Date;
        var inicio = fin.AddDays(-Configuracion.Nasa.DiasConsulta);
        var rango = string.Create(CultureInfo.InvariantCulture, $"startDate={inicio:yyyy-MM-dd}&endDate={fin:yyyy-MM-dd}");
        var clave = string.IsNullOrEmpty(Configuracion.Nasa.Clave) ? Configuracion.Nasa.ClaveReserva : Configuracion.Nasa.Clave;
        return
        [
            new Uri($"{Configuracion.Nasa.UrlFlr}?{rango}&api_key={clave}"),
            new Uri($"{Configuracion.Nasa.UrlFlrCcmc}?{rango}"),
        ];
    }

    /// <summary>El dato de la tormenta: en vivo, de la caché o el respaldo histórico. Nunca lanza excepciones.</summary>
    public async Task<Tormenta> ObtenerAsync(CancellationToken cancelacion = default)
    {
        try
        {
            var ahora = reloj();
            if (LeerCache(ahora) is { } cacheada) return cacheada;
            foreach (var direccion in Direcciones(ahora))
            {
                if (await ConsultarAsync(direccion, ahora, cancelacion).ConfigureAwait(false) is not { } tormenta) continue;
                GuardarCache(tormenta);
                return tormenta;
            }
        }
        catch (Exception error)
        {
            Registrar("DONKI: error inesperado: " + error.Message);
        }
        return DatosDonki.RespaldoHistorico();
    }

    private async Task<Tormenta?> ConsultarAsync(Uri direccion, DateTimeOffset ahora, CancellationToken cancelacion)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancelacion);
        limite.CancelAfter(espera);
        try
        {
            using var respuesta = await http.GetAsync(direccion, limite.Token).ConfigureAwait(false);
            if (!respuesta.IsSuccessStatusCode)
            {
                Registrar($"DONKI: {direccion.Host} respondió {(int)respuesta.StatusCode}");
                return null;
            }
            var json = await respuesta.Content.ReadAsStringAsync(limite.Token).ConfigureAwait(false);
            var llamaradas = ValidacionDonki.Leer(json);
            if (llamaradas is null)
            {
                Registrar($"DONKI: {direccion.Host} no devolvió una lista JSON válida");
                return null;
            }
            if (ValidacionDonki.MasIntensa(llamaradas) is not { } elegida)
            {
                Registrar($"DONKI: {direccion.Host} no tiene llamaradas válidas en 30 días");
                return null;
            }
            return new Tormenta(elegida.Clase, elegida.FechaTexto, "NASA DONKI", elegida.Url, EnVivo: true,
                LlamaradasX: llamaradas.Count(l => l.Clase[0] == 'X'), Consulta: ahora);
        }
        catch (Exception error)
        {
            Registrar($"DONKI: {direccion.Host} no respondió ({error.GetType().Name})");
            return null;
        }
    }

    // ---------- Caché de 6 horas ----------

    private sealed record Guardado(string Clase, string Fecha, string Url, int LlamaradasX, DateTimeOffset Consulta);

    private Tormenta? LeerCache(DateTimeOffset ahora)
    {
        if (rutaCache is null || !File.Exists(rutaCache)) return null;
        try
        {
            var guardado = JsonSerializer.Deserialize<Guardado>(File.ReadAllText(rutaCache));
            if (guardado is null) return null;
            var edad = ahora - guardado.Consulta;
            var valida = edad >= TimeSpan.Zero && edad < TimeSpan.FromMilliseconds(Configuracion.Nasa.CacheMs)
                && ValidacionDonki.ClaseValida(guardado.Clase) && ValidacionDonki.EnlaceSeguro(guardado.Url)
                && DateTimeOffset.TryParse(guardado.Fecha, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _);
            return valida
                ? new Tormenta(guardado.Clase, guardado.Fecha, "NASA DONKI", guardado.Url, EnVivo: true, guardado.LlamaradasX, guardado.Consulta)
                : null;
        }
        catch (Exception error) when (error is IOException or JsonException or UnauthorizedAccessException)
        {
            Registrar("DONKI: caché ilegible, se ignora");
            return null;
        }
    }

    private void GuardarCache(Tormenta tormenta)
    {
        if (rutaCache is null || tormenta.Consulta is not { } consulta) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(rutaCache)!);
            File.WriteAllText(rutaCache, JsonSerializer.Serialize(new Guardado(tormenta.Clase, tormenta.Fecha, tormenta.Url, tormenta.LlamaradasX, consulta)));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Registrar("DONKI: no se pudo guardar la caché");
        }
    }

    private void Registrar(string mensaje) => registro?.Invoke(mensaje);

    public void Dispose() => http.Dispose();
}
