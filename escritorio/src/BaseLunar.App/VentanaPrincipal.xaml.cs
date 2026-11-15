using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using BaseLunar.App.Audio;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.App;

/// <summary>Ventana principal: aloja la pantalla del juego, los atajos de teclado y la pantalla completa (F11).</summary>
public partial class VentanaPrincipal : Window
{
    private (WindowState Estado, WindowStyle Estilo, ResizeMode Redimension)? antesDePantallaCompleta;

    public VentanaPrincipal() : this(CrearJuego())
    {
    }

    /// <summary>
    /// El juego con sonido, voz y ajustes reales. Con --silencio (lo usan las pruebas de UI Automation) no
    /// suena nada y no se tocan los ajustes guardados.
    /// </summary>
    private static JuegoVM CrearJuego()
    {
        var tormenta = DatosDonki.RespaldoHistorico();
        if (Environment.GetCommandLineArgs().Contains("--silencio", StringComparer.OrdinalIgnoreCase))
            return new JuegoVM(tormenta, new ReproductorMudo(), new NarradorMudo(), Ajustes.EnMemoria());
        return new JuegoVM(tormenta, new ReproductorWindows(), new NarradorWindows(), Ajustes.Cargar(Ajustes.RutaPredeterminada));
    }

    public VentanaPrincipal(JuegoVM juego)
    {
        InitializeComponent();
        Juego = juego;
        DataContext = juego;
        Closed += (_, _) => juego.Dispose();
        var area = SystemParameters.WorkArea;
        Width = Math.Max(MinWidth, Math.Min(Width, area.Width * 0.92));
        Height = Math.Max(MinHeight, Math.Min(Height, area.Height * 0.92));
    }

    public JuegoVM Juego { get; }

    public bool EnPantallaCompleta => antesDePantallaCompleta is not null;

    private void AlCargar(object sender, RoutedEventArgs e) => Pantalla.Enfocar(Juego.Fase);

    /// <summary>Atajos (5.5): A/B o 1/2 eligen en un nivel; Enter usa el botón principal; F11 y Esc.</summary>
    private void AlPulsarTecla(object sender, KeyEventArgs e)
    {
        var tecla = e.Key == Key.System ? e.SystemKey : e.Key;
        var atajo = Atajos.Interpretar(tecla, Keyboard.Modifiers, e.IsRepeat, e.OriginalSource is TextBoxBase);
        switch (atajo)
        {
            case Atajo.OpcionA or Atajo.OpcionB when Juego.EnNivel:
                e.Handled = true;
                Juego.ElegirOpcion(atajo == Atajo.OpcionA ? "A" : "B");
                break;
            case Atajo.Principal when !Juego.EnNivel:
                e.Handled = true;
                Juego.AccionPrincipal();
                break;
            case Atajo.PantallaCompleta:
                e.Handled = true;
                AlternarPantallaCompleta();
                break;
            case Atajo.SalirPantallaCompleta when EnPantallaCompleta:
                e.Handled = true;
                AlternarPantallaCompleta();
                break;
        }
    }

    /// <summary>Pantalla completa sin bordes, o vuelta a la ventana como estaba.</summary>
    public void AlternarPantallaCompleta()
    {
        if (antesDePantallaCompleta is { } antes)
        {
            antesDePantallaCompleta = null;
            WindowStyle = antes.Estilo;
            ResizeMode = antes.Redimension;
            WindowState = antes.Estado;
            return;
        }
        antesDePantallaCompleta = (WindowState, WindowStyle, ResizeMode);
        WindowState = WindowState.Normal;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowState = WindowState.Maximized;
    }
}
