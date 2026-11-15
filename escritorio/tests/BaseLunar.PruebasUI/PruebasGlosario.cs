using System.Text.RegularExpressions;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.PruebasUI;

/// <summary>
/// Glosario de escritorio (D-45): solo palabras con fuente verificada (docs/FUENTES.md, G-01 a G-06),
/// en frases de 15 palabras o menos, y cada palabra aparece de verdad en los textos de sus niveles.
/// </summary>
[TestClass]
public sealed partial class PruebasGlosario
{
    [TestMethod(DisplayName = "Cada explicación tiene fuente https de la NASA y frases de 15 palabras o menos")]
    public void FuentesYFrasesCortas()
    {
        Assert.HasCount(6, Glosario.Terminos);
        foreach (var termino in Glosario.Terminos)
        {
            Assert.IsNotEmpty(termino.Fuentes, termino.Palabra);
            foreach (var fuente in termino.Fuentes) Assert.IsTrue(Enlaces.EsSeguro(new Uri(fuente.Url)), fuente.Url);
            foreach (var frase in Frase().Split(termino.Definicion).Where(f => f.Trim().Length > 0))
            {
                var palabras = frase.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsLessThanOrEqualTo(15, palabras, $"«{frase.Trim()}» ({termino.Palabra})");
            }
        }
    }

    [TestMethod(DisplayName = "Cada palabra del glosario aparece en los textos de los niveles donde se ofrece")]
    public void PalabrasDeSuNivel()
    {
        foreach (var termino in Glosario.Terminos)
        {
            foreach (var id in termino.Niveles)
            {
                var textos = TextosDelNivel(Guion.Niveles[id - 1]);
                Assert.Contains(termino.Busca.ToLowerInvariant(), textos.ToLowerInvariant(), $"«{termino.Busca}» no sale en el Nivel {id}");
            }
        }
    }

    [TestMethod(DisplayName = "Abrir una palabra muestra su explicación con fuentes y la anuncia; al cambiar de nivel se cierra")]
    public void AbrirYCerrar() => HiloUI.Ejecutar(() =>
    {
        var juego = Partida.Jugar("P", "A", "P", "B", "P");
        CollectionAssert.AreEqual(new[] { "rayos cósmicos", "campo magnético", "regolito" }, juego.Terminos.Select(t => t.Palabra).ToArray());
        var anuncios = new List<Anuncio>();
        juego.Anunciar += (_, a) => anuncios.Add(a);
        var pantalla = Capturas.Montar(juego, 1280, 720);
        var regolito = juego.Terminos.Single(t => t.Palabra == "regolito");
        Assert.AreEqual("¿Qué es regolito?", regolito.Pregunta);
        juego.VerTermino.Execute(regolito);
        Assert.AreEqual("Regolito", juego.Definicion!.Titulo);
        Assert.AreEqual("Regolito: Es el polvo gris y las piedritas que cubren casi toda la Luna.", anuncios.Single().Texto);
        Capturas.Guardar(pantalla, "f33-glosario-regolito");
        juego.VerTermino.Execute(regolito);
        Assert.IsNull(juego.Definicion, "Pulsar otra vez la cierra");
        juego.VerTermino.Execute(regolito);
        Partida.Paso(juego, "A");
        Partida.Paso(juego, "P");
        Assert.IsNull(juego.Definicion, "Al pasar de nivel se cierra");
        CollectionAssert.AreEqual(new[] { "llamarada solar", "clase X" }, juego.Terminos.Select(t => t.Palabra).ToArray());
    });

    /// <summary>Todo lo que se lee en un nivel: tarjeta, opciones, resultado, dato, avisos y nombres de los medidores.</summary>
    private static string TextosDelNivel(Nivel nivel)
    {
        var partes = new List<string> { nivel.Titulo, nivel.Etiqueta, nivel.Historia, nivel.Dato.Texto };
        partes.AddRange(nivel.Dato.Extra ?? []);
        partes.AddRange(nivel.Opciones.Values.SelectMany(o => new[] { o.Texto, o.Resultado, o.Fallo ?? string.Empty }));
        partes.AddRange(Guion.InfoMedidores.Select(m => m.Nombre));
        if (nivel.AplicaPenalizacionX) partes.Add(Textos.PenalizacionX);
        return string.Join(" ", partes);
    }

    [GeneratedRegex(@"[.!?]")]
    private static partial Regex Frase();
}
