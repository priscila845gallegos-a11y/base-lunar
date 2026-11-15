using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace BaseLunar.App.Audio;

/// <summary>
/// Reproduce los efectos con la API de audio de Windows (winmm, la misma que usa SoundPlayer por dentro).
/// Cada sonido abre su propia salida, así que varios suenan a la vez y Windows los mezcla. Los datos salen
/// de la memoria: no hay archivos. Si algo falla, el juego sigue en silencio (D-49).
/// </summary>
public sealed class ReproductorWindows : IReproductor, IDisposable
{
    private const uint MapeadorDeOnda = 0xFFFFFFFF;
    private const int Hecho = 0x1;

    private readonly List<Reproduccion> activas = [];
    private readonly List<DispatcherTimer> pendientes = [];
    private readonly DispatcherTimer limpieza;
    private bool roto;

    public ReproductorWindows()
    {
        limpieza = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        limpieza.Tick += (_, _) => Limpiar(forzar: false);
    }

    public void Programar(IEnumerable<Pista> pistas)
    {
        Cancelar();
        foreach (var pista in pistas)
        {
            if (pista.RetrasoMs <= 0)
            {
                Sonar(pista.Sonido);
                continue;
            }
            var espera = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(pista.RetrasoMs) };
            espera.Tick += (_, _) =>
            {
                espera.Stop();
                pendientes.Remove(espera);
                Sonar(pista.Sonido);
            };
            pendientes.Add(espera);
            espera.Start();
        }
    }

    public void Cancelar()
    {
        foreach (var espera in pendientes) espera.Stop();
        pendientes.Clear();
    }

    public void Sonar(Sonido sonido) => SonarWav(Sintetizador.Wav(sonido));

    /// <summary>Reproduce un WAV PCM de 16 bits mono a 22 050 Hz (las pruebas usan uno en silencio).</summary>
    internal void SonarWav(byte[] wav)
    {
        if (roto) return;
        try
        {
            activas.Add(Reproduccion.Empezar(wav));
            limpieza.Start();
        }
        catch (Exception)
        {
            roto = true; // sin dispositivo de audio o sin winmm: el juego sigue en silencio
        }
    }

    /// <summary>Pruebas: salidas abiertas, si el audio falló y limpieza inmediata.</summary>
    internal int Activas => activas.Count;

    internal bool Roto => roto;

    internal void LimpiarTerminadas() => Limpiar(forzar: false);

    /// <summary>Libera las salidas que ya terminaron (o todas, al cerrar).</summary>
    private void Limpiar(bool forzar)
    {
        for (var i = activas.Count - 1; i >= 0; i--)
        {
            if (!forzar && !activas[i].Terminada) continue;
            activas[i].Liberar(detener: forzar);
            activas.RemoveAt(i);
        }
        if (activas.Count == 0) limpieza.Stop();
    }

    public void Dispose()
    {
        Cancelar();
        limpieza.Stop();
        Limpiar(forzar: true);
    }

    /// <summary>Un sonido sonando: su salida de audio, su cabecera y su memoria nativa.</summary>
    private sealed class Reproduccion
    {
        private const int CabeceraWav = 44;

        private IntPtr salida;
        private IntPtr datos;
        private IntPtr cabecera;

        public bool Terminada => cabecera == IntPtr.Zero
            || (Marshal.ReadInt32(cabecera, Marshal.OffsetOf<CabeceraOnda>(nameof(CabeceraOnda.Banderas)).ToInt32()) & Hecho) != 0;

        public static Reproduccion Empezar(byte[] wav)
        {
            var reproduccion = new Reproduccion();
            try
            {
                var largo = wav.Length - CabeceraWav;
                reproduccion.datos = Marshal.AllocHGlobal(largo);
                Marshal.Copy(wav, CabeceraWav, reproduccion.datos, largo);
                var formato = new FormatoOnda
                {
                    Etiqueta = 1,
                    Canales = 1,
                    MuestrasPorSegundo = Sintetizador.Muestreo,
                    BytesPorSegundo = Sintetizador.Muestreo * 2,
                    Alineacion = 2,
                    BitsPorMuestra = 16,
                };
                Comprobar(Nativo.waveOutOpen(out reproduccion.salida, MapeadorDeOnda, ref formato, IntPtr.Zero, IntPtr.Zero, 0));
                var tamano = Marshal.SizeOf<CabeceraOnda>();
                reproduccion.cabecera = Marshal.AllocHGlobal(tamano);
                Marshal.StructureToPtr(new CabeceraOnda { Datos = reproduccion.datos, Largo = largo }, reproduccion.cabecera, false);
                Comprobar(Nativo.waveOutPrepareHeader(reproduccion.salida, reproduccion.cabecera, tamano));
                Comprobar(Nativo.waveOutWrite(reproduccion.salida, reproduccion.cabecera, tamano));
                return reproduccion;
            }
            catch
            {
                reproduccion.Liberar(detener: true);
                throw;
            }
        }

        public void Liberar(bool detener)
        {
            var tamano = Marshal.SizeOf<CabeceraOnda>();
            if (salida != IntPtr.Zero)
            {
                if (detener) Nativo.waveOutReset(salida);
                if (cabecera != IntPtr.Zero) Nativo.waveOutUnprepareHeader(salida, cabecera, tamano);
                Nativo.waveOutClose(salida);
                salida = IntPtr.Zero;
            }
            if (cabecera != IntPtr.Zero) Marshal.FreeHGlobal(cabecera);
            if (datos != IntPtr.Zero) Marshal.FreeHGlobal(datos);
            cabecera = IntPtr.Zero;
            datos = IntPtr.Zero;
        }

        private static void Comprobar(int resultado)
        {
            if (resultado != 0) throw new InvalidOperationException("winmm devolvió el error " + resultado);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FormatoOnda
    {
        public short Etiqueta;
        public short Canales;
        public int MuestrasPorSegundo;
        public int BytesPorSegundo;
        public short Alineacion;
        public short BitsPorMuestra;
        public short Extra;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CabeceraOnda
    {
        public IntPtr Datos;
        public int Largo;
        public int Grabados;
        public IntPtr Usuario;
        public int Banderas;
        public int Bucles;
        public IntPtr Siguiente;
        public IntPtr Reservado;
    }

    private static class Nativo
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutOpen(out IntPtr salida, uint dispositivo, ref FormatoOnda formato, IntPtr llamada, IntPtr instancia, uint banderas);

        [DllImport("winmm.dll")]
        public static extern int waveOutPrepareHeader(IntPtr salida, IntPtr cabecera, int tamano);

        [DllImport("winmm.dll")]
        public static extern int waveOutWrite(IntPtr salida, IntPtr cabecera, int tamano);

        [DllImport("winmm.dll")]
        public static extern int waveOutUnprepareHeader(IntPtr salida, IntPtr cabecera, int tamano);

        [DllImport("winmm.dll")]
        public static extern int waveOutReset(IntPtr salida);

        [DllImport("winmm.dll")]
        public static extern int waveOutClose(IntPtr salida);
    }
}
