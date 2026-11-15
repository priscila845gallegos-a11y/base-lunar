using System.Windows;
using System.Windows.Controls;

namespace BaseLunar.App.Vistas;

/// <summary>
/// Escena ilustrada: fondo común, una capa de objetos por nivel y efectos. No anima nada por sí misma:
/// Animacion/Director.cs elige la capa y reproduce cada momento.
/// </summary>
public partial class EscenaNivel : UserControl
{
    public EscenaNivel()
    {
        InitializeComponent();
    }

    /// <summary>Los elementos propios de cada nivel (1 a 5).</summary>
    internal IReadOnlyList<UIElement> CapasDe(int nivel) => nivel switch
    {
        1 => [CieloNoche, EscarchaSuelo, Props1],
        2 => [Props2],
        3 => [Rayos3, Props3],
        4 => [Sol4, Props4, Bolsas4],
        5 => [Props5],
        _ => [],
    };

    /// <summary>Muestra solo las capas del nivel indicado (0: ninguna).</summary>
    internal void MostrarNivel(int nivel)
    {
        for (var n = 1; n <= 5; n++)
        {
            foreach (var capa in CapasDe(n)) capa.Visibility = n == nivel ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>El icono de rebobinado (dentro de su círculo).</summary>
    internal UIElement IconoRebobinar => (UIElement)Rebobinar.Children[1];
}
