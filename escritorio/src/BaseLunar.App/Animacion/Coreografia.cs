using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BaseLunar.App.Controles;

namespace BaseLunar.App.Animacion;

/// <summary>Qué se anima de un elemento. Solo transformaciones y opacidad: nunca se recalcula el diseño.</summary>
internal enum Eje
{
    X,
    Y,
    EscalaX,
    EscalaY,
    Giro,
    Opacidad,
}

/// <summary>
/// Construye el guion (un Storyboard) de un momento de la escena. Los tiempos van en milisegundos desde el
/// comienzo. Cada elemento lleva un grupo de transformaciones [escala, giro, traslado] y cada llamada añade
/// un tramo «desde el último valor hasta este» en el intervalo indicado.
/// Con movimiento reducido (D-43, 5.4) el movimiento se cambia por fundidos: las transformaciones saltan a
/// su valor final y las opacidades se comprimen en un fundido corto; no hay partículas.
/// </summary>
internal sealed class Coreografia
{
    private const double FundidoReducido = 400;

    private readonly Dictionary<(DependencyObject Elemento, Eje Eje), Pista> pistas = [];
    private readonly Dictionary<UIElement, List<(Visibility Valor, double Tiempo)>> visibilidades = [];
    private readonly List<(DependencyObject Elemento, DependencyProperty Propiedad, double Desde, double Hasta, double Inicio, double Duracion)> otras = [];
    private readonly Random azar;

    public Coreografia(bool reducido, int semilla = 7)
    {
        Reducido = reducido;
        azar = new Random(semilla);
    }

    public bool Reducido { get; }

    /// <summary>Instante en que termina el último tramo.</summary>
    public double Fin { get; private set; }

    /// <summary>Lo que anima el guion: al detenerlo, cada propiedad vuelve a su valor de partida en el acto.</summary>
    public List<(IAnimatable Objeto, DependencyProperty Propiedad)> Animados { get; } = [];

    private sealed class Pista(double inicial)
    {
        public List<(double Tiempo, double Valor, IEasingFunction? Curva, bool Salto)> Tramos { get; } = [];
        public double Ultimo { get; set; } = inicial;
        public double UltimoTiempo { get; set; }
    }

    // ---------- Primitivas ----------

    /// <summary>Lleva el eje del elemento a <paramref name="valor"/> entre inicio e inicio + duración.</summary>
    public Coreografia Hacia(UIElement elemento, Eje eje, double valor, double inicio, double duracion, IEasingFunction? curva = null)
    {
        var pista = PistaDe(elemento, eje);
        if (inicio > pista.UltimoTiempo) pista.Tramos.Add((inicio, pista.Ultimo, null, false));
        pista.Tramos.Add((inicio + duracion, valor, curva ?? Suave, duracion <= 0));
        pista.Ultimo = valor;
        pista.UltimoTiempo = Math.Max(pista.UltimoTiempo, inicio + duracion);
        Fin = Math.Max(Fin, inicio + duracion);
        return this;
    }

    /// <summary>
    /// Valor de partida del eje (en el instante 0), solo si todavía no tiene tramos: así se puede encadenar
    /// varias transiciones sobre el mismo elemento en un guion.
    /// </summary>
    public Coreografia Partir(UIElement elemento, Eje eje, double valor)
    {
        var pista = PistaDe(elemento, eje);
        if (pista.Tramos.Count > 0) return this;
        pista.Tramos.Add((0, valor, null, true));
        pista.Ultimo = valor;
        return this;
    }

    /// <summary>Fija un valor de golpe en un instante.</summary>
    public Coreografia Fijar(UIElement elemento, Eje eje, double valor, double tiempo = 0) => Hacia(elemento, eje, valor, tiempo, 0);

    public Coreografia Mover(UIElement elemento, double x, double y, double inicio, double duracion, IEasingFunction? curva = null) =>
        Hacia(elemento, Eje.X, x, inicio, duracion, curva).Hacia(elemento, Eje.Y, y, inicio, duracion, curva);

    public Coreografia Escalar(UIElement elemento, double escala, double inicio, double duracion, IEasingFunction? curva = null) =>
        Hacia(elemento, Eje.EscalaX, escala, inicio, duracion, curva).Hacia(elemento, Eje.EscalaY, escala, inicio, duracion, curva);

    public Coreografia Aparecer(UIElement elemento, double inicio, double duracion = 300) => Hacia(elemento, Eje.Opacidad, 1, inicio, duracion);

    public Coreografia Desaparecer(UIElement elemento, double inicio, double duracion = 300) => Hacia(elemento, Eje.Opacidad, 0, inicio, duracion);

    /// <summary>Va y vuelve alrededor del valor actual (pasos, temblores, saludos). Termina en el valor de partida.</summary>
    public Coreografia Oscilar(UIElement elemento, Eje eje, double amplitud, double periodo, double inicio, double duracion)
    {
        var centro = PistaDe(elemento, eje).Ultimo;
        var medio = periodo / 2;
        var signo = 1;
        double t;
        for (t = inicio; t + medio <= inicio + duracion; t += medio, signo = -signo)
            Hacia(elemento, eje, centro + signo * amplitud, t, medio, Seno);
        return Hacia(elemento, eje, centro, t, medio / 2, Seno);
    }

