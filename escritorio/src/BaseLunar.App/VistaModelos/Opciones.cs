using System.Globalization;

namespace BaseLunar.App.VistaModelos;

/// <summary>
/// Opciones de la línea de comandos: los parámetros de demostración de la web (?nivel, ?offline, ?debug) en
/// versión de escritorio, más --silencio y --alto-contraste (D-08, D-56).
/// </summary>
/// <param name="Nivel">--nivel N: cada partida empieza en el nivel N (1 a 5); cualquier otro valor se ignora.</param>
/// <param name="Offline">--offline: siempre el respaldo histórico de NASA DONKI.</param>
/// <param name="Depuracion">--debug: muestra el estado interno y el registro técnico.</param>
/// <param name="Silencio">--silencio: sin sonido ni voz y sin guardar ajustes.</param>
/// <param name="AltoContraste">--alto-contraste: colores de alto contraste aunque Windows no los tenga activados.</param>
public sealed record Opciones(int? Nivel = null, bool Offline = false, bool Depuracion = false, bool Silencio = false, bool AltoContraste = false)
{
    public static Opciones Leer(IReadOnlyList<string> argumentos)
    {
        var opciones = new Opciones();
        for (var i = 0; i < argumentos.Count; i++)
        {
            var argumento = argumentos[i].Trim().ToLowerInvariant();
            if (argumento.StartsWith("--nivel", StringComparison.Ordinal))
            {
                var valor = argumento.Length > "--nivel".Length && argumento["--nivel".Length] == '='
                    ? argumento["--nivel=".Length..]
                    : i + 1 < argumentos.Count && argumento == "--nivel" ? argumentos[++i] : null;
                opciones = opciones with { Nivel = NivelValido(valor) };
                continue;
            }
            opciones = argumento switch
            {
                "--offline" => opciones with { Offline = true },
                "--debug" => opciones with { Depuracion = true },
                "--silencio" => opciones with { Silencio = true },
                "--alto-contraste" => opciones with { AltoContraste = true },
                _ => opciones,
            };
        }
        return opciones;
    }

    /// <summary>Las opciones con las que se abrió el programa.</summary>
    public static Opciones DelProceso() => Leer(Environment.GetCommandLineArgs().Skip(1).ToArray());

    private static int? NivelValido(string? valor) =>
        int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var nivel) && nivel >= 1 && nivel <= Guion.Niveles.Count ? nivel : null;
}
