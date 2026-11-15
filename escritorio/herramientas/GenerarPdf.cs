#:property PublishAot=false
// Genera el PDF de un documento Markdown de docs/. Desde la raíz del repositorio:
//
//   dotnet run escritorio/herramientas/GenerarPdf.cs -- docs/MANUAL-DE-USUARIO.md docs/pdf/MANUAL-DE-USUARIO.pdf "Manual de usuario"
//
// Convierte a HTML el Markdown que usan los documentos (títulos, párrafos, listas, tablas, código, enlaces e
// imágenes) con estilos de impresión y la tipografía del juego, y lo imprime en A4 con Microsoft Edge sin ventana,
// por su protocolo de DevTools. Los enlaces relativos pasan a apuntar al repositorio de GitHub. Sin paquetes externos.

using System.Diagnostics;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string Repositorio = "https://github.com/priscila845gallegos-a11y/base-lunar/blob/main/";
const string Web = "https://priscila845gallegos-a11y.github.io/base-lunar/";
const string Version = "Versión 1.0.0 · 15-11-2026";

if (args.Length < 3)
{
    Console.Error.WriteLine("Uso: dotnet run GenerarPdf.cs -- <entrada.md> <salida.pdf> <título>");
    return 1;
}

var entrada = Path.GetFullPath(args[0]);
var salida = Path.GetFullPath(args[1]);
var titulo = args[2];
var carpeta = Path.GetDirectoryName(entrada)!;
var raiz = carpeta;
while (!File.Exists(Path.Combine(raiz, "index.html"))) raiz = Path.GetDirectoryName(raiz) ?? throw new DirectoryNotFoundException("No se encontró la raíz del repositorio");

