using System.Globalization;

namespace BaseLunar.App.VistaModelos;

/// <summary>
/// Una palabra difícil con su explicación para niños. Cada explicación tiene su fuente, verificada en
/// docs/FUENTES.md (G-01 a G-06), y frases de 15 palabras o menos.
/// </summary>
/// <param name="Palabra">Cómo aparece en el botón «?».</param>
/// <param name="Busca">Lo que tiene que aparecer en los textos de sus niveles (lo comprueba una prueba).</param>
public sealed record Termino(string Palabra, string Busca, string Definicion, IReadOnlyList<FuenteVM> Fuentes, IReadOnlyList<int> Niveles)
{
    /// <summary>Nombre accesible del botón: «¿Qué es regolito?».</summary>
    public string Pregunta => Textos.Rellenar(TextosApp.PreguntaGlosario, ("palabra", Palabra));

    /// <summary>La explicación con el formato del 💡 Dato real, para pintarla con sus fuentes.</summary>
    public DatoVM ComoDato() => new(
        char.ToUpper(Palabra[0], CultureInfo.GetCultureInfo("es")) + Palabra[1..],
        Definicion,
        [],
        Presentacion.EtiquetaFuentes(Fuentes.Count),
        Fuentes);
}

/// <summary>
/// Glosario de escritorio (propuesta de F3, «Lenguaje»): las palabras científicas de cada nivel, explicadas
/// en frases cortas. Solo entran palabras con fuente verificada (D-45).
/// </summary>
public static class Glosario
{
    private static FuenteVM Nasa(string nombre, string url) => new(nombre, url);

    public static readonly IReadOnlyList<Termino> Terminos =
    [
        new("soporte vital", "soporte",
            "Son las máquinas que dan agua limpia y oxígeno para respirar.",
            [Nasa("NASA, sistemas de soporte vital (ECLSS)", "https://www.nasa.gov/reference/environmental-control-and-life-support-systems-eclss/")],
            [1, 2]),
        new("rayos cósmicos", "rayos cósmicos",
            "No son rayos de luz: son partículas con mucha energía. Viajan por el espacio casi a la velocidad de la luz.",
            [Nasa("NASA, Imagine the Universe: Cosmic Rays", "https://imagine.gsfc.nasa.gov/science/toolbox/cosmic_rays1.html")],
            [3]),
        new("campo magnético", "campo magnético",
            "Es un escudo invisible de la Tierra. Desvía casi todo el material que llega del Sol. El de la Luna es muy débil.",
            [
                Nasa("NASA, Earth's Magnetosphere", "https://science.nasa.gov/science-research/planetary-science/earths-magnetosphere/"),
                Nasa("NASA, Moon Facts", "https://science.nasa.gov/moon/facts/"),
            ],
            [3]),
        new("regolito", "regolito",
            "Es el polvo gris y las piedritas que cubren casi toda la Luna.",
            [Nasa("NASA, Moon Facts", "https://science.nasa.gov/moon/facts/")],
            [3]),
        new("llamarada solar", "llamarada",
            "Es un destello muy fuerte de luz y radiación en el Sol.",
            [Nasa("NASA, la mayor llamarada del ciclo 25", "https://svs.gsfc.nasa.gov/14592")],
            [4, 5]),
        new("clase X", "clase X",
            "Así se llaman las llamaradas más fuertes. De menor a mayor, las clases son B, C, M y X.",
            [Nasa("NASA, X-Class: A Guide to Solar Flares", "https://svs.gsfc.nasa.gov/10109")],
            [4]),
    ];

    /// <summary>Las palabras del glosario que salen en un nivel.</summary>
    public static IReadOnlyList<Termino> De(int nivel) => Terminos.Where(t => t.Niveles.Contains(nivel)).ToArray();
}
