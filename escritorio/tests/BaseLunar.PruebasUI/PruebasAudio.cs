using System.IO;
using System.Windows;
using BaseLunar.App.Audio;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.PruebasUI;

/// <summary>
/// F3.4: efectos sintetizados (WAV válidos, sin saturar ni chasquear), qué suena tras cada acción, el botón
/// de efectos con su preferencia guardada, la voz local y la salida de audio de Windows.
/// </summary>
[TestClass]
public sealed class PruebasAudio
{
    // ---------- Sintetizador ----------

    [TestMethod(DisplayName = "Cada efecto es un WAV PCM de 16 bits mono a 22 050 Hz, corto, audible y sin saturar")]
    public void EfectosValidos()
    {
        foreach (var sonido in Enum.GetValues<Sonido>())
        {
            var wav = Sintetizador.Wav(sonido);
            Assert.AreEqual("RIFF", System.Text.Encoding.ASCII.GetString(wav, 0, 4), sonido.ToString());
            Assert.AreEqual("WAVE", System.Text.Encoding.ASCII.GetString(wav, 8, 4));
            Assert.AreEqual(1, BitConverter.ToInt16(wav, 20), "PCM");
            Assert.AreEqual(1, BitConverter.ToInt16(wav, 22), "mono");
            Assert.AreEqual(Sintetizador.Muestreo, BitConverter.ToInt32(wav, 24));
            Assert.AreEqual(16, BitConverter.ToInt16(wav, 34));
            Assert.AreEqual(wav.Length - 44, BitConverter.ToInt32(wav, 40), "Tamaño de los datos");
            var duracion = Sintetizador.Duracion(sonido);
            Assert.IsTrue(duracion is >= 50 and <= 1500, $"{sonido}: {duracion} ms");
            var muestras = Enumerable.Range(0, (wav.Length - 44) / 2).Select(i => (int)BitConverter.ToInt16(wav, 44 + i * 2)).ToArray();
            var pico = muestras.Max(Math.Abs);
            Assert.IsLessThanOrEqualTo((int)(0.86 * short.MaxValue), pico, $"{sonido} satura");
            Assert.IsGreaterThan(2000, pico, $"{sonido} casi no se oye");
            Assert.IsLessThan(1500, Math.Abs(muestras[0]), $"{sonido} empieza con un chasquido");
            Assert.IsLessThan(1500, Math.Abs(muestras[^1]), $"{sonido} termina con un chasquido");
        }
    }

    // ---------- Qué suena tras cada acción ----------

    private static IReadOnlyList<Pista> Ultimos(string[] pasos)
    {
        var reproductor = new ReproductorEspia();
        var juego = new JuegoVM(DatosDonki.RespaldoHistorico(), reproductor, new NarradorMudo(), Ajustes.EnMemoria());
        foreach (var paso in pasos) Partida.Paso(juego, paso);
        return reproductor.Programados[^1];
    }

    [TestMethod(DisplayName = "Sonidos de cada momento: elegir, ganar, costar, al máximo, superar, respaldo, llamarada, victoria y abortada")]
    public void SonidosDeCadaMomento() => HiloUI.Ejecutar(() =>
    {
        CollectionAssert.AreEqual(new[] { new Pista(Sonido.Seleccion), new Pista(Sonido.Ganancia, 300), new Pista(Sonido.Costo, 600), new Pista(Sonido.NivelSuperado, 1300) },
            Ultimos(Partida.HastaResultado1).ToArray());
        CollectionAssert.Contains(Ultimos(["P", "A", "P", "A"]).ToArray(), new Pista(Sonido.AlMaximo, 850), "N2-A sube Protección por encima de 100");
        CollectionAssert.AreEqual(new[] { new Pista(Sonido.Seleccion), new Pista(Sonido.NivelSuperado, 1300) }, Ultimos([.. Partida.HastaNivel5, "A"]).ToArray());
        CollectionAssert.AreEqual(new[] { new Pista(Sonido.Entrar), new Pista(Sonido.Llamarada, 1900) }, Ultimos(Partida.HastaNivel4).ToArray());
        CollectionAssert.AreEqual(new[] { new Pista(Sonido.Seleccion), new Pista(Sonido.Respaldo, 350) }, Ultimos(Partida.HastaRespaldo4).ToArray());
        CollectionAssert.AreEqual(new[] { new Pista(Sonido.Rebobinar) }, Ultimos([.. Partida.HastaRespaldo4, "P"]).ToArray());
        CollectionAssert.AreEqual(new[] { new Pista(Sonido.Victoria) }, Ultimos(Partida.HastaVictoria).ToArray());
        CollectionAssert.AreEqual(new[] { new Pista(Sonido.Seleccion), new Pista(Sonido.Respaldo, 350), new Pista(Sonido.Abortada, 1600) },
            Ultimos(Partida.HastaAbortada).ToArray());
    });

