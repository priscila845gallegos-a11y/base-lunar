using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using BaseLunar.App.VistaModelos;
using BaseLunar.Nucleo;

namespace BaseLunar.PruebasUI;

/// <summary>
/// F5 · Validación de la web (el acceso por enlace y desde el móvil, D-32), que se conserva sin cambios.
/// Se maneja con los navegadores instalados (Edge, Chrome y Firefox) sin ventana, desde C#: tests.html,
/// partidas completas en un móvil emulado, tamaños de P1, rotación, zoom, doble toque, recarga, sin conexión,
/// movimiento reducido, cero errores de consola y ningún pedido fuera de la propia web.
/// </summary>
[TestClass]
[TestCategory("Navegador")]
public sealed class PruebasWeb
{
    private static ServidorWeb? servidor;

    private static string Carpeta => Path.Combine(Capturas.Carpeta, "web");

    [ClassInitialize]
    public static void Iniciar(TestContext _) => servidor = new ServidorWeb(Rutas.Raiz());

    [ClassCleanup]
    public static void Cerrar() => servidor?.Dispose();

    /// <summary>
    /// La web servida en localhost o, con la variable BASELUNAR_WEB, la publicada (por ejemplo, la de GitHub Pages,
    /// terminada en «/»): así las mismas pruebas validan el enlace público (P6).
    /// </summary>
    private static string Origen => Environment.GetEnvironmentVariable("BASELUNAR_WEB") is { Length: > 0 } publicada ? publicada : servidor!.Origen;

    private static string Url(string ruta) => Origen + ruta;

    public TestContext TestContext { get; set; } = null!;

    /// <summary>Abre el navegador; si la prueba pasa de su tiempo máximo, se cierra y la prueba falla en vez de colgarse.</summary>
    private async Task<Navegador> Abrir(string cual)
    {
        var navegador = await Navegador.Abrir(cual) ?? throw new AssertInconclusiveException($"{cual} no está instalado en este equipo.");
        TestContext.CancellationToken.Register(() => _ = navegador.DisposeAsync().AsTask());
        return navegador;
    }

    // ---------- tests.html ----------

    [TestMethod]
    [Timeout(300_000, CooperativeCancellation = true)]
    [DataRow("edge", DisplayName = "Web en Edge: tests.html al 100 % (motor y simulador), sin errores")]
    [DataRow("chrome", DisplayName = "Web en Chrome: tests.html al 100 % (motor y simulador), sin errores")]
    [DataRow("firefox", DisplayName = "Web en Firefox: tests.html al 100 % (motor y simulador), sin errores")]
    public async Task PruebasDeLaWeb(string cual)
    {
        await using var navegador = await Abrir(cual);
        await navegador.Navegar(Url("tests/tests.html"));
        var resumen = await navegador.Evaluar("""
            for (let i = 0; i < 200 && !/pruebas en verde/.test(document.getElementById('resumen').textContent); i++) await new Promise(r => setTimeout(r, 50));
            return { texto: document.getElementById('resumen').textContent, clase: document.getElementById('resumen').className };
            """);
        var texto = resumen.GetProperty("texto").GetString()!;
        Console.WriteLine($"{navegador.Nombre}: {texto}");
        var cuenta = Regex.Match(texto, @"(\d+) de (\d+) pruebas en verde");
        Assert.IsTrue(cuenta.Success, texto);
        Assert.AreEqual(cuenta.Groups[2].Value, cuenta.Groups[1].Value, texto);
        Assert.AreEqual("ok", resumen.GetProperty("clase").GetString());
        await Guardar(navegador, $"{cual}-tests-html");
        ComprobarLimpio(navegador, "favicon.ico"); // tests.html (herramienta de desarrollo) no declara ícono
    }

    // ---------- Partida completa en un móvil ----------

