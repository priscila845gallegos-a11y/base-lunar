using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BaseLunar.App.VistaModelos;
using BaseLunar.App.Vistas;

namespace BaseLunar.App.Animacion;

/// <summary>
/// Director de escena: convierte cada momento que publica el modelo de vista en un guion de animación sobre
/// el mapa y la escena. Nunca decide reglas. Cada momento nuevo detiene el anterior (así, «Continuar» o
/// Enter saltan una animación) y deja como valores fijos lo que tiene que quedar al terminar.
/// </summary>
internal sealed class Director(Escenario escenario)
{
    private Storyboard? guion;
    private Storyboard? ambiente;
    private readonly List<(IAnimatable Objeto, DependencyProperty Propiedad)> animados = [];
    private int nivel;
    private bool enEscena;

    private EscenaNivel E => escenario.Escena;
    private MapaMision M => escenario.Mapa;

    /// <summary>Duración en milisegundos del último guion.</summary>
    public double Duracion { get; private set; }

    /// <summary>El último momento representado.</summary>
    public Momento? Ultimo { get; private set; }

    /// <summary>Coloca mapa, escena y ficha según el estado, sin animación (al abrir la pantalla).</summary>
    public void Sincronizar(Estado estado)
    {
        Detener();
        nivel = Presentacion.NivelDe(estado)?.Id ?? 0;
        E.MostrarNivel(nivel);
        FijarCasilla(Presentacion.CasillaDe(estado));
        enEscena = estado.Fase is Fase.Nivel or Fase.Resultado or Fase.Respaldo;
        FijarVista(enEscena);
        IniciarAmbiente();
    }

    /// <summary>Representa un momento: construye su guion y lo pone en marcha.</summary>
    public void Representar(Momento momento)
    {
        Detener();
        Ultimo = momento;
        var c = new Coreografia(Coreografia.MovimientoReducido, semilla: momento.Nivel * 10 + (momento.Opcion?[0] ?? 0));
        switch (momento.Tipo)
        {
            case TipoMomento.Inicio:
                CambiarCapas(c, 0, 400);
                Vista(c, escena: false, 0, 400);
                FijarCasilla(0);
                break;
            case TipoMomento.EntrarNivel:
                EntrarNivel(c, momento);
                break;
            case TipoMomento.Consecuencia:
                if (Consecuencias.Todas.TryGetValue($"{momento.Nivel}{momento.Opcion}", out var consecuencia)) consecuencia(c, E);
                break;
            case TipoMomento.Fallo:
                Consecuencias.Fallos[momento.Medidor ?? Medidor.Soporte](c, E);
                Consecuencias.BajarSalvavidas(c, E, 900);
                break;
            case TipoMomento.Reintento:
                Reintento(c, momento);
                break;
            case TipoMomento.Victoria:
                Victoria(c);
                break;
            case TipoMomento.Abortada:
                Abortada(c, momento);
                break;
        }
        guion = c.Construir();
        animados.AddRange(c.Animados);
        Duracion = c.Fin;
        guion.Begin(escenario, HandoffBehavior.SnapshotAndReplace, isControllable: true);
        IniciarAmbiente();
    }

    /// <summary>Pruebas: lleva el guion a su final (estado que se queda en pantalla), con efecto inmediato.</summary>
    internal void Terminar() => Saltar(Duracion + 10);

    /// <summary>Pruebas: lleva el guion a un instante (en milisegundos) para capturar un fotograma.</summary>
    internal void Saltar(double milisegundos) =>
        guion?.SeekAlignedToLastTick(escenario, TimeSpan.FromMilliseconds(milisegundos), TimeSeekOrigin.BeginTime);

    // ---------- Momentos ----------

    /// <summary>Del mapa a la escena: el astronauta camina hasta la estación y la cámara entra.</summary>
    private void EntrarNivel(Coreografia c, Momento momento)
    {
        var t = 0.0;
        if (enEscena)
        {
            Vista(c, escena: false, 0, 400);
            t = 400;
        }
        CambiarCapas(c, momento.Nivel, t);
        Caminar(c, momento.Desde, momento.Nivel, t + 100, 300 + 450 * Math.Max(1, momento.Nivel - momento.Desde));
        t = c.Fin + 100;
        Vista(c, escena: true, t, 600, zoomCasilla: momento.Nivel);
        t += 600;
        Posturas.Saludar(c, E.Astro, t, 1100);
        if (momento.ClaseX)
        {
            c.Parpadear(E.Destello, 1, t + 150, 420, 0.75, 0);
            c.Parpadear(E.DestelloCupula, 3, t + 250, 320, 0.85, 0);
        }
        if (momento.Ayuda) c.Parpadear(E.LuzAyuda, 4, t, 420, 1, 0);
    }

    /// <summary>La escena se «rebobina»: el salvavidas sube y el nivel vuelve a empezar.</summary>
    private void Reintento(Coreografia c, Momento momento)
    {
        c.Fijar(E.Salvavidas, Eje.Y, 0).Hacia(E.Salvavidas, Eje.Y, -780, 300, 700);
        c.Aparecer(E.Rebobinar, 0, 150);
        c.Hacia(E.IconoRebobinar, Eje.Giro, -720, 0, 900);
        c.Desaparecer(E.Rebobinar, 850, 200);
        c.Parpadear(E.Destello, 1, 0, 320, 0.35, 0);
        if (momento.Ayuda) c.Parpadear(E.LuzAyuda, 4, 600, 420, 1, 0);
    }