    [TestMethod(DisplayName = "Con los efectos apagados no se programa ningún sonido; el glosario suena con su «pop»")]
    public void EfectosApagados() => HiloUI.Ejecutar(() =>
    {
        var reproductor = new ReproductorEspia();
        var juego = new JuegoVM(DatosDonki.RespaldoHistorico(), reproductor, new NarradorMudo(), Ajustes.EnMemoria());
        juego.SonidoActivo = false;
        Assert.AreEqual(1, reproductor.Cancelaciones, "Al apagar, se cancelan los pendientes");
        Partida.Paso(juego, "P");
        Partida.Paso(juego, "A");
        Assert.IsEmpty(reproductor.Programados);
        juego.SonidoActivo = true;
        foreach (var paso in new[] { "P", "B", "P" }) Partida.Paso(juego, paso);
        juego.VerTermino.Execute(juego.Terminos[0]);
        CollectionAssert.AreEqual(new[] { Sonido.Glosario }, reproductor.Sonados.ToArray());
    });

    // ---------- Ajustes guardados ----------

    [TestMethod(DisplayName = "La preferencia de efectos se guarda en un JSON local; un archivo dañado vuelve a los valores por defecto")]
    public void AjustesGuardados()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "BaseLunarPruebas-" + Guid.NewGuid().ToString("N"));
        var ruta = Path.Combine(carpeta, "ajustes.json");
        try
        {
            Assert.IsTrue(Ajustes.Cargar(ruta).Sonido, "Sin archivo: efectos encendidos");
            var ajustes = Ajustes.Cargar(ruta);
            ajustes.Sonido = false;
            ajustes.Guardar();
            Assert.IsFalse(Ajustes.Cargar(ruta).Sonido, "Se recuerda entre partidas");
            Assert.AreEqual("{\"Sonido\":false}", File.ReadAllText(ruta), "Solo se guarda esta preferencia");
            File.WriteAllText(ruta, "{esto no es json");
            Assert.IsTrue(Ajustes.Cargar(ruta).Sonido, "Archivo dañado: valores por defecto");
        }
        finally
        {
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, recursive: true);
        }
    }

    // ---------- Voz ----------

    [TestMethod(DisplayName = "«Escuchar» lee la tarjeta de cada fase, sin emojis, y la voz se calla al actuar")]
    public void Escuchar() => HiloUI.Ejecutar(() =>
    {
        var narrador = new NarradorEspia();
        var juego = new JuegoVM(DatosDonki.RespaldoHistorico(), new ReproductorMudo(), narrador, Ajustes.EnMemoria());
        Assert.IsTrue(juego.PuedeEscuchar);

        juego.Escuchar.Execute(null);
        StringAssert.StartsWith(narrador.Leidos[^1], "¡Hola, comandante! La NASA planea construir una base en la Luna.");
        StringAssert.Contains(narrador.Leidos[^1], "Cuida los 3 medidores: Protección, Soporte Vital y Batería.");

        Partida.Paso(juego, "P");
        Assert.IsGreaterThanOrEqualTo(1, narrador.Silencios, "Al actuar, la voz se calla");
        juego.Escuchar.Execute(null);
        StringAssert.StartsWith(narrador.Leidos[^1], "Nivel 1: La Noche Lunar. ¡Empezó la noche lunar!");

        Partida.Paso(juego, "A");
        juego.Escuchar.Execute(null);
        StringAssert.StartsWith(narrador.Leidos[^1], Guion.Niveles[0].Opciones["A"].Resultado + " Dato real: ");

        var respaldo = new JuegoVM(DatosDonki.RespaldoHistorico(), new ReproductorMudo(), narrador, Ajustes.EnMemoria());
        foreach (var paso in Partida.HastaRespaldo4) Partida.Paso(respaldo, paso);
        respaldo.Escuchar.Execute(null);
        Assert.AreEqual(Textos.Respaldo.Soporte + " " + Textos.Respaldo.Pista, narrador.Leidos[^1]);

        foreach (var texto in narrador.Leidos) Assert.IsFalse(texto.Any(char.IsSurrogate), $"Sin emojis: «{texto}»");
    });

    [TestMethod(DisplayName = "Sin voz local en español, el botón «Escuchar» no se muestra; el de efectos sí, con su nombre de la web")]
    public void BotonesDeLaBarra() => HiloUI.Ejecutar(() =>
    {
        var sinVoz = Capturas.Montar(new JuegoVM(), 1280, 720);
        Assert.AreEqual(Visibility.Collapsed, sinVoz.Estado.BotonEscuchar.Visibility);
        Assert.AreEqual(Visibility.Visible, sinVoz.Estado.BotonSonido.Visibility);
        Assert.AreEqual("Efectos de sonido", System.Windows.Automation.AutomationProperties.GetName(sinVoz.Estado.BotonSonido));
        Assert.IsTrue(sinVoz.Estado.BotonSonido.IsChecked);

        var juego = new JuegoVM(DatosDonki.RespaldoHistorico(), new ReproductorMudo(), new NarradorEspia(), Ajustes.EnMemoria());
        var conVoz = Capturas.Montar(juego, 1280, 720);
        Assert.AreEqual(Visibility.Visible, conVoz.Estado.BotonEscuchar.Visibility);
        Assert.AreEqual("Escuchar", System.Windows.Automation.AutomationProperties.GetName(conVoz.Estado.BotonEscuchar));
        juego.SonidoActivo = false;
        Assert.IsFalse(conVoz.Estado.BotonSonido.IsChecked, "El botón refleja la preferencia");
        Capturas.Guardar(conVoz, "f34-barra-sin-efectos");
        var compacta = Capturas.Montar(juego, 800, 600);
        Assert.AreEqual(Visibility.Collapsed, compacta.Estado.TextoEscuchar.Visibility, "En la compacta, solo el ícono");
        Capturas.Guardar(compacta, "f34-barra-compacta");
    });

    // ---------- Dispositivos reales de Windows ----------

    [TestMethod(DisplayName = "Salida de audio real (winmm): abre, reproduce un WAV en silencio y libera la memoria")]
    public void SalidaDeAudioReal() => HiloUI.Ejecutar(() =>
    {
        using var reproductor = new ReproductorWindows();
        var silencio = Sintetizador.Empaquetar(new float[Sintetizador.Muestreo / 20]); // 50 ms en silencio
        reproductor.SonarWav(silencio);
        reproductor.SonarWav(silencio);
        if (reproductor.Roto)
        {
            Assert.Inconclusive("Este equipo no tiene salida de audio: el juego seguiría en silencio.");
            return;
        }
        Assert.AreEqual(2, reproductor.Activas, "Dos sonidos a la vez, cada uno con su salida");
        Assert.IsTrue(HiloUI.Esperar(() =>
        {
            reproductor.LimpiarTerminadas();
            return reproductor.Activas == 0;
        }, 3000), "Las salidas terminadas se liberan");
    });

    [TestMethod(DisplayName = "Voz local (SAPI): se inicia sin conectarse y solo con una voz en español")]
    public void VozLocal()
    {
        using var narrador = new NarradorWindows();
        if (!narrador.Disponible)
        {
            Assert.Inconclusive("Este equipo no tiene voces en español: el botón «Escuchar» se ocultaría.");
            return;
        }
        Assert.IsNotNull(narrador.NombreVoz);
        using var sintetizador = new System.Speech.Synthesis.SpeechSynthesizer();
        var voz = sintetizador.GetInstalledVoices().Single(v => v.VoiceInfo.Name == narrador.NombreVoz).VoiceInfo;
        Assert.AreEqual("es", voz.Culture.TwoLetterISOLanguageName);
        Console.WriteLine("Voz elegida: " + narrador.NombreVoz + " (" + voz.Culture.Name + ")");
    }

    // ---------- Dobles de prueba ----------

    private sealed class ReproductorEspia : IReproductor
    {
        public List<IReadOnlyList<Pista>> Programados { get; } = [];
        public List<Sonido> Sonados { get; } = [];
        public int Cancelaciones { get; private set; }

        public void Programar(IEnumerable<Pista> pistas) => Programados.Add(pistas.ToArray());

        public void Sonar(Sonido sonido) => Sonados.Add(sonido);

        public void Cancelar() => Cancelaciones++;
    }

    private sealed class NarradorEspia : INarrador
    {
        public List<string> Leidos { get; } = [];
        public int Silencios { get; private set; }

        public bool Disponible => true;

        public void Leer(string texto) => Leidos.Add(texto);

        public void Callar() => Silencios++;
    }
}
