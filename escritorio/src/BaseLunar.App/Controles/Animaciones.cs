using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BaseLunar.App.Controles;

/// <summary>
/// Animaciones que se disparan desde los datos, como propiedades adjuntas. Los «disparos» son contadores:
/// cada vez que el modelo de vista los incrementa, la animación vuelve a empezar. Todas respetan
/// <see cref="Movimiento.Reducido"/>.
/// </summary>
public static class Animaciones
{
    private static readonly Duration Transicion = TimeSpan.FromMilliseconds(Configuracion.Tiempos.TransicionMs);

    // ---------- Barra que se llena suavemente (5.4) ----------
    public static readonly DependencyProperty ValorSuaveProperty = DependencyProperty.RegisterAttached(
        "ValorSuave", typeof(double), typeof(Animaciones), new PropertyMetadata(0.0, AlCambiarValor));

    public static double GetValorSuave(DependencyObject d) => (double)d.GetValue(ValorSuaveProperty);

    public static void SetValorSuave(DependencyObject d, double valor) => d.SetValue(ValorSuaveProperty, valor);

    private static void AlCambiarValor(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RangeBase barra) return;
        var valor = (double)e.NewValue;
        if (Movimiento.Reducido)
        {
            barra.BeginAnimation(RangeBase.ValueProperty, null);
            barra.Value = valor;
            return;
        }
        barra.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation(valor, Transicion) { EasingFunction = new QuadraticEase() });
    }

    // ---------- Temblor de la tarjeta al activarse un respaldo ----------
    public static readonly DependencyProperty TemblorProperty = DependencyProperty.RegisterAttached(
        "Temblor", typeof(int), typeof(Animaciones), new PropertyMetadata(0, AlTemblar));

    public static int GetTemblor(DependencyObject d) => (int)d.GetValue(TemblorProperty);

    public static void SetTemblor(DependencyObject d, int valor) => d.SetValue(TemblorProperty, valor);

    private static void AlTemblar(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement elemento || (int)e.NewValue == 0 || Movimiento.Reducido) return;
        var animacion = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(Configuracion.Tiempos.TemblorMs) };
        double[] pasos = [0, -12, 12, -9, 9, -5, 5, 0];
        for (var i = 0; i < pasos.Length; i++)
            animacion.KeyFrames.Add(new LinearDoubleKeyFrame(pasos[i], KeyTime.FromPercent(i / (double)(pasos.Length - 1))));
        Desplazamiento(elemento).BeginAnimation(TranslateTransform.XProperty, animacion);
    }

    // ---------- Salto de alegría al superar un nivel ----------
    public static readonly DependencyProperty SaltoProperty = DependencyProperty.RegisterAttached(
        "Salto", typeof(int), typeof(Animaciones), new PropertyMetadata(0, AlSaltar));

    public static int GetSalto(DependencyObject d) => (int)d.GetValue(SaltoProperty);

    public static void SetSalto(DependencyObject d, int valor) => d.SetValue(SaltoProperty, valor);

    private static void AlSaltar(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement elemento || (int)e.NewValue == 0 || Movimiento.Reducido) return;
        var animacion = new DoubleAnimation(0, -60, TimeSpan.FromMilliseconds(260))
        {
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
        };
        Desplazamiento(elemento).BeginAnimation(TranslateTransform.YProperty, animacion);
    }

    // ---------- Número flotante junto al medidor («+20 ⚡», «¡Al máximo!») ----------
    public static readonly DependencyProperty FlotanteProperty = DependencyProperty.RegisterAttached(
        "Flotante", typeof(int), typeof(Animaciones), new PropertyMetadata(0, AlFlotar));

    public static int GetFlotante(DependencyObject d) => (int)d.GetValue(FlotanteProperty);

    public static void SetFlotante(DependencyObject d, int valor) => d.SetValue(FlotanteProperty, valor);

    private static void AlFlotar(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement elemento || (int)e.NewValue == 0) return;
        var duracion = TimeSpan.FromMilliseconds(1600);
        var opacidad = new DoubleAnimationUsingKeyFrames { Duration = duracion };
        opacidad.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.1)));
        opacidad.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.7)));
        opacidad.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));
        elemento.BeginAnimation(UIElement.OpacityProperty, opacidad);
        if (Movimiento.Reducido) return;
        var subida = new DoubleAnimation(10, -18, duracion) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
        Desplazamiento(elemento).BeginAnimation(TranslateTransform.YProperty, subida);
    }

    // ---------- Pulso de un medidor bajo ----------
    public static readonly DependencyProperty PulsoProperty = DependencyProperty.RegisterAttached(
        "Pulso", typeof(bool), typeof(Animaciones), new PropertyMetadata(false, AlPulsar));

    public static bool GetPulso(DependencyObject d) => (bool)d.GetValue(PulsoProperty);

    public static void SetPulso(DependencyObject d, bool valor) => d.SetValue(PulsoProperty, valor);

    private static void AlPulsar(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement elemento) return;
        if (!(bool)e.NewValue || Movimiento.Reducido)
        {
            elemento.BeginAnimation(UIElement.OpacityProperty, null);
            return;
        }
        elemento.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, 0.55, TimeSpan.FromMilliseconds(800))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
        });
    }

    // ---------- Recorte con esquinas redondeadas ----------
    public static readonly DependencyProperty RadioProperty = DependencyProperty.RegisterAttached(
        "Radio", typeof(double), typeof(Animaciones), new PropertyMetadata(0.0, AlCambiarRadio));

    public static double GetRadio(DependencyObject d) => (double)d.GetValue(RadioProperty);

    public static void SetRadio(DependencyObject d, double valor) => d.SetValue(RadioProperty, valor);

    private static void AlCambiarRadio(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement elemento) return;
        elemento.SizeChanged -= Recortar;
        elemento.SizeChanged += Recortar;
        Recortar(elemento, null);
    }

    private static void Recortar(object sender, SizeChangedEventArgs? e)
    {
        var elemento = (FrameworkElement)sender;
        var radio = GetRadio(elemento);
        elemento.Clip = new RectangleGeometry(new Rect(0, 0, elemento.ActualWidth, elemento.ActualHeight), radio, radio);
    }

    /// <summary>El desplazamiento propio del elemento, creado la primera vez.</summary>
    private static TranslateTransform Desplazamiento(UIElement elemento)
    {
        if (elemento.RenderTransform is TranslateTransform propio) return propio;
        var nuevo = new TranslateTransform();
        elemento.RenderTransform = nuevo;
        return nuevo;
    }
}
