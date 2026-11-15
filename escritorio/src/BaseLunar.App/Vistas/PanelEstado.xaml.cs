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

    /// <summary>
    /// En pantallas estrechas se oculta el nombre (queda el logo; el nombre está en el título de la ventana)
    /// para dejar sitio al minimapa, al progreso y a los botones de sonido.
    /// </summary>
    public void Disponer(bool compacto)
    {
        Nombre.Visibility = compacto ? Visibility.Collapsed : Visibility.Visible;
        // En la compacta, «Escuchar» queda solo con el ícono; la palabra sigue siendo su nombre accesible (D-21).
        TextoEscuchar.Visibility = compacto ? Visibility.Collapsed : Visibility.Visible;
    }
}
