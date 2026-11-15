using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;

namespace BaseLunar.App.Audio;

/// <summary>
/// Voz con las voces de Windows instaladas en el equipo (SAPI): nada sale a internet (D-13, D-50). Solo se
/// usan voces en español; si no hay ninguna, el botón «Escuchar» se oculta. Habla un poco más despacio para
/// el público de 7 a 12 años.
/// </summary>
public sealed class NarradorWindows : INarrador, IDisposable
{
    private readonly SpeechSynthesizer? voz;

    public NarradorWindows()
    {
        try
        {
            var sintetizador = new SpeechSynthesizer();
            var espanol = sintetizador.GetInstalledVoices()
                .Where(v => v.Enabled && v.VoiceInfo.Culture.TwoLetterISOLanguageName == "es")
                .Select(v => v.VoiceInfo)
                .ToList();
            if (espanol.Count == 0)
            {
                sintetizador.Dispose();
                return;
            }
            // Español neutro primero (5.1): México, Estados Unidos o Latinoamérica; si no, cualquiera.
            var elegida = espanol.FirstOrDefault(v => v.Culture.Name is "es-MX" or "es-US" or "es-419") ?? espanol[0];
            sintetizador.SelectVoice(elegida.Name);
            sintetizador.Rate = -1;
            sintetizador.SetOutputToDefaultAudioDevice();
            voz = sintetizador;
            NombreVoz = elegida.Name;
        }
        catch (Exception)
        {
            voz = null;
        }
    }

    public bool Disponible => voz is not null;

    /// <summary>La voz elegida (para el registro de decisiones y las pruebas manuales).</summary>
    public string? NombreVoz { get; }

    public void Leer(string texto)
    {
        if (voz is null || string.IsNullOrWhiteSpace(texto)) return;
        try
        {
            voz.SpeakAsyncCancelAll();
            voz.SpeakAsync(texto);
        }
        catch (Exception)
        {
            // Una voz que falla no puede bloquear el juego.
        }
    }

    /// <summary>
    /// Pruebas: genera en memoria (sin altavoces) lo que se oiría, con la misma voz y velocidad. Devuelve un
    /// WAV PCM de 16 bits mono a 22 050 Hz, o null si no hay voz.
    /// </summary>
    internal byte[]? Grabar(string texto)
    {
        if (voz is null || NombreVoz is null) return null;
        using var sintetizador = new SpeechSynthesizer();
        sintetizador.SelectVoice(NombreVoz);
        sintetizador.Rate = voz.Rate;
        using var memoria = new MemoryStream();
        sintetizador.SetOutputToAudioStream(memoria, new SpeechAudioFormatInfo(Sintetizador.Muestreo, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
        sintetizador.Speak(texto);
        sintetizador.SetOutputToNull();
        var pcm = memoria.ToArray();
        var wav = new byte[44 + pcm.Length];
        Array.Copy(Sintetizador.Empaquetar([]), wav, 44);
        BitConverter.GetBytes(36 + pcm.Length).CopyTo(wav, 4);
        BitConverter.GetBytes(pcm.Length).CopyTo(wav, 40);
        pcm.CopyTo(wav, 44);
        return wav;
    }

    public void Callar()
    {
        try
        {
            voz?.SpeakAsyncCancelAll();
        }
        catch (Exception)
        {
        }
    }

    public void Dispose()
    {
        Callar();
        voz?.Dispose();
    }
}