    /// <summary>Parpadeo de opacidad: se enciende y apaga <paramref name="veces"/> veces.</summary>
    public Coreografia Parpadear(UIElement elemento, int veces, double inicio, double periodo = 260, double alto = 1, double bajo = 0)
    {
        for (var i = 0; i < veces; i++)
        {
            Hacia(elemento, Eje.Opacidad, alto, inicio + i * periodo, periodo / 2);
            Hacia(elemento, Eje.Opacidad, bajo, inicio + i * periodo + periodo / 2, periodo / 2);
        }
        return this;
    }

    /// <summary>Cambia la visibilidad en un instante (para mostrar u ocultar el mapa o la escena).</summary>
    public Coreografia Visibilidad(UIElement elemento, Visibility valor, double tiempo)
    {
        if (!visibilidades.TryGetValue(elemento, out var cambios)) visibilidades[elemento] = cambios = [];
        cambios.Add((valor, tiempo));
        Fin = Math.Max(Fin, tiempo);
        return this;
    }

    /// <summary>Anima cualquier propiedad numérica (por ejemplo, el trazo de una línea discontinua).</summary>
    public Coreografia Propiedad(DependencyObject elemento, DependencyProperty propiedad, double desde, double hasta, double inicio, double duracion)
    {
        otras.Add((elemento, propiedad, desde, hasta, inicio, duracion));
        Fin = Math.Max(Fin, inicio + duracion);
        return this;
    }

    // ---------- Partículas ----------

    /// <summary>
    /// Lanza partículas desde un punto: cada una viaja con dispersión, gira y se desvanece. Con movimiento
    /// reducido no se lanza ninguna. Se limitan a 60 para no bajar de 60 cuadros por segundo.
    /// </summary>
    public Coreografia Emitir(Canvas lienzo, Func<int, Shape> forma, Point origen, Vector viaje, double dispersion, int cantidad,
        double inicio, double duracion, double escalonado = 60, double giro = 0)
    {
        if (Reducido) return this;
        cantidad = Math.Min(cantidad, 60);
        for (var i = 0; i < cantidad; i++)
        {
            var particula = forma(i);
            particula.Opacity = 0;
            particula.IsHitTestVisible = false;
            Canvas.SetLeft(particula, origen.X - particula.Width / 2);
            Canvas.SetTop(particula, origen.Y - particula.Height / 2);
            lienzo.Children.Add(particula);
            var comienzo = inicio + i * escalonado;
            var destino = new Vector(viaje.X + (azar.NextDouble() - 0.5) * dispersion, viaje.Y + (azar.NextDouble() - 0.5) * dispersion);
            Aparecer(particula, comienzo, 120);
            Mover(particula, destino.X, destino.Y, comienzo, duracion, new QuadraticEase { EasingMode = EasingMode.EaseOut });
            if (giro != 0) Hacia(particula, Eje.Giro, giro * (azar.NextDouble() + 0.5), comienzo, duracion);
            Desaparecer(particula, comienzo + duracion * 0.65, duracion * 0.35);
        }
        return this;
    }

    public double Azar(double minimo, double maximo) => minimo + azar.NextDouble() * (maximo - minimo);

    // ---------- Construcción ----------

    /// <summary>Crea el Storyboard. Con movimiento reducido aplica los cambios de golpe o con un fundido corto.</summary>
    public Storyboard Construir()
    {
        var tablero = new Storyboard { FillBehavior = FillBehavior.HoldEnd };
        var escalaTiempo = Reducido && Fin > 0 ? FundidoReducido / Fin : 1;
        foreach (var ((elemento, eje), pista) in pistas)
        {
            var animacion = new DoubleAnimationUsingKeyFrames();
            if (Reducido && eje != Eje.Opacidad)
            {
                animacion.KeyFrames.Add(new DiscreteDoubleKeyFrame(pista.Ultimo, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            }
            else
            {
                foreach (var (tiempo, valor, curva, salto) in pista.Tramos)
                {
                    var momento = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(tiempo * escalaTiempo));
                    animacion.KeyFrames.Add(salto
                        ? new DiscreteDoubleKeyFrame(valor, momento)
                        : new EasingDoubleKeyFrame(valor, momento, Reducido ? null : curva));
                }
            }
            Storyboard.SetTarget(animacion, elemento);
            Storyboard.SetTargetProperty(animacion, Ruta(eje));
            tablero.Children.Add(animacion);
            Animados.Add(Destino((UIElement)elemento, eje));
        }
        foreach (var (elemento, cambios) in visibilidades)
        {
            var animacion = new ObjectAnimationUsingKeyFrames();
            foreach (var (valor, tiempo) in cambios.OrderBy(c => c.Tiempo))
                animacion.KeyFrames.Add(new DiscreteObjectKeyFrame(valor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(tiempo * escalaTiempo))));
            Storyboard.SetTarget(animacion, elemento);
            Storyboard.SetTargetProperty(animacion, new PropertyPath(UIElement.VisibilityProperty));
            tablero.Children.Add(animacion);
            Animados.Add((elemento, UIElement.VisibilityProperty));
        }
        foreach (var (elemento, propiedad, desde, hasta, inicio, duracion) in otras)
        {
            var animacion = Reducido
                ? new DoubleAnimation(hasta, TimeSpan.Zero)
                : new DoubleAnimation(desde, hasta, TimeSpan.FromMilliseconds(duracion)) { BeginTime = TimeSpan.FromMilliseconds(inicio) };
            Storyboard.SetTarget(animacion, elemento);
            Storyboard.SetTargetProperty(animacion, new PropertyPath(propiedad));
            tablero.Children.Add(animacion);
            if (elemento is IAnimatable animable) Animados.Add((animable, propiedad));
        }
        return tablero;
    }

