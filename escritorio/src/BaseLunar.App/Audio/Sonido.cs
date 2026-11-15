namespace BaseLunar.App.Audio;

/// <summary>Los efectos de sonido del juego. Todos se generan con código (Sintetizador), sin archivos.</summary>
public enum Sonido
{
    /// <summary>Clic al elegir una opción.</summary>
    Seleccion,

    /// <summary>Arpegio que sube: un medidor gana.</summary>
    Ganancia,

    /// <summary>Tono suave que baja: un medidor cuesta.</summary>
    Costo,

    /// <summary>Destello agudo: «¡Al máximo!».</summary>
    AlMaximo,

    /// <summary>Melodía alegre: nivel superado.</summary>
    NivelSuperado,

    /// <summary>Dos notas amables que bajan (nada de alarmas): Protocolo de Respaldo.</summary>
    Respaldo,

    /// <summary>Barrido que sube: la escena se rebobina.</summary>
    Rebobinar,

    /// <summary>Fanfarria: misión cumplida.</summary>
    Victoria,

    /// <summary>Despegue de la cápsula y un acorde cálido: la tripulación vuelve a salvo.</summary>
    Abortada,

    /// <summary>Zumbido corto: la llamarada de clase X golpea el escudo.</summary>
    Llamarada,

    /// <summary>Soplo suave: la cámara entra en el nivel.</summary>
    Entrar,

    /// <summary>«Pop»: se abre una palabra del glosario.</summary>
    Glosario,
}

/// <summary>Un sonido y cuántos milisegundos espera antes de sonar.</summary>
public sealed record Pista(Sonido Sonido, double RetrasoMs = 0);

/// <summary>Reproduce efectos de sonido. Nunca debe dejar al juego sin responder: si falla, se calla.</summary>
public interface IReproductor
{
    /// <summary>Programa los sonidos de un momento y cancela los que quedaban pendientes del anterior.</summary>
    void Programar(IEnumerable<Pista> pistas);

    /// <summary>Suena ya, sin cancelar nada.</summary>
    void Sonar(Sonido sonido);

    /// <summary>Cancela los sonidos pendientes (los que ya suenan terminan solos).</summary>
    void Cancelar();
}

/// <summary>Lee textos en voz alta con una voz en español del propio equipo (D-13, D-50).</summary>
public interface INarrador
{
    /// <summary>Hay una voz local en español; si no, el botón «Escuchar» se oculta.</summary>
    bool Disponible { get; }

    void Leer(string texto);

    void Callar();
}

/// <summary>Sin sonido: lo usan las pruebas y la opción --silencio.</summary>
public sealed class ReproductorMudo : IReproductor
{
    public void Programar(IEnumerable<Pista> pistas)
    {
    }

    public void Sonar(Sonido sonido)
    {
    }

    public void Cancelar()
    {
    }
}

/// <summary>Sin voz: lo usan las pruebas y la opción --silencio.</summary>
public sealed class NarradorMudo : INarrador
{
    public bool Disponible => false;

    public void Leer(string texto)
    {
    }

    public void Callar()
    {
    }
}
