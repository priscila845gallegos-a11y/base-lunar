using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BaseLunar.PruebasUI;

/// <summary>
/// Servidor estático mínimo (HttpListener de .NET) que sirve la web en localhost, como GitHub Pages. Solo
/// responde GET y solo a los archivos de la web: index.html, style.css, js/, assets/ y tests/tests.html.
/// </summary>
internal sealed class ServidorWeb : IDisposable
{
    private readonly HttpListener oyente = new();
    private readonly string raiz;

    public ServidorWeb(string raiz)
    {
        this.raiz = Path.GetFullPath(raiz);
        Origen = $"http://localhost:{PuertoLibre()}/";
        oyente.Prefixes.Add(Origen);
        oyente.Start();
        _ = Atender();
    }

    public string Origen { get; }

    public static int PuertoLibre()
    {
        var oyenteTcp = new TcpListener(IPAddress.Loopback, 0);
        oyenteTcp.Start();
        var puerto = ((IPEndPoint)oyenteTcp.LocalEndpoint).Port;
        oyenteTcp.Stop();
        return puerto;
    }

    private async Task Atender()
    {
        while (oyente.IsListening)
        {
            HttpListenerContext contexto;
            try
            {
                contexto = await oyente.GetContextAsync();
            }
            catch (Exception)
            {
                return;
            }
            Responder(contexto);
        }
    }

    private void Responder(HttpListenerContext contexto)
    {
        try
        {
            var ruta = Uri.UnescapeDataString(contexto.Request.Url!.AbsolutePath.TrimStart('/'));
            if (ruta.Length == 0) ruta = "index.html";
            var archivo = Path.GetFullPath(Path.Combine(raiz, ruta));
            var permitido = ruta is "index.html" or "style.css" or "tests/tests.html" || ruta.StartsWith("js/", StringComparison.Ordinal) || ruta.StartsWith("assets/", StringComparison.Ordinal);
            if (contexto.Request.HttpMethod != "GET" || !permitido || !archivo.StartsWith(raiz + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(archivo))
            {
                contexto.Response.StatusCode = 404;
                return;
            }
            var bytes = File.ReadAllBytes(archivo);
            contexto.Response.ContentType = Path.GetExtension(archivo).ToLowerInvariant() switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "text/javascript; charset=utf-8",
                ".svg" => "image/svg+xml",
                ".woff2" => "font/woff2",
                _ => "application/octet-stream",
            };
            contexto.Response.ContentLength64 = bytes.Length;
            contexto.Response.OutputStream.Write(bytes);
        }
        catch (Exception)
        {
            // El navegador pudo cerrar la conexión: no importa.
        }
        finally
        {
            try
            {
                contexto.Response.Close();
            }
            catch (Exception)
            {
                // Igual que arriba.
            }
        }
    }

    public void Dispose()
    {
        oyente.Stop();
        oyente.Close();
    }
}

/// <summary>
/// Conexión JSON por WebSocket (ClientWebSocket de .NET): sirve para el protocolo de DevTools de Edge y Chrome
/// y para WebDriver BiDi de Firefox, que comparten la forma {id, method, params} → {id, result | error}.
/// </summary>
internal sealed class Conexion : IAsyncDisposable
{
    private readonly ClientWebSocket socket = new();
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> pendientes = new();
    private int ultimo;

    public event Action<string, JsonElement>? Evento;

    public static async Task<Conexion> Abrir(Uri direccion, TimeSpan espera)
    {
        var limite = DateTime.UtcNow + espera;
        while (true)
        {
            var conexion = new Conexion();
            try
            {
                await conexion.socket.ConnectAsync(direccion, CancellationToken.None);
                _ = conexion.Leer();
                return conexion;
            }
            catch (Exception) when (DateTime.UtcNow < limite)
            {
                conexion.socket.Dispose();
                await Task.Delay(250);
            }
        }
    }

