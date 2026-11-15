namespace BaseLunar.Pruebas;

/// <summary>Las 12 pruebas del simulador de tests/tests.html: 6 comprobaciones, sin y con clase X.</summary>
[TestClass]
public sealed class PruebasSimulador
{
    private static readonly ResumenSimulacion SinX = Simulador.Simular(false);
    private static readonly ResumenSimulacion ConX = Simulador.Simular(true);

    public TestContext TestContext { get; set; } = null!;

    private static ResumenSimulacion De(bool claseX) => claseX ? ConX : SinX;

    [TestMethod]
    [DataRow(false, DisplayName = "Simulador (sin clase X): ningún estado sin salida")]
    [DataRow(true, DisplayName = "Simulador (con clase X): ningún estado sin salida")]
    public void NingunEstadoSinSalida(bool claseX) => Assert.AreEqual(0, De(claseX).SinSalida);

    [TestMethod]
    [DataRow(false, DisplayName = "Simulador (sin clase X): el Nivel 5 siempre tiene una opción ganadora")]
    [DataRow(true, DisplayName = "Simulador (con clase X): el Nivel 5 siempre tiene una opción ganadora")]
    public void Nivel5SiempreGanable(bool claseX) => Assert.AreEqual(0, De(claseX).N5SinGanadora);

    [TestMethod]
    [DataRow(false, DisplayName = "Simulador (sin clase X): el antibloqueo nunca se activa")]
    [DataRow(true, DisplayName = "Simulador (con clase X): el antibloqueo nunca se activa")]
    public void AntibloqueoNuncaSeActiva(bool claseX) => Assert.AreEqual(0, De(claseX).Ayudas);

    [TestMethod]
    [DataRow(false, DisplayName = "Simulador (sin clase X): en ningún estado de ninguna ruta un medidor sale de 0–100")]
    [DataRow(true, DisplayName = "Simulador (con clase X): en ningún estado de ninguna ruta un medidor sale de 0–100")]
    public void MedidoresEnRango(bool claseX) => Assert.AreEqual(0, De(claseX).FueraDeRango);

    [TestMethod]
    [DataRow(false, DisplayName = "Simulador (sin clase X): existe una ruta sin perder respaldos")]
    [DataRow(true, DisplayName = "Simulador (con clase X): existe una ruta sin perder respaldos")]
    public void ExisteRutaPerfecta(bool claseX) => Assert.IsTrue(De(claseX).Perfecta);

    [TestMethod]
    [DataRow(false, DisplayName = "Simulador (sin clase X): existe una ruta que pierde respaldos")]
    [DataRow(true, DisplayName = "Simulador (con clase X): existe una ruta que pierde respaldos")]
    public void ExisteRutaConPerdida(bool claseX) => Assert.IsTrue(De(claseX).ConPerdida);

    [TestMethod]
    [DataRow(false, DisplayName = "Simulador (sin clase X): reproduce las cifras de referencia (59 · 16/13/16 · 14)")]
    [DataRow(true, DisplayName = "Simulador (con clase X): reproduce las cifras de referencia (59 · 16/13/16 · 14)")]
    public void CifrasDeReferencia(bool claseX)
    {
        var s = De(claseX);
        TestContext.WriteLine($"Simulador {(claseX ? "con" : "sin")} clase X: {s}");
        CollectionAssert.AreEqual(new[] { 59, 16, 13, 16, 14 }, new[] { s.Rutas, s.Victorias[3], s.Victorias[2], s.Victorias[1], s.Abortadas });
    }
}
