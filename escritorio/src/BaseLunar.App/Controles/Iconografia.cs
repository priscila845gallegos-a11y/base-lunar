using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace BaseLunar.App.Controles;

/// <summary>Un trozo de texto: palabras o un emoji que se dibuja como ícono vectorial.</summary>
public sealed record Trozo(string Texto, string? Icono, string? Pincel);

/// <summary>
/// WPF no dibuja emojis en color. Los textos del juego (los mismos de la web) llevan emojis, así que aquí cada
/// emoji se cambia por un ícono vectorial; el lector de pantalla recibe el texto sin ellos.
/// </summary>
public static partial class Iconografia
{
    private const char Variante = '️';

    /// <summary>Emoji → (clave del ícono en Temas/Iconos.xaml, pincel opcional de Temas/Paleta.xaml).</summary>
    private static readonly Dictionary<string, (string Icono, string? Pincel)> PorEmoji = new()
    {
        ["🛡"] = ("Icono.Proteccion", "Pincel.Proteccion"),
        ["💧"] = ("Icono.Soporte", "Pincel.Soporte"),
        ["⚡"] = ("Icono.Bateria", "Pincel.Bateria"),
        ["💡"] = ("Icono.Bombilla", "Pincel.Estrella"),
        ["📡"] = ("Icono.Antena", null),
        ["🚀"] = ("Icono.Cohete", null),
        ["🪂"] = ("Icono.Salvavidas", null),
        ["🏅"] = ("Icono.Estrella", "Pincel.Estrella"),
        ["⭐"] = ("Icono.Estrella", "Pincel.Estrella"),
        ["🌕"] = ("Icono.Luna", null),
        ["🌑"] = ("Icono.Luna", null),
        ["🌱"] = ("Icono.Brote", null),
        ["☢"] = ("Icono.Radiacion", null),
        ["☀"] = ("Icono.Sol", null),
        ["⚠"] = ("Icono.Alerta", null),
    };

    private static string Normalizar(string elemento) => elemento.Replace(Variante.ToString(), string.Empty, StringComparison.Ordinal);

    /// <summary>Clave del ícono que corresponde a un emoji, o null si no hay.</summary>
    public static string? ClaveDe(string? emoji) =>
        emoji is not null && PorEmoji.TryGetValue(Normalizar(emoji), out var par) ? par.Icono : null;

    /// <summary>Busca la geometría de un emoji en los recursos de la aplicación.</summary>
    public static Geometry? GeometriaDe(string? emoji) =>
        ClaveDe(emoji) is { } clave ? Application.Current?.TryFindResource(clave) as Geometry : null;

    /// <summary>Parte el texto en palabras e íconos, respetando los emojis compuestos (base + U+FE0F).</summary>
    public static IReadOnlyList<Trozo> Partir(string texto)
    {
        var trozos = new List<Trozo>();
        var actual = new StringBuilder();
        var elementos = StringInfo.GetTextElementEnumerator(texto);
        while (elementos.MoveNext())
        {
            var elemento = elementos.GetTextElement();
            if (PorEmoji.TryGetValue(Normalizar(elemento), out var par))
            {
                if (actual.Length > 0) trozos.Add(new Trozo(actual.ToString(), null, null));
                actual.Clear();
                trozos.Add(new Trozo(elemento, par.Icono, par.Pincel));
            }
            else
            {
                actual.Append(elemento);
            }
        }
        if (actual.Length > 0) trozos.Add(new Trozo(actual.ToString(), null, null));
        return trozos;
    }

    /// <summary>El texto sin emojis y sin espacios dobles: es lo que lee el lector de pantalla.</summary>
    public static string SinIconos(string texto)
    {
        var palabras = string.Concat(Partir(texto).Where(t => t.Icono is null).Select(t => t.Texto));
        return EspaciosDobles().Replace(palabras, " ").Trim();
    }

    [GeneratedRegex(@"[ \t]{2,}")]
    private static partial Regex EspaciosDobles();
}
