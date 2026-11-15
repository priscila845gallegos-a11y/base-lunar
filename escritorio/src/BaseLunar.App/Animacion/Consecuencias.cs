using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BaseLunar.App.Vistas;

namespace BaseLunar.App.Animacion;

/// <summary>
/// Lo que se ve tras cada elección (propuesta de F3, sección 4): una consecuencia por opción (10), una
/// variante amable de fallo por medidor (3) y las piezas de los demás momentos. Solo representan lo que el
/// motor ya decidió; la tripulación siempre está a salvo.
/// </summary>
internal static class Consecuencias
{
    private static readonly IEasingFunction Rebote = new BounceEase { Bounces = 2, Bounciness = 3, EasingMode = EasingMode.EaseOut };
    private static readonly IEasingFunction Elastico = new BackEase { Amplitude = 0.5, EasingMode = EasingMode.EaseOut };
    private static readonly IEasingFunction Frenada = new QuadraticEase { EasingMode = EasingMode.EaseOut };

    /// <summary>Las 10 consecuencias, por nivel y opción («1A» … «5B»).</summary>
    public static readonly IReadOnlyDictionary<string, Action<Coreografia, EscenaNivel>> Todas = new Dictionary<string, Action<Coreografia, EscenaNivel>>
    {
        ["1A"] = ApagarRecicladores,
        ["1B"] = MantenerSoporte,
        ["2A"] = RegarCultivos,
        ["2B"] = GuardarAguaParaOxigeno,
        ["3A"] = RodearConAgua,
        ["3B"] = CubrirConRegolito,
        ["4A"] = Refugiarse,
        ["4B"] = CargarBaterias,
        ["5A"] = EncenderBaliza,
        ["5B"] = GuiadoAutomatico,
    };

    /// <summary>Las variantes de fallo, por el medidor que lo explica.</summary>
    public static readonly IReadOnlyDictionary<Medidor, Action<Coreografia, EscenaNivel>> Fallos = new Dictionary<Medidor, Action<Coreografia, EscenaNivel>>
    {
        [Medidor.Bateria] = FalloBateria,
        [Medidor.Soporte] = FalloSoporte,
        [Medidor.Proteccion] = FalloProteccion,
    };

    // ---------- Nivel 1 · La Noche Lunar ----------

    /// <summary>N1-A: el astronauta cierra la válvula; baja el agua del depósito y la batería se carga.</summary>
    private static void ApagarRecicladores(Coreografia c, EscenaNivel e)
    {
        var a = e.Astro;
        Posturas.Caminar(c, a, -70, 0, 800);
        c.Hacia(a.BrazoIzq, Eje.Giro, 88, 800, 300);
        c.Hacia(e.Valvula1, Eje.Giro, 540, 1000, 1100);
        c.Hacia(e.Agua1, Eje.EscalaY, 0.45, 1200, 1200);
        c.Hacia(e.Carga1, Eje.EscalaY, 1, 1400, 1000);
        c.Parpadear(e.ChispaBateria1, 2, 1700, 420);
        c.Hacia(e.Calor1, Eje.Opacidad, 0.7, 1900, 800);
        c.Hacia(a.BrazoIzq, Eje.Giro, 0, 2300, 300);
    }

    /// <summary>N1-B: suben burbujas de oxígeno, los calentadores brillan y la batería se vacía.</summary>
    private static void MantenerSoporte(Coreografia c, EscenaNivel e)
    {
        var a = e.Astro;
        c.Emitir(e.Particulas, Particulas.Burbuja, new Point(480, 412), new Vector(0, -190), 70, 10, 200, 1700, 130);
        c.Hacia(e.Calor1, Eje.Opacidad, 1, 600, 900);
        c.Hacia(e.Carga1, Eje.EscalaY, 0.4, 900, 1500);
        Posturas.Brazos(c, a, 35, -35, 300);
        Posturas.Brazos(c, a, 0, 0, 2400);
    }

    // ---------- Nivel 2 · El Huerto Espacial ----------

    /// <summary>N2-A: el astronauta riega; las plantas crecen y dan frutos, y baja el agua del depósito.</summary>
    private static void RegarCultivos(Coreografia c, EscenaNivel e)
    {
        var a = e.Astro;
        Posturas.Caminar(c, a, -50, 0, 600);
        c.Aparecer(a.Regadera, 500, 200);
        c.Hacia(a.BrazoIzq, Eje.Giro, 72, 600, 300);
        c.Emitir(e.Particulas, Particulas.Gota, new Point(530, 476), new Vector(-38, 72), 34, 14, 900, 650, 70);
        UIElement[] plantas = [e.Planta1, e.Planta2, e.Planta3];
        UIElement[] frutos = [e.Fruto1, e.Fruto2, e.Fruto3];
        for (var i = 0; i < plantas.Length; i++)
        {
            c.Escalar(plantas[i], 1.15, 1200 + i * 150, 800, Elastico);
            c.Escalar(frutos[i], 1, 1900 + i * 160, 420, Elastico);
        }
        c.Hacia(e.Agua2, Eje.EscalaY, 0.45, 1200, 1200);
        c.Desaparecer(a.Regadera, 2500, 200);
        c.Hacia(a.BrazoIzq, Eje.Giro, 0, 2500, 300);
    }

