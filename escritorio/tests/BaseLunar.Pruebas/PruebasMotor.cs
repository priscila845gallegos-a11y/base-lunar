using BaseLunar.Nucleo;
using static BaseLunar.Pruebas.Utilidades;

namespace BaseLunar.Pruebas;

/// <summary>
/// Pruebas del motor: las mismas 21 de tests/tests.html (sección 10 del Prompt 0, robustez y antibloqueo),
/// con los mismos datos y los mismos valores esperados.
/// </summary>
[TestClass]
public sealed class PruebasMotor
{
    private static readonly ContextoPartida ConX = new(ClaseX: true);

    // ---------- Motor (sección 10 del Prompt 0) ----------
    [TestMethod(DisplayName = "Aplicar efectos suma y resta bien")]
    public void AplicarEfectosSumaYResta()
    {
        var r = Motor.AplicarEfectos(M(80, 80, 80), Guion.Efectos((Medidor.Bateria, 20), (Medidor.Soporte, -35)));
        Assert.AreEqual(M(80, 45, 100), r.Medidores);
        Assert.IsEmpty(r.Fallidos);
    }

    [TestMethod(DisplayName = "Una ganancia que pasa de 100 se recorta y se marca «al máximo»")]
    public void GananciaSeRecortaAlMaximo()
    {
        var r = Motor.AplicarEfectos(M(80, 45, 100), Guion.Efectos((Medidor.Proteccion, 25), (Medidor.Soporte, -35)));
        Assert.AreEqual(M(100, 10, 100), r.Medidores);
        Assert.AreEqual(105, r.Crudos.Proteccion);
        CollectionAssert.AreEqual(new[] { Medidor.Proteccion }, r.AlMaximo.ToArray());
    }

    [TestMethod(DisplayName = "Los valores se recortan a 0 y el medidor falla")]
    public void ValoresSeRecortanACero()
    {
        var r = Motor.AplicarEfectos(M(100, 10, 70), Guion.Efectos((Medidor.Proteccion, 35), (Medidor.Soporte, -30)));
        Assert.AreEqual(0, r.Medidores.Soporte);
        Assert.AreEqual(-20, r.Crudos.Soporte);
        CollectionAssert.AreEqual(new[] { Medidor.Soporte }, r.Fallidos.ToArray());
    }

    [TestMethod(DisplayName = "Quedar exactamente en 0 también es un fallo")]
    public void CeroExactoEsFallo()
    {
        var r = Motor.AplicarEfectos(M(100, 30, 70), Guion.Efectos((Medidor.Proteccion, 35), (Medidor.Soporte, -30)));
        CollectionAssert.AreEqual(new[] { Medidor.Soporte }, r.Fallidos.ToArray());
    }

    [TestMethod(DisplayName = "Un medidor en 0 o menos activa el respaldo y resta 1")]
    public void FalloActivaRespaldo()
    {
        var r = Jugar("BAB");
        Assert.AreEqual(Fase.Respaldo, r.Estado.Fase);
        Assert.AreEqual(2, r.Estado.Respaldos);
        CollectionAssert.AreEqual(new[] { Medidor.Bateria }, r.Estado.Fallidos.ToArray());
        Assert.IsTrue(r.Hay<RespaldoActivado>(e => e.Motivo == Medidor.Bateria), "evento RespaldoActivado");
    }

    [TestMethod(DisplayName = "Reintento: restaura la foto y sube a 30 o más el medidor que falló")]
    public void ReintentoRestauraFoto()
    {
        Assert.AreEqual(M(100, 60, 40), Motor.Reintentar(Jugar("BAB").Estado).Estado.Medidores, "⚡ = máx(40, 30)");
        var r = Motor.Reintentar(Jugar("AAAA").Estado);
        Assert.AreEqual(Fase.Nivel, r.Estado.Fase);
        Assert.AreEqual(M(100, 30, 70), r.Estado.Medidores, "💧 = máx(10, 30); el resto, como en la foto");
    }

    [TestMethod(DisplayName = "Reintento: el estado restaurado pasa a ser la foto (D-01)")]
    public void ReintentoRenuevaFoto()
    {
        var r = Motor.Reintentar(Jugar("AAAA").Estado);
        Assert.AreEqual(r.Estado.Medidores, r.Estado.Foto);
    }

