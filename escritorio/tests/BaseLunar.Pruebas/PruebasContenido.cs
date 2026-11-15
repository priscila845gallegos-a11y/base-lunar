using System.Text.RegularExpressions;
using BaseLunar.Nucleo;

namespace BaseLunar.Pruebas;

/// <summary>Pruebas de datos y textos: las mismas 6 de tests/tests.html.</summary>
[TestClass]
public sealed partial class PruebasContenido
{
    /// <summary>Valores más largos posibles para los {marcadores} (como en tests.html).</summary>
    private static readonly Dictionary<string, string> ValoresLargos = new()
    {
        ["clase"] = "X8.7",
        ["fecha"] = "14 de septiembre de 2024",
        ["n"] = "5",
        ["recurso"] = "agua y oxígeno y energía",
    };

    private static string Expandir(string texto) =>
        Marcador().Replace(texto, m => ValoresLargos.TryGetValue(m.Groups[1].Value, out var v) ? v : "Los sensores del guiado necesitaban más protección.");

    private static IEnumerable<string> Frases(string texto) => FinDeFrase().Split(texto).Where(f => f.Length > 0);

    private static int Palabras(string frase) =>
        frase.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Count(p => LetraODigito().IsMatch(p));

    /// <summary>Los textos que comprueba tests.html: guion, mensajes de respaldo, ayuda y llamarada.</summary>
    private static IEnumerable<string> TextosDelJuego()
    {
        foreach (var nivel in Guion.Niveles)
        {
            yield return nivel.Historia;
            yield return nivel.Dato.Texto;
            foreach (var extra in nivel.Dato.Extra ?? []) yield return extra;
            foreach (var opcion in nivel.Opciones.Values)
            {
                yield return opcion.Texto;
                yield return opcion.Resultado;
                if (opcion.Fallo is not null) yield return opcion.Fallo;
            }
        }
        string[] mensajes =
        [
            Textos.Respaldo.Soporte, Textos.Respaldo.Proteccion, Textos.Respaldo.Bateria, Textos.Respaldo.Condicion,
            Textos.Respaldo.Pista, Textos.Respaldo.PistaCondicion, Textos.Ayuda, Textos.PenalizacionX,
            Textos.Llamarada.Linea, Textos.Llamarada.SerieX,
        ];
        foreach (var mensaje in mensajes) yield return mensaje;
    }

    [TestMethod(DisplayName = "Historias de 150 caracteres como máximo")]
    public void Historias150Caracteres()
    {
        var largas = Guion.Niveles.Where(n => n.Historia.Length > 150).Select(n => $"Nivel {n.Id}: {n.Historia.Length} caracteres").ToList();
        Assert.IsEmpty(largas, string.Join(" | ", largas));
    }

    [TestMethod(DisplayName = "Frases de 15 palabras o menos (guion y mensajes)")]
    public void Frases15Palabras()
    {
        var largas = TextosDelJuego().Select(Expandir).SelectMany(Frases).Where(f => Palabras(f) > 15).ToList();
        Assert.IsEmpty(largas, string.Join(" | ", largas));
    }

    [TestMethod(DisplayName = "Cada dato tiene al menos una fuente https y está verificado")]
    public void DatosConFuente()
    {
        foreach (var nivel in Guion.Niveles)
        {
            Assert.IsTrue(nivel.Dato.Verificado, $"Nivel {nivel.Id}: verificado");
            Assert.IsTrue(
                nivel.Dato.Fuentes.Count > 0 && nivel.Dato.Fuentes.All(f => f.Nombre.Length > 0 && f.Url.StartsWith("https://", StringComparison.Ordinal)),
                $"Nivel {nivel.Id}: fuentes");
        }
    }

    [TestMethod(DisplayName = "Niveles 1 a 4: ninguna opción es gratis (una ganancia y un costo)")]
    public void NingunaOpcionGratis()
    {
        foreach (var nivel in Guion.Niveles.Where(n => n.Tipo == TipoNivel.Decision))
        {
            foreach (var (id, opcion) in nivel.Opciones)
            {
                var valores = opcion.Efectos!.Values.ToList();
                Assert.IsTrue(valores.Any(v => v > 0) && valores.Any(v => v < 0), $"Nivel {nivel.Id}-{id}");
            }
        }
    }

    [TestMethod(DisplayName = "Ninguna opción está dominada (en sus cifras)")]
    public void NingunaOpcionDominada()
    {
        static int V(IReadOnlyDictionary<Medidor, int> efectos, Medidor m) => efectos.TryGetValue(m, out var v) ? v : 0;
        static bool PeorOIgual(IReadOnlyDictionary<Medidor, int> a, IReadOnlyDictionary<Medidor, int> b) =>
            Medidores.Todos.All(m => V(a, m) <= V(b, m)) && Medidores.Todos.Any(m => V(a, m) < V(b, m));

        foreach (var nivel in Guion.Niveles.Where(n => n.Tipo == TipoNivel.Decision))
        {
            var a = nivel.Opciones["A"].Efectos!;
            var b = nivel.Opciones["B"].Efectos!;
            Assert.IsTrue(!PeorOIgual(a, b) && !PeorOIgual(b, a), $"Nivel {nivel.Id}");
        }
    }

    [TestMethod(DisplayName = "Respaldo histórico: la X8.7 del 14-05-2024 y 5 llamaradas X")]
    public void RespaldoHistorico()
    {
        var t = DatosDonki.RespaldoHistorico();
        Assert.AreEqual("X8.7", t.Clase);
        Assert.AreEqual("2024-05-14T16:51Z", t.Fecha);
        Assert.IsFalse(t.EnVivo);
        Assert.AreEqual(5, t.LlamaradasX);
        Assert.IsTrue(t.Url.StartsWith("https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/", StringComparison.Ordinal), "enlace a DONKI");
    }

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex Marcador();

    [GeneratedRegex(@"(?<=[.!?…])\s+")]
    private static partial Regex FinDeFrase();

    [GeneratedRegex(@"[\p{L}\p{N}]")]
    private static partial Regex LetraODigito();
}
