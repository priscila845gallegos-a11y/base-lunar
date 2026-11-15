using System.IO;
using System.Text;

namespace BaseLunar.App.Audio;

/// <summary>
/// Sintetizador propio: genera cada efecto con ondas, envolventes, barridos y ruido filtrado, y lo empaqueta
/// como WAV en memoria (PCM de 16 bits, mono, 22 050 Hz). No hay archivos de audio en el proyecto.
/// Los sonidos son cortos, suaves y amables: nada de sirenas ni de sonidos que asusten (prioridad 1).
/// </summary>
public static class Sintetizador
{
    public const int Muestreo = 22050;
    private const double Margen = 0.85;

    private enum Onda
    {
        Seno,
        Triangulo,
        Sierra,
    }

    private static readonly Dictionary<Sonido, byte[]> Cache = [];

    /// <summary>El WAV completo (cabecera RIFF incluida) de un efecto. Se genera una sola vez.</summary>
    public static byte[] Wav(Sonido sonido)
    {
        lock (Cache)
        {
            if (!Cache.TryGetValue(sonido, out var wav)) Cache[sonido] = wav = Empaquetar(Muestras(sonido));
            return wav;
        }
    }

    /// <summary>Las muestras de un efecto, entre −1 y 1.</summary>
    public static float[] Muestras(Sonido sonido) => sonido switch
    {
        Sonido.Seleccion => Mezclar((Tono(660, 55, Onda.Triangulo, 0.35), 0), (Tono(880, 75, Onda.Triangulo, 0.35), 55)),
        Sonido.Ganancia => Mezclar((Tono(523.3, 110, Onda.Seno, 0.4), 0), (Tono(659.3, 110, Onda.Seno, 0.4), 80), (Tono(784, 160, Onda.Seno, 0.4), 160)),
        Sonido.Costo => Tono(523.3, 230, Onda.Triangulo, 0.3, hasta: 392),
        Sonido.AlMaximo => Mezclar((Tono(1568, 130, Onda.Seno, 0.25), 0), (Tono(2093, 130, Onda.Seno, 0.25), 60),
            (Tono(2637, 130, Onda.Seno, 0.22), 120), (Tono(3136, 180, Onda.Seno, 0.2), 180)),
        Sonido.NivelSuperado => Mezclar((Tono(392, 120, Onda.Triangulo, 0.35), 0), (Tono(523.3, 120, Onda.Triangulo, 0.35), 110),
            (Tono(659.3, 120, Onda.Triangulo, 0.35), 220), (Tono(784, 320, Onda.Triangulo, 0.35), 330)),
        Sonido.Respaldo => Mezclar((Tono(493.9, 180, Onda.Triangulo, 0.33), 0), (Tono(392, 280, Onda.Triangulo, 0.33), 200)),
        Sonido.Rebobinar => Mezclar((Ruido(420, 0.22, 600, 4000), 0), (Tono(300, 420, Onda.Seno, 0.25, hasta: 900), 0)),
        Sonido.Victoria => Mezclar((Tono(523.3, 140, Onda.Triangulo, 0.33), 0), (Tono(659.3, 140, Onda.Triangulo, 0.33), 130),
            (Tono(784, 140, Onda.Triangulo, 0.33), 260), (Tono(1046.5, 280, Onda.Triangulo, 0.33), 390),
            (Tono(523.3, 650, Onda.Seno, 0.22), 660), (Tono(659.3, 650, Onda.Seno, 0.22), 660), (Tono(784, 650, Onda.Seno, 0.22), 660)),
        Sonido.Abortada => Mezclar((Ruido(700, 0.25, 300, 1200), 0), (Tono(200, 700, Onda.Seno, 0.25, hasta: 600), 0),
            (Tono(523.3, 450, Onda.Seno, 0.2), 750), (Tono(659.3, 450, Onda.Seno, 0.2), 750), (Tono(784, 450, Onda.Seno, 0.2), 750)),
        Sonido.Llamarada => Mezclar((Tono(1200, 280, Onda.Sierra, 0.22, hasta: 180), 0), (Ruido(280, 0.2, 3000, 500), 0)),
        Sonido.Entrar => Mezclar((Ruido(380, 0.16, 3000, 600), 0), (Tono(440, 220, Onda.Seno, 0.2, hasta: 660), 150)),
        Sonido.Glosario => Tono(600, 85, Onda.Seno, 0.35, hasta: 900),
        _ => Silencio(50),
    };

