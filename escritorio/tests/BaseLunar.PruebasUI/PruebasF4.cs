using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BaseLunar.App;
using BaseLunar.App.Temas;
using BaseLunar.App.VistaModelos;
using BaseLunar.App.Vistas;

namespace BaseLunar.PruebasUI;

/// <summary>
/// F4 · Pulido y finales: modo demo (D-08, D-56), certificado con nombre e impresión (3.7, D-17),
/// «Control de Misión» (D-57), contraste AA y alto contraste, y P4: todo se juega solo con el teclado.
/// </summary>
[TestClass]
public sealed class PruebasF4
{
    // ---------- Opciones y modo demo ----------

    [TestMethod(DisplayName = "Opciones: --nivel N (1 a 5; otro valor se ignora), --offline, --debug, --silencio y --alto-contraste")]
    public void Opciones_()
    {
        Assert.AreEqual(4, Opciones.Leer(["--nivel", "4"]).Nivel);
        Assert.AreEqual(3, Opciones.Leer(["--NIVEL=3"]).Nivel);
        Assert.IsNull(Opciones.Leer(["--nivel", "9"]).Nivel);
        Assert.IsNull(Opciones.Leer(["--nivel", "cuatro"]).Nivel);
        Assert.IsNull(Opciones.Leer(["--nivel=0"]).Nivel);
        Assert.AreEqual(new Opciones(2, true, true, true, true), Opciones.Leer(["--offline", "--debug", "--nivel", "2", "--silencio", "--alto-contraste"]));
        Assert.AreEqual(new Opciones(), Opciones.Leer(["--otra-cosa"]));
    }