var cuerpo = new Markdown(carpeta, raiz, Repositorio).AHtml(File.ReadAllText(entrada));
var html = Plantilla(titulo, cuerpo, raiz);
var temporal = Path.Combine(Path.GetTempPath(), "BaseLunarPdf-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temporal);
try
{
    var archivoHtml = Path.Combine(temporal, "documento.html");
    File.WriteAllText(archivoHtml, html, new UTF8Encoding(false));
    var pie = $"""<div style="font-family:'Segoe UI',sans-serif;font-size:8pt;color:#64748B;width:100%;padding:0 16mm;display:flex;justify-content:space-between;"><span>Base Lunar: El Escudo Cósmico · {titulo} · v1.0.0</span><span><span class="pageNumber"></span> / <span class="totalPages"></span></span></div>""";
    var pdf = await Edge.Imprimir(archivoHtml, pie, Path.Combine(temporal, "perfil"));
    Directory.CreateDirectory(Path.GetDirectoryName(salida)!);
    File.WriteAllBytes(salida, pdf);
    // El árbol de páginas tiene nodos intermedios: la raíz es el único sin /Parent.
    var paginas = Regex.Matches(Encoding.Latin1.GetString(pdf), @"<<[^<>]*?/Type\s*/Pages\b[^<>]*?>>").Select(m => m.Value)
        .Where(nodo => !nodo.Contains("/Parent")).Select(nodo => Regex.Match(nodo, @"/Count\s+(\d+)").Groups[1].Value).FirstOrDefault() ?? "?";
    Console.WriteLine($"{Path.GetFileName(salida)}: {paginas} páginas, {pdf.Length / 1024} KB");
    return 0;
}
finally
{
    for (var intento = 0; intento < 10 && Directory.Exists(temporal); intento++)
    {
        try { Directory.Delete(temporal, true); }
        catch (IOException) { Thread.Sleep(300); }
        catch (UnauthorizedAccessException) { Thread.Sleep(300); }
    }
}

static string Plantilla(string titulo, string cuerpo, string raiz)
{
    string Archivo(string ruta) => new Uri(Path.Combine(raiz, ruta)).AbsoluteUri;
    return $$"""
        <!doctype html>
        <html lang="es">
        <head>
        <meta charset="utf-8">
        <title>Base Lunar: El Escudo Cósmico · {{titulo}}</title>
        <style>
          @font-face { font-family: 'Atkinson Hyperlegible'; src: url('{{Archivo("assets/fonts/atkinson-hyperlegible-latin-400.woff2")}}') format('woff2'); font-weight: 400; }
          @font-face { font-family: 'Atkinson Hyperlegible'; src: url('{{Archivo("assets/fonts/atkinson-hyperlegible-latin-700.woff2")}}') format('woff2'); font-weight: 700; }
          @page { size: A4; margin: 16mm 16mm 18mm 16mm; }
          html { font-family: 'Atkinson Hyperlegible', 'Segoe UI', 'Segoe UI Emoji', sans-serif; font-size: 10.5pt; line-height: 1.45; color: #0F172A; }
          body { margin: 0; }
          .portada { height: 255mm; display: flex; flex-direction: column; justify-content: center; break-after: page; }
          .portada .juego { font-size: 13pt; font-weight: 700; color: #EA580C; letter-spacing: .5pt; margin: 0; }
          .portada h1 { font-size: 30pt; line-height: 1.15; margin: 6pt 0 4pt; border: none; }
          .portada .version { color: #475569; margin: 0 0 18pt; }
          .portada img { width: 100%; border: 1px solid #CBD5E1; border-radius: 10pt; }
          .portada .nota { color: #334155; margin-top: 16pt; }
          .portada .enlaces { font-size: 9.5pt; color: #475569; }
          h1 { font-size: 20pt; margin: 0 0 8pt; }
          h2 { font-size: 15pt; color: #1D4ED8; border-bottom: 2px solid #EA580C; padding-bottom: 3pt; margin: 20pt 0 8pt; }
          h3 { font-size: 12pt; margin: 14pt 0 6pt; }
          h4 { font-size: 11pt; margin: 12pt 0 4pt; }
          h2, h3, h4 { break-after: avoid; }
          p { margin: 5pt 0; }
          ul, ol { margin: 4pt 0; padding-left: 16pt; }
          li { margin: 2pt 0; }
          table { width: 100%; border-collapse: collapse; margin: 8pt 0; font-size: 9.5pt; }
          tr { break-inside: avoid; }
          th, td { border: 1px solid #CBD5E1; padding: 4pt 6pt; text-align: left; vertical-align: top; }
          th { background: #E2E8F0; }
          code { font-family: Consolas, 'Cascadia Mono', monospace; font-size: 9pt; background: #F1F5F9; padding: 0 2pt; border-radius: 3pt; }
          pre { background: #F1F5F9; border: 1px solid #CBD5E1; border-radius: 6pt; padding: 8pt; white-space: pre-wrap; overflow-wrap: anywhere; break-inside: avoid; }
          pre code { background: none; padding: 0; font-size: 8.5pt; }
          img { max-width: 100%; }
          p.figura { text-align: center; break-inside: avoid; margin: 8pt 0; }
          p.figura img { max-height: 120mm; border: 1px solid #E2E8F0; border-radius: 8pt; }
          a { color: #1D4ED8; text-decoration: none; }
          hr { border: none; border-top: 1px solid #CBD5E1; margin: 12pt 0; }
        </style>
        </head>
        <body>
        <section class="portada">
          <p class="juego">BASE LUNAR: EL ESCUDO CÓSMICO</p>
          <h1>{{titulo}}</h1>
          <p class="version">{{Version}}</p>
          <img src="{{Archivo("assets/og-imagen.png")}}" alt="Pantalla de inicio del juego">
          <p class="nota">Juego educativo independiente de decisiones para niñas y niños de 7 a 12 años, con datos reales de NASA y CSA. No tiene respaldo oficial de ninguna agencia.</p>
          <p class="enlaces">Jugar: {{Web}}<br>Código y descargas: https://github.com/priscila845gallegos-a11y/base-lunar</p>
        </section>
        {{cuerpo}}
        </body>
        </html>
        """;
}

/// <summary>El Markdown que usan los documentos del proyecto, convertido a HTML.</summary>
sealed class Markdown(string carpeta, string raiz, string repositorio)
{
    private sealed class Nodo(int sangria, bool ordenada, string texto)
    {
        public int Sangria { get; } = sangria;
        public bool Ordenada { get; } = ordenada;
        public string Texto { get; set; } = texto;
        public List<Nodo> Hijos { get; } = [];
    }

    private static readonly Regex MarcaLista = new(@"^(\s*)([-*]|\d+\.)\s+(.*)$");

    public string AHtml(string markdown)
    {
        var lineas = markdown.Replace("\r\n", "\n").Split('\n');
        var html = new StringBuilder();
        var parrafo = new List<string>();
        var saltarTitulo = true; // el primer título va en la portada, junto con su línea de versión
        void CerrarParrafo()
        {
            if (parrafo.Count == 0) return;
            var texto = string.Join(" ", parrafo);
            parrafo.Clear();
            var clase = Regex.IsMatch(texto, @"^!\[[^\]]*\]\([^)]+\)$") ? " class=\"figura\"" : string.Empty;
            html.Append($"<p{clase}>").Append(Inline(texto)).Append("</p>\n");
        }

        for (var i = 0; i < lineas.Length;)
        {
            var linea = lineas[i];
            if (linea.StartsWith("```", StringComparison.Ordinal))
            {
                CerrarParrafo();
                var codigo = new List<string>();
                for (i++; i < lineas.Length && !lineas[i].StartsWith("```", StringComparison.Ordinal); i++) codigo.Add(lineas[i]);
                i++;
                html.Append("<pre><code>").Append(Escapar(string.Join("\n", codigo))).Append("</code></pre>\n");
                continue;
            }
            if (string.IsNullOrWhiteSpace(linea))
            {
                CerrarParrafo();
                i++;
                continue;
            }
            if (Regex.Match(linea, @"^(#{1,6})\s+(.*)$") is { Success: true } titulo)
            {
                CerrarParrafo();
                var nivel = titulo.Groups[1].Length;
                var texto = titulo.Groups[2].Value.Trim();
                i++;
                if (nivel == 1 && saltarTitulo)
                {
                    saltarTitulo = false;
                    while (i < lineas.Length && string.IsNullOrWhiteSpace(lineas[i])) i++;
                    if (i < lineas.Length && lineas[i].StartsWith("Versión", StringComparison.Ordinal)) i++;
                    continue;
                }
                html.Append($"<h{nivel} id=\"{Ancla(texto)}\">{Inline(texto)}</h{nivel}>\n");
                continue;
            }
            if (Regex.IsMatch(linea, @"^\s*(---|\*\*\*)\s*$"))
            {
                CerrarParrafo();
                html.Append("<hr>\n");
                i++;
                continue;
            }
            if (linea.TrimStart().StartsWith('|'))
            {
                CerrarParrafo();
                var filas = new List<string[]>();
                for (; i < lineas.Length && lineas[i].TrimStart().StartsWith('|'); i++)
                    filas.Add(lineas[i].Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray());
                html.Append("<table><thead><tr>");
                foreach (var celda in filas[0]) html.Append("<th>").Append(Inline(celda)).Append("</th>");
                html.Append("</tr></thead><tbody>\n");
                foreach (var fila in filas.Skip(2))
                {
                    html.Append("<tr>");
                    foreach (var celda in fila) html.Append("<td>").Append(Inline(celda)).Append("</td>");
                    html.Append("</tr>\n");
                }
                html.Append("</tbody></table>\n");
                continue;
            }
            if (MarcaLista.IsMatch(linea))
            {
                CerrarParrafo();
                i = Lista(lineas, i, html);
                continue;
            }
            if (linea.StartsWith('>'))
            {
                CerrarParrafo();
                var cita = new List<string>();
                for (; i < lineas.Length && lineas[i].StartsWith('>'); i++) cita.Add(lineas[i].TrimStart('>').TrimStart());
                html.Append("<blockquote><p>").Append(Inline(string.Join(" ", cita.Where(c => c.Length > 0)))).Append("</p></blockquote>\n");
                continue;
            }
            parrafo.Add(linea.Trim());
            i++;
        }
        CerrarParrafo();
        return html.ToString();
    }

    private int Lista(string[] lineas, int i, StringBuilder html)
    {
        var raices = new List<Nodo>();
        var pila = new Stack<Nodo>();
        Nodo? ultimo = null;
        for (; i < lineas.Length && !string.IsNullOrWhiteSpace(lineas[i]); i++)
        {
            if (MarcaLista.Match(lineas[i]) is { Success: true } marca)
            {
                var nodo = new Nodo(marca.Groups[1].Value.Length, char.IsDigit(marca.Groups[2].Value[0]), marca.Groups[3].Value.Trim());
                while (pila.Count > 0 && pila.Peek().Sangria >= nodo.Sangria) pila.Pop();
                (pila.Count == 0 ? raices : pila.Peek().Hijos).Add(nodo);
                pila.Push(nodo);
                ultimo = nodo;
            }
            else if (ultimo is not null && char.IsWhiteSpace(lineas[i][0]))
            {
                ultimo.Texto += " " + lineas[i].Trim();
            }
            else
            {
                break;
            }
        }
        Pintar(raices, html);
        return i;
    }

    private void Pintar(List<Nodo> nodos, StringBuilder html)
    {
        for (var k = 0; k < nodos.Count;)
        {
            var ordenada = nodos[k].Ordenada;
            html.Append(ordenada ? "<ol>" : "<ul>");
            for (; k < nodos.Count && nodos[k].Ordenada == ordenada; k++)
            {
                html.Append("<li>").Append(Inline(nodos[k].Texto));
                if (nodos[k].Hijos.Count > 0) Pintar(nodos[k].Hijos, html);
                html.Append("</li>");
            }
            html.Append(ordenada ? "</ol>\n" : "</ul>\n");
        }
    }

    /// <summary>
    /// Formato dentro de una línea. El código se aparta primero (con marcas), para que el resto del formato
    /// (negrita, enlaces) pueda envolverlo sin tocar lo que hay dentro.
    /// </summary>
    private string Inline(string texto)
    {
        var codigos = new List<string>();
        var t = Regex.Replace(texto, "`([^`]+)`", m =>
        {
            codigos.Add("<code>" + Escapar(m.Groups[1].Value) + "</code>");
            return $"\u0001{codigos.Count - 1}\u0002";
        });
        t = Escapar(t);
        t = Regex.Replace(t, @"!\[([^\]]*)\]\(([^)\s]+)\)", m => $"<img src=\"{Imagen(m.Groups[2].Value)}\" alt=\"{m.Groups[1].Value}\">");
        t = Regex.Replace(t, @"\[([^\]]+)\]\(([^)\s]+)\)", m => $"<a href=\"{Enlace(m.Groups[2].Value)}\">{m.Groups[1].Value}</a>");
        t = Regex.Replace(t, @"&lt;(https?://[^&\s]+)&gt;", m => $"<a href=\"{m.Groups[1].Value}\">{m.Groups[1].Value}</a>");
        t = Regex.Replace(t, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        return Regex.Replace(t, "\u0001(\\d+)\u0002", m => codigos[int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)]);
    }

    private string Imagen(string ruta) =>
        ruta.StartsWith("http", StringComparison.Ordinal) ? ruta : new Uri(Path.GetFullPath(Path.Combine(carpeta, ruta))).AbsoluteUri;

    /// <summary>Los enlaces a otros archivos del repositorio pasan a GitHub, para que funcionen en el PDF.</summary>
    private string Enlace(string url)
    {
        if (url.StartsWith('#') || url.StartsWith("http", StringComparison.Ordinal) || url.StartsWith("mailto:", StringComparison.Ordinal)) return url;
        var ancla = url.IndexOf('#') is var posicion and >= 0 ? url[posicion..] : string.Empty;
        var ruta = ancla.Length > 0 ? url[..url.IndexOf('#')] : url;
        var relativa = Path.GetRelativePath(raiz, Path.GetFullPath(Path.Combine(carpeta, ruta))).Replace('\\', '/');
        return repositorio + relativa + ancla;
    }

    /// <summary>El mismo identificador que GitHub da a cada título, para que funcione el índice.</summary>
    private static string Ancla(string titulo)
    {
        var limpio = Regex.Replace(titulo, @"\[([^\]]+)\]\([^)]+\)", "$1").Replace("`", string.Empty).Replace("*", string.Empty).ToLowerInvariant();
        var ancla = new StringBuilder();
        foreach (var c in limpio)
        {
            if (char.IsLetterOrDigit(c) || c is '-' or '_') ancla.Append(c);
            else if (c == ' ') ancla.Append('-');
        }
        return ancla.ToString();
    }

    private static string Escapar(string texto) =>
        texto.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}

/// <summary>Microsoft Edge sin ventana, manejado por su protocolo de DevTools, para imprimir a PDF.</summary>
static class Edge
{
    public static async Task<byte[]> Imprimir(string archivoHtml, string pie, string perfil)
    {
        var exe = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application\msedge.exe"),
        }.FirstOrDefault(File.Exists) ?? throw new FileNotFoundException("No se encontró Microsoft Edge");
        Directory.CreateDirectory(perfil);
        var inicio = new ProcessStartInfo(exe) { UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (var argumento in new[]
        {
            "--headless=new", "--remote-debugging-port=0", "--user-data-dir=" + perfil, "--no-first-run", "--no-default-browser-check",
            "--disable-extensions", "--disable-background-networking", "--disable-component-update", "--disable-sync", "--mute-audio", "about:blank",
        }) inicio.ArgumentList.Add(argumento);
        using var proceso = Process.Start(inicio)!;
        proceso.BeginErrorReadLine();
        proceso.BeginOutputReadLine();
        try
        {
            var archivoPuerto = Path.Combine(perfil, "DevToolsActivePort");
            string? puerto = null;
            for (var i = 0; i < 120 && puerto is null; i++)
            {
                try
                {
                    if (File.Exists(archivoPuerto) && File.ReadAllLines(archivoPuerto) is [var primera, ..] && primera.Length > 0) puerto = primera;
                }
                catch (IOException)
                {
                }
                if (puerto is null) await Task.Delay(250);
            }
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            using var lista = JsonDocument.Parse(await http.GetStringAsync($"http://127.0.0.1:{puerto ?? throw new TimeoutException("Edge no abrió DevTools")}/json/list"));
            var pestana = lista.RootElement.EnumerateArray().First(t => t.GetProperty("type").GetString() == "page").GetProperty("webSocketDebuggerUrl").GetString()!;
            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(pestana), CancellationToken.None);
            var cdp = new Cdp(socket);
            await cdp.Enviar("Page.enable");
            var cargado = cdp.Esperar("Page.loadEventFired");
            await cdp.Enviar("Page.navigate", new { url = new Uri(archivoHtml).AbsoluteUri });
            await cargado.WaitAsync(TimeSpan.FromSeconds(30));
            await cdp.Enviar("Runtime.evaluate", new
            {
                expression = "Promise.all([document.fonts.ready, ...[...document.images].map(i => i.complete ? 0 : new Promise(r => { i.onload = i.onerror = r; }))]).then(() => true)",
                awaitPromise = true,
            });
            var resultado = await cdp.Enviar("Page.printToPDF", new
            {
                printBackground = true,
                preferCSSPageSize = true,
                displayHeaderFooter = true,
                headerTemplate = "<span></span>",
                footerTemplate = pie,
                generateDocumentOutline = true,
                generateTaggedPDF = true,
            });
            return Convert.FromBase64String(resultado.GetProperty("data").GetString()!);
        }
        finally
        {
            try
            {
                if (!proceso.HasExited) proceso.Kill(entireProcessTree: true);
            }
            catch (Exception)
            {
            }
            proceso.WaitForExit(5000);
        }
    }
}