    public async Task<JsonElement> Enviar(string metodo, object? parametros = null)
    {
        var id = Interlocked.Increment(ref ultimo);
        var respuesta = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        pendientes[id] = respuesta;
        var json = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object> { ["id"] = id, ["method"] = metodo, ["params"] = parametros ?? new { } });
        await socket.SendAsync(json, WebSocketMessageType.Text, true, CancellationToken.None);
        return await respuesta.Task.WaitAsync(TimeSpan.FromSeconds(60));
    }

    private async Task Leer()
    {
        var bufer = new byte[1 << 16];
        using var mensaje = new MemoryStream();
        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult recibido;
            try
            {
                recibido = await socket.ReceiveAsync(bufer, CancellationToken.None);
            }
            catch (Exception)
            {
                break;
            }
            if (recibido.MessageType == WebSocketMessageType.Close) break;
            mensaje.Write(bufer, 0, recibido.Count);
            if (!recibido.EndOfMessage) continue;
            JsonElement raiz;
            using (var documento = JsonDocument.Parse(mensaje.ToArray())) raiz = documento.RootElement.Clone();
            mensaje.SetLength(0);
            if (raiz.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.Number && pendientes.TryRemove(id.GetInt32(), out var respuesta))
            {
                if (raiz.TryGetProperty("error", out var error))
                    respuesta.TrySetException(new InvalidOperationException(error + (raiz.TryGetProperty("message", out var detalle) ? ": " + detalle : string.Empty)));
                else respuesta.TrySetResult(raiz.TryGetProperty("result", out var resultado) ? resultado : default);
            }
            else if (raiz.TryGetProperty("method", out var metodo))
            {
                Evento?.Invoke(metodo.GetString()!, raiz.TryGetProperty("params", out var parametros) ? parametros : default);
            }
        }
        foreach (var pendiente in pendientes.Values) pendiente.TrySetException(new IOException("Se cerró la conexión con el navegador"));
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (socket.State == WebSocketState.Open)
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, new CancellationTokenSource(1000).Token);
        }
        catch (Exception)
        {
            // El navegador ya se cerró.
        }
        socket.Dispose();
    }
}

/// <summary>
/// Un navegador instalado, sin ventana y con un perfil temporal y vacío, manejado por su protocolo de
/// automatización. Anota los errores de la página (consola, excepciones, CSP) y cada pedido que hace.
/// </summary>
internal abstract class Navegador : IAsyncDisposable
{
    protected Process? proceso;
    protected Conexion conexion = null!;
    protected readonly string perfil = Path.Combine(Path.GetTempPath(), "BaseLunarNavegador-" + Guid.NewGuid().ToString("N"));

    protected Navegador(string nombre) => Nombre = nombre;

    public string Nombre { get; }

    public ConcurrentQueue<string> Errores { get; } = new();

    public ConcurrentQueue<string> Pedidos { get; } = new();

    /// <summary>Si puede emular la red sin conexión, el movimiento reducido y la pantalla táctil (Edge y Chrome).</summary>
    public virtual bool EmulacionCompleta => false;

    public static string? Ruta(string cual)
    {
        var programas = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programas86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string[] candidatas = cual switch
        {
            "edge" => [Path.Combine(programas86, @"Microsoft\Edge\Application\msedge.exe"), Path.Combine(programas, @"Microsoft\Edge\Application\msedge.exe")],
            "chrome" => [Path.Combine(programas, @"Google\Chrome\Application\chrome.exe"), Path.Combine(local, @"Google\Chrome\Application\chrome.exe")],
            "firefox" => [Path.Combine(programas, @"Mozilla Firefox\firefox.exe"), Path.Combine(programas86, @"Mozilla Firefox\firefox.exe")],
            _ => [],
        };
        return candidatas.FirstOrDefault(File.Exists);
    }

    /// <summary>Abre el navegador pedido, o null si no está instalado.</summary>
    public static async Task<Navegador?> Abrir(string cual)
    {
        if (Ruta(cual) is not { } exe) return null;
        Navegador navegador = cual == "firefox" ? new Firefox(cual) : new Chromium(cual);
        try
        {
            await navegador.Iniciar(exe);
            return navegador;
        }
        catch
        {
            await navegador.DisposeAsync();
            throw;
        }
    }

