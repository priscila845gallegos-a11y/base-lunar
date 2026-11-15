using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using BaseLunar.App;
using BaseLunar.App.Audio;
using BaseLunar.App.Controles;
using BaseLunar.App.Temas;
using BaseLunar.App.VistaModelos;
using BaseLunar.App.Vistas;
using BaseLunar.Nucleo;

namespace BaseLunar.PruebasUI;

/// <summary>
/// F5 · Validación: evidencias de los criterios de aceptación de la sección 10 que no cubrían las pruebas
/// anteriores (ver docs/VALIDACION.md): duración de una partida, temblor o su alternativa, cero errores,
/// privacidad, fuentes enlazadas y ventana que cabe con el zoom de Windows.
/// </summary>
[TestClass]
public sealed class PruebasValidacion
{
    private static readonly string[] RutaVictoria = Partida.HastaVictoria;

    // La primera partida (en orden A antes que B) que pierde exactamente 1 respaldo y llega a la Victoria.
    private static readonly string[] RutaConPerdidas = BuscarRuta(Configuracion.Reglas.RespaldosIniciales - 1)
        ?? throw new InvalidOperationException("No hay ruta que gane perdiendo 1 respaldo");

    /// <summary>Busca con el motor una ruta (en pasos de <see cref="Partida"/>) que gane con esos respaldos.</summary>
    private static string[]? BuscarRuta(int respaldos)
    {
        var contexto = new ContextoPartida();
        return Buscar(Motor.Empezar(Motor.CrearEstadoInicial(), contexto).Estado, ["P"]);

        string[]? Buscar(Estado estado, List<string> pasos)
        {
            switch (estado.Fase)
            {
                case Fase.Victoria:
                    return estado.Respaldos == respaldos ? [.. pasos] : null;
                case Fase.MisionAbortada:
                    return null;
                case Fase.Resultado:
                    return Buscar(Motor.Continuar(estado, contexto).Estado, [.. pasos, "P"]);
                case Fase.Respaldo:
                    return Buscar(Motor.Reintentar(estado).Estado, [.. pasos, "P"]);
            }
            foreach (var opcion in new[] { "A", "B" })
            {
                if (Buscar(Motor.AplicarDecision(estado, opcion).Estado, [.. pasos, opcion]) is { } ruta) return ruta;
            }
            return null;
        }
    }

    // ---------- Duración (3 a 5 minutos) ----------

    [TestMethod(DisplayName = "Duración: una partida leída en voz alta con sus animaciones cabe en 3 a 5 minutos (informe)")]
    public void Duracion() => HiloUI.Ejecutar(() =>
    {
        using var narrador = new NarradorWindows();
        if (!narrador.Disponible)
        {
            Assert.Inconclusive("Este equipo no tiene voces en español: no se puede medir la lectura en voz alta.");
            return;
        }
        var informe = new StringBuilder();
        var resultados = new Dictionary<string, double>();
        foreach (var (nombre, ruta) in new[] { ("Victoria sin perder respaldos", RutaVictoria), ("Victoria con 1 respaldo perdido", RutaConPerdidas), ("Misión Abortada", Partida.HastaAbortada) })
        {
            var juego = new JuegoVM();
            var pantalla = Capturas.Montar(juego, 1280, 720);
            var director = pantalla.Escenario.Director;
            double lectura = 0, animacion = 0, total = 0;
            var pantallas = 0;
            var animacionDeLaPantalla = 0.0;
            for (var i = 0; ; i++)
            {
                // Cada pantalla dura lo que tarde más: leerla en voz alta o ver su animación.
                var voz = SegundosDeVoz(narrador, juego.TextoParaEscuchar());
                lectura += voz;
                animacion += animacionDeLaPantalla;
                total += Math.Max(voz, animacionDeLaPantalla) + Configuracion.Tiempos.TransicionMs / 1000.0;
                pantallas++;
                if (i == ruta.Length) break;
                var antes = director.Ultimo;
                Animar(() => Partida.Paso(juego, ruta[i]));
                animacionDeLaPantalla = ReferenceEquals(antes, director.Ultimo) ? 0 : director.Duracion / 1000;
                director.Terminar();
            }
            Assert.IsTrue(juego.EsVictoria || juego.EsAbortada, $"{nombre}: la ruta termina la partida");
            resultados[nombre] = total;
            informe.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{nombre,-34} {pantallas,2} pantallas · voz {lectura,5:0} s · animaciones {animacion,4:0} s · total {total,5:0} s ({total / 60:0.0} min)"));
        }
        var texto = $"Voz «{narrador.NombreVoz}», sin contar el tiempo de pensar cada decisión.{Environment.NewLine}{informe}";
        File.WriteAllText(Path.Combine(Capturas.Carpeta, "f5-duracion.txt"), texto);
        Console.WriteLine(texto);
        foreach (var (nombre, segundos) in resultados)
            Assert.IsLessThanOrEqualTo(300.0, segundos, $"{nombre}: {segundos:0} s sin pensar ya pasa de 5 minutos");
        Assert.IsGreaterThanOrEqualTo(150.0, resultados["Victoria sin perder respaldos"], "Una partida sin pensar ya ronda los 3 minutos");
    });

