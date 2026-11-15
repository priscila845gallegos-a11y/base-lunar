using System.Windows;
using System.Windows.Controls;

namespace BaseLunar.App.Vistas;

/// <summary>
/// Mapa de progreso: la base, las 5 estaciones (superadas con estrella, la actual late) y el relevo.
/// La ficha del astronauta la mueve Animacion/Director.cs de casilla en casilla.
/// </summary>
public partial class MapaMision : UserControl
{
    /// <summary>Ancho de cada casilla del camino en el lienzo del mapa.</summary>
    public const double AnchoCasilla = 180;

    public MapaMision()
    {
        InitializeComponent();
    }

    /// <summary>Centro del círculo de una casilla (0 base, 1–5 estaciones, 6 meta), en coordenadas del mapa.</summary>
    internal Point CentroDeCasilla(int casilla)
    {
        if (Camino.ActualWidth <= 0 || ActualWidth <= 0) return new Point(ActualWidth / 2, ActualHeight / 2);
        return Camino.TranslatePoint(new Point(AnchoCasilla / 2 + AnchoCasilla * casilla, 110), this);
    }
}
