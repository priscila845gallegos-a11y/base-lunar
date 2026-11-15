using System.Globalization;
using System.IO;
using BaseLunar.App.Audio;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.PruebasUI;

/// <summary>
/// Calidad de los efectos medida sin oírlos: volumen parecido entre ellos, sin agudos chillones ni graves
/// que retumben, sin continua, y con margen cuando suenan juntos. Deja en escritorio/capturas/audio los WAV,
/// las mezclas de cada momento y su espectrograma para revisarlos.
/// </summary>
[TestClass]
public sealed class PruebasCalidadAudio
{
    private static readonly string Carpeta = Path.Combine(Capturas.Carpeta, "audio");

    private static double[] Efecto(Sonido sonido) => AnalisisAudio.Muestras(Sintetizador.Wav(sonido));

    [TestMethod(DisplayName = "Informe de audio: medidas de cada efecto, WAV y espectrogramas")]
    public void Informe() => HiloUI.Ejecutar(() =>
    {
        Directory.CreateDirectory(Carpeta);
        var lineas = new List<string> { "Efecto           ms     RMS dB   Pico dB   >4 kHz   <120 Hz" };
        foreach (var sonido in Enum.GetValues<Sonido>())
        {
            var x = Efecto(sonido);
            var m = AnalisisAudio.Medir(x, Sintetizador.Muestreo);
            lineas.Add(string.Create(CultureInfo.InvariantCulture,
                $"{sonido,-14} {m.DuracionMs,6:0} {m.RmsDb,9:0.0} {m.PicoDb,9:0.0} {m.Agudos,8:P1} {m.Graves,9:P1}"));
            File.WriteAllBytes(Path.Combine(Carpeta, $"{sonido}.wav"), Sintetizador.Wav(sonido));
            AnalisisAudio.GuardarEspectrograma(x, Sintetizador.Muestreo, Path.Combine(Carpeta, $"{sonido}.png"));
        }
        File.WriteAllLines(Path.Combine(Carpeta, "informe.txt"), lineas);
        foreach (var linea in lineas) Console.WriteLine(linea);
    });

    [TestMethod(DisplayName = "Cada efecto suena a su nivel (±1 dB), con pico ≤ −9 dBFS, sin agudos chillones ni graves que retumben")]
    public void NivelesYEspectro()
    {
        foreach (var sonido in Enum.GetValues<Sonido>())
        {
            var m = AnalisisAudio.Medir(Efecto(sonido), Sintetizador.Muestreo);
            Assert.AreEqual(Sintetizador.NivelDb(sonido), m.RmsDb, 1.0, $"{sonido}: nivel medio");
            Assert.IsLessThanOrEqualTo(Sintetizador.TechoDb + 0.1, m.PicoDb, $"{sonido}: pico");
            Assert.IsLessThanOrEqualTo(0.05, m.Agudos, $"{sonido}: energía por encima de 4 kHz");
            Assert.IsLessThanOrEqualTo(0.05, m.Graves, $"{sonido}: energía por debajo de 120 Hz");
            Assert.IsLessThan(0.005, Math.Abs(m.Continua), $"{sonido}: continua");
        }
        var niveles = Enum.GetValues<Sonido>().Select(Sintetizador.NivelDb).ToArray();
        Assert.IsLessThanOrEqualTo(4.0, niveles.Max() - niveles.Min(), "Ningún efecto sobresale más de 4 dB");
    }

    /// <summary>Rutas que producen cada momento con sus sonidos (incluido «¡Al máximo!» y la llamarada X).</summary>
    public static IEnumerable<object[]> Momentos() =>
    [
        ["consecuencia", Partida.HastaResultado1],
        ["al-maximo", new[] { "P", "A", "P", "A" }],
        ["llamarada", Partida.HastaNivel4],
        ["respaldo", Partida.HastaRespaldo4],
        ["victoria", Partida.HastaVictoria],
        ["abortada", Partida.HastaAbortada],
    ];

