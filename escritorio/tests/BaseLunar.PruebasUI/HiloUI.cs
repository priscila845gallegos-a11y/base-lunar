using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BaseLunar.App;
using BaseLunar.App.Controles;
using BaseLunar.App.VistaModelos;
using BaseLunar.App.Vistas;

namespace BaseLunar.PruebasUI;

/// <summary>
/// Un único hilo STA con la aplicación y sus recursos (paleta, íconos, estilos). WPF exige que cada objeto
/// se use en el hilo que lo creó, así que todas las pruebas de pantalla se ejecutan aquí.
/// </summary>
internal static class HiloUI
{
    private static readonly Lazy<Dispatcher> Despachador = new(Iniciar, LazyThreadSafetyMode.ExecutionAndPublication);

    private static Dispatcher Iniciar()
    {
        var listo = new TaskCompletionSource<Dispatcher>(TaskCreationOptions.RunContinuationsAsynchronously);
        var hilo = new Thread(() =>
        {
            try
            {
                var aplicacion = new BaseLunar.App.App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                aplicacion.InitializeComponent();
                Movimiento.Forzado = true; // capturas deterministas: sin animaciones
                listo.SetResult(Dispatcher.CurrentDispatcher);
            }
            catch (Exception error)
            {
                listo.SetException(error);
                return;
            }
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "Interfaz de las pruebas",
        };
        hilo.SetApartmentState(ApartmentState.STA);
        hilo.Start();
        return listo.Task.GetAwaiter().GetResult();
    }

    public static void Ejecutar(Action accion) => Despachador.Value.Invoke(accion);

    public static T Ejecutar<T>(Func<T> funcion) => Despachador.Value.Invoke(funcion);

    /// <summary>Procesa lo pendiente en la cola (enlaces de datos, diseño) hasta quedar en reposo.</summary>
    public static void Vaciar() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);

    /// <summary>Deja correr el hilo (temporizadores incluidos) hasta que se cumpla la condición.</summary>
    public static bool Esperar(Func<bool> condicion, int milisegundos = 3000)
    {
        var limite = Environment.TickCount64 + milisegundos;
        while (!condicion())
        {
            if (Environment.TickCount64 > limite) return false;
            var marco = new DispatcherFrame();
            var reloj = new DispatcherTimer(TimeSpan.FromMilliseconds(15), DispatcherPriority.Background, (_, _) => marco.Continue = false, Dispatcher.CurrentDispatcher);
            reloj.Start();
            Dispatcher.PushFrame(marco);
            reloj.Stop();
        }
        return true;
    }
}

/// <summary>Partidas guiadas por el modelo de vista, sin esperar las transiciones.</summary>
internal static class Partida
{
    /// <summary>Pasos: «P» es el botón principal; «A» y «B», las opciones.</summary>
    public static JuegoVM Jugar(params string[] pasos)
    {
        var juego = new JuegoVM();
        foreach (var paso in pasos) Paso(juego, paso);
        return juego;
    }

    public static void Paso(JuegoVM juego, string paso)
    {
        if (paso == "P") juego.AccionPrincipal();
        else juego.ElegirOpcion(paso);
        juego.TerminarTransicion();
    }

    // Rutas de referencia (calculadas a mano y con el simulador: ver PruebasJuegoVM).
    public static readonly string[] HastaNivel1 = ["P"];
    public static readonly string[] HastaResultado1 = ["P", "A"];
    public static readonly string[] HastaNivel4 = ["P", "A", "P", "A", "P", "A", "P"];
    public static readonly string[] HastaRespaldo4 = ["P", "A", "P", "A", "P", "A", "P", "A"];
    public static readonly string[] HastaNivel5 = ["P", "A", "P", "B", "P", "A", "P", "A", "P"];
    public static readonly string[] HastaVictoria = ["P", "A", "P", "B", "P", "A", "P", "A", "P", "A", "P"];
    public static readonly string[] HastaAbortada = ["P", "A", "P", "A", "P", "A", "P", "A", "P", "A", "P", "A"];
}

/// <summary>Monta la pantalla del juego a un tamaño exacto y la guarda como PNG.</summary>
internal static class Capturas
{
    public static string Carpeta { get; } = CrearCarpeta();

    private static string CrearCarpeta()
    {
        var carpeta = Path.Combine(Rutas.Escritorio(), "capturas");
        Directory.CreateDirectory(carpeta);
        return carpeta;
    }

    public static PantallaJuego Montar(JuegoVM juego, double ancho, double alto)
    {
        var pantalla = new PantallaJuego { DataContext = juego, Width = ancho, Height = alto };
        pantalla.Disponer(ancho);
        for (var i = 0; i < 2; i++)
        {
            pantalla.Measure(new Size(ancho, alto));
            pantalla.Arrange(new Rect(0, 0, ancho, alto));
            pantalla.UpdateLayout();
            HiloUI.Vaciar();
        }
        return pantalla;
    }

    public static string Guardar(FrameworkElement elemento, string nombre)
    {
        elemento.UpdateLayout();
        var mapa = new RenderTargetBitmap((int)Math.Ceiling(elemento.ActualWidth), (int)Math.Ceiling(elemento.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        mapa.Render(elemento);
        var codificador = new PngBitmapEncoder();
        codificador.Frames.Add(BitmapFrame.Create(mapa));
        var ruta = Path.Combine(Carpeta, nombre + ".png");
        using var archivo = File.Create(ruta);
        codificador.Save(archivo);
        return ruta;
    }
}

/// <summary>Rutas del repositorio, buscadas desde la carpeta de las pruebas.</summary>
internal static class Rutas
{
    public static string Raiz()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null && !File.Exists(Path.Combine(carpeta.FullName, "index.html"))) carpeta = carpeta.Parent;
        return carpeta?.FullName ?? throw new DirectoryNotFoundException("No se encontró la raíz del repositorio (index.html).");
    }

    public static string Escritorio() => Path.Combine(Raiz(), "escritorio");

    /// <summary>El ejecutable del juego: junto a las pruebas o en la salida del proyecto de la aplicación.</summary>
    public static string Ejecutable()
    {
        var junto = Path.Combine(AppContext.BaseDirectory, "BaseLunar.App.exe");
        if (File.Exists(junto)) return junto;
        var configuracion = AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}") ? "Release" : "Debug";
        var salida = Path.Combine(Escritorio(), "src", "BaseLunar.App", "bin", configuracion, "net10.0-windows", "BaseLunar.App.exe");
        return File.Exists(salida) ? salida : throw new FileNotFoundException("No se encontró BaseLunar.App.exe", salida);
    }
}
