namespace BaseLunar.Nucleo;

/// <summary>Tipo de nivel: con efectos (niveles 1 a 4) o con condición (nivel 5).</summary>
public enum TipoNivel
{
    Decision,
    Condicion,
}

/// <summary>Tema visual del nivel: normal o alerta (tormenta solar).</summary>
public enum Tema
{
    Normal,
    Alerta,
}

/// <summary>Requisito del Nivel 5: el medidor tiene que ser estrictamente mayor que el umbral (3.4).</summary>
public sealed record Requisito(Medidor Medidor, int Umbral);

/// <summary>
/// Una opción (A o B). Lleva efectos (niveles 1 a 4) o un requisito con su texto de fallo (nivel 5).
/// </summary>
public sealed record Opcion(
    string Texto,
    string Resultado,
    IReadOnlyDictionary<Medidor, int>? Efectos = null,
    Requisito? Requisito = null,
    string? Fallo = null);

/// <summary>Fuente de un dato, con su enlace.</summary>
public sealed record Fuente(string Nombre, string Url);

/// <summary>El 💡 Dato real de un nivel, con sus fuentes (D-25) y una línea extra opcional.</summary>
public sealed record Dato(
    string Texto,
    IReadOnlyList<Fuente> Fuentes,
    bool Verificado,
    IReadOnlyList<string>? Extra = null);

/// <summary>Un nivel del guion (modelo de datos de la sección 6 del Prompt 0).</summary>
/// <param name="AplicaPenalizacionX">Si la llamarada real es de clase X, resta protección al empezar (antes de la foto).</param>
/// <param name="DatoTormenta">Añade al dato la línea de la llamarada real (NASA DONKI).</param>
public sealed record Nivel(
    int Id,
    string Titulo,
    string Etiqueta,
    string Avatar,
    string Historia,
    Tema Tema,
    TipoNivel Tipo,
    IReadOnlyDictionary<string, Opcion> Opciones,
    Dato Dato,
    string? EtiquetaHistorico = null,
    bool AplicaPenalizacionX = false,
    bool DatoTormenta = false);

/// <summary>Nombre, icono y recurso de un medidor, para la interfaz.</summary>
public sealed record InfoMedidor(Medidor Clave, string Nombre, string Icono, string Recurso);
