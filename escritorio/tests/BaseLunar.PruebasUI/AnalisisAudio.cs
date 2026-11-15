using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BaseLunar.PruebasUI;

/// <summary>Medidas objetivas de un sonido, para revisarlo sin oírlo.</summary>
/// <param name="RmsDb">Nivel medio (dBFS) de la parte que suena: aproxima cuán fuerte se percibe.</param>
/// <param name="PicoDb">Pico (dBFS).</param>
/// <param name="Agudos">Fracción de la energía por encima de 4 kHz: mucha suena chillona.</param>
/// <param name="Graves">Fracción de la energía por debajo de 120 Hz: mucha retumba en altavoces pequeños.</param>
/// <param name="Continua">Desplazamiento de continua (media de las muestras).</param>
internal sealed record Medidas(double DuracionMs, double RmsDb, double PicoDb, double Agudos, double Graves, double Continua);

/// <summary>Análisis de audio para las pruebas: niveles, espectro (FFT) y espectrogramas en PNG.</summary>
internal static class AnalisisAudio
{
    /// <summary>Muestras entre −1 y 1 de un WAV PCM de 16 bits mono (cabecera de 44 bytes).</summary>
    public static double[] Muestras(byte[] wav) =>
        Enumerable.Range(0, (wav.Length - 44) / 2).Select(i => BitConverter.ToInt16(wav, 44 + i * 2) / 32768.0).ToArray();

    public static Medidas Medir(double[] x, int muestreo)
    {
        var pico = x.Max(Math.Abs);
        // Nivel medio de la parte que suena (por encima de −50 dB respecto al pico), en bloques de 10 ms.
        var bloque = muestreo / 100;
        var energias = Enumerable.Range(0, x.Length / bloque)
            .Select(b => x.Skip(b * bloque).Take(bloque).Average(v => v * v))
            .Where(e => e > pico * pico * 1e-5)
            .ToArray();
        var rms = energias.Length == 0 ? 0 : Math.Sqrt(energias.Average());
        var espectro = Espectro(x);
        var resolucion = (double)muestreo / (espectro.Length * 2);
        double total = 0, agudos = 0, graves = 0;
        for (var k = 1; k < espectro.Length; k++)
        {
            var energia = espectro[k];
            total += energia;
            if (k * resolucion > 4000) agudos += energia;
            if (k * resolucion < 120) graves += energia;
        }
        return new Medidas(
            x.Length * 1000.0 / muestreo,
            Db(rms),
            Db(pico),
            total == 0 ? 0 : agudos / total,
            total == 0 ? 0 : graves / total,
            x.Average());
    }

    public static double Db(double valor) => valor <= 0 ? -120 : 20 * Math.Log10(valor);

    /// <summary>Mezcla varios sonidos con su retraso (como los mezcla Windows al sonar a la vez).</summary>
    public static double[] Mezclar(IEnumerable<(double[] Muestras, double RetrasoMs)> partes, int muestreo)
    {
        var lista = partes.ToList();
        var largo = lista.Max(p => (int)(p.RetrasoMs * muestreo / 1000) + p.Muestras.Length);
        var mezcla = new double[largo];
        foreach (var (muestras, retraso) in lista)
        {
            var inicio = (int)(retraso * muestreo / 1000);
            for (var i = 0; i < muestras.Length; i++) mezcla[inicio + i] += muestras[i];
        }
        return mezcla;
    }

    /// <summary>Energía por frecuencia (mitad positiva del espectro) con ventana de Hann.</summary>
    private static double[] Espectro(double[] x)
    {
        var n = 1;
        while (n < x.Length) n <<= 1;
        var datos = new Complex[n];
        for (var i = 0; i < x.Length; i++) datos[i] = x[i] * (0.5 - 0.5 * Math.Cos(2 * Math.PI * i / Math.Max(1, x.Length - 1)));
        Fft(datos);
        return Enumerable.Range(0, n / 2).Select(k => datos[k].Magnitude * datos[k].Magnitude).ToArray();
    }

