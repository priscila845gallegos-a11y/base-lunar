using System.Windows;
using System.Windows.Media;
using BaseLunar.App.Animacion;
using BaseLunar.App.Controles;
using BaseLunar.App.VistaModelos;
using BaseLunar.App.Vistas;

namespace BaseLunar.PruebasUI;

/// <summary>
/// F3.3: la escena reacciona a cada elección. Se comprueba qué momento sale de cada acción del motor, que
/// existen las 10 consecuencias y las 3 variantes de fallo, y cómo queda la escena (capturas del estado
/// final y fotogramas a mitad de la animación).
/// </summary>
[TestClass]
public sealed class PruebasEscena
{
    // Rutas a los fallos de cada medidor (comprobadas con el motor en MomentosDeCadaAccion).
    private static readonly string[] HastaFalloBateria = ["P", "B", "P", "A", "P", "B"];
    private static readonly string[] HastaFalloProteccion = ["P", "A", "P", "B", "P", "A", "P", "B", "P", "B"];

    // ---------- Momentos (funciones puras) ----------

    [TestMethod(DisplayName = "Cada acción produce su momento: entrar, consecuencia, fallo (con su medidor), reintento, victoria y abortada")]
    public void MomentosDeCadaAccion() => HiloUI.Ejecutar(() =>
    {
        Assert.AreEqual(new Momento(TipoMomento.EntrarNivel, 1, Desde: 0), Partida.Jugar(Partida.HastaNivel1).UltimoMomento);
        Assert.AreEqual(new Momento(TipoMomento.Consecuencia, 1, "A"), Partida.Jugar(Partida.HastaResultado1).UltimoMomento);
        Assert.AreEqual(new Momento(TipoMomento.EntrarNivel, 4, ClaseX: true, Desde: 3), Partida.Jugar(Partida.HastaNivel4).UltimoMomento);
        Assert.AreEqual(new Momento(TipoMomento.Fallo, 4, "A", Medidor.Soporte), Partida.Jugar(Partida.HastaRespaldo4).UltimoMomento);
        Assert.AreEqual(new Momento(TipoMomento.Fallo, 3, "B", Medidor.Bateria), Partida.Jugar(HastaFalloBateria).UltimoMomento);
        Assert.AreEqual(new Momento(TipoMomento.Fallo, 5, "B", Medidor.Proteccion), Partida.Jugar(HastaFalloProteccion).UltimoMomento,
            "En el Nivel 5 el fallo lo explica el medidor del requisito");
        Assert.AreEqual(new Momento(TipoMomento.Reintento, 4), Partida.Jugar([.. Partida.HastaRespaldo4, "P"]).UltimoMomento);
        Assert.AreEqual(new Momento(TipoMomento.Victoria, 5, Desde: 5), Partida.Jugar(Partida.HastaVictoria).UltimoMomento);
        Assert.AreEqual(new Momento(TipoMomento.Abortada, 4, "A", Medidor.Soporte), Partida.Jugar(Partida.HastaAbortada).UltimoMomento);
        Assert.AreEqual(new Momento(TipoMomento.Inicio, 0), Partida.Jugar([.. Partida.HastaAbortada, "P"]).UltimoMomento);
    });

    [TestMethod(DisplayName = "Hay una consecuencia para cada una de las 10 opciones y una variante de fallo para cada medidor")]
    public void CatalogoCompleto()
    {
        var opciones = Guion.Niveles.SelectMany(n => n.Opciones.Keys.Select(o => $"{n.Id}{o}")).ToArray();
        Assert.HasCount(10, opciones);
        CollectionAssert.AreEquivalent(opciones, Consecuencias.Todas.Keys.ToArray());
        CollectionAssert.AreEquivalent(Medidores.Todos.ToArray(), Consecuencias.Fallos.Keys.ToArray());
    }

    // ---------- La escena montada ----------

    public static IEnumerable<object[]> Consecuencia() =>
        Guion.Niveles.SelectMany(n => n.Opciones.Keys.Select(o => new object[] { n.Id, o }));

    [TestMethod(DisplayName = "Las 10 consecuencias: estado final y fotograma a mitad de la animación")]
    [DynamicData(nameof(Consecuencia))]
    public void Consecuencias10(int nivel, string opcion) => HiloUI.Ejecutar(() =>
    {
        var pasos = RutaHastaNivel(nivel);
        var (pantalla, juego) = Montar(pasos);
        Animar(() => Partida.Paso(juego, opcion));
        var director = pantalla.Escenario.Director;
        Assert.AreEqual(new Momento(TipoMomento.Consecuencia, nivel, opcion), director.Ultimo);
        director.Saltar(director.Duracion * 0.5);
        Capturas.Guardar(pantalla, $"f33-{nivel}{opcion}-medio");
        director.Terminar();
        Capturas.Guardar(pantalla, $"f33-{nivel}{opcion}-final");
        ComprobarFinal(pantalla.Escenario.Escena, $"{nivel}{opcion}");
    });

