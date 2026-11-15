using System.Windows;
using System.Windows.Controls;

namespace BaseLunar.App.Vistas;

/// <summary>Botones de decisión A y B, o el botón principal (comenzar, continuar, reintentar, jugar de nuevo).</summary>
public partial class PanelDecisiones : UserControl
{
    public PanelDecisiones()
    {
        InitializeComponent();
    }

    /// <summary>En pantallas anchas los botones van uno encima del otro; en las estrechas, lado a lado.</summary>
    public void Disponer(bool apiladas)
    {
        Opciones.Columns = apiladas ? 1 : 2;
        BotonA.Margin = apiladas ? new Thickness(0, 0, 0, 10) : new Thickness(0, 0, 6, 0);
        BotonB.Margin = apiladas ? new Thickness(0) : new Thickness(6, 0, 0, 0);
    }

    public void EnfocarPrincipal() => BotonPrincipal.Focus();
}
