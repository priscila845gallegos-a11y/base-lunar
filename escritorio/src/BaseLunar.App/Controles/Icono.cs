using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BaseLunar.App.Controles;

/// <summary>
/// Ícono de trazo de Lucide (caja de 24×24) escalado al tamaño del control. Toma el color del texto que lo
/// rodea, como currentColor en SVG. Es decorativo: no aparece en el árbol de accesibilidad.
/// </summary>
public sealed class Icono : FrameworkElement
{
    private const double Caja = 24;

    public static readonly DependencyProperty GeometriaProperty = DependencyProperty.Register(
        nameof(Geometria), typeof(Geometry), typeof(Icono),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty GrosorProperty = DependencyProperty.Register(
        nameof(Grosor), typeof(double), typeof(Icono),
        new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RellenoProperty = DependencyProperty.Register(
        nameof(Relleno), typeof(Brush), typeof(Icono),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Color del trazo: se hereda del texto (TextElement.Foreground).</summary>
    public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
        typeof(Icono),
        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Tamaño de la letra que lo rodea: sin ancho ni alto explícitos, el ícono mide 1,15 em.</summary>
    public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
        typeof(Icono),
        new FrameworkPropertyMetadata(18.0, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public Geometry? Geometria
    {
        get => (Geometry?)GetValue(GeometriaProperty);
        set => SetValue(GeometriaProperty, value);
    }

    public double Grosor
    {
        get => (double)GetValue(GrosorProperty);
        set => SetValue(GrosorProperty, value);
    }

    public Brush? Relleno
    {
        get => (Brush?)GetValue(RellenoProperty);
        set => SetValue(RellenoProperty, value);
    }

    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var lado = FontSize * 1.15;
        return new Size(Math.Min(lado, availableSize.Width), Math.Min(lado, availableSize.Height));
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (Geometria is null) return;
        var escala = Math.Min(ActualWidth, ActualHeight) / Caja;
        if (escala <= 0) return;
        var x = (ActualWidth - Caja * escala) / 2;
        var y = (ActualHeight - Caja * escala) / 2;
        var lapiz = new Pen(Foreground, Grosor)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round,
        };
        dc.PushTransform(new MatrixTransform(escala, 0, 0, escala, x, y));
        dc.DrawGeometry(Relleno, lapiz, Geometria);
        dc.Pop();
    }
}