    [TestMethod]
    [Timeout(300_000, CooperativeCancellation = true)]
    [DataRow("edge", DisplayName = "Web en Edge, móvil 360×640 táctil y sin conexión: Victoria y Misión Abortada de principio a fin; «Jugar de nuevo» sin recargar")]
    [DataRow("chrome", DisplayName = "Web en Chrome, móvil 360×640 táctil y sin conexión: Victoria y Misión Abortada de principio a fin; «Jugar de nuevo» sin recargar")]
    [DataRow("firefox", DisplayName = "Web en Firefox, móvil 360×640: Victoria y Misión Abortada de principio a fin; «Jugar de nuevo» sin recargar")]
    public async Task PartidaEnMovil(string cual)
    {
        await using var navegador = await Abrir(cual);
        await navegador.Ventana(360, 640, 2, movil: true);
        foreach (var (nombre, ruta) in new[] { ("victoria", Partida.HastaVictoria), ("abortada", Partida.HastaAbortada) })
        {
            if (navegador.EmulacionCompleta) await navegador.SinConexion(false);
            await navegador.Navegar(Url("index.html"));
            if (navegador.EmulacionCompleta) await navegador.SinConexion(); // una vez abierta, ya no necesita la red
            await navegador.Evaluar("window.__sinRecargar = true; return true;");
            var inicial = await Foto(navegador);
            CollectionAssert.AreEqual(new[] { 80, 80, 80 }, inicial.Medidores);
            var respaldos = 0;
            for (var i = 0; i < ruta.Length; i++)
            {
                var antes = await Foto(navegador);
                if (antes.Fase == "nivel" && !antes.Respaldo && !antes.Resultado) ComprobarOpciones(await Opciones(navegador), antes.Progreso);
                var pulsado = await Pulsar(navegador, ruta[i]);
                Assert.IsTrue(pulsado.GetProperty("visible").GetBoolean(), $"{cual}, paso {i + 1}: el botón no se ve sin desplazar la página (360×640)");
                var despues = await Foto(navegador);
                Assert.IsFalse(despues.ScrollHorizontal, $"{cual}, paso {i + 1}: scroll horizontal");
                Assert.IsTrue(despues.Medidores.All(v => v is >= 0 and <= 100));
                if (despues.Respaldo && !antes.Respaldo)
                {
                    respaldos++;
                    Assert.AreEqual("temblor", pulsado.GetProperty("animacion").GetString(), "Al perder un respaldo, la tarjeta tiembla");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(despues.MensajeRespaldo));
                    await Guardar(navegador, $"{cual}-360x640-{nombre}-respaldo{respaldos}");
                    // El reintento usa la foto, con el medidor que falló en 30 o más.
                    var fallidos = Enumerable.Range(0, 3).Where(m => despues.Medidores[m] == 0).ToArray();
                    if (i + 1 < ruta.Length && ruta[i + 1] == "P")
                    {
                        await Pulsar(navegador, "P");
                        var reintento = await Foto(navegador);
                        if (reintento.Fase == "nivel") Assert.IsTrue(fallidos.All(m => reintento.Medidores[m] >= 30), "Reintento con el medidor restaurado");
                        i++;
                    }
                }
                if (i == 1 && nombre == "victoria") await Guardar(navegador, $"{cual}-360x640-resultado1");
                if (i == 0 && nombre == "victoria") await Guardar(navegador, $"{cual}-360x640-nivel1");
            }
            var final = await Foto(navegador);
            Assert.AreEqual(nombre, final.Fase);
            await Guardar(navegador, $"{cual}-360x640-{nombre}");
            if (nombre == "victoria")
            {
                var enlaces = (await navegador.Evaluar("return [...document.querySelectorAll('#vista-victoria a[href]')].map(a => a.href);"))
                    .EnumerateArray().Select(e => e.GetString()).ToHashSet();
                var tormenta = DatosDonki.RespaldoHistorico();
                foreach (var url in Guion.Niveles.SelectMany(n => DatoVM.De(n, tormenta).Fuentes).Select(f => new Uri(f.Url).AbsoluteUri).Distinct())
                    Assert.Contains(url, enlaces, $"Falta en la Victoria de la web: {url}");
            }
            else
            {
                Assert.AreEqual(2, respaldos, "Dos pantallas de respaldo; al perder el tercero, Misión Abortada");
            }
            await Pulsar(navegador, "P"); // Jugar de nuevo
            var otraVez = await Foto(navegador);
            Assert.AreEqual("inicio", otraVez.Fase);
            CollectionAssert.AreEqual(new[] { 80, 80, 80 }, otraVez.Medidores);
            Assert.IsTrue((await navegador.Evaluar("return window.__sinRecargar === true;")).GetBoolean(), "«Jugar de nuevo» no recarga la página");
        }
        ComprobarLimpio(navegador);
    }

    // ---------- Tamaños, rotación, zoom y robustez ----------

    [TestMethod]
    [Timeout(300_000, CooperativeCancellation = true)]
    [DataRow("edge", DisplayName = "Web en Edge: sin scroll horizontal a 320, 360×640, 768, 1280 y 1440, en horizontal y con zoom al 200 %; rotar, doble toque y recargar")]
    [DataRow("chrome", DisplayName = "Web en Chrome: sin scroll horizontal a 320, 360×640, 768, 1280 y 1440, en horizontal y con zoom al 200 %; rotar, doble toque y recargar")]
    [DataRow("firefox", DisplayName = "Web en Firefox: sin scroll horizontal a 320, 360×640, 768, 1280 y 1440, en horizontal y con zoom al 200 %; rotar, doble toque y recargar")]
    public async Task TamanosYRobustez(string cual)
    {
        await using var navegador = await Abrir(cual);
        // El zoom del navegador al 200 % en una ventana de 1280×800 deja 640×400 píxeles CSS al doble de densidad.
        (int Ancho, int Alto, double Escala, bool Movil, string Nombre)[] tamanos =
        [
            (320, 568, 2, true, "320x568"), (360, 640, 2, true, "360x640"), (640, 360, 2, true, "640x360-horizontal"),
            (768, 1024, 2, true, "768x1024"), (1280, 800, 1, false, "1280x800"), (1440, 900, 1, false, "1440x900"), (640, 400, 2, false, "1280x800-zoom200"),
        ];
        foreach (var (ancho, alto, escala, movil, nombre) in tamanos)
        {
            await navegador.Ventana(ancho, alto, escala, movil);
            await navegador.Navegar(Url("index.html"));
            foreach (var (paso, etiqueta) in new[] { ("", "inicio"), ("P", "nivel1"), ("A", "resultado1"), ("P", ""), ("A", ""), ("P", ""), ("A", ""), ("P", "nivel4"), ("A", "respaldo4") })
            {
                if (paso.Length > 0)
                {
                    var pulsado = await Pulsar(navegador, paso);
                    if (ancho <= 360) Assert.IsTrue(pulsado.GetProperty("visible").GetBoolean(), $"{cual} {nombre}: botón {paso} fuera de la pantalla");
                }
                var foto = await Foto(navegador);
                Assert.IsFalse(foto.ScrollHorizontal, $"{cual} {nombre} ({etiqueta}): scroll horizontal");
                if (etiqueta is "nivel1" or "nivel4" or "respaldo4") await Guardar(navegador, $"{cual}-{nombre}-{etiqueta}");
            }
        }

        // Rotar el móvil a mitad de partida: la partida sigue igual.
        await navegador.Ventana(360, 640, 2, true);
        await navegador.Navegar(Url("index.html"));
        foreach (var paso in new[] { "P", "A", "P" }) await Pulsar(navegador, paso);
        var vertical = await Foto(navegador);
        await navegador.Ventana(640, 360, 2, true);
        var horizontal = await Foto(navegador);
        Assert.AreEqual(vertical.Progreso, horizontal.Progreso);
        CollectionAssert.AreEqual(vertical.Medidores, horizontal.Medidores);
        Assert.IsFalse(horizontal.ScrollHorizontal);
        await navegador.Ventana(360, 640, 2, true);

        // Doble toque rápido: una sola decisión.
        await navegador.Navegar(Url("index.html"));
        await Pulsar(navegador, "P");
        await navegador.Evaluar("""
            for (let i = 0; i < 100 && document.getElementById('opcion-a').disabled; i++) await new Promise(r => setTimeout(r, 25));
            const a = document.getElementById('opcion-a');
            a.click(); a.click(); document.getElementById('opcion-b').click();
            await new Promise(r => setTimeout(r, 700));
            return true;
            """);
        var contexto = new ContextoPartida();
        var unaVez = Motor.AplicarDecision(Motor.Empezar(Motor.CrearEstadoInicial(), contexto).Estado, "A").Estado.Medidores;
        var tras = await Foto(navegador);
        CollectionAssert.AreEqual(new[] { unaVez.Proteccion, unaVez.Soporte, unaVez.Bateria }, tras.Medidores, "La decisión se aplicó una sola vez");
        Assert.IsTrue(tras.Resultado);

        // Recargar a mitad de partida: vuelve al inicio, limpio.
        foreach (var paso in new[] { "P", "B", "P" }) await Pulsar(navegador, paso);
        Assert.AreEqual("Nivel 3 de 5", (await Foto(navegador)).Progreso);
        await navegador.Recargar();
        var recargada = await Foto(navegador);
        Assert.AreEqual("inicio", recargada.Fase);
        CollectionAssert.AreEqual(new[] { 80, 80, 80 }, recargada.Medidores);

        // Teclado (5.5): Enter comienza y A elige; el resultado se anuncia en la región aria-live.
        var teclado = await navegador.Evaluar("""
            const tecla = (key) => document.body.dispatchEvent(new KeyboardEvent('keydown', { key, bubbles: true }));
            const esperar = (ms) => new Promise(r => setTimeout(r, ms));
            tecla('Enter');
            await esperar(700);
            const nivel = !document.getElementById('vista-nivel').hidden;
            tecla('a');
            await esperar(900);
            return { nivel, resultado: !document.getElementById('resultado').hidden, anuncio: document.getElementById('anuncio').textContent };
            """);
        Assert.IsTrue(teclado.GetProperty("nivel").GetBoolean(), "Enter comienza la misión");
        Assert.IsTrue(teclado.GetProperty("resultado").GetBoolean(), "A elige la opción A");
        Assert.IsFalse(string.IsNullOrWhiteSpace(teclado.GetProperty("anuncio").GetString()), "El resultado se anuncia al lector de pantalla");
        ComprobarLimpio(navegador);
    }

    // ---------- Movimiento reducido ----------

    [TestMethod]
    [Timeout(300_000, CooperativeCancellation = true)]
    [DataRow("edge", DisplayName = "Web en Edge con movimiento reducido: sin temblor, la tarjeta destella al perder un respaldo")]
    [DataRow("chrome", DisplayName = "Web en Chrome con movimiento reducido: sin temblor, la tarjeta destella al perder un respaldo")]
    public async Task MovimientoReducido(string cual)
    {
        await using var navegador = await Abrir(cual);
        await navegador.MovimientoReducido(true);
        await navegador.Ventana(360, 640, 2, true);
        await navegador.Navegar(Url("index.html"));
        Assert.IsTrue((await navegador.Evaluar("return matchMedia('(prefers-reduced-motion: reduce)').matches;")).GetBoolean());
        foreach (var paso in Partida.HastaNivel4) await Pulsar(navegador, paso);
        var pulsado = await Pulsar(navegador, "A");
        Assert.IsTrue((await Foto(navegador)).Respaldo);
        Assert.AreEqual("destello", pulsado.GetProperty("animacion").GetString());
        ComprobarLimpio(navegador);
    }

    // ---------- ZIP sin servidor (doble clic) ----------

    [TestMethod]
    [Timeout(300_000, CooperativeCancellation = true)]
    [DataRow("edge", DisplayName = "Web en Edge abierta con doble clic (file://, como el ZIP): se juega hasta el Nivel 2 sin errores")]
    [DataRow("firefox", DisplayName = "Web en Firefox abierta con doble clic (file://, como el ZIP): se juega hasta el Nivel 2 sin errores")]
    public async Task SinServidor(string cual)
    {
        await using var navegador = await Abrir(cual);
        await navegador.Ventana(360, 640, 2, true);
        await navegador.Navegar(new Uri(Path.Combine(Rutas.Raiz(), "index.html")).AbsoluteUri);
        foreach (var paso in new[] { "P", "A", "P" }) await Pulsar(navegador, paso);
        Assert.AreEqual("Nivel 2 de 5", (await Foto(navegador)).Progreso);
        await Guardar(navegador, $"{cual}-file-nivel2");
        Assert.IsEmpty(navegador.Errores, string.Join(Environment.NewLine, navegador.Errores));
        Assert.IsTrue(navegador.Pedidos.All(p => p.StartsWith("file:///", StringComparison.Ordinal) || p.StartsWith("data:", StringComparison.Ordinal) || p == "about:blank"),
            string.Join(Environment.NewLine, navegador.Pedidos));
    }

    // ---------- Ayudantes ----------

    /// <summary>Captura cuando termina la transición (450 ms con los botones bloqueados y atenuados).</summary>
    private static async Task Guardar(Navegador navegador, string nombre)
    {
        await navegador.Evaluar("""
            const libre = () => !['boton-principal', 'opcion-a', 'opcion-b'].some(id => document.getElementById(id)?.disabled);
            for (let i = 0; i < 100 && !libre(); i++) await new Promise(r => setTimeout(r, 25));
            await new Promise(r => setTimeout(r, 900));
            return true;
            """);
        await navegador.Guardar(Carpeta, nombre);
    }

    private sealed record FotoWeb(string Fase, string Progreso, int[] Medidores, bool ScrollHorizontal, bool Respaldo, bool Resultado, string MensajeRespaldo);

    private static async Task<FotoWeb> Foto(Navegador navegador)
    {
        var foto = await navegador.Evaluar("""
            const $ = (id) => document.getElementById(id);
            return {
              fase: ['inicio', 'nivel', 'abortada', 'victoria'].find(v => !$('vista-' + v).hidden) ?? '',
              progreso: $('progreso-texto').textContent,
              medidores: ['proteccion', 'soporte', 'bateria'].map(k => +document.querySelector('#medidor-' + k + ' [role=progressbar]').getAttribute('aria-valuenow')),
              scroll: document.documentElement.scrollWidth > innerWidth + 0.5 || document.body.scrollWidth > innerWidth + 0.5,
              respaldo: !$('aviso-respaldo').hidden && !$('vista-nivel').hidden,
              resultado: !$('resultado').hidden && !$('vista-nivel').hidden,
              mensaje: $('respaldo-mensaje').textContent,
            };
            """);
        return new FotoWeb(foto.GetProperty("fase").GetString()!, foto.GetProperty("progreso").GetString()!,
            foto.GetProperty("medidores").EnumerateArray().Select(e => e.GetInt32()).ToArray(), foto.GetProperty("scroll").GetBoolean(),
            foto.GetProperty("respaldo").GetBoolean(), foto.GetProperty("resultado").GetBoolean(), foto.GetProperty("mensaje").GetString()!);
    }

    /// <summary>
    /// Pulsa el botón principal («P») o una opción («A», «B») cuando deja de estar bloqueado. Devuelve si se
    /// veía entero sin desplazar la página y la animación de la tarjeta justo después.
    /// </summary>
    private static async Task<JsonElement> Pulsar(Navegador navegador, string paso)
    {
        var id = paso switch { "P" => "boton-principal", "A" => "opcion-a", "B" => "opcion-b", _ => throw new ArgumentException(paso) };
        var resultado = await navegador.Evaluar($$"""
            const b = document.getElementById('{{id}}');
            const listo = () => !b.disabled && !b.closest('[hidden]');
            for (let i = 0; i < 200 && !listo(); i++) await new Promise(r => setTimeout(r, 25));
            if (!listo()) return { ok: false };
            const r = b.getBoundingClientRect();
            const visible = r.top >= -0.5 && r.left >= -0.5 && r.bottom <= innerHeight + 0.5 && r.right <= innerWidth + 0.5;
            b.click();
            return { ok: true, visible, animacion: getComputedStyle(document.getElementById('tarjeta')).animationName };
            """);
        Assert.IsTrue(resultado.GetProperty("ok").GetBoolean(), $"{navegador.Nombre}: el botón {id} no se pudo pulsar");
        return resultado;
    }

    private static async Task<string[][]> Opciones(Navegador navegador) =>
        (await navegador.Evaluar("return ['opcion-a', 'opcion-b'].map(id => [...document.querySelectorAll('#' + id + ' .efecto')].map(e => e.textContent));"))
            .EnumerateArray().Select(o => o.EnumerateArray().Select(e => e.GetString()!).ToArray()).ToArray();

    /// <summary>Niveles 1 a 4: cada opción muestra al menos una ganancia (+) y un costo (−). Nivel 5: su requisito.</summary>
    private static void ComprobarOpciones(string[][] opciones, string progreso)
    {
        foreach (var efectos in opciones)
        {
            Assert.IsNotEmpty(efectos, progreso);
            if (progreso.StartsWith("Nivel 5", StringComparison.Ordinal)) continue;
            Assert.IsTrue(efectos.Any(e => e.StartsWith('+')) && efectos.Any(e => e.StartsWith('−')), $"{progreso}: {string.Join(" ", efectos)}");
        }
    }

    /// <summary>Cero errores en la consola y ningún pedido fuera de la propia web.</summary>
    private static void ComprobarLimpio(Navegador navegador, string? permitido = null)
    {
        var errores = navegador.Errores.Where(e => permitido is null || !e.EndsWith("/" + permitido, StringComparison.Ordinal)).ToArray();
        Assert.IsEmpty(errores, $"{navegador.Nombre}: {string.Join(Environment.NewLine, errores)}");
        var fuera = navegador.Pedidos.Where(p => !p.StartsWith(Origen, StringComparison.Ordinal) && !p.StartsWith("data:", StringComparison.Ordinal) && p != "about:blank"
            && (permitido is null || !p.EndsWith("/" + permitido, StringComparison.Ordinal))).ToArray();
        Assert.IsEmpty(fuera, $"{navegador.Nombre}: pedidos fuera de la web: {string.Join(", ", fuera)}");
        Assert.IsNotEmpty(navegador.Pedidos, "Se registraron los pedidos");
    }
}