    private static void Fft(Complex[] a)
    {
        var n = a.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            var bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (a[i], a[j]) = (a[j], a[i]);
        }
        for (var largo = 2; largo <= n; largo <<= 1)
        {
            var angulo = -2 * Math.PI / largo;
            var w = new Complex(Math.Cos(angulo), Math.Sin(angulo));
            for (var i = 0; i < n; i += largo)
            {
                var wk = Complex.One;
                for (var k = 0; k < largo / 2; k++)
                {
                    var u = a[i + k];
                    var v = a[i + k + largo / 2] * wk;
                    a[i + k] = u + v;
                    a[i + k + largo / 2] = u - v;
                    wk *= w;
                }
            }
        }
    }

    /// <summary>
    /// Espectrograma (tiempo en horizontal, de 0 a 11 kHz en vertical, intensidad en color) con la forma de
    /// onda debajo. Hay que llamarlo en el hilo de interfaz.
    /// </summary>
    public static void GuardarEspectrograma(double[] x, int muestreo, string ruta)
    {
        const int ventana = 512, salto = 96, alto = 256, altoOnda = 90;
        var cuadros = Math.Max(1, (x.Length - ventana) / salto + 1);
        var ancho = cuadros * 2;
        var pixeles = new byte[ancho * (alto + altoOnda) * 4];
        for (var c = 0; c < cuadros; c++)
        {
            var datos = new Complex[ventana];
            for (var i = 0; i < ventana; i++)
            {
                var indice = c * salto + i;
                var valor = indice < x.Length ? x[indice] : 0;
                datos[i] = valor * (0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (ventana - 1)));
            }
            Fft(datos);
            for (var fila = 0; fila < alto; fila++)
            {
                var db = Db(datos[fila].Magnitude / (ventana / 4.0));
                var t = Math.Clamp((db + 80) / 80, 0, 1);
                var (r, g, b) = Color(t);
                for (var dx = 0; dx < 2; dx++)
                {
                    var p = ((alto - 1 - fila) * ancho + c * 2 + dx) * 4;
                    pixeles[p] = b;
                    pixeles[p + 1] = g;
                    pixeles[p + 2] = r;
                    pixeles[p + 3] = 255;
                }
            }
        }
        // Forma de onda
        for (var columna = 0; columna < ancho; columna++)
        {
            var desde = columna * x.Length / ancho;
            var hasta = Math.Max(desde + 1, (columna + 1) * x.Length / ancho);
            var maximo = x.Skip(desde).Take(hasta - desde).DefaultIfEmpty(0).Max(Math.Abs);
            var mitad = altoOnda / 2;
            for (var fila = 0; fila < altoOnda; fila++)
            {
                var dentro = Math.Abs(fila - mitad) <= maximo * mitad;
                var limite = Math.Abs(Math.Abs(fila - mitad) - mitad * 0.5) < 0.6; // líneas a −6 dBFS
                var p = ((alto + fila) * ancho + columna) * 4;
                pixeles[p] = (byte)(dentro ? 250 : limite ? 90 : 30);
                pixeles[p + 1] = (byte)(dentro ? 210 : limite ? 90 : 25);
                pixeles[p + 2] = (byte)(dentro ? 90 : limite ? 90 : 25);
                pixeles[p + 3] = 255;
            }
        }
        var mapa = BitmapSource.Create(ancho, alto + altoOnda, 96, 96, PixelFormats.Bgra32, null, pixeles, ancho * 4);
        var codificador = new PngBitmapEncoder();
        codificador.Frames.Add(BitmapFrame.Create(mapa));
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        using var memoria = new MemoryStream();
        codificador.Save(memoria);
        File.WriteAllBytes(ruta, memoria.ToArray());
    }

    private static (byte R, byte G, byte B) Color(double t)
    {
        // Escala oscura → morado → naranja → amarillo.
        (double R, double G, double B)[] paradas = [(10, 8, 30), (110, 30, 140), (240, 110, 40), (255, 240, 120)];
        var posicion = t * (paradas.Length - 1);
        var i = Math.Min(paradas.Length - 2, (int)posicion);
        var f = posicion - i;
        return ((byte)(paradas[i].R + (paradas[i + 1].R - paradas[i].R) * f),
                (byte)(paradas[i].G + (paradas[i + 1].G - paradas[i].G) * f),
                (byte)(paradas[i].B + (paradas[i + 1].B - paradas[i].B) * f));
    }
}
