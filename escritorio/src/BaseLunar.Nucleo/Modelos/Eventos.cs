namespace BaseLunar.Nucleo;

/// <summary>
/// Evento que produce el motor. La interfaz lo convierte en animación, sonido y anuncio,
/// pero nunca cambia las reglas. Son los mismos eventos que en js/game.js.
/// </summary>
public abstract record Evento;

/// <summary>Efectos de la opción elegida (números flotantes y animación de la consecuencia).</summary>
public sealed record EfectosAplicados(IReadOnlyDictionary<Medidor, int> Efectos) : Evento;

/// <summary>Una ganancia pasó de 100 y se recortó: «¡Al máximo!».</summary>
public sealed record MedidorAlMaximo(Medidor Medidor) : Evento;

/// <summary>Se superó el nivel.</summary>
public sealed record NivelSuperado(int Nivel) : Evento;

/// <summary>
/// Se perdió un respaldo. Motivo: el medidor cuyo mensaje se muestra (💧 → 🛡️ → ⚡),
/// o null si no se cumplió el requisito del Nivel 5.
/// </summary>
public sealed record RespaldoActivado(Medidor? Motivo, IReadOnlyList<Medidor> Fallidos, int Respaldos) : Evento;

/// <summary>Sin respaldos: Misión Abortada (D-04).</summary>
public sealed record MisionAbortada : Evento;

/// <summary>Victoria tras el Nivel 5 (D-07).</summary>
public sealed record Victoria(int Respaldos) : Evento;

/// <summary>Penalización de la llamarada de clase X al entrar en el Nivel 4 (antes de la foto).</summary>
public sealed record PenalizacionX(Medidor Medidor, int Cantidad) : Evento;

/// <summary>Ayuda de Control de Misión (antibloqueo, D-03).</summary>
public sealed record AyudaActivada(IReadOnlyDictionary<Medidor, int> Ajustes) : Evento;

/// <summary>Reintento del nivel: medidores que se restauraron hasta 30 o más (D-01).</summary>
public sealed record Reintento(IReadOnlyList<Medidor> Restaurados) : Evento;
