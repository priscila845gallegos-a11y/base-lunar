using System.Windows.Input;
using BaseLunar.App.Controles;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.PruebasUI;

/// <summary>
/// Las funciones de pantalla de ui.js trasladadas a Presentacion: mismos textos, mismo orden, mismos signos.
/// Son funciones puras, así que no necesitan el hilo de interfaz.
/// </summary>
[TestClass]
public sealed class PruebasPresentacion
{
    private static Nivel N(int id) => Guion.Niveles[id - 1];

    [TestMethod(DisplayName = "Porcentaje con espacio de no separación y signo menos tipográfico, como en ui.js")]
    public void PorcentajeYSigno()
    {
        Assert.AreEqual("80\u00A0%", Presentacion.Porcentaje(80));
        Assert.AreEqual("+20", Presentacion.ConSigno(20));
        Assert.AreEqual("\u221235", Presentacion.ConSigno(-35));
    }

    [TestMethod(DisplayName = "Efectos del botón ordenados de mayor a menor, con el ícono de cada medidor")]
    public void LineasDeEfecto()
    {
        CollectionAssert.AreEqual(new[] { "+20 ⚡", "−35 💧" }, Presentacion.LineasEfecto(N(1).Opciones["A"]).ToArray());
        CollectionAssert.AreEqual(new[] { "+15 💧", "−40 ⚡" }, Presentacion.LineasEfecto(N(1).Opciones["B"]).ToArray());
        CollectionAssert.AreEqual(new[] { "Necesita más de 35\u00A0% ⚡" }, Presentacion.LineasEfecto(N(5).Opciones["A"]).ToArray());
    }

    [TestMethod(DisplayName = "Nombre accesible de los botones: «Opción A: …. Gana … y cuesta ….»")]
    public void NombreAccesibleDeLasOpciones()
    {
        Assert.AreEqual("Opción A: Apagar recicladores de agua. Gana 20 de Batería y cuesta 35 de Soporte Vital.",
            Presentacion.NombreOpcion("A", N(1).Opciones["A"]));
        Assert.AreEqual("Opción B: Guiado automático por computadora. Necesita más de 40\u00A0% de Protección.",
            Presentacion.NombreOpcion("B", N(5).Opciones["B"]));
    }

    [TestMethod(DisplayName = "Progreso, respaldos y botón principal según la fase")]
    public void TextosDeFase()
    {
        var inicio = Motor.CrearEstadoInicial();
        Assert.AreEqual("Antes de empezar", Presentacion.TextoProgreso(inicio));
        Assert.AreEqual("Nivel 3 de 5", Presentacion.TextoProgreso(inicio with { Fase = Fase.Nivel, Nivel = 3 }));
        Assert.AreEqual("¡Misión cumplida!", Presentacion.TextoProgreso(inicio with { Fase = Fase.Victoria, Nivel = 5 }));
        Assert.AreEqual("Protocolos de Respaldo: quedan 2 de 3", Presentacion.TextoRespaldos(2));
        Assert.AreEqual("Comenzar misión", Presentacion.TextoBoton(Fase.Inicio));
        Assert.AreEqual("Reintentar", Presentacion.TextoBoton(Fase.Respaldo));
        Assert.AreEqual("Jugar de nuevo", Presentacion.TextoBoton(Fase.MisionAbortada));
    }

    [TestMethod(DisplayName = "Estaciones del mapa: superadas, en curso y por jugar (renderProgreso de ui.js)")]
    public void EstacionesDelMapa()
    {
        var resultado3 = Motor.CrearEstadoInicial() with { Fase = Fase.Resultado, Nivel = 3 };
        Assert.IsTrue(Presentacion.EstacionHecha(resultado3, 3));
        Assert.IsFalse(Presentacion.EstacionActual(resultado3, 3));
        var nivel3 = resultado3 with { Fase = Fase.Nivel };
        Assert.IsTrue(Presentacion.EstacionHecha(nivel3, 2));
        Assert.IsTrue(Presentacion.EstacionActual(nivel3, 3));
        Assert.IsFalse(Presentacion.EstacionHecha(nivel3, 4));
        Assert.IsTrue(Presentacion.EstacionHecha(nivel3 with { Fase = Fase.Victoria }, 5));
    }

