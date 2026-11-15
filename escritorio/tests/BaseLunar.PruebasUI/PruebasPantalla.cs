using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BaseLunar.App;
using BaseLunar.App.VistaModelos;
using BaseLunar.App.Vistas;

namespace BaseLunar.PruebasUI;

/// <summary>
/// La pantalla montada de verdad (en el hilo de interfaz): capturas a varios tamaños con comprobación del
/// diseño, accesibilidad (nombres, valores, encabezados, regiones en vivo), foco y teclado.
/// </summary>
[TestClass]
public sealed class PruebasPantalla
{
    private static readonly (string Nombre, string[] Pasos)[] Fases =
    [
        ("1-inicio", []),
        ("2-nivel1", Partida.HastaNivel1),
        ("3-resultado1", Partida.HastaResultado1),
        ("4-nivel4-alerta", Partida.HastaNivel4),
        ("5-respaldo4", Partida.HastaRespaldo4),
        ("6-nivel5", Partida.HastaNivel5),
        ("7-victoria", Partida.HastaVictoria),
        ("8-abortada", Partida.HastaAbortada),
    ];

    [TestMethod]
    [DataRow(800, 600, DisplayName = "Capturas y diseño a 800×600")]
    [DataRow(1280, 720, DisplayName = "Capturas y diseño a 1280×720")]
    [DataRow(1920, 1080, DisplayName = "Capturas y diseño a 1920×1080")]
    [DataRow(784, 561, DisplayName = "Capturas y diseño en la ventana mínima (784×561 de área útil)")]
    // Zoom de Windows (F5): una pantalla de 1920×1080 al 150 % deja 1280×688 DIP útiles (maximizada, 1280×657 de
    // área útil); al 200 %, 960×516 (maximizada, 960×485). La captura se pinta con los píxeles reales.
    [DataRow(1280, 657, 144, DisplayName = "Zoom de Windows al 150 % en 1920×1080: ventana maximizada (1280×657 DIP, a 144 ppp)")]
    [DataRow(960, 485, 192, DisplayName = "Zoom de Windows al 200 % en 1920×1080: ventana maximizada (960×485 DIP, a 192 ppp)")]
    [DataRow(1093, 545, 120, DisplayName = "Zoom de Windows al 125 % en 1366×768: ventana maximizada (1093×545 DIP, a 120 ppp)")]
    public void CapturasYDiseno(int ancho, int alto, int ppp = 96) => HiloUI.Ejecutar(() =>
    {
        var zoom = ppp == 96 ? string.Empty : $"-zoom{ppp * 100 / 96}";
        foreach (var (nombre, pasos) in Fases)
        {
            var juego = Partida.Jugar(pasos);
            var pantalla = Capturas.Montar(juego, ancho, alto);
            pantalla.Enfocar(juego.Fase); // como al terminar la transición: la tarjeta muestra el resultado o el respaldo
            pantalla.UpdateLayout();
            Capturas.Guardar(pantalla, $"{ancho}x{alto}{zoom}-{nombre}", ppp);
            ComprobarDiseno(pantalla, juego, $"{nombre} a {ancho}×{alto}");
        }
    });

    [TestMethod(DisplayName = "Disposición: ancha desde 1000 DIP (escena a la izquierda), compacta por debajo")]
    public void Disposicion() => HiloUI.Ejecutar(() =>
    {
        var ancha = Capturas.Montar(Partida.Jugar(Partida.HastaNivel1), 1280, 720);
        Assert.IsTrue(ancha.EsAncha);
        Assert.AreEqual(1, Grid.GetColumn(ancha.Tarjeta));
        Assert.IsLessThanOrEqualTo(Caja(ancha.Tarjeta, ancha).Left, Caja(ancha.Escena, ancha).Right, "La escena queda a la izquierda de la tarjeta");
        var compacta = Capturas.Montar(Partida.Jugar(Partida.HastaNivel1), 800, 600);
        Assert.IsFalse(compacta.EsAncha);
        Assert.IsLessThanOrEqualTo(Caja(compacta.Tarjeta, compacta).Top, Caja(compacta.Escena, compacta).Bottom, "La escena queda encima de la tarjeta");
    });