    protected abstract Task Iniciar(string exe);

    public abstract Task Navegar(string url);

    public abstract Task Recargar();

    public abstract Task Ventana(int ancho, int alto, double escala, bool movil);

    public abstract Task<byte[]> Capturar();

    public virtual Task SinConexion(bool sinConexion = true) => Task.CompletedTask;

    public virtual Task MovimientoReducido(bool reducido) => Task.CompletedTask;

    protected abstract Task<string> EvaluarTexto(string expresion);

    /// <summary>Ejecuta el cuerpo de una función async en la página y devuelve lo que retorne, como JSON.</summary>
    public async Task<JsonElement> Evaluar(string cuerpo)
    {
        var texto = await EvaluarTexto($"(async () => JSON.stringify(await (async () => {{ {cuerpo} }})() ?? null))()");
        using var documento = JsonDocument.Parse(texto);
        return documento.RootElement.Clone();
    }

    public async Task Guardar(string carpeta, string nombre)
    {
        Directory.CreateDirectory(carpeta);
        var bytes = await Capturar();
        for (var intento = 1; ; intento++)
        {
            try
            {
                await File.WriteAllBytesAsync(Path.Combine(carpeta, nombre + ".png"), bytes);
                return;
            }
            catch (IOException) when (intento < 10)
            {
                await Task.Delay(150); // OneDrive bloquea un momento los archivos nuevos
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (conexion is not null) await conexion.DisposeAsync();
        if (proceso is not null)
        {
            try
            {
                if (!proceso.HasExited) proceso.Kill(entireProcessTree: true);
            }
            catch (Exception)
            {
                // Algún proceso hijo ya se cerró o no se deja: abajo se insiste con taskkill.
            }
            if (!proceso.WaitForExit(5000))
            {
                using var cierre = Process.Start(new ProcessStartInfo("taskkill", $"/F /T /PID {proceso.Id}") { UseShellExecute = false, CreateNoWindow = true });
                cierre?.WaitForExit(5000);
            }
            proceso.Dispose();
        }
        for (var intento = 0; intento < 10 && Directory.Exists(perfil); intento++)
        {
            try
            {
                Directory.Delete(perfil, true);
            }
            catch (IOException)
            {
                await Task.Delay(300);
            }
            catch (UnauthorizedAccessException)
            {
                await Task.Delay(300);
            }
        }
    }
}

/// <summary>Edge y Chrome, por el protocolo de DevTools (CDP), conectados directamente a la pestaña.</summary>
internal sealed class Chromium(string nombre) : Navegador(nombre)
{
    private TaskCompletionSource? carga;

    public override bool EmulacionCompleta => true;

    protected override async Task Iniciar(string exe)
    {
        Directory.CreateDirectory(perfil);
        var inicio = new ProcessStartInfo(exe) { UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (var argumento in new[]
        {
            "--headless=new", "--remote-debugging-port=0", "--user-data-dir=" + perfil, "--no-first-run", "--no-default-browser-check",
            "--disable-extensions", "--disable-background-networking", "--disable-component-update", "--disable-sync", "--no-pings",
            "--disable-default-apps", "--mute-audio",
            // Sin ventana, la pestaña cuenta como oculta: sin esto, los temporizadores se frenan hasta 1 por minuto.
            "--disable-background-timer-throttling", "--disable-renderer-backgrounding", "--disable-backgrounding-occluded-windows",
            "about:blank",
        }) inicio.ArgumentList.Add(argumento);
        proceso = Process.Start(inicio)!;
        proceso.BeginErrorReadLine();
        proceso.BeginOutputReadLine();

        var archivo = Path.Combine(perfil, "DevToolsActivePort");
        string? puerto = null;
        for (var i = 0; i < 120 && puerto is null; i++)
        {
            try
            {
                if (File.Exists(archivo) && File.ReadAllLines(archivo) is [var primera, ..] && primera.Length > 0) puerto = primera;
            }
            catch (IOException)
            {
                // Se está escribiendo.
            }
            if (puerto is null) await Task.Delay(250);
        }
        if (puerto is null) throw new TimeoutException($"{Nombre} no abrió el puerto de DevTools");

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        string? pestana = null;
        for (var i = 0; i < 40 && pestana is null; i++)
        {
            using var lista = JsonDocument.Parse(await http.GetStringAsync($"http://127.0.0.1:{puerto}/json/list"));
            pestana = lista.RootElement.EnumerateArray().Where(t => t.GetProperty("type").GetString() == "page")
                .Select(t => t.GetProperty("webSocketDebuggerUrl").GetString()).FirstOrDefault();
            if (pestana is null) await Task.Delay(250);
        }
        conexion = await Conexion.Abrir(new Uri(pestana ?? throw new TimeoutException($"{Nombre} no tiene pestaña")), TimeSpan.FromSeconds(10));
        conexion.Evento += AlEvento;
        foreach (var dominio in new[] { "Runtime", "Log", "Network", "Page" }) await conexion.Enviar(dominio + ".enable");
        await conexion.Enviar("Emulation.setFocusEmulationEnabled", new { enabled = true });
    }

    private void AlEvento(string metodo, JsonElement p)
    {
        switch (metodo)
        {
            case "Runtime.exceptionThrown":
                var detalle = p.GetProperty("exceptionDetails");
                Errores.Enqueue("Excepción: " + (detalle.TryGetProperty("exception", out var excepcion) && excepcion.TryGetProperty("description", out var d) ? d.GetString() : detalle.GetProperty("text").GetString()));
                break;
            case "Runtime.consoleAPICalled" when p.GetProperty("type").GetString() is "error" or "assert":
                Errores.Enqueue("console.error: " + string.Join(" ", p.GetProperty("args").EnumerateArray().Select(a => a.TryGetProperty("value", out var v) ? v.ToString() : a.TryGetProperty("description", out var dd) ? dd.GetString() : "?")));
                break;
            case "Log.entryAdded" when p.GetProperty("entry").GetProperty("level").GetString() == "error":
                var entrada = p.GetProperty("entry");
                Errores.Enqueue(entrada.GetProperty("source").GetString() + ": " + entrada.GetProperty("text").GetString() + (entrada.TryGetProperty("url", out var url) ? " " + url.GetString() : string.Empty));
                break;
            case "Network.requestWillBeSent":
                Pedidos.Enqueue(p.GetProperty("request").GetProperty("url").GetString()!);
                break;
            case "Page.loadEventFired":
                carga?.TrySetResult();
                break;
        }
    }

    private async Task Cargar(Func<Task> accion)
    {
        carga = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await accion();
        await carga.Task.WaitAsync(TimeSpan.FromSeconds(30));
        await Task.Delay(150);
    }

    public override Task Navegar(string url) => Cargar(() => conexion.Enviar("Page.navigate", new { url }));

    public override Task Recargar() => Cargar(() => conexion.Enviar("Page.reload", new { ignoreCache = true }));

    public override async Task Ventana(int ancho, int alto, double escala, bool movil)
    {
        await conexion.Enviar("Emulation.setDeviceMetricsOverride", new { width = ancho, height = alto, deviceScaleFactor = escala, mobile = movil });
        await conexion.Enviar("Emulation.setTouchEmulationEnabled", new { enabled = movil, maxTouchPoints = movil ? 5 : 1 });
    }

    public override async Task<byte[]> Capturar() =>
        Convert.FromBase64String((await conexion.Enviar("Page.captureScreenshot", new { format = "png" })).GetProperty("data").GetString()!);

    public override Task SinConexion(bool sinConexion = true) =>
        conexion.Enviar("Network.emulateNetworkConditions", new { offline = sinConexion, latency = 0, downloadThroughput = -1, uploadThroughput = -1 });

    public override Task MovimientoReducido(bool reducido) =>
        conexion.Enviar("Emulation.setEmulatedMedia", new { features = new[] { new { name = "prefers-reduced-motion", value = reducido ? "reduce" : string.Empty } } });

    protected override async Task<string> EvaluarTexto(string expresion)
    {
        var resultado = await conexion.Enviar("Runtime.evaluate", new { expression = expresion, awaitPromise = true, returnByValue = true });
        if (resultado.TryGetProperty("exceptionDetails", out var error)) throw new InvalidOperationException("Error en la página: " + error);
        return resultado.GetProperty("result").GetProperty("value").GetString()!;
    }
}

/// <summary>Firefox, por WebDriver BiDi (su protocolo de automatización actual), sin geckodriver.</summary>
internal sealed class Firefox(string nombre) : Navegador(nombre)
{
    private string contexto = string.Empty;

    protected override async Task Iniciar(string exe)
    {
        Directory.CreateDirectory(perfil);
        // Perfil vacío y sin conexiones propias del navegador (actualizaciones, telemetría, contenido recomendado).
        File.WriteAllText(Path.Combine(perfil, "user.js"), string.Join(Environment.NewLine,
            "user_pref(\"browser.shell.checkDefaultBrowser\", false);",
            "user_pref(\"app.update.enabled\", false);",
            "user_pref(\"datareporting.policy.dataSubmissionEnabled\", false);",
            "user_pref(\"toolkit.telemetry.enabled\", false);",
            "user_pref(\"browser.newtabpage.enabled\", false);",
            "user_pref(\"browser.startup.homepage_override.mstone\", \"ignore\");"));
        var puerto = ServidorWeb.PuertoLibre();
        var inicio = new ProcessStartInfo(exe) { UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (var argumento in new[] { "--headless", "--no-remote", "--profile", perfil, "--remote-debugging-port", puerto.ToString(System.Globalization.CultureInfo.InvariantCulture), "about:blank" })
            inicio.ArgumentList.Add(argumento);
        proceso = Process.Start(inicio)!;
        proceso.BeginErrorReadLine();
        proceso.BeginOutputReadLine();
        conexion = await Conexion.Abrir(new Uri($"ws://127.0.0.1:{puerto}/session"), TimeSpan.FromSeconds(30));
        conexion.Evento += AlEvento;
        await conexion.Enviar("session.new", new { capabilities = new { } });
        var arbol = await conexion.Enviar("browsingContext.getTree", new { });
        contexto = arbol.GetProperty("contexts")[0].GetProperty("context").GetString()!;
        await conexion.Enviar("session.subscribe", new { events = new[] { "log.entryAdded", "network.beforeRequestSent" } });
    }

    private void AlEvento(string metodo, JsonElement p)
    {
        if (metodo == "log.entryAdded" && p.GetProperty("level").GetString() == "error")
            Errores.Enqueue(p.GetProperty("type").GetString() + ": " + (p.TryGetProperty("text", out var texto) ? texto.GetString() : "?"));
        else if (metodo == "network.beforeRequestSent")
            Pedidos.Enqueue(p.GetProperty("request").GetProperty("url").GetString()!);
    }

    public override async Task Navegar(string url)
    {
        await conexion.Enviar("browsingContext.navigate", new { context = contexto, url, wait = "complete" });
        await Task.Delay(150);
    }

    public override async Task Recargar()
    {
        await conexion.Enviar("browsingContext.reload", new { context = contexto, wait = "complete" });
        await Task.Delay(150);
    }

    public override Task Ventana(int ancho, int alto, double escala, bool movil) =>
        conexion.Enviar("browsingContext.setViewport", new { context = contexto, viewport = new { width = ancho, height = alto }, devicePixelRatio = escala });

    public override async Task<byte[]> Capturar() =>
        Convert.FromBase64String((await conexion.Enviar("browsingContext.captureScreenshot", new { context = contexto })).GetProperty("data").GetString()!);

    protected override async Task<string> EvaluarTexto(string expresion)
    {
        var resultado = await conexion.Enviar("script.evaluate", new { expression = expresion, target = new { context = contexto }, awaitPromise = true });
        if (resultado.GetProperty("type").GetString() == "exception") throw new InvalidOperationException("Error en la página: " + resultado.GetProperty("exceptionDetails"));
        return resultado.GetProperty("result").GetProperty("value").GetString()!;
    }
}
