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
