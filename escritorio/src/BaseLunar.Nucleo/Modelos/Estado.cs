namespace BaseLunar.Nucleo;

/// <summary>Fases de la máquina de estados (sección 6 del Prompt 0).</summary>
public enum Fase
{
    Inicio,
    Nivel,
    Resultado,
    Respaldo,
    Victoria,
    MisionAbortada,
}

/// <summary>Estado de la partida. Es inmutable: el motor siempre devuelve uno nuevo.</summary>
/// <param name="Fase">Fase actual.</param>
/// <param name="Nivel">Número de nivel (1 a 5); 0 en el Inicio.</param>
/// <param name="Medidores">Valores actuales (0–100).</param>
/// <param name="Foto">Foto del estado al empezar el nivel (3.3).</param>
/// <param name="Respaldos">Protocolos de Respaldo que quedan.</param>
/// <param name="ClaseX">Si la llamarada del Nivel 4 es de clase X.</param>
/// <param name="Fallidos">Medidores que fallaron en la última decisión, por prioridad 💧 → 🛡️ → ⚡.</param>
/// <param name="UltimaOpcion">«A», «B» o null.</param>
public sealed record Estado(
    Fase Fase,
    int Nivel,
    Medidores Medidores,
    Medidores Foto,
    int Respaldos,
    bool ClaseX,
    IReadOnlyList<Medidor> Fallidos,
    string? UltimaOpcion);

/// <summary>Datos que llegan de fuera del motor: nivel inicial (D-08) y si la llamarada real es de clase X.</summary>
public sealed record ContextoPartida(int? Nivel = null, bool ClaseX = false);

/// <summary>Lo que devuelve cada acción del motor: el nuevo estado y los eventos que produjo.</summary>
public sealed record ResultadoMotor(Estado Estado, IReadOnlyList<Evento> Eventos);

/// <summary>Resultado de sumar efectos: valores recortados, valores sin recortar, fallidos y «al máximo».</summary>
public sealed record ResultadoEfectos(
    Medidores Medidores,
    Medidores Crudos,
    IReadOnlyList<Medidor> Fallidos,
    IReadOnlyList<Medidor> AlMaximo);

/// <summary>Ayuda de Control de Misión (D-03): medidores ajustados y cuánto subió cada uno.</summary>
public sealed record Ayuda(Medidores Medidores, IReadOnlyDictionary<Medidor, int> Ajustes);