    private static double SegundosDeVoz(NarradorWindows narrador, string texto) =>
        texto.Length == 0 ? 0 : AnalisisAudio.Muestras(narrador.Grabar(texto)!).Length / (double)Sintetizador.Muestreo;

    // ---------- Temblor o su alternativa ----------

    [TestMethod(DisplayName = "Al perder un respaldo la tarjeta tiembla; con movimiento reducido, un destello sin desplazamiento (como en la web)")]
    public void TemblorOAlternativa() => HiloUI.Ejecutar(() =>
    {
        var conMovimiento = Capturas.Montar(new JuegoVM(), 1280, 720);
        var juego = (JuegoVM)conMovimiento.DataContext;
        foreach (var paso in Partida.HastaNivel4) Partida.Paso(juego, paso);
        Animar(() => Partida.Paso(juego, "A"));
        Assert.IsTrue(juego.EsRespaldo);
        var marco = conMovimiento.Tarjeta.Marco;
        Assert.IsInstanceOfType<TranslateTransform>(marco.RenderTransform);
        Assert.IsTrue(((TranslateTransform)marco.RenderTransform).HasAnimatedProperties, "La tarjeta tiembla");
        Assert.IsNull(marco.Effect);

        var reducido = Capturas.Montar(Partida.Jugar(Partida.HastaNivel4), 1280, 720);
        var otro = (JuegoVM)reducido.DataContext;
        Partida.Paso(otro, "A"); // Movimiento.Forzado: movimiento reducido
        Assert.IsTrue(otro.EsRespaldo);
        Assert.IsFalse(reducido.Tarjeta.Marco.RenderTransform is TranslateTransform { HasAnimatedProperties: true }, "Nada se desplaza");
        var halo = reducido.Tarjeta.Marco.Effect as DropShadowEffect;
        Assert.IsNotNull(halo, "Destello del color de acento");
        Assert.IsTrue(halo.HasAnimatedProperties);
        Capturas.Guardar(reducido, "f5-destello-respaldo");
        Assert.IsTrue(HiloUI.Esperar(() => reducido.Tarjeta.Marco.Effect is null, 3000), "El destello se apaga y se quita");
    });

    // ---------- Cero errores ----------