    [TestMethod(DisplayName = "Con 0 respaldos se llega a Misión Abortada")]
    public void SinRespaldosMisionAbortada()
    {
        var r = Jugar("AAAA");
        r = Motor.AplicarDecision(Motor.Reintentar(r.Estado).Estado, "A");
        Assert.AreEqual(1, r.Estado.Respaldos, "💧 30 − 30 = 0 vuelve a fallar");
        r = Motor.AplicarDecision(Motor.Reintentar(r.Estado).Estado, "A");
        Assert.AreEqual(Fase.MisionAbortada, r.Estado.Fase);
        Assert.AreEqual(0, r.Estado.Respaldos);
        Assert.IsTrue(r.Hay<MisionAbortada>(), "evento MisionAbortada");
    }

    [TestMethod(DisplayName = "Nivel 5: las condiciones son estrictas (>, no ≥)")]
    public void Nivel5CondicionesEstrictas()
    {
        var a = Guion.Niveles[4].Opciones["A"].Requisito!;
        var b = Guion.Niveles[4].Opciones["B"].Requisito!;
        Assert.IsFalse(Motor.CumpleRequisito(M(0, 0, 35), a), "⚡ 35");
        Assert.IsTrue(Motor.CumpleRequisito(M(0, 0, 36), a), "⚡ 36");
        Assert.IsFalse(Motor.CumpleRequisito(M(40, 0, 0), b), "🛡️ 40");
        Assert.IsTrue(Motor.CumpleRequisito(M(41, 0, 0), b), "🛡️ 41");
    }

    [TestMethod(DisplayName = "Nivel 5: elegir mal resta 1 respaldo y no cambia los medidores")]
    public void Nivel5ElegirMal()
    {
        var r = Motor.AplicarDecision(EnNivel(5, M(40, 70, 100)), "B");
        Assert.AreEqual(Fase.Respaldo, r.Estado.Fase);
        Assert.AreEqual(2, r.Estado.Respaldos);
        Assert.AreEqual(M(40, 70, 100), r.Estado.Medidores);
        var reintento = Motor.Reintentar(r.Estado);
        Assert.AreEqual(M(40, 70, 100), reintento.Estado.Medidores);
        Assert.AreEqual(Fase.Resultado, Motor.AplicarDecision(reintento.Estado, "A").Estado.Fase);
    }

    [TestMethod(DisplayName = "Clase X: −5 🛡️ una sola vez, también si el nivel se reintenta")]
    public void ClaseXUnaSolaVez()
    {
        var n4 = Motor.Continuar(Jugar("AAA", claseX: true).Estado, ConX);
        Assert.AreEqual(95, n4.Estado.Medidores.Proteccion);
        Assert.IsTrue(n4.Hay<PenalizacionX>(), "evento PenalizacionX");
        var reintento = Motor.Reintentar(Motor.AplicarDecision(n4.Estado, "A").Estado);
        Assert.AreEqual(95, reintento.Estado.Medidores.Proteccion);
        Assert.IsFalse(reintento.Hay<PenalizacionX>(), "sin penalización al reintentar");
    }

    [TestMethod(DisplayName = "Sin clase X no hay penalización")]
    public void SinClaseXNoHayPenalizacion()
    {
        Assert.AreEqual(100, Motor.Continuar(Jugar("AAA").Estado, new ContextoPartida(ClaseX: false)).Estado.Medidores.Proteccion);
    }

    [TestMethod(DisplayName = "Reiniciar devuelve exactamente el estado inicial")]
    public void ReiniciarDevuelveEstadoInicial()
    {
        var inicial = Motor.CrearEstadoInicial();
        Assert.AreEqual(inicial, Motor.Reiniciar());
        Assert.AreEqual(new Estado(Fase.Inicio, 0, M(80, 80, 80), M(80, 80, 80), 3, false, Array.Empty<Medidor>(), null), inicial);
    }

    // ---------- Robustez y antibloqueo ----------
    [TestMethod(DisplayName = "Doble clic: una segunda decisión no se aplica")]
    public void DobleClic()
    {
        var r1 = Motor.AplicarDecision(Motor.Empezar(Motor.CrearEstadoInicial()).Estado, "A");
        var r2 = Motor.AplicarDecision(r1.Estado, "A");
        Assert.AreSame(r1.Estado, r2.Estado, "mismo estado");
        Assert.IsEmpty(r2.Eventos);
        Assert.AreEqual(M(80, 45, 100), r1.Estado.Medidores);
    }

    [TestMethod(DisplayName = "Las acciones fuera de su fase no hacen nada")]
    public void AccionesFueraDeFase()
    {
        var inicial = Motor.CrearEstadoInicial();
        Assert.AreSame(inicial, Motor.AplicarDecision(inicial, "A").Estado, "decidir en Inicio");
        Assert.AreSame(inicial, Motor.Reintentar(inicial).Estado, "reintentar en Inicio");
        Assert.AreSame(inicial, Motor.Continuar(inicial).Estado, "continuar en Inicio");
    }

