using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using BaseLunar.App.Controles;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.PruebasUI;

/// <summary>
/// Los textos de pantalla que ya existen en la web se copian literalmente de index.html: esta prueba lee
/// el archivo (sin modificarlo) y busca cada texto, sin etiquetas HTML ni emojis.
/// </summary>
[TestClass]
public sealed partial class PruebasTextosApp
{
    private static readonly Lazy<string> Html = new(() =>
    {
        var html = File.ReadAllText(Path.Combine(Rutas.Raiz(), "index.html"));
        var sinEtiquetas = WebUtility.HtmlDecode(Etiqueta().Replace(html, " "));
        return Normalizar(sinEtiquetas);
    });

    private static string Normalizar(string texto) => Espacios().Replace(Iconografia.SinIconos(texto), " ");

    public static IEnumerable<object[]> TextosDeLaWeb()
    {
        string[] textos =
        [
            TextosApp.TituloVentana, TextosApp.Titulo, TextosApp.Subtitulo, TextosApp.DatoReal,
            TextosApp.Inicio.Etiqueta, TextosApp.Inicio.Titulo, TextosApp.Inicio.Historia, TextosApp.Inicio.Nota,
            .. TextosApp.Inicio.Instrucciones,
            TextosApp.Abortada.Etiqueta, TextosApp.Abortada.Titulo, TextosApp.Abortada.Historia,
            TextosApp.Victoria.Etiqueta, TextosApp.Victoria.Titulo, TextosApp.Victoria.Historia,
            TextosApp.Victoria.Fecha, TextosApp.Victoria.Aprendiste, TextosApp.Victoria.Nota,
        ];
        return textos.Select(t => new object[] { t });
    }

    [TestMethod(DisplayName = "Cada texto de pantalla que existe en la web aparece igual en index.html")]
    [DynamicData(nameof(TextosDeLaWeb))]
    public void TextoCopiadoDeLaWeb(string texto)
    {
        var buscado = Normalizar(texto);
        Assert.Contains(buscado, Html.Value, $"No está en index.html: «{buscado}»");
    }

    [TestMethod(DisplayName = "Los nombres de las regiones accesibles son los de la web")]
    public void RegionesAccesibles()
    {
        var html = File.ReadAllText(Path.Combine(Rutas.Raiz(), "index.html"));
        Assert.Contains($"aria-label=\"{TextosApp.RegionMedidores}\"", html);
        Assert.Contains($"aria-label=\"{TextosApp.RegionTarjeta}\"", html);
        Assert.Contains($"aria-label=\"{TextosApp.RegionDecisiones}\"", html);
        Assert.Contains("<span class=\"sr-only\">Opción A: </span>", html);
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Etiqueta();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espacios();
}
