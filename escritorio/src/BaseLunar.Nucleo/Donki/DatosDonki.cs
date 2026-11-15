using System.Text.Json;

namespace BaseLunar.Nucleo;

/// <summary>
/// Dato de una llamarada para el Nivel 4: clase, fecha, fuente, enlace, si es en vivo, cuántas de clase X hubo
/// y, si es en vivo, cuándo se consultó (para la etiqueta honesta de la sección 7).
/// </summary>
public sealed record Tormenta(string Clase, string Fecha, string Fuente, string Url, bool EnVivo, int LlamaradasX, DateTimeOffset? Consulta = null);

/// <summary>
/// Datos de NASA DONKI: el respaldo histórico (copia literal, la misma de js/nasaData.js). La consulta en
/// vivo, con espera máxima, validación y caché, está en <see cref="ClienteDonki"/>.
/// </summary>
public static class DatosDonki
{
    private const string NombreRecurso = "BaseLunar.Nucleo.Donki.respaldo-mayo-2024.json";

    /// <summary>JSON literal de las 5 llamaradas X del 14 y 15-05-2024 (docs/FUENTES.md, F-12 y F-13).</summary>
    public static string LeerRespaldoLiteral()
    {
        using var flujo = typeof(DatosDonki).Assembly.GetManifestResourceStream(NombreRecurso)
            ?? throw new InvalidOperationException("Falta el recurso " + NombreRecurso);
        using var lector = new StreamReader(flujo);
        return lector.ReadToEnd();
    }

    /// <summary>Dato de tormenta del respaldo histórico: la llamarada configurada en FlrRespaldoId.</summary>
    public static Tormenta RespaldoHistorico()
    {
        using var documento = JsonDocument.Parse(LeerRespaldoLiteral());
        var lista = documento.RootElement.EnumerateArray().ToList();
        var principal = lista.First(f => f.GetProperty("flrID").GetString() == Configuracion.Nasa.FlrRespaldoId);
        return new Tormenta(
            Clase: Texto(principal, "classType"),
            Fecha: Texto(principal, "peakTime", "beginTime"),
            Fuente: "NASA DONKI",
            Url: Texto(principal, "link"),
            EnVivo: false,
            LlamaradasX: lista.Count);
    }

    /// <summary>Primer campo de texto no vacío de la lista (peakTime o, si falta, beginTime).</summary>
    private static string Texto(JsonElement elemento, params string[] campos)
    {
        foreach (var campo in campos)
        {
            if (elemento.TryGetProperty(campo, out var valor) && valor.ValueKind == JsonValueKind.String && valor.GetString() is { Length: > 0 } texto)
                return texto;
        }
        throw new InvalidOperationException("Falta el campo " + string.Join(" o ", campos));
    }
}
