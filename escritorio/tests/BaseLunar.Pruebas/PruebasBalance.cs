using System.Globalization;
using BaseLunar.Nucleo;

namespace BaseLunar.Pruebas;

/// <summary>
/// Tabla de balance de la primera respuesta (14-11-2026), calculada a mano y verificada con un simulador aparte:
/// estado tras el Nivel 4 en las 16 combinaciones y qué opción gana en el Nivel 5, sin y con clase X.
/// «N3:Bateria» significa que la ruta falla en el Nivel 3 por ⚡ (y se activa un respaldo).
/// </summary>
[TestClass]
public sealed class PruebasBalance
{
    [TestMethod]
    [DataRow("AAAA", "N4:Soporte", "N4:Soporte", "", "")]
    [DataRow("AAAB", "55/10/100", "50/10/100", "AB", "AB")]
    [DataRow("AABA", "N4:Soporte", "N4:Soporte", "", "")]
    [DataRow("AABB", "55/10/85", "50/10/85", "AB", "AB")]
    [DataRow("ABAA", "100/40/70", "100/40/70", "AB", "AB")]
    [DataRow("ABAB", "40/70/100", "35/70/100", "A", "A")]
    [DataRow("ABBA", "100/40/55", "100/40/55", "AB", "AB")]
    [DataRow("ABBB", "50/70/85", "45/70/85", "AB", "AB")]
    [DataRow("BAAA", "100/30/10", "100/30/10", "B", "B")]
    [DataRow("BAAB", "55/60/40", "50/60/40", "AB", "AB")]
    [DataRow("BABA", "N3:Bateria", "N3:Bateria", "", "")]
    [DataRow("BABB", "N3:Bateria", "N3:Bateria", "", "")]
    [DataRow("BBAA", "100/70/10", "100/70/10", "B", "B")]
    [DataRow("BBAB", "40/100/40", "35/100/40", "A", "A")]
    [DataRow("BBBA", "N3:Bateria", "N3:Bateria", "", "")]
    [DataRow("BBBB", "N3:Bateria", "N3:Bateria", "", "")]
    public void TablaDeBalance(string ruta, string sinX, string conX, string ganaSinX, string ganaConX)
    {
        Comprobar(ruta, claseX: false, sinX, ganaSinX);
        Comprobar(ruta, claseX: true, conX, ganaConX);
    }

    private static void Comprobar(string ruta, bool claseX, string esperado, string ganadorasEsperadas)
    {
        var contexto = $"{ruta} {(claseX ? "con" : "sin")} clase X";
        var r = Utilidades.Jugar(ruta, claseX);
        if (esperado.StartsWith('N'))
        {
            var partes = esperado.Split(':');
            Assert.AreEqual(Fase.Respaldo, r.Estado.Fase, $"{contexto}: se esperaba un respaldo");
            Assert.AreEqual(int.Parse(partes[0][1..], CultureInfo.InvariantCulture), r.Estado.Nivel, contexto);
            Assert.AreEqual(Enum.Parse<Medidor>(partes[1]), r.Estado.Fallidos[0], contexto);
            return;
        }
        var valores = esperado.Split('/').Select(v => int.Parse(v, CultureInfo.InvariantCulture)).ToArray();
        Assert.AreEqual(Fase.Resultado, r.Estado.Fase, contexto);
        Assert.AreEqual(new Medidores(valores[0], valores[1], valores[2]), r.Estado.Medidores, contexto);
        var n5 = Motor.Continuar(r.Estado, new ContextoPartida(ClaseX: claseX)).Estado;
        var ganadoras = string.Concat(Guion.Niveles[4].Opciones
            .Where(o => Motor.CumpleRequisito(n5.Medidores, o.Value.Requisito!))
            .Select(o => o.Key));
        Assert.AreEqual(ganadorasEsperadas, ganadoras, $"{contexto}: opciones ganadoras en el Nivel 5");
    }
}
