using System.IO;
using System.Text.Json;

namespace BaseLunar.App.VistaModelos;

/// <summary>
/// Preferencias que se recuerdan entre partidas. Hoy solo una: si los efectos de sonido están encendidos.
/// Se guardan en la carpeta local del usuario (%LOCALAPPDATA%\BaseLunar\ajustes.json) y nunca salen del
/// equipo. Si el archivo falta o está dañado, se usan los valores por defecto (D-51).
/// </summary>
public sealed class Ajustes
{
    private readonly string? ruta;

    private Ajustes(string? ruta)
    {
        this.ruta = ruta;
    }

    public bool Sonido { get; set; } = true;

    public static string RutaPredeterminada =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BaseLunar", "ajustes.json");

    /// <summary>Ajustes que no se guardan en disco (pruebas y --silencio).</summary>
    public static Ajustes EnMemoria() => new(null);

    public static Ajustes Cargar(string ruta)
    {
        var ajustes = new Ajustes(ruta);
        try
        {
            if (File.Exists(ruta) && JsonSerializer.Deserialize<Datos>(File.ReadAllText(ruta)) is { } datos)
                ajustes.Sonido = datos.Sonido;
        }
        catch (Exception)
        {
            // Archivo dañado: se ignora y se sigue con los valores por defecto.
        }
        return ajustes;
    }

    public void Guardar()
    {
        if (ruta is null) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
            File.WriteAllText(ruta, JsonSerializer.Serialize(new Datos(Sonido)));
        }
        catch (Exception)
        {
            // Sin permiso de escritura: la preferencia vale solo para esta partida.
        }
    }

    private sealed record Datos(bool Sonido);
}
