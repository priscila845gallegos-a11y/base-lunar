using System.Windows.Media;
using System.Windows.Shapes;

namespace BaseLunar.App.Animacion;

/// <summary>Formas de las partículas: burbujas de oxígeno, gotas, estrellas, confeti, polvo, chispas y radiación.</summary>
internal static class Particulas
{
    private static readonly Geometry FormaEstrella = Geometry.Parse("M10,0 L12.9,6.5 L20,7.3 L14.7,12.1 L16.2,19.1 L10,15.5 L3.8,19.1 L5.3,12.1 L0,7.3 L7.1,6.5 Z");
    private static readonly Brush[] ColoresConfeti =
    [
        Congelar(0xF9, 0x73, 0x16), Congelar(0x22, 0xD3, 0xEE), Congelar(0xFA, 0xCC, 0x15),
        Congelar(0xA7, 0x8B, 0xFA), Congelar(0x4A, 0xDE, 0x80), Congelar(0xF4, 0x72, 0xB6),
    ];

    private static SolidColorBrush Congelar(byte r, byte g, byte b, byte a = 0xFF)
    {
        var pincel = new SolidColorBrush(Color.FromArgb(a, r, g, b));
        pincel.Freeze();
        return pincel;
    }

    public static Shape Burbuja(int i) => new Ellipse
    {
        Width = 14 + i % 3 * 6,
        Height = 14 + i % 3 * 6,
        Stroke = Congelar(0xA5, 0xF3, 0xFC),
        StrokeThickness = 3,
        Fill = Congelar(0x67, 0xE8, 0xF9, 0x44),
    };

    public static Shape Gota(int i) => new Ellipse { Width = 9, Height = 13, Fill = Congelar(0x22, 0xD3, 0xEE) };

    public static Shape Estrella(int i) => new Path
    {
        Data = FormaEstrella,
        Width = 20,
        Height = 20,
        Stretch = Stretch.Fill,
        Fill = Congelar(0xFD, 0xE0, 0x47),
    };

    public static Shape Confeti(int i) => new Rectangle
    {
        Width = 10,
        Height = 18,
        RadiusX = 2,
        RadiusY = 2,
        Fill = ColoresConfeti[i % ColoresConfeti.Length],
    };

    public static Shape Polvo(int i) => new Ellipse { Width = 16 + i % 4 * 6, Height = 14 + i % 4 * 5, Fill = Congelar(0xA8, 0xA2, 0x9E, 0xCC) };

    public static Shape Chispa(int i) => new Ellipse { Width = 10, Height = 10, Fill = Congelar(0xFD, 0xE0, 0x47) };

    public static Shape Radiacion(int i) => new Ellipse { Width = 14, Height = 14, Fill = Congelar(0xFB, 0x92, 0x3C) };

    public static Shape Flujo(int i) => new Ellipse { Width = 9, Height = 9, Fill = Congelar(0x67, 0xE8, 0xF9) };
}
