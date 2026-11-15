using System.Diagnostics;
using System.Windows.Automation;

namespace BaseLunar.PruebasUI;

/// <summary>
/// Partidas completas desde fuera, como las vería un lector de pantalla: se abre BaseLunar.App.exe y se
/// juega con UI Automation (sin ratón ni teclado reales, así que no toca lo que haga quien esté al equipo).
/// Cada prueba abre su propia ventana unos segundos.
/// </summary>
[TestClass]
[TestCategory("UIAutomation")]
public sealed class PruebasAutomatizacion
{
    private const int Espera = 10000;

    [TestMethod(DisplayName = "UI Automation: partida completa hasta la Victoria (A, B, A, A, A) con certificado 3 de 3")]
    public void PartidaHastaLaVictoria()
    {
        using var app = new AppAbierta();
        app.Pulsar("BotonPrincipal");
        app.EsperarNombre("ProgresoTexto", "Nivel 1 de 5");
        Assert.StartsWith("Opción A: Apagar recicladores de agua. Gana 20 de Batería", app.Elemento("OpcionA").Current.Name);

        string[] opciones = ["A", "B", "A", "A", "A"];
        for (var i = 0; i < opciones.Length; i++)
        {
            var nivel = Guion.Niveles[i];
            app.EsperarNombre("TituloNivel", $"Nivel {nivel.Id}: {nivel.Titulo}");
            app.Pulsar("Opcion" + opciones[i]);
            app.EsperarNombre("TextoResultado", nivel.Opciones[opciones[i]].Resultado);
            if (i == 0)
            {
                Assert.AreEqual(100, app.Valor("MedidorBateria"));
                Assert.AreEqual(45, app.Valor("MedidorSoporte"));
                app.EsperarNombre("AnuncioCortes", n => n.StartsWith(nivel.Opciones["A"].Resultado, StringComparison.Ordinal) && n.EndsWith(nivel.Dato.Texto, StringComparison.Ordinal));
            }
            app.Pulsar("BotonPrincipal");
        }

        app.EsperarNombre("TituloVictoria", "Certificado de Comandante Junior de la Base Lunar");
        app.EsperarNombre("RespaldosCertificado", "Respaldos sin usar: 3 de 3");
        app.EsperarNombre("ProgresoTexto", "¡Misión cumplida!");
        Assert.AreEqual(100, app.Valor("MedidorProteccion"));
        Assert.AreEqual(40, app.Valor("MedidorSoporte"));
        Assert.AreEqual(70, app.Valor("MedidorBateria"));
        app.EsperarNombre("BotonPrincipal", "Jugar de nuevo");
    }

    [TestMethod(DisplayName = "UI Automation: tres respaldos en el Nivel 4 llevan a Misión Abortada; «Jugar de nuevo» vuelve al Inicio")]
    public void PartidaHastaMisionAbortada()
    {
        using var app = new AppAbierta();
        app.Pulsar("BotonPrincipal");
        foreach (var opcion in new[] { "A", "A", "A" })
        {
            app.Pulsar("Opcion" + opcion);
            app.Pulsar("BotonPrincipal");
        }
        app.EsperarNombre("TituloNivel", "Nivel 4: Tormenta Solar");
        app.EsperarNombre("AvisoX", "Esta llamarada es de clase X, la más fuerte. La protección bajó 5 puntos.");
        app.EsperarNombre("EtiquetaNivel", "Dato HISTÓRICO REAL: NASA DONKI");

        for (var quedan = 2; quedan >= 1; quedan--)
        {
            app.Pulsar("OpcionA");
            app.EsperarNombre("MensajeRespaldo", Textos.Respaldo.Soporte);
            app.EsperarNombre("AnuncioUrgente", Textos.Respaldo.Soporte);
            app.EsperarNombre("RespaldosTexto", $"Protocolos de Respaldo: quedan {quedan} de 3");
            app.EsperarNombre("BotonPrincipal", "Reintentar");
            app.Pulsar("BotonPrincipal");
            app.EsperarNombre("TituloNivel", "Nivel 4: Tormenta Solar");
            Assert.AreEqual(30, app.Valor("MedidorSoporte"), "El reintento restaura Soporte Vital a 30 (D-01)");
        }
        app.Pulsar("OpcionA");
        app.EsperarNombre("TituloAbortada", "Misión Abortada");
        app.EsperarNombre("BotonPrincipal", "Jugar de nuevo");
        app.Pulsar("BotonPrincipal");
        app.EsperarNombre("TituloInicio", "¡Hola, comandante!");
        app.EsperarNombre("ProgresoTexto", "Antes de empezar");
        app.EsperarNombre("RespaldosTexto", "Protocolos de Respaldo: quedan 3 de 3");
        Assert.AreEqual(80, app.Valor("MedidorProteccion"));
    }

