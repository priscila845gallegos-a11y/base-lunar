namespace BaseLunar.App.VistaModelos;

/// <summary>Un medidor del panel: valor, aviso de nivel bajo y número flotante.</summary>
public sealed class MedidorVM(InfoMedidor info) : Observable
{
    public Medidor Clave => info.Clave;
    public string Nombre => info.Nombre;
    public string Icono => info.Icono;
    public string ClavePincel => "Pincel." + info.Clave;

    public int Valor { get; private set; }
    public bool Bajo => Valor < Configuracion.Reglas.UmbralBajo;
    public string Texto => Presentacion.Porcentaje(Valor);

    /// <summary>Como el aria-valuetext de la web: «10 %, nivel bajo».</summary>
    public string TextoAccesible => Bajo ? Texto + TextosApp.NivelBajo : Texto;

    public string Flotante { get; private set; } = string.Empty;
    public bool FlotanteGana { get; private set; }

    /// <summary>Contador que dispara la animación del número flotante.</summary>
    public int Disparo { get; private set; }

    /// <summary>Contador del estallido de estrellas de «¡Al máximo!».</summary>
    public int Destello { get; private set; }

    internal void Actualizar(int valor)
    {
        if (valor == Valor) return;
        Valor = valor;
        Avisar(nameof(Valor));
        Avisar(nameof(Bajo));
        Avisar(nameof(Texto));
        Avisar(nameof(TextoAccesible));
    }

    internal void Destellar()
    {
        Destello++;
        Avisar(nameof(Destello));
    }

    internal void Flotar(string texto, bool gana)
    {
        Flotante = texto;
        FlotanteGana = gana;
        Disparo++;
        Avisar(nameof(Flotante));
        Avisar(nameof(FlotanteGana));
        Avisar(nameof(Disparo));
    }
}

/// <summary>Una ganancia, un costo o el requisito del Nivel 5 (Gana = null).</summary>
public sealed record EfectoVM(string Texto, bool? Gana);

/// <summary>Un botón de decisión, A o B.</summary>
public sealed class OpcionVM(string id) : Observable
{
    public string Id { get; } = id;
    public string Texto { get; private set; } = string.Empty;
    public IReadOnlyList<EfectoVM> Efectos { get; private set; } = [];
    public bool EsCondicion { get; private set; }
    public string NombreAccesible { get; private set; } = string.Empty;

    internal void Actualizar(Opcion opcion)
    {
        Texto = opcion.Texto;
        EsCondicion = opcion.Requisito is not null;
        var lineas = Presentacion.LineasEfecto(opcion);
        Efectos = EsCondicion
            ? [new EfectoVM(lineas[0], null)]
            : Presentacion.EfectosOrdenados(opcion.Efectos!).Zip(lineas, (e, texto) => new EfectoVM(texto, e.Value > 0)).ToArray();
        NombreAccesible = Presentacion.NombreOpcion(Id, opcion);
        AvisarTodo();
    }
}

public enum Situacion
{
    Pendiente,
    Actual,
    Hecha,
}

/// <summary>Una estación del mapa de la misión (un nivel).</summary>
public sealed class EstacionVM(Nivel nivel) : Observable
{
    public int Numero => nivel.Id;
    public string Titulo => nivel.Titulo;
    public string Avatar => nivel.Avatar;
    public Situacion Situacion { get; private set; }
    public bool EsHecha => Situacion == Situacion.Hecha;
    public bool EsActual => Situacion == Situacion.Actual;

    public string TextoAccesible => Textos.Rellenar(TextosApp.Mapa.Estacion, ("n", Numero), ("titulo", Titulo), ("situacion", Situacion switch
    {
        Situacion.Hecha => TextosApp.Mapa.Hecha,
        Situacion.Actual => TextosApp.Mapa.Actual,
        _ => TextosApp.Mapa.Pendiente,
    }));

    internal void Actualizar(Estado estado)
    {
        var nueva = Presentacion.EstacionHecha(estado, Numero) ? Situacion.Hecha
            : Presentacion.EstacionActual(estado, Numero) ? Situacion.Actual
            : Situacion.Pendiente;
        if (nueva == Situacion) return;
        Situacion = nueva;
        AvisarTodo();
    }

    public override string ToString() => TextoAccesible;
}

/// <summary>Una letra de la mini escala de llamaradas; Activa es la clase de la llamarada mostrada.</summary>
public sealed record LetraEscala(string Letra, bool Activa);

/// <summary>Una fuente con enlace. El nombre accesible avisa de que se abre el navegador.</summary>
public sealed record FuenteVM(string Nombre, string Url)
{
    public string NombreAccesible => Nombre + TextosApp.EnlaceExterno;
}

/// <summary>El 💡 Dato real de un nivel, con sus líneas extra y sus fuentes.</summary>
public sealed record DatoVM(string Titulo, string Texto, IReadOnlyList<string> Extra, string EtiquetaFuentes, IReadOnlyList<FuenteVM> Fuentes)
{
    public static DatoVM De(Nivel nivel, Tormenta tormenta)
    {
        var fuentes = Presentacion.FuentesDe(nivel, tormenta);
        return new DatoVM(
            nivel.Titulo,
            nivel.Dato.Texto,
            Presentacion.LineasExtra(nivel, tormenta),
            Presentacion.EtiquetaFuentes(fuentes.Count),
            fuentes.Select(f => new FuenteVM(f.Nombre, f.Url)).ToArray());
    }
}