    [TestMethod(DisplayName = "Accesibilidad: botones con nombre completo y atajo, medidores con nombre y valor, título como encabezado")]
    public void Accesibilidad() => HiloUI.Ejecutar(() =>
    {
        var juego = Partida.Jugar("P", "A", "P", "A", "P");
        var pantalla = Capturas.Montar(juego, 1280, 720);

        var botonA = new ButtonAutomationPeer(pantalla.Decisiones.BotonA);
        Assert.AreEqual("Opción A: Rodear los dormitorios con agua. Gana 30 de Protección y cuesta 30 de Batería.", botonA.GetName());
        Assert.AreEqual("OpcionA", botonA.GetAutomationId());
        Assert.AreEqual("A, 1", botonA.GetAcceleratorKey());

        var barras = Descendientes<ProgressBar>(pantalla).ToList();
        Assert.HasCount(3, barras);
        var nombres = barras.Select(b => UIElementAutomationPeer.CreatePeerForElement(b).GetName()).ToArray();
        CollectionAssert.AreEqual(new[] { "Protección", "Soporte Vital", "Batería" }, nombres);
        var soporte = UIElementAutomationPeer.CreatePeerForElement(barras[1]);
        Assert.AreEqual(10.0, ((IRangeValueProvider)soporte.GetPattern(PatternInterface.RangeValue)).Value);
        Assert.AreEqual("10 %, nivel bajo", soporte.GetHelpText());

        Assert.AreEqual("Nivel 3: Rayos Invisibles", AutomationProperties.GetName(pantalla.Tarjeta.TituloNivel));
        Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel(pantalla.Tarjeta.TituloNivel));
        Assert.AreEqual(AutomationLiveSetting.Polite, AutomationProperties.GetLiveSetting(pantalla.AnuncioCortes));
        Assert.AreEqual(AutomationLiveSetting.Assertive, AutomationProperties.GetLiveSetting(pantalla.AnuncioUrgente));
        Assert.AreEqual("Tarjeta de misión", AutomationProperties.GetName(pantalla.Tarjeta.Desplazamiento));
        Assert.IsTrue(pantalla.Tarjeta.Desplazamiento.Focusable);
        Assert.AreEqual("Dato NASA: OSDR / RadLab", AutomationProperties.GetName(Descendientes<TextBlock>(pantalla).First(t => AutomationProperties.GetAutomationId(t) == "EtiquetaNivel")));
    });

    [TestMethod(DisplayName = "Teclado y foco en una ventana real: Enter, 1/2, A/B, bloqueo y Enter sobre un botón (una sola acción)")]
    public void TecladoYFoco() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM();
        var ventana = new VentanaPrincipal(juego)
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -32000,
            Top = -32000,
            ShowActivated = false,
            ShowInTaskbar = false,
        };
        ventana.Show();
        try
        {
            HiloUI.Vaciar();
            var pantalla = ventana.Pantalla;
            Assert.AreSame(pantalla.Tarjeta.TituloInicio, FocusManager.GetFocusedElement(ventana), "Al abrir, el foco está en el título");

            Pulsar(pantalla.Tarjeta.TituloInicio, Key.Enter);
            Assert.AreEqual(Fase.Nivel, juego.Fase, "Enter fuera de un botón comienza la misión");
            juego.TerminarTransicion();
            Assert.AreSame(pantalla.Tarjeta.TituloNivel, FocusManager.GetFocusedElement(ventana), "Al entrar al nivel, el foco va al título");

            Pulsar(pantalla.Tarjeta.TituloNivel, Key.D1);
            Assert.AreEqual("A", juego.Estado.UltimaOpcion, "1 elige la opción A");
            Pulsar(pantalla.Tarjeta.TituloNivel, Key.Enter);
            Assert.AreEqual(Fase.Resultado, juego.Fase, "Durante la transición, Enter no hace nada");
            juego.TerminarTransicion();
            Assert.AreSame(pantalla.Decisiones.BotonPrincipal, FocusManager.GetFocusedElement(ventana), "Tras decidir, el foco va al botón principal");
            Assert.IsTrue(HiloUI.Esperar(() => pantalla.AnuncioCortes.Text.StartsWith("Ahorraste energía", StringComparison.Ordinal)), "Se anuncia el resultado");

            Pulsar(pantalla.Decisiones.BotonPrincipal, Key.Enter);
            Assert.AreEqual(2, juego.Estado.Nivel, "Enter sobre el botón lo pulsa una sola vez: Nivel 2");
            Assert.AreEqual(Fase.Nivel, juego.Fase);
            juego.TerminarTransicion();

            Pulsar(pantalla.Tarjeta.TituloNivel, Key.B);
            Assert.AreEqual("B", juego.Estado.UltimaOpcion, "B elige la opción B");
            juego.TerminarTransicion();
            Pulsar(pantalla.Tarjeta.TituloNivel, Key.A);
            Assert.AreEqual(Fase.Resultado, juego.Fase, "En el resultado, A no hace nada");
            Pulsar(pantalla.Tarjeta.TituloNivel, Key.NumPad2);
            Assert.AreEqual(Fase.Resultado, juego.Fase);
        }
        finally
        {
            ventana.Close();
        }
    });

    [TestMethod(DisplayName = "F11: pantalla completa sin bordes y vuelta a la ventana como estaba")]
    public void PantallaCompleta() => HiloUI.Ejecutar(() =>
    {
        var ventana = new VentanaPrincipal(new JuegoVM());
        ventana.AlternarPantallaCompleta();
        Assert.IsTrue(ventana.EnPantallaCompleta);
        Assert.AreEqual(WindowStyle.None, ventana.WindowStyle);
        Assert.AreEqual(WindowState.Maximized, ventana.WindowState);
        Assert.AreEqual(ResizeMode.NoResize, ventana.ResizeMode);
        ventana.AlternarPantallaCompleta();
        Assert.IsFalse(ventana.EnPantallaCompleta);
        Assert.AreEqual(WindowStyle.SingleBorderWindow, ventana.WindowStyle);
        Assert.AreEqual(WindowState.Normal, ventana.WindowState);
        Assert.AreEqual(ResizeMode.CanResize, ventana.ResizeMode);
        ventana.Close();
    });

    // ---------- Ayudantes ----------

    /// <summary>Simula una tecla pulsada con el foco en <paramref name="destino"/>: el evento sube hasta la ventana.</summary>
    private static void Pulsar(UIElement destino, Key tecla)
    {
        var fuente = PresentationSource.FromVisual(destino) ?? throw new InvalidOperationException("El elemento no está en una ventana");
        destino.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, fuente, Environment.TickCount, tecla) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
        destino.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, fuente, Environment.TickCount, tecla) { RoutedEvent = Keyboard.KeyDownEvent });
    }

    private static void ComprobarDiseno(PantallaJuego pantalla, JuegoVM juego, string caso)
    {
        var limites = new Rect(-0.5, -0.5, pantalla.ActualWidth + 1, pantalla.ActualHeight + 1);
        Button[] botones = juego.EnNivel ? [pantalla.Decisiones.BotonA, pantalla.Decisiones.BotonB] : [pantalla.Decisiones.BotonPrincipal];
        foreach (var boton in botones)
        {
            Assert.IsTrue(Visible(boton, pantalla), $"{caso}: {boton.Name} no se ve");
            var caja = Caja(boton, pantalla);
            Assert.IsTrue(limites.Contains(caja), $"{caso}: {boton.Name} se sale de la pantalla ({caja})");
            Assert.IsGreaterThanOrEqualTo(48.0, boton.ActualHeight, $"{caso}: {boton.Name} mide menos de 48 DIP de alto");
            Assert.IsGreaterThanOrEqualTo(120.0, boton.ActualWidth, $"{caso}: {boton.Name} es demasiado estrecho");
        }
        // La escena solo se oculta en la disposición compacta con poca altura (D-59).
        Assert.AreEqual(!pantalla.EsAncha && pantalla.ActualHeight < PantallaJuego.AltoMinimoConEscena, pantalla.SinEscena, $"{caso}: escena oculta sin motivo");
        FrameworkElement[] paneles = pantalla.SinEscena ? [pantalla.Estado, pantalla.Medidores, pantalla.Tarjeta] : [pantalla.Estado, pantalla.Medidores, pantalla.Escena, pantalla.Tarjeta];
        foreach (var panel in paneles)
        {
            Assert.IsTrue(Visible(panel, pantalla), $"{caso}: {panel.Name} no se ve");
            Assert.IsTrue(limites.Contains(Caja(panel, pantalla)), $"{caso}: {panel.Name} se sale de la pantalla");
        }
        if (!pantalla.SinEscena) Assert.IsGreaterThanOrEqualTo(100.0, pantalla.Escena.ActualHeight, $"{caso}: la escena queda aplastada");
        Assert.IsGreaterThanOrEqualTo(140.0, pantalla.Tarjeta.ActualHeight, $"{caso}: la tarjeta queda aplastada");
    }

    private static Rect Caja(FrameworkElement elemento, Visual raiz) =>
        elemento.TransformToAncestor(raiz).TransformBounds(new Rect(elemento.RenderSize));

    /// <summary>Visible si él y todos sus antecesores lo son (IsVisible exige una ventana).</summary>
    private static bool Visible(DependencyObject elemento, DependencyObject raiz)
    {
        for (var actual = elemento; actual is not null && actual != raiz; actual = VisualTreeHelper.GetParent(actual))
        {
            if (actual is UIElement { Visibility: not Visibility.Visible }) return false;
        }
        return true;
    }

    private static IEnumerable<T> Descendientes<T>(DependencyObject raiz) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(raiz); i++)
        {
            var hijo = VisualTreeHelper.GetChild(raiz, i);
            if (hijo is T encontrado) yield return encontrado;
            foreach (var nieto in Descendientes<T>(hijo)) yield return nieto;
        }
    }
}
