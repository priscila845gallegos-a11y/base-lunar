namespace BaseLunar.Nucleo;

/// <summary>Los 3 medidores de la base (3.1).</summary>
public enum Medidor
{
    Proteccion,
    Soporte,
    Bateria,
}

/// <summary>Valores de los 3 medidores. Es un valor inmutable: <see cref="Con"/> devuelve una copia.</summary>
public readonly record struct Medidores(int Proteccion, int Soporte, int Bateria)
{
    /// <summary>Orden de recorrido, igual que CLAVES en js/game.js.</summary>
    public static readonly IReadOnlyList<Medidor> Todos = [Medidor.Proteccion, Medidor.Soporte, Medidor.Bateria];

    public int this[Medidor medidor] => medidor switch
    {
        Medidor.Proteccion => Proteccion,
        Medidor.Soporte => Soporte,
        Medidor.Bateria => Bateria,
        _ => throw new ArgumentOutOfRangeException(nameof(medidor)),
    };

    /// <summary>Copia con un medidor cambiado.</summary>
    public Medidores Con(Medidor medidor, int valor) => medidor switch
    {
        Medidor.Proteccion => this with { Proteccion = valor },
        Medidor.Soporte => this with { Soporte = valor },
        Medidor.Bateria => this with { Bateria = valor },
        _ => throw new ArgumentOutOfRangeException(nameof(medidor)),
    };
}