    [TestMethod(DisplayName = "Nivel 4 con el respaldo histórico: etiqueta HISTÓRICO, llamarada X8.7 y fuente de DONKI")]
    public void DatoDeLaTormentaHistorica()
    {
        var tormenta = DatosDonki.RespaldoHistorico();
        Assert.AreEqual("📡 Dato HISTÓRICO REAL: NASA DONKI", Presentacion.EtiquetaNivel(N(4), tormenta));
        Assert.AreEqual("📡 Dato EN VIVO: NASA DONKI", Presentacion.EtiquetaNivel(N(4), tormenta with { EnVivo = true }));
        CollectionAssert.AreEqual(
            new[] { "Llamarada real: clase X8.7 del 14 de mayo de 2024 (NASA DONKI).", "¡En solo 2 días, el Sol lanzó 5 llamaradas de clase X!" },
            Presentacion.LineasExtra(N(4), tormenta).ToArray());
        var fuentes = Presentacion.FuentesDe(N(4), tormenta);
        Assert.HasCount(2, fuentes);
        Assert.AreEqual("NASA DONKI, registro de la llamarada", fuentes[1].Nombre);
        Assert.AreEqual(tormenta.Url, fuentes[1].Url);
        Assert.AreEqual("Fuentes:", Presentacion.EtiquetaFuentes(fuentes.Count));
    }

    [TestMethod(DisplayName = "Mensajes de respaldo, ayuda y aviso de clase X")]
    public void Avisos()
    {
        var respaldo = Motor.CrearEstadoInicial() with { Fase = Fase.Respaldo, Nivel = 4, Fallidos = [Medidor.Soporte], UltimaOpcion = "A" };
        Assert.AreEqual(Textos.Respaldo.Soporte, Presentacion.TextosRespaldo(respaldo, N(4)).Mensaje);
        var condicion = respaldo with { Nivel = 5, Fallidos = [], UltimaOpcion = "B" };
        Assert.AreEqual("¡Protocolo de Respaldo activado! Los sensores del guiado necesitaban más protección. ¡Reintentemos este nivel!",
            Presentacion.TextosRespaldo(condicion, N(5)).Mensaje);
        Assert.AreEqual(Textos.Respaldo.PistaCondicion, Presentacion.TextosRespaldo(condicion, N(5)).Pista);
        Assert.AreEqual("¡Ayuda de Control de Misión! Te enviamos energía y agua y oxígeno extra para que puedas seguir.",
            Presentacion.TextoAyuda(Guion.Efectos((Medidor.Bateria, 5), (Medidor.Soporte, 3))));
        Assert.AreEqual("Esta llamarada es de clase X, la más fuerte. La protección bajó 5 puntos.", Presentacion.TextoAvisoX());
    }

    [TestMethod(DisplayName = "Anuncio del resultado: resultado, cambios de cada medidor (con «al máximo») y dato")]
    public void MensajesDelResultado()
    {
        var estado = Motor.CrearEstadoInicial() with { Fase = Fase.Resultado, Nivel = 2, Medidores = new Medidores(100, 10, 100), UltimaOpcion = "A" };
        Evento[] eventos = [new EfectosAplicados(N(2).Opciones["A"].Efectos!), new MedidorAlMaximo(Medidor.Proteccion), new NivelSuperado(2)];
        CollectionAssert.AreEqual(
            new[] { "¡Vitaminas frescas! La tripulación está más fuerte.", "Protección está al máximo.", "Soporte Vital baja a 10\u00A0%.", "Dato real: " + N(2).Dato.Texto },
            Presentacion.MensajesResultado(estado, N(2), eventos).ToArray());
    }