    [TestMethod(DisplayName = "Dos medidores a la vez: un solo mensaje (💧 → 🛡️ → ⚡) y se restauran ambos")]
    public void DosMedidoresALaVez()
    {
        var nivel = NivelDePrueba(9,
            Guion.Efectos((Medidor.Proteccion, -90), (Medidor.Soporte, -90), (Medidor.Bateria, 5)),
            Guion.Efectos((Medidor.Bateria, -10), (Medidor.Soporte, 5)));
        var r = Motor.AplicarDecision(EnNivel(1, M(50, 20, 50)), "A", [nivel]);
        CollectionAssert.AreEqual(new[] { Medidor.Soporte, Medidor.Proteccion }, r.Estado.Fallidos.ToArray());
        Assert.IsTrue(r.Hay<RespaldoActivado>(e => e.Motivo == Medidor.Soporte), "mensaje de 💧");
        Assert.AreEqual(M(50, 30, 50), Motor.Reintentar(r.Estado, [nivel]).Estado.Medidores);
    }

    [TestMethod(DisplayName = "Antibloqueo: sube lo justo para que alguna opción sea viable")]
    public void AntibloqueoSubeLoJusto()
    {
        var nivel = NivelDePrueba(9,
            Guion.Efectos((Medidor.Proteccion, 30), (Medidor.Bateria, -30)),
            Guion.Efectos((Medidor.Proteccion, 40), (Medidor.Bateria, -45)));
        var ayuda = Motor.CalcularAyuda(M(100, 60, 20), nivel);
        Assert.IsNotNull(ayuda);
        CollectionAssert.AreEqual(new[] { KeyValuePair.Create(Medidor.Bateria, 11) }, ayuda.Ajustes.ToArray());
        Assert.IsTrue(Motor.EsSobrevivible(ayuda.Medidores, nivel), "sobrevivible tras la ayuda");
        Assert.IsNull(Motor.CalcularAyuda(M(100, 60, 40), nivel), "sin ayuda si hay salida");
    }

    [TestMethod(DisplayName = "Antibloqueo en el Nivel 5: sube lo justo para superar un requisito")]
    public void AntibloqueoNivel5()
    {
        var ayuda = Motor.CalcularAyuda(M(30, 50, 30), Guion.Niveles[4]);
        Assert.IsNotNull(ayuda);
        CollectionAssert.AreEqual(new[] { KeyValuePair.Create(Medidor.Bateria, 6) }, ayuda.Ajustes.ToArray());
    }

    [TestMethod(DisplayName = "Antibloqueo: se activa al entrar en un nivel sin salida")]
    public void AntibloqueoAlEntrar()
    {
        var imposible = NivelDePrueba(1,
            Guion.Efectos((Medidor.Bateria, -90), (Medidor.Soporte, 5)),
            Guion.Efectos((Medidor.Bateria, -95), (Medidor.Proteccion, 5)));
        var r = Motor.Empezar(Motor.CrearEstadoInicial(), new ContextoPartida(), [imposible]);
        Assert.IsTrue(r.Hay<AyudaActivada>(), "evento AyudaActivada");
        Assert.AreEqual(91, r.Estado.Medidores.Bateria);
        Assert.AreEqual(r.Estado.Medidores, r.Estado.Foto);
    }

    [TestMethod(DisplayName = "Continuar: del resultado al siguiente nivel, y del Nivel 5 a la Victoria")]
    public void ContinuarHastaVictoria()
    {
        Assert.AreEqual(2, Motor.Continuar(Jugar("A").Estado).Estado.Nivel);
        var n5 = Motor.Continuar(Jugar("ABAA").Estado, ConX);
        var ganada = Motor.AplicarDecision(n5.Estado, "A");
        Assert.AreEqual(Fase.Resultado, ganada.Estado.Fase);
        var victoria = Motor.Continuar(ganada.Estado);
        Assert.AreEqual(Fase.Victoria, victoria.Estado.Fase);
        Assert.IsTrue(victoria.Hay<Victoria>(), "evento Victoria");
    }

    [TestMethod(DisplayName = "Empezar en un nivel concreto (D-08) con el estado inicial")]
    public void EmpezarEnNivelConcreto()
    {
        var r = Motor.Empezar(Motor.CrearEstadoInicial(), new ContextoPartida(Nivel: 4, ClaseX: true));
        Assert.AreEqual(4, r.Estado.Nivel);
        Assert.AreEqual(M(75, 80, 80), r.Estado.Medidores);
        Assert.AreEqual(1, Motor.Empezar(Motor.CrearEstadoInicial(), new ContextoPartida(Nivel: 9)).Estado.Nivel, "nivel fuera de rango");
    }
}
