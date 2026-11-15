namespace BaseLunar.App.VistaModelos;

/// <summary>
/// Solo se abren enlaces https de la NASA y de la CSA, las fuentes del juego. Así un dato inesperado
/// (por ejemplo, de la consulta en vivo de F3.5) nunca abre otra cosa en el equipo del niño.
/// </summary>
public static class Enlaces
{
    private static readonly string[] Dominios = ["nasa.gov", "asc-csa.gc.ca"];

    public static bool EsSeguro(Uri? direccion) =>
        direccion is { IsAbsoluteUri: true }
        && direccion.Scheme == Uri.UriSchemeHttps
        && Dominios.Any(d => direccion.Host.Equals(d, StringComparison.OrdinalIgnoreCase)
                          || direccion.Host.EndsWith("." + d, StringComparison.OrdinalIgnoreCase));
}