    [TestMethod(DisplayName = "Los emojis se cambian por íconos y el lector de pantalla recibe el texto sin ellos")]
    public void EmojisComoIconos()
    {
        var trozos = Iconografia.Partir("+20 ⚡");
        Assert.HasCount(2, trozos);
        Assert.AreEqual("Icono.Bateria", trozos[1].Icono);
        Assert.AreEqual("Cuida los 3 medidores: Protección, Soporte Vital y Batería.",
            Iconografia.SinIconos("Cuida los 3 medidores: 🛡️ Protección, 💧 Soporte Vital y ⚡ Batería."));
        Assert.AreEqual("Dato NASA: OSDR / RadLab", Iconografia.SinIconos("📡 Dato NASA: OSDR / RadLab"));
        foreach (var nivel in Guion.Niveles) Assert.IsNotNull(Iconografia.ClaveDe(nivel.Avatar), nivel.Avatar);
        foreach (var info in Guion.InfoMedidores) Assert.IsNotNull(Iconografia.ClaveDe(info.Icono), info.Icono);
    }

    [TestMethod(DisplayName = "Solo se abren enlaces https de la NASA y la CSA: todas las fuentes del guion pasan")]
    public void EnlacesSeguros()
    {
        var urls = Guion.Niveles.SelectMany(n => n.Dato.Fuentes).Select(f => f.Url).Append(DatosDonki.RespaldoHistorico().Url);
        foreach (var url in urls) Assert.IsTrue(Enlaces.EsSeguro(new Uri(url)), url);
        Assert.IsFalse(Enlaces.EsSeguro(new Uri("http://www.nasa.gov/")));
        Assert.IsFalse(Enlaces.EsSeguro(new Uri("https://nasa.gov.example.com/")));
        Assert.IsFalse(Enlaces.EsSeguro(new Uri("https://falsanasa.gov/")));
        Assert.IsFalse(Enlaces.EsSeguro(new Uri("file:///C:/Windows/System32/calc.exe")));
        Assert.IsFalse(Enlaces.EsSeguro(null));
    }

    [TestMethod(DisplayName = "Atajos (5.5): A/1 y B/2 eligen, Enter continúa, F11 y Esc; con Ctrl, Alt o repetición no hacen nada")]
    public void AtajosDeTeclado()
    {
        Atajo Tecla(Key tecla, ModifierKeys mod = ModifierKeys.None, bool repetida = false, bool texto = false) => Atajos.Interpretar(tecla, mod, repetida, texto);
        Assert.AreEqual(Atajo.OpcionA, Tecla(Key.A));
        Assert.AreEqual(Atajo.OpcionA, Tecla(Key.D1));
        Assert.AreEqual(Atajo.OpcionA, Tecla(Key.NumPad1));
        Assert.AreEqual(Atajo.OpcionB, Tecla(Key.B, ModifierKeys.Shift));
        Assert.AreEqual(Atajo.OpcionB, Tecla(Key.D2));
        Assert.AreEqual(Atajo.Principal, Tecla(Key.Enter));
        Assert.AreEqual(Atajo.PantallaCompleta, Tecla(Key.F11));
        Assert.AreEqual(Atajo.SalirPantallaCompleta, Tecla(Key.Escape));
        Assert.AreEqual(Atajo.Ninguno, Tecla(Key.A, ModifierKeys.Control));
        Assert.AreEqual(Atajo.Ninguno, Tecla(Key.A, ModifierKeys.Alt));
        Assert.AreEqual(Atajo.Ninguno, Tecla(Key.Enter, repetida: true));
        Assert.AreEqual(Atajo.Ninguno, Tecla(Key.A, texto: true));
        Assert.AreEqual(Atajo.Ninguno, Tecla(Key.C));
    }
}