    /// <summary>Duración del efecto en milisegundos.</summary>
    public static double Duracion(Sonido sonido) => Muestras(sonido).Length * 1000.0 / Muestreo;

    // ---------- Generadores ----------

    private static int Cantidad(double milisegundos) => (int)(milisegundos * Muestreo / 1000);

    private static float[] Silencio(double milisegundos) => new float[Cantidad(milisegundos)];

    /// <summary>Un tono con ataque corto y final suave; con <paramref name="hasta"/>, la frecuencia se desliza.</summary>
    private static float[] Tono(double frecuencia, double milisegundos, Onda onda, double volumen, double? hasta = null)
    {
        var n = Cantidad(milisegundos);
        var muestras = new float[n];
        var fase = 0.0;
        for (var i = 0; i < n; i++)
        {
            var avance = i / (double)n;
            var f = frecuencia + ((hasta ?? frecuencia) - frecuencia) * avance;
            fase += f / Muestreo;
            fase -= Math.Floor(fase);
            var valor = onda switch
            {
                Onda.Triangulo => 4 * Math.Abs(fase - 0.5) - 1,
                Onda.Sierra => 2 * fase - 1,
                _ => Math.Sin(2 * Math.PI * fase),
            };
            muestras[i] = (float)(valor * volumen * Envolvente(i, n));
        }
        return muestras;
    }

    /// <summary>Ruido suave con un filtro de paso bajo que barre de una frecuencia de corte a otra.</summary>
    private static float[] Ruido(double milisegundos, double volumen, double corteDesde, double corteHasta)
    {
        var azar = new Random(17);
        var n = Cantidad(milisegundos);
        var muestras = new float[n];
        var filtrado = 0.0;
        for (var i = 0; i < n; i++)
        {
            var corte = corteDesde + (corteHasta - corteDesde) * i / n;
            var alfa = 1 - Math.Exp(-2 * Math.PI * corte / Muestreo);
            filtrado += alfa * (azar.NextDouble() * 2 - 1 - filtrado);
            muestras[i] = (float)(filtrado * volumen * 2 * Envolvente(i, n));
        }
        return muestras;
    }

    /// <summary>Ataque de 6 ms y caída lineal en el último 35 %: sin chasquidos al empezar ni al terminar.</summary>
    private static double Envolvente(int i, int n)
    {
        var ataque = Math.Min(1, i / (0.006 * Muestreo));
        var desde = n * 0.65;
        var caida = i < desde ? 1 : Math.Max(0, (n - i) / (n - desde));
        return ataque * caida;
    }

    /// <summary>Suma varias partes, cada una desde su instante.</summary>
    private static float[] Mezclar(params (float[] Muestras, double DesdeMs)[] partes)
    {
        var largo = partes.Max(p => Cantidad(p.DesdeMs) + p.Muestras.Length);
        var mezcla = new float[largo];
        foreach (var (muestras, desde) in partes)
        {
            var inicio = Cantidad(desde);
            for (var i = 0; i < muestras.Length; i++) mezcla[inicio + i] += muestras[i];
        }
        return mezcla;
    }

    // ---------- WAV ----------

    /// <summary>Normaliza (sin saturar) y escribe un WAV PCM de 16 bits mono.</summary>
    public static byte[] Empaquetar(float[] muestras)
    {
        var pico = Math.Max(1f, muestras.Length == 0 ? 1f : muestras.Max(Math.Abs));
        using var memoria = new MemoryStream();
        using (var escritor = new BinaryWriter(memoria, Encoding.ASCII, leaveOpen: true))
        {
            var datos = muestras.Length * 2;
            escritor.Write("RIFF"u8);
            escritor.Write(36 + datos);
            escritor.Write("WAVE"u8);
            escritor.Write("fmt "u8);
            escritor.Write(16);
            escritor.Write((short)1);
            escritor.Write((short)1);
            escritor.Write(Muestreo);
            escritor.Write(Muestreo * 2);
            escritor.Write((short)2);
            escritor.Write((short)16);
            escritor.Write("data"u8);
            escritor.Write(datos);
            foreach (var muestra in muestras) escritor.Write((short)Math.Round(muestra / pico * Margen * short.MaxValue));
        }
        return memoria.ToArray();
    }
}