    [TestMethod(DisplayName = "Cero errores: partidas enteras (anchas, compactas, alto contraste, en vivo, glosario, nombre) sin errores de enlace ni Control de Misión")]
    public void CeroErrores() => HiloUI.Ejecutar(() =>
    {
        var escucha = new Escucha();
        PresentationTraceSources.Refresh(); // sin depurador, WPF solo informa de los errores de enlace tras un Refresh
        var fuente = PresentationTraceSources.DataBindingSource;
        var nivelAnterior = fuente.Switch.Level;
        fuente.Switch.Level = SourceLevels.Warning;
        fuente.Listeners.Add(escucha);
        var recursos = Application.Current.Resources;
        try
        {
            // Comprobación de la propia escucha: un enlace roto a propósito tiene que aparecer.
            var roto = new TextBlock { DataContext = new JuegoVM() };
            roto.SetBinding(TextBlock.TextProperty, new Binding("PropiedadQueNoExiste"));
            roto.Measure(new Size(100, 100));
            Assert.IsTrue(HiloUI.Esperar(() => escucha.Mensajes.Count > 0, 1000), "La escucha de errores funciona");
            escucha.Mensajes.Clear();

            var enVivo = new Tormenta("M3.5", "2026-11-05T12:00Z", "NASA DONKI", "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/42/-1", true, 0, DateTimeOffset.Now);
            var casos = 0;
            foreach (var (ancho, alto) in new[] { (1280, 720), (800, 600), (960, 485) })
            {
                foreach (var contraste in new[] { false, true })
                {
                    var diccionario = contraste ? AltoContraste.Aplicar(recursos, ColoresContraste.Noche) : null;
                    try
                    {
                        foreach (var (tormenta, ruta) in new[] { (DatosDonki.RespaldoHistorico(), RutaVictoria), (enVivo, RutaConPerdidas), (DatosDonki.RespaldoHistorico(), Partida.HastaAbortada) })
                        {
                            using var juego = new JuegoVM(tormenta);
                            var pantalla = Capturas.Montar(juego, ancho, alto);
                            foreach (var paso in ruta)
                            {
                                foreach (var termino in juego.Terminos) juego.VerTermino.Execute(termino);
                                Partida.Paso(juego, paso);
                                pantalla.UpdateLayout();
                                HiloUI.Vaciar();
                            }
                            if (juego.EsVictoria) juego.NombreCertificado = "Valentina";
                            pantalla.UpdateLayout();
                            HiloUI.Vaciar();
                            Assert.IsFalse(juego.ControlMisionVisible, "No apareció Control de Misión");
                            casos++;
                        }
                    }
                    finally
                    {
                        if (diccionario is not null) AltoContraste.Restaurar(recursos, diccionario);
                    }
                }
            }
            Assert.AreEqual(18, casos);
            Assert.IsEmpty(escucha.Mensajes, string.Join(Environment.NewLine, escucha.Mensajes.Take(5)));
        }
        finally
        {
            fuente.Listeners.Remove(escucha);
            fuente.Switch.Level = nivelAnterior;
        }
    });

    private sealed class Escucha : TraceListener
    {
        private readonly StringBuilder linea = new();

        public List<string> Mensajes { get; } = [];

        public override void Write(string? mensaje) => linea.Append(mensaje);

        public override void WriteLine(string? mensaje)
        {
            linea.Append(mensaje);
            Mensajes.Add(linea.ToString());
            linea.Clear();
        }
    }

    // ---------- Privacidad ----------