    /// <summary>N2-B: el agua entra en la máquina, que suelta burbujas de oxígeno.</summary>
    private static void GuardarAguaParaOxigeno(Coreografia c, EscenaNivel e)
    {
        c.Emitir(e.Particulas, Particulas.Flujo, new Point(806, 545), new Vector(16, 0), 2, 6, 200, 450, 140);
        c.Hacia(e.Agua2, Eje.EscalaY, 0.55, 300, 1500);
        c.Aparecer(e.LuzMaquina2, 700, 300);
        c.Emitir(e.Particulas, Particulas.Burbuja, new Point(836, 488), new Vector(0, -180), 70, 12, 900, 1600, 110);
        Posturas.Saludar(c, e.Astro, 900, 1600);
    }

    // ---------- Nivel 3 · Rayos Invisibles ----------

    /// <summary>N3-A: el agua llena las paredes y aparece un escudo azul; los rayos rebotan en él.</summary>
    private static void RodearConAgua(Coreografia c, EscenaNivel e)
    {
        c.Hacia(e.AguaParedes, Eje.EscalaY, 1, 200, 1300);
        c.Aparecer(e.Escudo3, 1200, 600);
        c.Desaparecer(e.Rayos3, 1500, 600);
        c.Emitir(e.Particulas, Particulas.Chispa, new Point(1010, 352), new Vector(0, -70), 160, 12, 1700, 700, 70);
        Posturas.Brazos(c, e.Astro, 30, -150, 1500);
        Posturas.Brazos(c, e.Astro, 0, 0, 2600);
    }

    /// <summary>N3-B: los robots excavadores cubren el módulo con polvo lunar.</summary>
    private static void CubrirConRegolito(Coreografia c, EscenaNivel e)
    {
        c.Hacia(e.Robot1, Eje.X, 240, 0, 1500, Frenada);
        c.Hacia(e.Robot2, Eje.X, 230, 200, 1500, Frenada);
        c.Oscilar(e.Robot1, Eje.Y, -3, 200, 0, 1500);
        c.Oscilar(e.Robot2, Eje.Y, -3, 200, 200, 1500);
        c.Emitir(e.Particulas, Particulas.Polvo, new Point(800, 548), new Vector(150, -40), 90, 16, 700, 900, 55);
        c.Hacia(e.Monticulo, Eje.EscalaY, 1, 1300, 1300, Frenada);
    }

    // ---------- Nivel 4 · Tormenta Solar ----------

    /// <summary>N4-A: el astronauta se protege entre las bolsas de provisiones y las partículas rebotan.</summary>
    private static void Refugiarse(Coreografia c, EscenaNivel e)
    {
        var a = e.Astro;
        Posturas.Caminar(c, a, -150, 0, 900);
        UIElement[] bolsas = [e.Bolsa1, e.Bolsa2, e.Bolsa3, e.Bolsa4, e.Bolsa5];
        for (var i = 0; i < bolsas.Length; i++)
        {
            c.Fijar(bolsas[i], Eje.Y, -90);
            c.Aparecer(bolsas[i], 700 + i * 110, 150);
            c.Hacia(bolsas[i], Eje.Y, 0, 700 + i * 110, 420, Rebote);
        }
        Posturas.Agacharse(c, a, 1300);
        Rebotar(c, e, new Point(330, 230), new Point(480, 460), 1600, 6);
    }

    /// <summary>N4-B: los paneles cargan la batería y el blindaje de la base recibe golpes.</summary>
    private static void CargarBaterias(Coreografia c, EscenaNivel e)
    {
        c.Parpadear(e.BrilloPanel4, 3, 200, 480, 1, 0.35);
        c.Hacia(e.BrilloPanel4, Eje.Opacidad, 0.8, 1640, 300);
        c.Hacia(e.Carga4, Eje.EscalaY, 1, 500, 1200);
        c.Emitir(e.Particulas, Particulas.Radiacion, new Point(380, 240), new Vector(620, 180), 50, 8, 700, 800, 120);
        c.Parpadear(e.DestelloCupula, 3, 1400, 360, 0.85, 0);
        Posturas.Brazos(c, e.Astro, 0, -125, 900);
        Posturas.Brazos(c, e.Astro, 0, 0, 2700);
    }

    // ---------- Nivel 5 · El Relevo Final ----------

    /// <summary>N5-A: la baliza brilla y el módulo sigue la luz hasta aterrizar.</summary>
    private static void EncenderBaliza(Coreografia c, EscenaNivel e)
    {
        c.Aparecer(e.LuzBaliza, 200, 300);
        c.Aparecer(e.HazBaliza, 300, 400);
        c.Parpadear(e.LuzBaliza, 3, 600, 520, 1, 0.55);
        c.Hacia(e.LuzBaliza, Eje.Opacidad, 1, 2160, 200);
        c.Hacia(e.Modulo5, Eje.X, 50, 600, 600);
        c.Hacia(e.Modulo5, Eje.X, -30, 1200, 600);
        c.Hacia(e.Modulo5, Eje.X, 0, 1800, 500);
        c.Hacia(e.Modulo5, Eje.Y, 0, 600, 1900, Frenada);
        Aterrizar(c, e, 2500);
        Posturas.Saludar(c, e.Astro, 2400, 1000);
    }

