using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;
using BaseLunar.App.Controles;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.App.Vistas;

/// <summary>
/// La pantalla completa del juego. Elige la disposición según el ancho, mueve el foco al terminar cada
/// transición, anuncia en las regiones en vivo y abre los enlaces de las fuentes en el navegador.
/// </summary>
public partial class PantallaJuego : UserControl
{
    /// <summary>
    /// Desde este ancho (en DIP) la disposición es ancha: la escena deja al menos 640 DIP para los
    /// 3 medidores, que van sobre ella.
    /// </summary>
    public const double AnchoDisposicionAncha = 1100;

    private JuegoVM? juego;

    public PantallaJuego()
    {
        InitializeComponent();
        Raiz.SizeChanged += (_, e) => Disponer(e.NewSize.Width);
        DataContextChanged += AlCambiarModelo;
        AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(AlAbrirEnlace));
        Disponer(0);
    }

    public bool EsAncha { get; private set; }

    /// <summary>Coloca los paneles para la disposición ancha o la compacta.</summary>
    public void Disponer(double ancho)
    {
        var ancha = ancho >= AnchoDisposicionAncha;
        EsAncha = ancha;
        Disposicion.SetCompacta(this, !ancha);
        ColumnaLateral.Width = ancha ? new GridLength(Math.Clamp(ancho * 0.36, 420, 600)) : new GridLength(0);
        FilaEscena.Height = new GridLength(ancha ? 1 : 2, GridUnitType.Star);
        FilaTarjeta.Height = new GridLength(ancha ? 1 : 5, GridUnitType.Star);
        FilaEscena.MinHeight = ancha ? 0 : 112; // incluye el margen inferior de 10

        // Ancha: los medidores flotan sobre la parte alta de la escena, como el HUD de un videojuego.
        Ubicar(Medidores, fila: 1, columna: 0);
        Medidores.Margin = ancha ? new Thickness(12, 12, 26, 0) : new Thickness(0, 0, 0, 10);
        Ubicar(Escena, fila: ancha ? 1 : 2, columna: 0, filas: ancha ? 4 : 1);
        Escena.Margin = ancha ? new Thickness(0, 0, 14, 0) : new Thickness(0, 0, 0, 10);
        Ubicar(Tarjeta, fila: ancha ? 1 : 3, columna: ancha ? 1 : 0, filas: ancha ? 3 : 1);
        Ubicar(Decisiones, fila: 4, columna: ancha ? 1 : 0);

        Decisiones.Disponer(apiladas: ancha);
        Estado.Disponer(compacto: ancho < 900);
    }

    private static void Ubicar(UIElement elemento, int fila, int columna, int filas = 1)
    {
        Grid.SetRow(elemento, fila);
        Grid.SetColumn(elemento, columna);
        Grid.SetRowSpan(elemento, filas);
    }

    /// <summary>
    /// Mueve el foco como enfocar() de ui.js: tras decidir, al resultado o al respaldo y al botón principal;
    /// al cambiar de nivel o de pantalla, al título de la tarjeta.
    /// </summary>
    public void Enfocar(Fase fase)
    {
        if (fase is Fase.Resultado or Fase.Respaldo)
        {
            Tarjeta.MostrarSeccion(fase);
            Decisiones.EnfocarPrincipal();
            return;
        }
        Tarjeta.EnfocarTitulo(fase);
    }

    private void AlCambiarModelo(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is JuegoVM anterior)
        {
            anterior.Anunciar -= AlAnunciar;
            anterior.Enfocar -= AlEnfocar;
            anterior.Escena -= AlCambiarEscena;
        }
        juego = e.NewValue as JuegoVM;
        if (juego is null) return;
        juego.Anunciar += AlAnunciar;
        juego.Enfocar += AlEnfocar;
        juego.Escena += AlCambiarEscena;
        Escenario.Sincronizar(juego.Estado);
    }

    private void AlCambiarEscena(object? sender, Momento momento) => Escenario.Representar(momento);

    private void AlEnfocar(object? sender, Fase fase) => Enfocar(fase);

    private void AlAnunciar(object? sender, Anuncio anuncio) => Decir(anuncio.Urgente ? AnuncioUrgente : AnuncioCortes, anuncio.Texto);

    /// <summary>Vacía la región y escribe el texto un instante después, como decir() en ui.js.</summary>
    private static void Decir(TextBlock region, string texto)
    {
        region.Text = string.Empty;
        var espera = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Configuracion.Tiempos.AnuncioMs) };
        espera.Tick += (_, _) =>
        {
            espera.Stop();
            region.Text = texto;
            UIElementAutomationPeer.CreatePeerForElement(region)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        };
        espera.Start();
    }

    /// <summary>Abre la fuente en el navegador, solo si es un enlace https de la NASA o de la CSA.</summary>
    private void AlAbrirEnlace(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        if (!Enlaces.EsSeguro(e.Uri)) return;
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true })?.Dispose();
    }
}
