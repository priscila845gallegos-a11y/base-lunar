using System.Windows;
using System.Windows.Media.Animation;
using BaseLunar.App.Vistas;

namespace BaseLunar.App.Animacion;

/// <summary>
/// Animaciones de ambiente que se repiten mientras se ve la escena: el astronauta respira, los rayos
/// cósmicos caen (N3), la radiación viaja desde el Sol (N4), la radio tiene estática (N5) y la escarcha
/// brilla (N1). Con movimiento reducido no hay ambiente.
/// </summary>
internal static class Ambiente
{
    public static Storyboard? Crear(EscenaNivel escena, int nivel, List<(IAnimatable Objeto, DependencyProperty Propiedad)> animados)
    {
        if (Coreografia.MovimientoReducido) return null;
        var tablero = new Storyboard();
        tablero.SetValue(AnimadosProperty, animados);
        Vaiven(tablero, escena.Astro.Torso, Eje.EscalaY, 1, 1.03, 1300);
        switch (nivel)
        {
            case 1:
                Vaiven(tablero, escena.EscarchaSuelo, Eje.Opacidad, 0.35, 0.9, 1500);
                break;
            case 3:
                UIElement[] rayos = [escena.Rayo1, escena.Rayo2, escena.Rayo3, escena.Rayo4, escena.Rayo5];
                for (var i = 0; i < rayos.Length; i++) Viajar(tablero, rayos[i], 380, 380, 1700, i * 340);
                Vaiven(tablero, escena.LuzDosimetro, Eje.Opacidad, 1, 0.15, 380);
                break;
            case 4:
                for (var i = 0; i < escena.Radiacion4.Children.Count; i++)
                    Viajar(tablero, escena.Radiacion4.Children[i], 560, 250, 2300, i * 380);
                Vaiven(tablero, escena.HaloSol, Eje.Opacidad, 1, 0.75, 1500);
                break;
            case 5:
                Vaiven(tablero, escena.Estatica5, Eje.Opacidad, 1, 0.3, 140);
                break;
        }
        return tablero;
    }

    /// <summary>Va y vuelve entre dos valores sin parar.</summary>
    private static void Vaiven(Storyboard tablero, UIElement elemento, Eje eje, double desde, double hasta, double milisegundos)
    {
        Coreografia.Transformaciones(elemento);
        var animacion = new DoubleAnimation(desde, hasta, TimeSpan.FromMilliseconds(milisegundos))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        Agregar(tablero, animacion, elemento, eje);
    }

    /// <summary>Recorre un trayecto apareciendo y desvaneciéndose, una y otra vez.</summary>
    private static void Viajar(Storyboard tablero, UIElement elemento, double dx, double dy, double milisegundos, double retraso)
    {
        Coreografia.Transformaciones(elemento);
        var duracion = TimeSpan.FromMilliseconds(milisegundos);
        var comienzo = TimeSpan.FromMilliseconds(retraso);
        Agregar(tablero, new DoubleAnimation(0, dx, duracion) { RepeatBehavior = RepeatBehavior.Forever, BeginTime = comienzo }, elemento, Eje.X);
        Agregar(tablero, new DoubleAnimation(0, dy, duracion) { RepeatBehavior = RepeatBehavior.Forever, BeginTime = comienzo }, elemento, Eje.Y);
        var opacidad = new DoubleAnimationUsingKeyFrames { Duration = duracion, RepeatBehavior = RepeatBehavior.Forever, BeginTime = comienzo };
        opacidad.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
        opacidad.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.15)));
        opacidad.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.7)));
        opacidad.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));
        Agregar(tablero, opacidad, elemento, Eje.Opacidad);
    }

    private static void Agregar(Storyboard tablero, AnimationTimeline animacion, UIElement elemento, Eje eje)
    {
        Storyboard.SetTarget(animacion, elemento);
        Storyboard.SetTargetProperty(animacion, Coreografia.RutaDe(eje));
        tablero.Children.Add(animacion);
        ((List<(IAnimatable, DependencyProperty)>)tablero.GetValue(AnimadosProperty)).Add(Coreografia.Destino(elemento, eje));
    }

    /// <summary>Lista de lo animado, guardada en el propio tablero mientras se construye.</summary>
    private static readonly DependencyProperty AnimadosProperty = DependencyProperty.RegisterAttached(
        "Animados", typeof(List<(IAnimatable, DependencyProperty)>), typeof(Ambiente));
}
