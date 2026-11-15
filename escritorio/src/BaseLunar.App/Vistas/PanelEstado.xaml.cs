using System.Windows;
using System.Windows.Controls;

namespace BaseLunar.App.Vistas;

/// <summary>Barra superior: nombre, minimapa de progreso y respaldos.</summary>
public partial class PanelEstado : UserControl
{
    public PanelEstado()
    {
        InitializeComponent();
    }

    /// <summary>En pantallas estrechas se oculta el subtítulo para dejar sitio al minimapa.</summary>
    public void Disponer(bool compacto) => Subtitulo.Visibility = compacto ? Visibility.Collapsed : Visibility.Visible;
}