    [TestMethod(DisplayName = "Fallos amables: ⚡ apaga luces y hiela, 💧 vacía la gota, 🛡️ lleva al refugio; baja el salvavidas")]
    public void Fallos() => HiloUI.Ejecutar(() =>
    {
        foreach (var (nombre, pasos) in new[] { ("bateria", HastaFalloBateria), ("soporte", Partida.HastaRespaldo4), ("proteccion", HastaFalloProteccion) })
        {
            var (pantalla, _) = Montar(pasos[..^1]);
            var juego = (JuegoVM)pantalla.DataContext;
            Animar(() => Partida.Paso(juego, pasos[^1]));
            var escena = pantalla.Escenario.Escena;
            pantalla.Escenario.Director.Saltar(1200);
            Capturas.Guardar(pantalla, $"f33-fallo-{nombre}-medio");
            pantalla.Escenario.Director.Terminar();
            Capturas.Guardar(pantalla, $"f33-fallo-{nombre}-final");
            Assert.AreEqual(0, Traslado(escena.Salvavidas).Y, 0.5, $"{nombre}: el salvavidas bajó");
            switch (nombre)
            {
                case "bateria": Assert.AreEqual(1, escena.VentanasApagadas.Opacity, 0.01); break;
                case "soporte": Assert.AreEqual(0, Escala(escena.GotaLlena).ScaleY, 0.01); break;
                default: Assert.AreEqual(0, escena.Astro.Opacity, 0.01, "La tripulación entró en la base"); break;
            }
        }
    });

    [TestMethod(DisplayName = "Del mapa a la escena: la ficha camina hasta la estación y la cámara entra")]
    public void EntrarEnElNivel() => HiloUI.Ejecutar(() =>
    {
        var (pantalla, juego) = Montar([]);
        Animar(() => Partida.Paso(juego, "P"));
        var director = pantalla.Escenario.Director;
        var mapa = pantalla.Escenario.Mapa;
        director.Saltar(600);
        Capturas.Guardar(pantalla, "f33-entrar-camina");
        Assert.IsGreaterThan(0.0, Traslado(mapa.Ficha).X, "La ficha ya salió de la base");
        Assert.AreEqual(Visibility.Visible, mapa.Visibility);
        director.Saltar(director.Duracion - 1300);
        Capturas.Guardar(pantalla, "f33-entrar-zoom");
        director.Terminar();
        Capturas.Guardar(pantalla, "f33-entrar-final");
        Assert.AreEqual(MapaMision.AnchoCasilla, Traslado(mapa.Ficha).X, 0.5, "La ficha está en la estación 1");
        Assert.AreEqual(Visibility.Hidden, mapa.Visibility);
        Assert.AreEqual(1, pantalla.Escenario.Escena.Opacity, 0.01);
    });

    [TestMethod(DisplayName = "Victoria con confeti y la ficha en la meta; Misión Abortada con la cápsula y vuelta al mapa")]
    public void VictoriaYAbortada() => HiloUI.Ejecutar(() =>
    {
        var (victoria, juegoV) = Montar(Partida.HastaVictoria[..^1]);
        Animar(() => Partida.Paso(juegoV, "P"));
        victoria.Escenario.Director.Saltar(1500);
        Capturas.Guardar(victoria, "f33-victoria-medio");
        victoria.Escenario.Director.Terminar();
        Capturas.Guardar(victoria, "f33-victoria-final");
        Assert.AreEqual(MapaMision.AnchoCasilla * Presentacion.CasillaMeta, Traslado(victoria.Escenario.Mapa.Ficha).X, 0.5);
        Assert.AreEqual(0, victoria.Escenario.Escena.HazBaliza.Opacity, 0.01, "Cada momento nuevo quita el guion anterior");
        Assert.AreEqual(Visibility.Visible, victoria.Escenario.Mapa.Visibility);

        var (abortada, juegoA) = Montar(Partida.HastaAbortada[..^1]);
        Animar(() => Partida.Paso(juegoA, "A"));
        abortada.Escenario.Director.Saltar(2500);
        Capturas.Guardar(abortada, "f33-abortada-capsula");
        abortada.Escenario.Director.Terminar();
        Assert.AreEqual(Visibility.Visible, abortada.Escenario.Mapa.Visibility);
        Assert.AreEqual(MapaMision.AnchoCasilla * 4, Traslado(abortada.Escenario.Mapa.Ficha).X, 0.5, "La ficha se queda en la estación 4");
    });