    /// <summary>N5-B: la pantalla dibuja líneas de guía y el módulo baja recto.</summary>
    private static void GuiadoAutomatico(Coreografia c, EscenaNivel e)
    {
        c.Desaparecer(e.Radio5, 0, 200);
        c.Aparecer(e.Guia5, 100, 300);
        c.Aparecer(e.LineasGuia5, 400, 300);
        c.Propiedad(e.LineasGuia5, Shape.StrokeDashOffsetProperty, 0, -30, 400, 2000);
        c.Hacia(e.Modulo5, Eje.Y, 0, 700, 1800, Frenada);
        Aterrizar(c, e, 2500);
        Posturas.Brazos(c, e.Astro, 0, -95, 300);
        Posturas.Brazos(c, e.Astro, 0, 0, 2500);
    }

    /// <summary>Polvo al tocar el suelo y la llama del motor que se apaga.</summary>
    private static void Aterrizar(Coreografia c, EscenaNivel e, double tiempo)
    {
        c.Parpadear(e.Llama5, 6, tiempo - 1800, 300, 1, 0.4);
        c.Desaparecer(e.Llama5, tiempo, 200);
        c.Emitir(e.Particulas, Particulas.Polvo, new Point(490, 552), new Vector(0, -34), 170, 12, tiempo, 700, 15);
    }

    // ---------- Fallos (variantes por medidor, siempre con la tripulación a salvo) ----------

    /// <summary>⚡: se apagan las luces, aparece escarcha y el astronauta tirita.</summary>
    private static void FalloBateria(Coreografia c, EscenaNivel e)
    {
        var a = e.Astro;
        c.Aparecer(e.VentanasApagadas, 100, 300);
        c.Desaparecer(e.Calor1, 100, 300);
        c.Aparecer(e.EscarchaBase, 400, 600);
        c.Aparecer(a.EscarchaVisor, 500, 500);
        Posturas.Brazos(c, a, -25, 25, 300);
        Posturas.Tiritar(c, a, 400, 1000);
    }

    /// <summary>💧: el agua se acaba: la gota se vacía y las burbujas se detienen.</summary>
    private static void FalloSoporte(Coreografia c, EscenaNivel e)
    {
        var a = e.Astro;
        c.Aparecer(e.GotaFallo, 100, 300);
        c.Hacia(e.GotaLlena, Eje.EscalaY, 0, 400, 1100);
        c.Hacia(e.Agua1, Eje.EscalaY, 0.04, 300, 1100);
        c.Hacia(e.Agua2, Eje.EscalaY, 0.04, 300, 1100);
        c.Hacia(a.Cabeza, Eje.Giro, 12, 300, 300);
        c.Hacia(a.Cabeza, Eje.Giro, 0, 1400, 300);
    }

    /// <summary>🛡️: todos corren al refugio: el astronauta entra en la base.</summary>
    private static void FalloProteccion(Coreografia c, EscenaNivel e)
    {
        var a = e.Astro;
        Posturas.Caminar(c, a, 460, 0, 1100);
        c.Parpadear(e.Puerta, 2, 900, 300, 1, 0.5);
        c.Desaparecer(a, 1100, 300);
    }

    /// <summary>El salvavidas del Protocolo de Respaldo baja balanceándose.</summary>
    public static void BajarSalvavidas(Coreografia c, EscenaNivel e, double inicio)
    {
        c.Hacia(e.Salvavidas, Eje.Y, 0, inicio, 1100, Rebote);
        c.Oscilar(e.Salvavidas, Eje.Giro, 5, 900, inicio + 1100, 1800);
    }

    /// <summary>Partículas de radiación que chocan contra un punto y rebotan hacia atrás.</summary>
    private static void Rebotar(Coreografia c, EscenaNivel e, Point origen, Point impacto, double inicio, int cantidad)
    {
        if (c.Reducido) return;
        for (var i = 0; i < cantidad; i++)
        {
            var particula = Particulas.Radiacion(i);
            particula.Opacity = 0;
            System.Windows.Controls.Canvas.SetLeft(particula, origen.X);
            System.Windows.Controls.Canvas.SetTop(particula, origen.Y);
            e.Particulas.Children.Add(particula);
            var t = inicio + i * 160;
            var llegada = new Vector(impacto.X - origen.X + c.Azar(-20, 20), impacto.Y - origen.Y + c.Azar(-30, 30));
            c.Aparecer(particula, t, 100);
            c.Mover(particula, llegada.X, llegada.Y, t, 500);
            c.Mover(particula, llegada.X - c.Azar(80, 140), llegada.Y - c.Azar(60, 120), t + 500, 450, Frenada);
            c.Desaparecer(particula, t + 750, 200);
        }
    }
}