    /// <summary>El relevo ya aterrizó: la tripulación celebra, cae confeti y la ficha llega a la meta.</summary>
    private void Victoria(Coreografia c)
    {
        c.Fijar(E.Modulo5, Eje.Y, 0);
        c.Fijar(E.Llama5, Eje.Opacidad, 0);
        Posturas.Celebrar(c, E.Astro, 0);
        for (var i = 0; i < 4; i++)
            c.Emitir(E.Particulas, Particulas.Confeti, new Point(300 + i * 280, -20), new Vector(0, 640), 260, 12, i * 120, 2200, 90, giro: 540);
        c.Emitir(E.Particulas, Particulas.Estrella, new Point(670, 360), new Vector(0, -120), 360, 10, 300, 900, 40, giro: 180);
        Vista(c, escena: false, 3600, 600);
        Caminar(c, Presentacion.CasillaMeta - 1, Presentacion.CasillaMeta, 4300, 900);
    }

    /// <summary>Fallo amable, la cápsula despega hacia la Tierra y se vuelve al mapa.</summary>
    private void Abortada(Coreografia c, Momento momento)
    {
        Consecuencias.Fallos[momento.Medidor ?? Medidor.Soporte](c, E);
        c.Aparecer(E.Capsula, 1500, 200);
        c.Mover(E.Capsula, 310, -160, 1700, 1600, new QuadraticEase { EasingMode = EasingMode.EaseIn });
        c.Escalar(E.Capsula, 0.4, 1700, 1600);
        c.Emitir(E.Particulas, Particulas.Radiacion, new Point(1010, 410), new Vector(-20, 60), 40, 10, 1700, 600, 120);
        c.Desaparecer(E.Capsula, 3200, 200);
        Vista(c, escena: false, 3600, 600);
    }

    // ---------- Piezas ----------

    /// <summary>
    /// Cambia entre el mapa y la escena con un fundido. Los valores finales quedan fijos y el guion anima desde
    /// la vista anterior. Con <paramref name="zoomCasilla"/>, la cámara entra en esa estación del mapa.
    /// </summary>
    private void Vista(Coreografia c, bool escena, double inicio, double duracion, int? zoomCasilla = null)
    {
        var anterior = enEscena;
        enEscena = escena;
        FijarVista(escena);
        if (anterior == escena) return;
        UIElement entra = escena ? E : M;
        UIElement sale = escena ? M : E;
        c.Partir(entra, Eje.Opacidad, 0).Visibilidad(entra, Visibility.Visible, inicio).Hacia(entra, Eje.Opacidad, 1, inicio, duracion);
        c.Partir(sale, Eje.Opacidad, 1).Visibilidad(sale, Visibility.Visible, 0).Hacia(sale, Eje.Opacidad, 0, inicio, duracion)
            .Visibilidad(sale, Visibility.Hidden, inicio + duracion);
        if (zoomCasilla is not { } casilla) return;
        var centro = M.CentroDeCasilla(casilla);
        M.RenderTransformOrigin = M.ActualWidth > 0 && M.ActualHeight > 0
            ? new Point(centro.X / M.ActualWidth, centro.Y / M.ActualHeight)
            : new Point(0.5, 0.5);
        E.RenderTransformOrigin = new Point(0.5, 0.5);
        c.Partir(M, Eje.EscalaX, 1).Partir(M, Eje.EscalaY, 1).Escalar(M, 2.6, inicio, duracion);
        c.Partir(E, Eje.EscalaX, 1.3).Partir(E, Eje.EscalaY, 1.3).Escalar(E, 1, inicio, duracion);
    }

    private void FijarVista(bool escena)
    {
        M.Opacity = escena ? 0 : 1;
        M.Visibility = escena ? Visibility.Hidden : Visibility.Visible;
        E.Opacity = escena ? 1 : 0;
        E.Visibility = escena ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>La ficha camina de una casilla a otra del mapa, rebotando a cada paso.</summary>
    private void Caminar(Coreografia c, int desde, int hasta, double inicio, double duracion)
    {
        FijarCasilla(hasta);
        c.Partir(M.Ficha, Eje.X, MapaMision.AnchoCasilla * desde);
        c.Hacia(M.Ficha, Eje.X, MapaMision.AnchoCasilla * hasta, inicio, duracion, new SineEase { EasingMode = EasingMode.EaseInOut });
        if (duracion > 0) c.Oscilar(M.Ficha, Eje.Y, -9, 300, inicio, duracion);
    }

    private void FijarCasilla(int casilla) =>
        ((TranslateTransform)Coreografia.Transformaciones(M.Ficha).Children[2]).X = MapaMision.AnchoCasilla * casilla;

    /// <summary>Cambia los objetos del nivel en un instante (cuando la escena no se ve).</summary>
    private void CambiarCapas(Coreografia c, int nuevo, double tiempo)
    {
        var viejo = nivel;
        nivel = nuevo;
        E.MostrarNivel(nuevo);
        if (viejo == nuevo) return;
        foreach (var capa in E.CapasDe(viejo)) c.Visibilidad(capa, Visibility.Visible, 0).Visibilidad(capa, Visibility.Collapsed, tiempo);
        foreach (var capa in E.CapasDe(nuevo)) c.Visibilidad(capa, Visibility.Collapsed, 0).Visibilidad(capa, Visibility.Visible, tiempo);
    }

    private void IniciarAmbiente()
    {
        ambiente = Ambiente.Crear(E, nivel, animados);
        ambiente?.Begin(escenario, HandoffBehavior.SnapshotAndReplace, isControllable: true);
    }

    /// <summary>Quita el guion y el ambiente anteriores: todo vuelve a su estado de partida.</summary>
    private void Detener()
    {
        guion?.Remove(escenario);
        guion = null;
        ambiente?.Remove(escenario);
        ambiente = null;
        Coreografia.Quitar(animados);
        animados.Clear();
        E.Particulas.Children.Clear();
    }
}