    [TestMethod(DisplayName = "Movimiento reducido (D-43): sin desplazamientos; el estado final se ve al instante")]
    public void MovimientoReducido() => HiloUI.Ejecutar(() =>
    {
        var (pantalla, juego) = Montar(RutaHastaNivel(1));
        Partida.Paso(juego, "A"); // Movimiento.Forzado está activo en el hilo de pruebas
        var escena = pantalla.Escenario.Escena;
        pantalla.Escenario.Director.Saltar(1);
        Assert.AreEqual(0.45, Escala(escena.Agua1).ScaleY, 0.01, "El agua ya bajó, sin animación");
        Assert.AreEqual(1, Escala(escena.Carga1).ScaleY, 0.01);
        Assert.IsEmpty(escena.Particulas.Children, "Sin partículas");
    });

    // ---------- Ayudantes ----------

    /// <summary>La ruta de referencia hasta entrar en un nivel (eligiendo siempre lo que lleva a la victoria).</summary>
    private static string[] RutaHastaNivel(int nivel)
    {
        string[] decisiones = ["A", "B", "A", "A"];
        var pasos = new List<string> { "P" };
        for (var i = 0; i < nivel - 1; i++) pasos.AddRange([decisiones[i], "P"]);
        return [.. pasos];
    }

    /// <summary>Monta la pantalla a 1280×720 y juega los pasos con la escena ya conectada.</summary>
    private static (PantallaJuego Pantalla, JuegoVM Juego) Montar(string[] pasos)
    {
        var juego = new JuegoVM();
        var pantalla = Capturas.Montar(juego, 1280, 720);
        foreach (var paso in pasos) Partida.Paso(juego, paso);
        pantalla.Escenario.Director.Terminar();
        pantalla.UpdateLayout();
        return (pantalla, juego);
    }

    /// <summary>Ejecuta la acción con animaciones completas (sin movimiento reducido), para poder capturar fotogramas.</summary>
    private static void Animar(Action accion)
    {
        Movimiento.Forzado = false;
        try
        {
            accion();
        }
        finally
        {
            Movimiento.Forzado = true;
        }
    }

    /// <summary>Lo esencial de cada consecuencia, al terminar.</summary>
    private static void ComprobarFinal(EscenaNivel e, string clave)
    {
        switch (clave)
        {
            case "1A": Assert.AreEqual(0.45, Escala(e.Agua1).ScaleY, 0.01); Assert.AreEqual(1, Escala(e.Carga1).ScaleY, 0.01); break;
            case "1B": Assert.AreEqual(0.4, Escala(e.Carga1).ScaleY, 0.01); Assert.AreEqual(1, e.Calor1.Opacity, 0.01); break;
            case "2A": Assert.AreEqual(1, Escala(e.Fruto2).ScaleX, 0.01); Assert.AreEqual(1.15, Escala(e.Planta2).ScaleY, 0.01); break;
            case "2B": Assert.AreEqual(1, e.LuzMaquina2.Opacity, 0.01); break;
            case "3A": Assert.AreEqual(1, Escala(e.AguaParedes).ScaleY, 0.01); Assert.AreEqual(1, e.Escudo3.Opacity, 0.01); break;
            case "3B": Assert.AreEqual(1, Escala(e.Monticulo).ScaleY, 0.01); Assert.AreEqual(240, Traslado(e.Robot1).X, 0.5); break;
            case "4A": Assert.AreEqual(0.84, Escala(e.Astro.Cuerpo).ScaleY, 0.01); Assert.AreEqual(1, e.Bolsa3.Opacity, 0.01); break;
            case "4B": Assert.AreEqual(1, Escala(e.Carga4).ScaleY, 0.01); break;
            default: Assert.AreEqual(0, Traslado(e.Modulo5).Y, 0.5, "El módulo aterrizó"); break;
        }
    }

    private static ScaleTransform Escala(UIElement e) => (ScaleTransform)Coreografia.Transformaciones(e).Children[0];

    private static TranslateTransform Traslado(UIElement e) => (TranslateTransform)Coreografia.Transformaciones(e).Children[2];
}