    [TestMethod(DisplayName = "Cuando suenan juntos, la mezcla de cada momento no satura (pico ≤ −3 dBFS)")]
    [DynamicData(nameof(Momentos))]
    public void MezclaConMargen(string nombre, string[] pasos) => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM();
        foreach (var paso in pasos[..^1]) Partida.Paso(juego, paso);
        var anterior = juego.Estado;
        var contexto = new ContextoPartida(ClaseX: true); // el respaldo histórico es una X8.7
        var resultado = pasos[^1] != "P"
            ? Motor.AplicarDecision(anterior, pasos[^1])
            : anterior.Fase switch
            {
                Fase.Resultado => Motor.Continuar(anterior, contexto),
                Fase.Respaldo => Motor.Reintentar(anterior),
                _ => Motor.Empezar(anterior, contexto),
            };
        var pistas = Presentacion.SonidosDe(anterior, resultado);
        Assert.IsNotEmpty(pistas, nombre);
        var mezcla = AnalisisAudio.Mezclar(pistas.Select(p => (Efecto(p.Sonido), p.RetrasoMs)), Sintetizador.Muestreo);
        var pico = AnalisisAudio.Db(mezcla.Max(Math.Abs));
        Assert.IsLessThanOrEqualTo(-3.0, pico, $"{nombre}: {string.Join(", ", pistas)}");
        File.WriteAllBytes(Path.Combine(Carpeta, $"momento-{nombre}.wav"), Sintetizador.Empaquetar(mezcla.Select(v => (float)v).ToArray()));
        AnalisisAudio.GuardarEspectrograma(mezcla, Sintetizador.Muestreo, Path.Combine(Carpeta, $"momento-{nombre}.png"));
    });

    [TestMethod(DisplayName = "Los 12 efectos reales pasan por la salida de audio de Windows (con el volumen a cero) y se liberan")]
    public void EfectosRealesPorWindows() => HiloUI.Ejecutar(() =>
    {
        using var reproductor = new ReproductorWindows { VolumenCero = true };
        foreach (var sonido in Enum.GetValues<Sonido>()) reproductor.Sonar(sonido);
        if (reproductor.Roto)
        {
            Assert.Inconclusive("Este equipo no tiene salida de audio: el juego seguiría en silencio.");
            return;
        }
        Assert.AreEqual(Enum.GetValues<Sonido>().Length, reproductor.Activas, "Todos suenan a la vez");
        Assert.IsTrue(HiloUI.Esperar(() =>
        {
            reproductor.LimpiarTerminadas();
            return reproductor.Activas == 0;
        }, 5000), "Al terminar, cada salida se libera");
    });

    [TestMethod(DisplayName = "La voz se oye (nivel medio > −35 dBFS) y habla despacio para niños (100 a 170 palabras por minuto)")]
    public void VozAudibleYPausada() => HiloUI.Ejecutar(() =>
    {
        using var narrador = new NarradorWindows();
        if (!narrador.Disponible)
        {
            Assert.Inconclusive("Este equipo no tiene voces en español: el botón «Escuchar» se ocultaría.");
            return;
        }
        var juego = Partida.Jugar(Partida.HastaNivel1);
        var texto = juego.TextoParaEscuchar();
        var wav = narrador.Grabar(texto)!;
        File.WriteAllBytes(Path.Combine(Carpeta, "voz-nivel1.wav"), wav);
        var muestras = AnalisisAudio.Muestras(wav);
        var m = AnalisisAudio.Medir(muestras, Sintetizador.Muestreo);
        var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var porMinuto = palabras / (m.DuracionMs / 60000);
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"Voz {narrador.NombreVoz}: {palabras} palabras en {m.DuracionMs / 1000:0.0} s = {porMinuto:0} por minuto; nivel {m.RmsDb:0.0} dBFS, pico {m.PicoDb:0.0}"));
        Assert.IsGreaterThan(-35.0, m.RmsDb, "La voz se oye");
        Assert.IsTrue(porMinuto is >= 100 and <= 170, $"{porMinuto:0} palabras por minuto");
        AnalisisAudio.GuardarEspectrograma(muestras, Sintetizador.Muestreo, Path.Combine(Carpeta, "voz-nivel1.png"));
    });
}
