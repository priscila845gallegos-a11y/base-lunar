using BaseLunar.App.VistaModelos;

namespace BaseLunar.PruebasUI;

/// <summary>
/// El modelo de vista sigue el flujo de ui.js y no cambia ninguna regla: cada estado que publica es el que
/// devuelve el motor para las mismas acciones.
/// </summary>
[TestClass]
public sealed class PruebasJuegoVM
{
    [TestMethod(DisplayName = "Victoria sin respaldos (A, B, A, A, A): mismos medidores que el motor y certificado 3 de 3")]
    public void Victoria() => HiloUI.Ejecutar(() =>
    {
        var juego = Partida.Jugar(Partida.HastaVictoria);
        Assert.AreEqual(Fase.Victoria, juego.Fase);
        Assert.AreEqual(new Medidores(100, 40, 70), juego.Estado.Medidores);
        Assert.AreEqual("Respaldos sin usar: 3 de 3", juego.TextoRespaldosCertificado);
        Assert.HasCount(3, juego.Estrellas);
        Assert.HasCount(5, juego.DatosCertificado);
        Assert.AreEqual("¡Misión cumplida!", juego.TextoProgreso);
        Assert.IsTrue(juego.Estaciones.All(e => e.EsHecha));
        Assert.AreEqual("Jugar de nuevo", juego.TextoBotonPrincipal);
    });