    // ---------- Transformaciones ----------

    private static readonly IEasingFunction Suave = new CubicEase { EasingMode = EasingMode.EaseInOut };
    private static readonly IEasingFunction Seno = new SineEase { EasingMode = EasingMode.EaseInOut };

    private Pista PistaDe(UIElement elemento, Eje eje)
    {
        if (pistas.TryGetValue((elemento, eje), out var pista)) return pista;
        var grupo = Transformaciones(elemento);
        var inicial = eje switch
        {
            Eje.X => ((TranslateTransform)grupo.Children[2]).X,
            Eje.Y => ((TranslateTransform)grupo.Children[2]).Y,
            Eje.EscalaX => ((ScaleTransform)grupo.Children[0]).ScaleX,
            Eje.EscalaY => ((ScaleTransform)grupo.Children[0]).ScaleY,
            Eje.Giro => ((RotateTransform)grupo.Children[1]).Angle,
            _ => elemento.Opacity,
        };
        pista = new Pista(inicial);
        pistas[(elemento, eje)] = pista;
        return pista;
    }

    /// <summary>El grupo [escala, giro, traslado] del elemento; si no lo tiene, lo crea (sin cambiar cómo se ve).</summary>
    public static TransformGroup Transformaciones(UIElement elemento)
    {
        if (elemento.RenderTransform is TransformGroup { Children.Count: 3 } grupo
            && grupo.Children[0] is ScaleTransform && grupo.Children[1] is RotateTransform && grupo.Children[2] is TranslateTransform)
            return grupo;
        var nuevo = new TransformGroup();
        nuevo.Children.Add(new ScaleTransform());
        nuevo.Children.Add(new RotateTransform());
        nuevo.Children.Add(new TranslateTransform());
        elemento.RenderTransform = nuevo;
        return nuevo;
    }

    /// <summary>El objeto y la propiedad que anima un eje (la transformación o el propio elemento).</summary>
    public static (IAnimatable Objeto, DependencyProperty Propiedad) Destino(UIElement elemento, Eje eje)
    {
        var grupo = Transformaciones(elemento);
        return eje switch
        {
            Eje.Opacidad => (elemento, UIElement.OpacityProperty),
            Eje.X => (grupo.Children[2], TranslateTransform.XProperty),
            Eje.Y => (grupo.Children[2], TranslateTransform.YProperty),
            Eje.EscalaX => (grupo.Children[0], ScaleTransform.ScaleXProperty),
            Eje.EscalaY => (grupo.Children[0], ScaleTransform.ScaleYProperty),
            _ => (grupo.Children[1], RotateTransform.AngleProperty),
        };
    }

    /// <summary>Quita de golpe las animaciones de esas propiedades (Storyboard.Remove espera al siguiente cuadro).</summary>
    public static void Quitar(IEnumerable<(IAnimatable Objeto, DependencyProperty Propiedad)> animados)
    {
        foreach (var (objeto, propiedad) in animados) objeto.BeginAnimation(propiedad, null);
    }

    /// <summary>Ruta de la propiedad que anima un eje (también la usa el ambiente).</summary>
    public static PropertyPath RutaDe(Eje eje) => Ruta(eje);

    private static PropertyPath Ruta(Eje eje) => eje switch
    {
        Eje.Opacidad => new PropertyPath(UIElement.OpacityProperty),
        Eje.X => Transformacion(2, TranslateTransform.XProperty),
        Eje.Y => Transformacion(2, TranslateTransform.YProperty),
        Eje.EscalaX => Transformacion(0, ScaleTransform.ScaleXProperty),
        Eje.EscalaY => Transformacion(0, ScaleTransform.ScaleYProperty),
        _ => Transformacion(1, RotateTransform.AngleProperty),
    };

    private static PropertyPath Transformacion(int indice, DependencyProperty propiedad) =>
        new($"(0).(1)[{indice}].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, propiedad);

    /// <summary>¿Hay que reducir el movimiento ahora? (preferencia de Windows o pruebas).</summary>
    public static bool MovimientoReducido => Movimiento.Reducido;
}