    [TestMethod(DisplayName = "--nivel 4: cada partida empieza en el Nivel 4 con 80/80/80, 3 respaldos y la penalización X (D-08)")]
    public void NivelInicial() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM { NivelInicial = 4 };
        Partida.Paso(juego, "P");
        Assert.AreEqual(4, juego.Estado.Nivel);
        Assert.AreEqual(new Medidores(75, 80, 80), juego.Estado.Medidores, "La X8.7 del respaldo resta 5 de protección");
        Assert.AreEqual(3, juego.Estado.Respaldos);
        Assert.AreEqual(new Momento(TipoMomento.EntrarNivel, 4, ClaseX: true, Desde: 0), juego.UltimoMomento, "La ficha camina desde la base");
        foreach (var paso in new[] { "B", "P", "A", "P" }) Partida.Paso(juego, paso);
        Assert.AreEqual(Fase.Victoria, juego.Fase);
        Partida.Paso(juego, "P");
        Partida.Paso(juego, "P");
        Assert.AreEqual(4, juego.Estado.Nivel, "Al jugar de nuevo, otra vez desde el Nivel 4");
    });

    [TestMethod(DisplayName = "--debug: estado interno y registro técnico (6 líneas); sin --debug, nada técnico se guarda ni se ve")]
    public void Depuracion() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM { ModoDepuracion = true };
        Partida.Paso(juego, "P");
        StringAssert.StartsWith(juego.TextoDepuracion, "Fase: Nivel · Nivel: 1 · Respaldos: 3");
        StringAssert.Contains(juego.TextoDepuracion, "DONKI: X8.7 (histórico)");
        for (var i = 1; i <= 8; i++) juego.Registrar("mensaje " + i);
        Assert.HasCount(6, juego.RegistroDepuracion);
        StringAssert.EndsWith(juego.RegistroDepuracion[^1], "mensaje 8");
        var pantalla = Capturas.Montar(juego, 1280, 720);
        Capturas.Guardar(pantalla, "f4-depuracion");

        var normal = new JuegoVM();
        normal.Registrar("no se guarda");
        Assert.IsEmpty(normal.RegistroDepuracion);
        var sinDepuracion = Capturas.Montar(normal, 1280, 720);
        Assert.IsFalse(Descendientes<TextBlock>(sinDepuracion).Any(t => AutomationProperties(t) == "Depuracion" && t.IsVisibleEnArbol(sinDepuracion)));
    });

    // ---------- Certificado ----------

    [TestMethod(DisplayName = "Nombre del certificado: sin caracteres de control, 24 como máximo, nunca se lee en voz alta y se borra al jugar de nuevo")]
    public void NombreDelCertificado() => HiloUI.Ejecutar(() =>
    {
        Assert.AreEqual("AnaMaría", Presentacion.LimpiarNombre("Ana\tMaría\n"));
        Assert.AreEqual(24, Presentacion.LimpiarNombre(new string('x', 40)).Length);
        Assert.AreEqual(23, Presentacion.LimpiarNombre(new string('x', 23) + "🚀").Length, "No parte un emoji por la mitad");

        var juego = Partida.Jugar(Partida.HastaVictoria);
        var anuncios = new List<Anuncio>();
        juego.Anunciar += (_, a) => anuncios.Add(a);
        juego.NombreCertificado = "  Valentina  ";
        Assert.AreEqual("Comandante: Valentina", juego.LineaNombre);
        Assert.DoesNotContain("Valentina", juego.TextoParaEscuchar(), "«Escuchar» nunca lee el nombre (D-13)");
        var pantalla = Capturas.Montar(juego, 1280, 720);
        Capturas.Guardar(pantalla, "f4-certificado-pantalla");
        var hoja = new CertificadoImpresion { DataContext = juego };
        hoja.Measure(CertificadoImpresion.Hoja);
        hoja.Arrange(new Rect(CertificadoImpresion.Hoja));
        hoja.UpdateLayout();
        Capturas.Guardar(hoja, "f4-certificado-impresion");
        var contenido = (FrameworkElement)VisualTreeHelper.GetChild(hoja, 0);
        Assert.IsLessThanOrEqualTo(CertificadoImpresion.Hoja.Height, contenido.DesiredSize.Height, "Todo cabe en una hoja A4");
        Partida.Paso(juego, "P");
        Assert.AreEqual(string.Empty, juego.NombreCertificado, "«Jugar de nuevo» borra el nombre");
        Assert.IsTrue(anuncios.All(a => !a.Texto.Contains("Valentina", StringComparison.Ordinal)), "El nombre nunca se anuncia");
    });

    // ---------- Control de Misión ----------

    [TestMethod(DisplayName = "Control de Misión: un error muestra un aviso amable sin detalles técnicos; «Seguir» vuelve al mismo estado, desbloqueado")]
    public void ControlDeMision() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM { ModoDepuracion = true };
        Partida.Paso(juego, "P");
        juego.ElegirOpcion("A"); // en plena transición
        var antes = juego.Estado;
        var anuncios = new List<Anuncio>();
        var fases = new List<Fase>();
        juego.Anunciar += (_, a) => anuncios.Add(a);
        juego.Enfocar += (_, f) => fases.Add(f);
        BaseLunar.App.App.ManejarError(new InvalidOperationException("detalle técnico"), juego);
        Assert.IsTrue(juego.ControlMisionVisible);
        Assert.AreEqual(TextosApp.ControlMision.Mensaje, anuncios.Single().Texto);
        Assert.IsTrue(anuncios.Single().Urgente);
        Assert.IsTrue(juego.RegistroDepuracion.Any(l => l.Contains("detalle técnico", StringComparison.Ordinal)), "El detalle solo va al registro (--debug)");
        var pantalla = Capturas.Montar(juego, 1280, 720);
        Capturas.Guardar(pantalla, "f4-control-de-mision");
        juego.SeguirMision.Execute(null);
        Assert.IsFalse(juego.ControlMisionVisible);
        Assert.IsFalse(juego.Bloqueado);
        Assert.AreSame(antes, juego.Estado, "El estado del motor no cambia");
        CollectionAssert.AreEqual(new[] { Fase.Resultado }, fases.ToArray(), "El foco vuelve a su sitio");
        BaseLunar.App.App.ManejarError(new Exception("sin juego"), null); // nunca lanza
    });

    // ---------- Contraste ----------

    private static double Luminancia(Color c)
    {
        static double Canal(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Canal(c.R) + 0.7152 * Canal(c.G) + 0.0722 * Canal(c.B);
    }

    private static double Contraste(Color a, Color b)
    {
        var (l1, l2) = (Luminancia(a), Luminancia(b));
        return (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);
    }

    private static Color Pincel(string clave) => ((SolidColorBrush)Application.Current.Resources[clave]).Color;

    /// <summary>Color opaco que se ve al poner uno semitransparente sobre otro.</summary>
    private static Color Sobre(Color arriba, Color abajo)
    {
        var a = arriba.A / 255.0;
        return Color.FromRgb((byte)(arriba.R * a + abajo.R * (1 - a)), (byte)(arriba.G * a + abajo.G * (1 - a)), (byte)(arriba.B * a + abajo.B * (1 - a)));
    }

    private static readonly (string Texto, string Fondo)[] Pares =
    [
        ("Pincel.Texto", "Pincel.Fondo"), ("Pincel.Texto", "Pincel.Superficie"), ("Pincel.TextoSuave", "Pincel.Superficie"),
        ("Pincel.TextoOscuro", "Pincel.Primario"), ("Pincel.TextoOscuro", "Pincel.Acento"), ("Pincel.Enlace", "Pincel.Superficie"),
        ("Pincel.Enlace", "Pincel.Fondo"), ("Pincel.Estrella", "Pincel.Fondo"), ("Pincel.Exito", "Pincel.FondoEfecto"),
        ("Pincel.Costo", "Pincel.FondoEfecto"), ("Pincel.Acento", "Pincel.Superficie"), ("Pincel.Texto", "Pincel.FondoEfecto"),
    ];

    [TestMethod(DisplayName = "Contraste AA de toda la paleta: cada par de texto y fondo que se usa llega a 4,5:1")]
    public void ContrasteAA() => HiloUI.Ejecutar(() =>
    {
        var fondo = Pincel("Pincel.Fondo");
        foreach (var (texto, base_) in Pares)
        {
            var colorFondo = Sobre(Pincel(base_), fondo);
            var razon = Contraste(Sobre(Pincel(texto), colorFondo), colorFondo);
            Assert.IsGreaterThanOrEqualTo(4.5, razon, $"{texto} sobre {base_}: {razon:0.00}:1");
        }
    });

    [TestMethod(DisplayName = "Alto contraste (D-56): todos los pares pasan a 7:1 o más con los colores del tema (captura)")]
    public void AltoContraste_() => HiloUI.Ejecutar(() =>
    {
        var recursos = Application.Current.Resources;
        var copia = AltoContraste.Aplicar(recursos, ColoresContraste.Noche);
        try
        {
            foreach (var (texto, base_) in Pares)
                Assert.IsGreaterThanOrEqualTo(7.0, Contraste(Pincel(texto), Pincel(base_)), $"{texto} sobre {base_}");
            var juego = Partida.Jugar(Partida.HastaNivel4);
            Capturas.Guardar(Capturas.Montar(juego, 1280, 720), "f4-alto-contraste");
        }
        finally
        {
            AltoContraste.Restaurar(recursos, copia);
        }
        Assert.AreEqual(Color.FromRgb(0x0F, 0x17, 0x2A), Pincel("Pincel.Fondo"), "Se restauran los colores");
    });

    // ---------- P4: solo con el teclado ----------

    [TestMethod(DisplayName = "P4: una partida completa hasta la Victoria y «Jugar de nuevo» solo con el teclado, donde esté el foco")]
    public void SoloConTeclado() => HiloUI.Ejecutar(() =>
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
            void Tecla(Key tecla)
            {
                var destino = FocusManager.GetFocusedElement(ventana) as UIElement ?? ventana;
                var fuente = PresentationSource.FromVisual(ventana)!;
                destino.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, fuente, Environment.TickCount, tecla) { RoutedEvent = Keyboard.KeyDownEvent });
                juego.TerminarTransicion();
            }

            Tecla(Key.Enter); // Comenzar misión (el foco está en el título)
            foreach (var opcion in new[] { Key.D1, Key.B, Key.NumPad1, Key.A, Key.D1 })
            {
                Tecla(opcion);
                Assert.AreEqual(Fase.Resultado, juego.Fase, $"Tras {opcion}");
                Assert.AreSame(ventana.Pantalla.Decisiones.BotonPrincipal, FocusManager.GetFocusedElement(ventana), "El foco va al botón principal");
                Tecla(Key.Enter); // Continuar: Enter sobre el botón enfocado
            }
            Assert.AreEqual(Fase.Victoria, juego.Fase);
            Assert.AreEqual("Respaldos sin usar: 3 de 3", juego.TextoRespaldosCertificado);

            // Escribir en el nombre no activa atajos: A, 1 y Enter son letras del campo.
            var campo = ventana.Pantalla.Tarjeta.CampoNombre;
            foreach (var tecla in new[] { Key.A, Key.D1, Key.B, Key.Enter })
            {
                campo.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(ventana)!, Environment.TickCount, tecla) { RoutedEvent = Keyboard.KeyDownEvent });
                Assert.AreEqual(Fase.Victoria, juego.Fase);
            }

            ventana.Pantalla.Decisiones.BotonPrincipal.Focus();
            Tecla(Key.Enter); // Jugar de nuevo
            Assert.AreEqual(Fase.Inicio, juego.Fase);
        }
        finally
        {
            ventana.Close();
        }
    });

    [TestMethod(DisplayName = "P4: con Tab se llega a todo: barra, tarjeta, glosario, opciones; en el certificado, nombre, Imprimir, enlaces y Jugar de nuevo")]
    public void OrdenDeTabulacion() => HiloUI.Ejecutar(() =>
    {
        var nivel3 = Capturas.Montar(Partida.Jugar("P", "A", "P", "B", "P"), 1280, 720);
        var ids = ParadasDeTab(nivel3);
        CollectionAssert.IsSubsetOf(new[] { "BotonSonido", "Tarjeta", "¿Qué es rayos cósmicos?", "¿Qué es campo magnético?", "¿Qué es regolito?", "OpcionA", "OpcionB" }, ids.ToArray(),
            string.Join(" → ", ids));
        Assert.IsLessThan(ids.IndexOf("OpcionA"), ids.IndexOf("Tarjeta"), "Primero se lee la tarjeta y después se decide");

        var victoria = Capturas.Montar(Partida.Jugar(Partida.HastaVictoria), 1280, 720);
        var idsVictoria = ParadasDeTab(victoria);
        CollectionAssert.IsSubsetOf(new[] { "NombreCertificado", "BotonImprimir", "BotonPrincipal" }, idsVictoria.ToArray(), string.Join(" → ", idsVictoria));
        var enlaces = Descendientes<TextBlock>(victoria).SelectMany(t => t.Inlines.OfType<Hyperlink>()).ToList();
        Assert.IsGreaterThanOrEqualTo(10, enlaces.Count, "Las fuentes de los 5 datos son enlaces");
        Assert.IsTrue(enlaces.All(e => e.Focusable), "Cada enlace se alcanza con el teclado");
    });

    // ---------- Ayudantes ----------

    /// <summary>Elementos que reciben el foco con Tab, en el orden del árbol (el de KeyboardNavigation por defecto).</summary>
    private static List<string> ParadasDeTab(DependencyObject raiz) =>
        Descendientes<Control>(raiz)
            .Where(c => c.Focusable && c.IsTabStop && c.IsEnabled && c.IsVisibleEnArbol(raiz))
            .Select(c => System.Windows.Automation.AutomationProperties.GetAutomationId(c) is { Length: > 0 } id ? id : System.Windows.Automation.AutomationProperties.GetName(c))
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

    private static string AutomationProperties(DependencyObject d) => System.Windows.Automation.AutomationProperties.GetAutomationId(d);

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

internal static class Arbol
{
    /// <summary>Visible si él y todos sus antecesores hasta la raíz lo son (IsVisible exige una ventana).</summary>
    public static bool IsVisibleEnArbol(this DependencyObject elemento, DependencyObject raiz)
    {
        for (var actual = elemento; actual is not null && actual != raiz; actual = VisualTreeHelper.GetParent(actual))
        {
            if (actual is UIElement { Visibility: not Visibility.Visible }) return false;
        }
        return true;
    }
}