    [TestMethod(DisplayName = "UI Automation: doble clic. Una segunda pulsación no decide dos veces ni elige en el nivel siguiente")]
    public void DobleClic()
    {
        using var app = new AppAbierta();
        app.Pulsar("BotonPrincipal");

        // Dos pulsaciones seguidas sobre A: se aplica una sola decisión.
        var opcionA = app.EsperarHabilitado("OpcionA");
        var invocar = (InvokePattern)opcionA.GetCurrentPattern(InvokePattern.Pattern);
        invocar.Invoke();
        AppAbierta.IntentarInvocar(invocar);
        app.EsperarNombre("TextoResultado", Guion.Niveles[0].Opciones["A"].Resultado);
        Assert.AreEqual(100, app.Valor("MedidorBateria"));
        Assert.AreEqual(45, app.Valor("MedidorSoporte"));

        // «Continuar» y enseguida A (donde estaba el botón): A aparece bloqueada y no elige.
        app.Pulsar("BotonPrincipal");
        var bloqueada = app.EsperarPresente("OpcionA");
        Assert.IsFalse(bloqueada.Current.IsEnabled, "Durante la transición (450 ms) las opciones están bloqueadas");
        AppAbierta.IntentarInvocar((InvokePattern)bloqueada.GetCurrentPattern(InvokePattern.Pattern));
        app.EsperarHabilitado("OpcionA");
        app.EsperarNombre("TituloNivel", "Nivel 2: El Huerto Espacial");
        Assert.IsNull(app.Buscar("TextoResultado"), "No se eligió nada en el Nivel 2");
        Assert.AreEqual(45, app.Valor("MedidorSoporte"));
    }

    /// <summary>Abre el juego, encuentra su ventana y la cierra al terminar.</summary>
    private sealed class AppAbierta : IDisposable
    {
        private readonly Process proceso;
        private readonly AutomationElement ventana;

        public AppAbierta()
        {
            proceso = Process.Start(new ProcessStartInfo(Rutas.Ejecutable()) { UseShellExecute = false })
                ?? throw new InvalidOperationException("No se pudo abrir el juego");
            var condicion = new AndCondition(
                new PropertyCondition(AutomationElement.ProcessIdProperty, proceso.Id),
                new PropertyCondition(AutomationElement.AutomationIdProperty, "VentanaBaseLunar"));
            AutomationElement? encontrada = null;
            Esperar(() => (encontrada = AutomationElement.RootElement.FindFirst(TreeScope.Children, condicion)) is not null, "la ventana del juego", 20000);
            ventana = encontrada!;
        }

        public AutomationElement? Buscar(string id) =>
            ventana.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, id));

        public AutomationElement Elemento(string id) => Buscar(id) ?? throw new AssertFailedException($"No se encontró «{id}»");

        public AutomationElement EsperarPresente(string id)
        {
            AutomationElement? elemento = null;
            Esperar(() => (elemento = Buscar(id)) is not null, id);
            return elemento!;
        }

        public AutomationElement EsperarHabilitado(string id)
        {
            AutomationElement? elemento = null;
            Esperar(() => (elemento = Buscar(id)) is { Current.IsEnabled: true }, id + " habilitado");
            return elemento!;
        }

        /// <summary>Pulsa un botón en cuanto está disponible (pasada la transición).</summary>
        public void Pulsar(string id) => ((InvokePattern)EsperarHabilitado(id).GetCurrentPattern(InvokePattern.Pattern)).Invoke();

        public void EsperarNombre(string id, string esperado) => EsperarNombre(id, n => n == esperado, esperado);

        public void EsperarNombre(string id, Func<string, bool> condicion, string? descripcion = null)
        {
            string? ultimo = null;
            var ok = EsperarSinFallar(() => Buscar(id) is { } e && condicion(ultimo = e.Current.Name), Espera);
            if (!ok) throw new AssertFailedException($"«{id}» no llegó a «{descripcion ?? "la condición"}». Último valor: «{ultimo}»");
        }

        /// <summary>Valor de una barra de medidor (patrón RangeValue), ya terminada su animación.</summary>
        public int Valor(string id)
        {
            var patron = (RangeValuePattern)Elemento(id).GetCurrentPattern(RangeValuePattern.Pattern);
            double anterior = -1;
            EsperarSinFallar(() =>
            {
                var actual = patron.Current.Value;
                var estable = actual == anterior && actual == Math.Round(actual);
                anterior = actual;
                return estable;
            }, Espera);
            return (int)Math.Round(patron.Current.Value);
        }

        /// <summary>Invoca y acepta que el botón esté bloqueado u oculto: es lo que se quiere comprobar.</summary>
        public static void IntentarInvocar(InvokePattern patron)
        {
            try
            {
                patron.Invoke();
            }
            catch (ElementNotEnabledException)
            {
            }
            catch (ElementNotAvailableException)
            {
            }
        }

        private static void Esperar(Func<bool> condicion, string que, int milisegundos = Espera)
        {
            if (!EsperarSinFallar(condicion, milisegundos)) throw new AssertFailedException($"Tiempo agotado esperando {que}");
        }

        private static bool EsperarSinFallar(Func<bool> condicion, int milisegundos)
        {
            var reloj = Stopwatch.StartNew();
            while (reloj.ElapsedMilliseconds < milisegundos)
            {
                try
                {
                    if (condicion()) return true;
                }
                catch (ElementNotAvailableException)
                {
                }
                Thread.Sleep(25);
            }
            return false;
        }

        public void Dispose()
        {
            try
            {
                if (!proceso.HasExited)
                {
                    proceso.CloseMainWindow();
                    if (!proceso.WaitForExit(3000)) proceso.Kill();
                }
            }
            finally
            {
                proceso.Dispose();
            }
        }
    }
}
