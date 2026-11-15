namespace BaseLunar.App.VistaModelos;

/// <summary>Qué tiene que representar la escena después de una acción.</summary>
public enum TipoMomento
{
    /// <summary>Pantalla de Inicio: el mapa con el astronauta en la base.</summary>
    Inicio,

    /// <summary>El astronauta camina por el mapa hasta la estación y la cámara entra en la escena.</summary>
    EntrarNivel,

    /// <summary>La consecuencia de la opción elegida (una por opción: 10 en total).</summary>
    Consecuencia,

    /// <summary>Un medidor llegó a 0 o no se cumplió el requisito: variante amable del fallo y un salvavidas.</summary>
    Fallo,

    /// <summary>La escena se «rebobina» al inicio del nivel.</summary>
    Reintento,

    /// <summary>El relevo aterriza, la tripulación celebra y el astronauta llega a la meta del mapa.</summary>
    Victoria,

    /// <summary>La cápsula despega hacia la Tierra.</summary>
    Abortada,
}

/// <summary>
/// Un momento de la escena, deducido de los eventos del motor. La escena solo lo representa: no cambia
/// ninguna regla.
/// </summary>
/// <param name="Nivel">Nivel en el que ocurre (0 en Inicio).</param>
/// <param name="Opcion">Opción elegida («A» o «B») en consecuencias y fallos.</param>
/// <param name="Medidor">Medidor que explica el fallo: el que llegó a 0 (💧 → 🛡️ → ⚡) o el del requisito del Nivel 5.</param>
/// <param name="ClaseX">Al entrar en el nivel, la llamarada de clase X golpeó el escudo.</param>
/// <param name="Ayuda">Llegó la ayuda de Control de Misión (antibloqueo).</param>
/// <param name="Desde">Casilla del mapa desde la que camina el astronauta (0 es la base y 6 la meta).</param>
public sealed record Momento(
    TipoMomento Tipo,
    int Nivel,
    string? Opcion = null,
    Medidor? Medidor = null,
    bool ClaseX = false,
    bool Ayuda = false,
    int Desde = 0);