    [TestMethod(DisplayName = "Privacidad: tras una partida con nombre, en disco solo queda ajustes.json con «Sonido»; el nombre no aparece en ningún archivo")]
    public void NombreNoSeGuarda() => HiloUI.Ejecutar(() =>
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "BaseLunarPrivacidad-" + Guid.NewGuid().ToString("N"));
        var ruta = Path.Combine(carpeta, "ajustes.json");
        try
        {
            using (var juego = new JuegoVM(DatosDonki.RespaldoHistorico(), new ReproductorMudo(), new NarradorMudo(), Ajustes.Cargar(ruta)))
            {
                foreach (var paso in RutaVictoria) Partida.Paso(juego, paso);
                juego.NombreCertificado = "Valentina Pérez";
                juego.SonidoActivo = false; // obliga a guardar los ajustes con el nombre ya escrito
                juego.SonidoActivo = true;
                Assert.DoesNotContain("Valentina", juego.TextoParaEscuchar());
                juego.AccionPrincipal(); // Jugar de nuevo
                Assert.AreEqual(string.Empty, juego.NombreCertificado);
            }
            var archivos = Directory.GetFiles(carpeta, "*", SearchOption.AllDirectories);
            CollectionAssert.AreEqual(new[] { ruta }, archivos);
            foreach (var archivo in archivos) Assert.DoesNotContain("Valentina", File.ReadAllText(archivo));
            using var json = JsonDocument.Parse(File.ReadAllText(ruta));
            CollectionAssert.AreEqual(new[] { "Sonido" }, json.RootElement.EnumerateObject().Select(p => p.Name).ToArray());
        }
        finally
        {
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
        }
    });

    [TestMethod(DisplayName = "Privacidad: en el código, la red solo está en ClienteDonki, la escritura en disco solo en ajustes y caché, y los enlaces pasan por Enlaces.EsSeguro")]
    public void AuditoriaDelCodigo()
    {
        var fuentes = Directory.GetFiles(Path.Combine(Rutas.Escritorio(), "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToDictionary(f => Path.GetFileName(f), File.ReadAllText);
        Assert.IsGreaterThan(30, fuentes.Count);
        (string Patron, string[] Permitidos)[] reglas =
        [
            (@"\bHttpClient\b", ["ClienteDonki.cs"]),
            (@"\b(WebRequest|WebClient|Socket|TcpClient|UdpClient|SmtpClient)\b", []),
            (@"\bFile\.(Write|Append|Create|Move|Copy)|\bStreamWriter\b|\bFileStream\b", ["Ajustes.cs", "ClienteDonki.cs"]),
            (@"\bProcess\.Start\b", ["PantallaJuego.xaml.cs"]),
            (@"\bRegistry\b|\bClipboard\b", []),
        ];
        foreach (var (patron, permitidos) in reglas)
        {
            var usan = fuentes.Where(f => Regex.IsMatch(f.Value, patron)).Select(f => f.Key).Order().ToArray();
            CollectionAssert.AreEqual(permitidos, usan, $"«{patron}» aparece en: {string.Join(", ", usan)}");
        }
        StringAssert.Contains(fuentes["PantallaJuego.xaml.cs"], "if (!Enlaces.EsSeguro(e.Uri)) return;");
    }

    // ---------- Fuentes enlazadas en la Victoria y en el README ----------

    [TestMethod(DisplayName = "Fuentes: cada fuente de los niveles y de DONKI está enlazada en la Victoria y en el README; las del glosario, en el README")]
    public void FuentesEnlazadas() => HiloUI.Ejecutar(() =>
    {
        var juego = Partida.Jugar(RutaVictoria);
        var pantalla = Capturas.Montar(juego, 1280, 720);
        var enlaces = Descendientes<TextBlock>(pantalla.Tarjeta).SelectMany(t => Enlaces(t.Inlines)).Select(u => u.AbsoluteUri).ToHashSet();
        var delJuego = Guion.Niveles.SelectMany(n => DatoVM.De(n, juego.Tormenta).Fuentes).Select(f => new Uri(f.Url).AbsoluteUri).Distinct().ToArray();
        Assert.HasCount(11, delJuego, "10 fuentes de los niveles y la llamarada de DONKI");
        foreach (var url in delJuego) Assert.Contains(url, enlaces, $"Falta en la Victoria: {url}");

        var readme = File.ReadAllText(Path.Combine(Rutas.Raiz(), "README.md"));
        var delGlosario = Glosario.Terminos.SelectMany(t => t.Fuentes).Select(f => f.Url).Distinct();
        foreach (var url in delJuego.Concat(delGlosario)) Assert.Contains($"({url})", readme, $"Falta en el README: {url}");
    });

    private static IEnumerable<Uri> Enlaces(InlineCollection inlines)
    {
        foreach (var inline in inlines)
        {
            if (inline is Hyperlink { NavigateUri: { } uri }) yield return uri;
            if (inline is Span span)
            {
                foreach (var interior in Enlaces(span.Inlines)) yield return interior;
            }
        }
    }

    // ---------- Ventana y zoom de Windows ----------

    [TestMethod(DisplayName = "Zoom de Windows: la ventana nunca es más grande que el área de trabajo (200 % en 1920×1080 = 960×516 DIP)")]
    public void VentanaCabe()
    {
        var minimo = new Size(800, 600);
        var pedido = new Size(1280, 800);
        Assert.AreEqual((800.0, 600.0, 1280.0, 800.0), VentanaPrincipal.Medidas(new Size(1920, 1032), minimo, pedido), "100 %: el tamaño pedido");
        var (minAncho, minAlto, ancho, alto) = VentanaPrincipal.Medidas(new Size(960, 516), minimo, pedido);
        Assert.IsLessThanOrEqualTo(960.0, ancho);
        Assert.IsLessThanOrEqualTo(516.0, alto);
        Assert.AreEqual(516.0, minAlto, "El mínimo baja hasta el área de trabajo");
        Assert.AreEqual(800.0, minAncho);
        var (_, _, ancho125, alto125) = VentanaPrincipal.Medidas(new Size(1093, 576), minimo, pedido);
        Assert.IsTrue(ancho125 <= 1093 && alto125 <= 576, "125 % en 1366×768");
    }

    // ---------- Ayudantes ----------

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

    private static IEnumerable<T> Descendientes<T>(DependencyObject raiz) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(raiz); i++)
        {
            var hijo = VisualTreeHelper.GetChild(raiz, i);
            if (hijo is T encontrado) yield return encontrado;
            foreach (var nieto in Descendientes<T>(hijo)) yield return nieto;
        }
    }
}