    [TestMethod(DisplayName = "Cada paso coincide con el motor: la vista nunca decide reglas")]
    public void MismoEstadoQueElMotor() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM();
        var estado = Motor.CrearEstadoInicial();
        var contexto = new ContextoPartida(ClaseX: true); // el respaldo histórico es una X8.7
        foreach (var paso in Partida.HastaAbortada)
        {
            Partida.Paso(juego, paso);
            estado = (paso, estado.Fase) switch
            {
                ("P", Fase.Inicio) => Motor.Empezar(estado, contexto).Estado,
                ("P", Fase.Resultado) => Motor.Continuar(estado, contexto).Estado,
                ("P", Fase.Respaldo) => Motor.Reintentar(estado).Estado,
                _ => Motor.AplicarDecision(estado, paso).Estado,
            };
            AssertMismoEstado(estado, juego.Estado, $"Paso {paso}");
        }
        Assert.AreEqual(Fase.MisionAbortada, juego.Fase);
    });

    /// <summary>Igualdad campo a campo (la lista de medidores fallidos se compara por contenido).</summary>
    private static void AssertMismoEstado(Estado esperado, Estado real, string mensaje)
    {
        IReadOnlyList<Medidor> ninguno = Array.Empty<Medidor>();
        Assert.AreEqual(esperado with { Fallidos = ninguno }, real with { Fallidos = ninguno }, mensaje);
        CollectionAssert.AreEqual(esperado.Fallidos.ToArray(), real.Fallidos.ToArray(), mensaje);
    }

    [TestMethod(DisplayName = "Durante la transición (450 ms) se ignoran las acciones: un doble clic no decide dos veces")]
    public void BloqueoDuranteLaTransicion() => HiloUI.Ejecutar(() =>
    {
        var juego = Partida.Jugar(Partida.HastaNivel1);
        juego.ElegirOpcion("A");
        Assert.IsTrue(juego.Bloqueado);
        Assert.IsFalse(juego.Elegir.CanExecute("A"));
        Assert.IsFalse(juego.Principal.CanExecute(null));
        var tras = juego.Estado;
        juego.ElegirOpcion("A");
        juego.AccionPrincipal();
        Assert.AreSame(tras, juego.Estado, "Una segunda acción durante la transición no hace nada");
        juego.TerminarTransicion();
        Assert.IsFalse(juego.Bloqueado);
        Assert.IsTrue(juego.Principal.CanExecute(null));
        Assert.AreEqual(new Medidores(80, 45, 100), juego.Estado.Medidores);
    });

    [TestMethod(DisplayName = "Al terminar la transición se pide el foco con la fase nueva")]
    public void FocoAlTerminar() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM();
        var fases = new List<Fase>();
        juego.Enfocar += (_, fase) => fases.Add(fase);
        Partida.Paso(juego, "P");
        Partida.Paso(juego, "A");
        CollectionAssert.AreEqual(new[] { Fase.Nivel, Fase.Resultado }, fases);
    });

    [TestMethod(DisplayName = "Anuncios: resultado + cambios + dato (cortés); respaldo (urgente); aviso de clase X")]
    public void Anuncios() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM();
        var anuncios = new List<Anuncio>();
        juego.Anunciar += (_, a) => anuncios.Add(a);
        foreach (var paso in Partida.HastaRespaldo4) Partida.Paso(juego, paso);

        var resultado1 = anuncios.First(a => a.Texto.StartsWith("Ahorraste energía", StringComparison.Ordinal));
        Assert.IsFalse(resultado1.Urgente);
        Assert.AreEqual("Ahorraste energía para los calentadores, pero queda menos agua limpia. Batería sube a 100 %. Soporte Vital baja a 45 %. Dato real: "
            + Guion.Niveles[0].Dato.Texto, resultado1.Texto);
        Assert.IsTrue(anuncios.Any(a => !a.Urgente && a.Texto == Presentacion.TextoAvisoX()), "Al entrar al Nivel 4 se anuncia la clase X");
        var urgente = anuncios.Single(a => a.Urgente);
        Assert.AreEqual(Textos.Respaldo.Soporte, urgente.Texto);
        Assert.AreEqual(anuncios[^1], urgente, "Fallar no da dato: el último anuncio es el del respaldo");
    });

    [TestMethod(DisplayName = "Pantallas: nivel con avisos, respaldo con mensaje y pista, resultado con dato y fuentes")]
    public void ContenidoDeLaTarjeta() => HiloUI.Ejecutar(() =>
    {
        var nivel4 = Partida.Jugar(Partida.HastaNivel4);
        Assert.IsTrue(nivel4.Alerta);
        Assert.AreEqual("📡 Dato HISTÓRICO REAL: NASA DONKI", nivel4.Etiqueta);
        Assert.AreEqual("Nivel 4: Tormenta Solar", nivel4.NombreTituloNivel);
        Assert.AreEqual(Presentacion.TextoAvisoX(), nivel4.AvisoX);
        Assert.IsNull(nivel4.AvisoAyuda);
        Assert.AreEqual("Opción A: Refugiarse rodeados de provisiones. Gana 35 de Protección y cuesta 30 de Soporte Vital.", nivel4.OpcionA.NombreAccesible);

        var respaldo = Partida.Jugar(Partida.HastaRespaldo4);
        Assert.AreEqual(Textos.Respaldo.Soporte, respaldo.MensajeRespaldo);
        Assert.AreEqual(Textos.Respaldo.Pista, respaldo.PistaRespaldo);
        Assert.AreEqual("Protocolos de Respaldo: quedan 2 de 3", respaldo.TextoRespaldos);
        CollectionAssert.AreEqual(new[] { true, true, false }, respaldo.Respaldos.ToArray());
        Assert.AreEqual("Reintentar", respaldo.TextoBotonPrincipal);

        var resultado = Partida.Jugar(Partida.HastaResultado1);
        Assert.AreEqual(Guion.Niveles[0].Opciones["A"].Resultado, resultado.TextoResultado);
        Assert.IsNotNull(resultado.Dato);
        Assert.HasCount(2, resultado.Dato.Fuentes);
        Assert.AreEqual("NASA, Moon Facts (se abre en el navegador)", resultado.Dato.Fuentes[0].NombreAccesible);
        Assert.IsTrue(resultado.Estaciones[0].EsHecha);
    });

    [TestMethod(DisplayName = "Números flotantes: ganancia, costo y «¡Al máximo!»")]
    public void NumerosFlotantes() => HiloUI.Ejecutar(() =>
    {
        var juego = Partida.Jugar("P", "A", "P", "A");
        var proteccion = juego.Medidores.Single(m => m.Clave == Medidor.Proteccion);
        var soporte = juego.Medidores.Single(m => m.Clave == Medidor.Soporte);
        Assert.AreEqual(Textos.AlMaximo, proteccion.Flotante);
        Assert.IsTrue(proteccion.FlotanteGana);
        Assert.AreEqual("−35 💧", soporte.Flotante);
        Assert.IsFalse(soporte.FlotanteGana);
        Assert.IsTrue(soporte.Bajo);
        Assert.AreEqual("10 %, nivel bajo", soporte.TextoAccesible);
    });

    [TestMethod(DisplayName = "Jugar de nuevo vuelve exactamente al estado inicial")]
    public void JugarDeNuevo() => HiloUI.Ejecutar(() =>
    {
        var juego = Partida.Jugar(Partida.HastaAbortada);
        Partida.Paso(juego, "P");
        AssertMismoEstado(Motor.CrearEstadoInicial(), juego.Estado, "Estado inicial");
        Assert.IsTrue(juego.EsInicio);
        Assert.AreEqual(0, Presentacion.CasillaDe(juego.Estado), "La ficha vuelve a la base del mapa");
    });
}
