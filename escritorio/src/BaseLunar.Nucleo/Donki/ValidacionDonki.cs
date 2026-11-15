using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BaseLunar.Nucleo;

/// <summary>Una llamarada válida de DONKI: identificador, clase («M3.5»), fecha (pico o inicio) y enlace al registro.</summary>
public sealed record Llamarada(string Id, string Clase, DateTimeOffset Fecha, string FechaTexto, string Url);

/// <summary>
/// Validación de la respuesta FLR de DONKI (sección 7): tiene que ser una lista; classType, una letra
/// (A, B, C, M o X) seguida de un número; la fecha (peakTime o, si falta, beginTime), válida; y el enlace,
/// https de la NASA. Lo que no cumple se descarta.
/// </summary>
public static partial class ValidacionDonki
{
    /// <summary>Las llamaradas válidas, o null si la respuesta no es una lista JSON.</summary>
    public static IReadOnlyList<Llamarada>? Leer(string json)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            if (documento.RootElement.ValueKind != JsonValueKind.Array) return null;
            var validas = new List<Llamarada>();
            foreach (var elemento in documento.RootElement.EnumerateArray())
            {
                if (elemento.ValueKind == JsonValueKind.Object && Validar(elemento) is { } llamarada) validas.Add(llamarada);
            }
            return validas;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static Llamarada? Validar(JsonElement e)
    {
        var clase = Texto(e, "classType");
        if (clase is null || !ClaseValida(clase)) return null;
        var fechaTexto = Texto(e, "peakTime") ?? Texto(e, "beginTime");
        if (fechaTexto is null || !DateTimeOffset.TryParse(fechaTexto, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var fecha)) return null;
        var url = Texto(e, "link");
        if (url is null || !EnlaceSeguro(url)) return null;
        return new Llamarada(Texto(e, "flrID") ?? string.Empty, clase, fecha, fechaTexto, url);
    }

    private static string? Texto(JsonElement e, string campo) =>
        e.TryGetProperty(campo, out var valor) && valor.ValueKind == JsonValueKind.String && valor.GetString() is { Length: > 0 } texto ? texto : null;

    /// <summary>Una letra A, B, C, M o X seguida de un número («X8.7», «M1», «C3.2»).</summary>
    public static bool ClaseValida(string clase) => Clase().IsMatch(clase);

    /// <summary>Solo enlaces https de la NASA (por ejemplo, el registro de ccmc.gsfc.nasa.gov).</summary>
    public static bool EnlaceSeguro(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var direccion)
        && direccion.Scheme == Uri.UriSchemeHttps
        && (direccion.Host.Equals("nasa.gov", StringComparison.OrdinalIgnoreCase) || direccion.Host.EndsWith(".nasa.gov", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Intensidad del pico de rayos X: cada letra multiplica por 10 a la anterior y el número multiplica dentro
    /// de la letra (una X2 es el doble que una X1).
    /// </summary>
    public static double Intensidad(string clase)
    {
        var potencia = clase[0] switch { 'A' => -8, 'B' => -7, 'C' => -6, 'M' => -5, _ => -4 };
        var numero = double.Parse(clase[1..], CultureInfo.InvariantCulture);
        return numero * Math.Pow(10, potencia);
    }

    /// <summary>La más intensa (D-09); si empatan, la más reciente. Null si no hay ninguna.</summary>
    public static Llamarada? MasIntensa(IEnumerable<Llamarada> llamaradas) =>
        llamaradas.OrderByDescending(l => Intensidad(l.Clase)).ThenByDescending(l => l.Fecha).FirstOrDefault();

    [GeneratedRegex(@"^[ABCMX]\d+(\.\d+)?$")]
    private static partial Regex Clase();
}