/// <summary>Conexión mínima al protocolo de DevTools: pedidos con id y espera de eventos.</summary>
sealed class Cdp
{
    private readonly ClientWebSocket socket;
    private readonly Dictionary<int, TaskCompletionSource<JsonElement>> pendientes = [];
    private readonly Dictionary<string, TaskCompletionSource> eventos = [];
    private readonly Lock cerrojo = new();
    private int ultimo;

    public Cdp(ClientWebSocket socket)
    {
        this.socket = socket;
        _ = Leer();
    }

    public Task Esperar(string evento)
    {
        var espera = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (cerrojo) eventos[evento] = espera;
        return espera.Task;
    }

    public async Task<JsonElement> Enviar(string metodo, object? parametros = null)
    {
        var respuesta = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        int id;
        lock (cerrojo)
        {
            id = ++ultimo;
            pendientes[id] = respuesta;
        }
        var json = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object> { ["id"] = id, ["method"] = metodo, ["params"] = parametros ?? new { } });
        await socket.SendAsync(json, WebSocketMessageType.Text, true, CancellationToken.None);
        return await respuesta.Task.WaitAsync(TimeSpan.FromSeconds(120));
    }

    private async Task Leer()
    {
        var bufer = new byte[1 << 20];
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
            using var documento = JsonDocument.Parse(mensaje.ToArray());
            mensaje.SetLength(0);
            var raiz = documento.RootElement.Clone();
            lock (cerrojo)
            {
                if (raiz.TryGetProperty("id", out var id) && pendientes.Remove(id.GetInt32(), out var respuesta))
                {
                    if (raiz.TryGetProperty("error", out var error)) respuesta.TrySetException(new InvalidOperationException(error.ToString()));
                    else respuesta.TrySetResult(raiz.GetProperty("result"));
                }
                else if (raiz.TryGetProperty("method", out var metodo) && eventos.Remove(metodo.GetString()!, out var espera))
                {
                    espera.TrySetResult();
                }
            }
        }
    }
}
