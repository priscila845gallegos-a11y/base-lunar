using System.Net;
using BaseLunar.Nucleo;
using System.Text;

namespace BaseLunar.Pruebas;

/// <summary>
/// NASA DONKI en vivo (F3.5, sección 7, D-09, D-19): validación, elección de la llamarada, orden de las fuentes,
/// espera máxima, caché de 6 h y respaldo ante cualquier fallo. Sin red: un servidor simulado responde.
/// </summary>
[TestClass]
public sealed class PruebasDonki
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 15, 10, 30, 0, TimeSpan.Zero);

    private static string Flr(string clase, string pico, string id = "X", string link = "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/1/-1") =>
        $$"""{"flrID":"{{id}}","classType":"{{clase}}","beginTime":"{{pico}}","peakTime":"{{pico}}","link":"{{link}}"}""";

    private static string Lista(params string[] llamaradas) => "[" + string.Join(",", llamaradas) + "]";

    /// <summary>Servidor simulado: responde según el host y anota cada pedido.</summary>
    private sealed class Servidor(Func<Uri, CancellationToken, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Pedidos { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken cancelacion)
        {
            Pedidos.Add(pedido);
            return responder(pedido.RequestUri!, cancelacion);
        }

        public static HttpResponseMessage Responder(HttpStatusCode codigo, string cuerpo = "") =>
            new(codigo) { Content = new StringContent(cuerpo, Encoding.UTF8, "application/json") };
    }

    private static Servidor PorHost(Func<string, HttpResponseMessage> porHost) =>
        new((uri, _) => Task.FromResult(porHost(uri.Host)));

    private static ClienteDonki Cliente(HttpMessageHandler servidor, string? cache = null, DateTimeOffset? ahora = null) =>
        new(servidor, cache, () => ahora ?? Ahora, TimeSpan.FromMilliseconds(200));

    private static string CacheTemporal() => Path.Combine(Path.GetTempPath(), "BaseLunarDonki-" + Guid.NewGuid().ToString("N"), "donki.json");

    // ---------- Validación y elección ----------

    [TestMethod(DisplayName = "Validación: lista JSON, clase letra + número, fecha válida y enlace https de la NASA")]
    public void Validacion()
    {
        var validas = ValidacionDonki.Leer(Lista(
            Flr("M3.5", "2026-11-05T12:00Z"),
            Flr("X1", "2026-11-06T08:15Z"),
            Flr("Z9.1", "2026-11-07T00:00Z"),
            Flr("m1.0", "2026-11-07T00:00Z"),
            Flr("X", "2026-11-07T00:00Z"),
            Flr("C2.0", "no-es-fecha"),
            Flr("C2.0", "2026-11-08T00:00Z", link: "http://ccmc.gsfc.nasa.gov/x"),
            Flr("C2.0", "2026-11-08T00:00Z", link: "https://evil.example.com/nasa.gov"),
            """{"classType":"C1.0"}"""))!;
        CollectionAssert.AreEqual(new[] { "M3.5", "X1" }, validas.Select(l => l.Clase).ToArray());
        Assert.IsNull(ValidacionDonki.Leer("""{"error":"no es una lista"}"""));
        Assert.IsNull(ValidacionDonki.Leer("<html>404</html>"));
        Assert.IsEmpty(ValidacionDonki.Leer("[]")!);
    }

    [TestMethod(DisplayName = "D-09: la más intensa de los 30 días; si empatan, la más reciente")]
    public void MasIntensa()
    {
        Llamarada L(string clase, string fecha) => new("id", clase, DateTimeOffset.Parse(fecha, System.Globalization.CultureInfo.InvariantCulture), fecha, "https://ccmc.gsfc.nasa.gov/");
        Assert.AreEqual("M3.5", ValidacionDonki.MasIntensa([L("M3.5", "2026-11-05T00:00Z"), L("M1.7", "2026-11-11T00:00Z"), L("C9.9", "2026-11-12T00:00Z")])!.Clase,
            "El ejemplo de D-09: la M3.5 del 05-11 gana a la M1.7 más reciente");
        Assert.AreEqual("X1.0", ValidacionDonki.MasIntensa([L("M9.9", "2026-11-01T00:00Z"), L("X1.0", "2026-10-20T00:00Z")])!.Clase);
        Assert.AreEqual("2026-11-09T00:00Z", ValidacionDonki.MasIntensa([L("X1.0", "2026-11-01T00:00Z"), L("X1.0", "2026-11-09T00:00Z")])!.FechaTexto);
        Assert.IsNull(ValidacionDonki.MasIntensa([]));
        Assert.AreEqual(2 * ValidacionDonki.Intensidad("X1"), ValidacionDonki.Intensidad("X2"), 1e-12);
        Assert.AreEqual(10 * ValidacionDonki.Intensidad("M1"), ValidacionDonki.Intensidad("X1"), 1e-12);
    }

    // ---------- Fuentes y fallos ----------

    [TestMethod(DisplayName = "D-19: primero api.nasa.gov con la clave personal; después el CCMC sin clave; rango de 30 días")]
    public void Direcciones()
    {
        var direcciones = ClienteDonki.Direcciones(Ahora);
        Assert.HasCount(2, direcciones);
        Assert.AreEqual($"https://api.nasa.gov/DONKI/FLR?startDate=2026-10-16&endDate=2026-11-15&api_key={Configuracion.Nasa.Clave}", direcciones[0].AbsoluteUri);
        Assert.AreEqual("https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR?startDate=2026-10-16&endDate=2026-11-15", direcciones[1].AbsoluteUri);
    }

    [TestMethod(DisplayName = "Si api.nasa.gov responde 404, el CCMC da el dato EN VIVO con su hora de consulta")]
    public async Task SegundaVia()
    {
        var servidor = PorHost(host => host == "api.nasa.gov"
            ? Servidor.Responder(HttpStatusCode.NotFound)
            : Servidor.Responder(HttpStatusCode.OK, Lista(Flr("M3.5", "2026-11-05T12:00Z", link: "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/42/-1"), Flr("M1.7", "2026-11-11T09:00Z"), Flr("X1.1", "2026-10-20T03:00Z"))));
        using var cliente = Cliente(servidor);
        var tormenta = await cliente.ObtenerAsync();
        Assert.AreEqual(new Tormenta("X1.1", "2026-10-20T03:00Z", "NASA DONKI", "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/1/-1", true, 1, Ahora), tormenta);
        CollectionAssert.AreEqual(new[] { "api.nasa.gov", "ccmc.gsfc.nasa.gov" }, servidor.Pedidos.Select(p => p.RequestUri!.Host).ToArray());
        Assert.AreEqual("BaseLunar/1.0", servidor.Pedidos[0].Headers.UserAgent.ToString(), "Un nombre de cliente genérico, sin datos personales");
    }

    [TestMethod(DisplayName = "Privacidad: solo pedidos GET a la NASA, sin cuerpo ni cookies; la dirección lleva solo fechas y la clave")]
    public async Task Privacidad()
    {
        var servidor = PorHost(host => host == "api.nasa.gov"
            ? Servidor.Responder(HttpStatusCode.NotFound)
            : Servidor.Responder(HttpStatusCode.OK, Lista(Flr("M3.5", "2026-11-05T12:00Z"))));
        using var cliente = Cliente(servidor);
        await cliente.ObtenerAsync();
        Assert.HasCount(2, servidor.Pedidos);
        foreach (var pedido in servidor.Pedidos)
        {
            Assert.AreEqual(HttpMethod.Get, pedido.Method);
            Assert.IsNull(pedido.Content, "Sin cuerpo: no se envía nada");
            Assert.IsFalse(pedido.Headers.Contains("Cookie"));
            Assert.AreEqual("https", pedido.RequestUri!.Scheme);
            Assert.IsTrue(pedido.RequestUri.Host is "api.nasa.gov" or "ccmc.gsfc.nasa.gov", pedido.RequestUri.Host);
            var claves = pedido.RequestUri.Query.TrimStart('?').Split('&').Select(p => p.Split('=')[0]).ToArray();
            Assert.IsTrue(claves.All(c => c is "startDate" or "endDate" or "api_key"), string.Join(",", claves));
            CollectionAssert.AreEquivalent(new[] { "User-Agent", "Accept" }, pedido.Headers.Select(h => h.Key).ToArray(), "Ninguna cabecera con datos personales");
            Assert.AreEqual("BaseLunar/1.0", pedido.Headers.UserAgent.ToString());
            Assert.AreEqual("application/json", pedido.Headers.Accept.ToString());
        }
    }

    public static IEnumerable<object[]> Fallos() =>
    [
        ["404 y 503", (Func<string, HttpResponseMessage>)(h => Servidor.Responder(h == "api.nasa.gov" ? HttpStatusCode.NotFound : HttpStatusCode.ServiceUnavailable))],
        ["429 y 403", (Func<string, HttpResponseMessage>)(h => Servidor.Responder(h == "api.nasa.gov" ? HttpStatusCode.TooManyRequests : HttpStatusCode.Forbidden))],
        ["JSON inválido", (Func<string, HttpResponseMessage>)(_ => Servidor.Responder(HttpStatusCode.OK, "<html>no es JSON</html>"))],
        ["lista vacía", (Func<string, HttpResponseMessage>)(_ => Servidor.Responder(HttpStatusCode.OK, "[]"))],
        ["sin llamaradas válidas", (Func<string, HttpResponseMessage>)(_ => Servidor.Responder(HttpStatusCode.OK, Lista(Flr("Q1", "2026-11-05T12:00Z"))))],
        ["sin red", (Func<string, HttpResponseMessage>)(_ => throw new HttpRequestException("sin conexión"))],
    ];

    [TestMethod(DisplayName = "Ante cualquier fallo de las dos vías, el respaldo histórico real (X8.7), nunca un error")]
    [DynamicData(nameof(Fallos))]
    public async Task Respaldo(string caso, Func<string, HttpResponseMessage> porHost)
    {
        var registro = new List<string>();
        using var cliente = new ClienteDonki(PorHost(porHost), null, () => Ahora, TimeSpan.FromMilliseconds(200), registro.Add);
        var tormenta = await cliente.ObtenerAsync();
        Assert.AreEqual(DatosDonki.RespaldoHistorico(), tormenta, caso);
        Assert.IsFalse(tormenta.EnVivo);
        Assert.HasCount(2, registro, "Los dos intentos quedan en el registro técnico (nunca en pantalla)");
    }

    [TestMethod(DisplayName = "Espera máxima: si ninguna vía responde a tiempo, sigue el respaldo sin colgarse")]
    public async Task EsperaMaxima()
    {
        var servidor = new Servidor(async (_, cancelacion) =>
        {
            await Task.Delay(Timeout.Infinite, cancelacion);
            return Servidor.Responder(HttpStatusCode.OK, "[]");
        });
        using var cliente = Cliente(servidor);
        var reloj = System.Diagnostics.Stopwatch.StartNew();
        var tormenta = await cliente.ObtenerAsync();
        Assert.IsFalse(tormenta.EnVivo);
        Assert.IsLessThan(3000L, reloj.ElapsedMilliseconds, "Dos esperas de 200 ms, no más");
    }

    // ---------- Caché de 6 horas ----------

    [TestMethod(DisplayName = "Caché de 6 h: no vuelve a consultar antes; después sí; una caché dañada o insegura se ignora")]
    public async Task Cache()
    {
        var cache = CacheTemporal();
        try
        {
            var servidor = PorHost(_ => Servidor.Responder(HttpStatusCode.OK, Lista(Flr("M2.4", "2026-11-14T07:00Z"))));
            using (var cliente = Cliente(servidor, cache)) Assert.AreEqual("M2.4", (await cliente.ObtenerAsync()).Clase);
            Assert.HasCount(1, servidor.Pedidos);

            using (var cliente = Cliente(servidor, cache, Ahora.AddHours(5.9)))
            {
                var cacheada = await cliente.ObtenerAsync();
                Assert.IsTrue(cacheada.EnVivo, "Menos de 6 h: sigue siendo EN VIVO");
                Assert.AreEqual(Ahora, cacheada.Consulta, "Con la hora de la consulta original");
            }
            Assert.HasCount(1, servidor.Pedidos, "Sin pedir de nuevo");

            using (var cliente = Cliente(servidor, cache, Ahora.AddHours(6.1))) await cliente.ObtenerAsync();
            Assert.HasCount(2, servidor.Pedidos, "Pasadas 6 h, consulta otra vez");

            var sinRed = PorHost(_ => throw new HttpRequestException());
            File.WriteAllText(cache, "{esto no es json");
            using (var cliente = Cliente(sinRed, cache)) Assert.IsFalse((await cliente.ObtenerAsync()).EnVivo, "Caché dañada: se ignora");
            File.WriteAllText(cache, $$"""{"Clase":"X9.9","Fecha":"2026-11-14T07:00Z","Url":"https://evil.example.com/","LlamaradasX":1,"Consulta":"{{Ahora:O}}"}""");
            using (var cliente = Cliente(sinRed, cache)) Assert.IsFalse((await cliente.ObtenerAsync()).EnVivo, "Enlace inseguro: se ignora");
            File.WriteAllText(cache, $$"""{"Clase":"X9.9","Fecha":"2026-11-14T07:00Z","Url":"https://ccmc.gsfc.nasa.gov/","LlamaradasX":1,"Consulta":"{{Ahora.AddHours(2):O}}"}""");
            using (var cliente = Cliente(sinRed, cache)) Assert.IsFalse((await cliente.ObtenerAsync()).EnVivo, "Consulta en el futuro: se ignora");
        }
        finally
        {
            var carpeta = Path.GetDirectoryName(cache)!;
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, recursive: true);
        }
    }

    // ---------- Red real ----------

    [TestMethod(DisplayName = "Red real: consulta NASA DONKI ahora (con red, dato en vivo; sin red, respaldo)")]
    [TestCategory("Red")]
    public async Task RedReal()
    {
        var registro = new List<string>();
        using var cliente = new ClienteDonki(registro: registro.Add);
        var tormenta = await cliente.ObtenerAsync();
        Assert.IsTrue(ValidacionDonki.ClaseValida(tormenta.Clase));
        Assert.IsTrue(ValidacionDonki.EnlaceSeguro(tormenta.Url));
        Console.WriteLine($"Resultado: {tormenta}");
        foreach (var linea in registro) Console.WriteLine(linea);
    }
}
