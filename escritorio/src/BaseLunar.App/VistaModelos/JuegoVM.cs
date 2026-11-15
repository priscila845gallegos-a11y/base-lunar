using System.Windows.Threading;
using static BaseLunar.Nucleo.Configuracion;

namespace BaseLunar.App.VistaModelos;

/// <summary>Texto para el lector de pantalla: cortés o urgente (solo «Respaldo activado», 5.5).</summary>
public sealed record Anuncio(string Texto, bool Urgente);

/// <summary>
/// Modelo de vista de la partida. Sigue el mismo flujo que js/ui.js: recibe la acción, llama al motor y
/// publica el nuevo estado; anima, anuncia y bloquea los controles durante la transición (D-30).
/// Nunca decide reglas: todo lo decide <see cref="Motor"/>.
/// </summary>
public sealed class JuegoVM : Observable
{
    private readonly DispatcherTimer transicion;
    private Estado estado = Motor.CrearEstadoInicial();
    private AyudaActivada? ayuda;

    public JuegoVM() : this(DatosDonki.RespaldoHistorico())
    {
    }

    /// <param name="tormenta">Dato del Nivel 4. F3.2: el respaldo histórico; F3.5: la consulta en vivo.</param>
    public JuegoVM(Tormenta tormenta)
    {
        Tormenta = tormenta;
        Medidores = Guion.InfoMedidores.Select(i => new MedidorVM(i)).ToArray();
        Estaciones = Guion.Niveles.Select(n => new EstacionVM(n)).ToArray();
        Elegir = new Comando(p => ElegirOpcion(p as string ?? string.Empty), () => !Bloqueado);
        Principal = new Comando(_ => AccionPrincipal(), () => !Bloqueado);
        transicion = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Tiempos.TransicionMs) };
        transicion.Tick += (_, _) => TerminarTransicion();
        Pintar();
    }

    /// <summary>Pide a la vista que anuncie un texto en la región en vivo.</summary>
    public event EventHandler<Anuncio>? Anunciar;

    /// <summary>Al terminar la transición: la vista mueve el foco según la fase (D-28).</summary>
    public event EventHandler<Fase>? Enfocar;

    public Tormenta Tormenta { get; }
    public Estado Estado => estado;
    public IReadOnlyList<MedidorVM> Medidores { get; }
    public IReadOnlyList<EstacionVM> Estaciones { get; }
    public OpcionVM OpcionA { get; } = new("A");
    public OpcionVM OpcionB { get; } = new("B");
    public Comando Elegir { get; }
    public Comando Principal { get; }
    public bool Bloqueado { get; private set; }

    /// <summary>Contadores que disparan animaciones: temblor de la tarjeta y salto del astronauta.</summary>
    public int Temblor { get; private set; }
    public int Celebracion { get; private set; }

    // ---------- Fase y pantalla ----------
    public Fase Fase => estado.Fase;
    public bool EsInicio => Fase == Fase.Inicio;
    public bool EsVictoria => Fase == Fase.Victoria;
    public bool EsAbortada => Fase == Fase.MisionAbortada;
    public bool EsVistaNivel => Fase is Fase.Nivel or Fase.Resultado or Fase.Respaldo;
    public bool EnNivel => Fase == Fase.Nivel;
    public bool EsRespaldo => Fase == Fase.Respaldo;
    public bool EsResultado => Fase == Fase.Resultado;
    public bool Alerta => EsVistaNivel && NivelActual?.Tema == Tema.Alerta;
    public Nivel? NivelActual => Presentacion.NivelDe(estado);

    // ---------- Panel de estado ----------
    public string TextoProgreso => Presentacion.TextoProgreso(estado);
    public string TextoRespaldos => Presentacion.TextoRespaldos(estado.Respaldos);

    /// <summary>Un elemento por respaldo inicial: true si todavía está disponible.</summary>
    public IReadOnlyList<bool> Respaldos => Enumerable.Range(0, Reglas.RespaldosIniciales).Select(i => i < estado.Respaldos).ToArray();

    public bool PinEnBase => EsInicio;
    public bool PinEnMeta => EsVictoria;

    // ---------- Tarjeta: nivel ----------
    public string Etiqueta => NivelActual is { } n ? Presentacion.EtiquetaNivel(n, Tormenta) : string.Empty;
    public string Avatar => NivelActual?.Avatar ?? TextosApp.Inicio.Avatar;

    /// <summary>Insignia de la escena: la del nivel, la medalla en la victoria o el salvavidas al abortar.</summary>
    public string AvatarEscena => EsVictoria ? TextosApp.Victoria.Avatar : EsAbortada ? "🪂" : Avatar;
    public string TituloNivel => NivelActual?.Titulo ?? string.Empty;
    public string NombreTituloNivel => NivelActual is { } n ? Textos.Rellenar(Textos.Tarjeta.PrefijoNivel, ("n", n.Id)) + n.Titulo : string.Empty;
    public string Historia => NivelActual?.Historia ?? string.Empty;
    public string? AvisoX => NivelActual is { AplicaPenalizacionX: true } && estado.ClaseX ? Presentacion.TextoAvisoX() : null;
    public string? AvisoAyuda => ayuda is not null && EnNivel ? Presentacion.TextoAyuda(ayuda.Ajustes) : null;
    public string? MensajeRespaldo => EsRespaldo && NivelActual is { } n ? Presentacion.TextosRespaldo(estado, n).Mensaje : null;
    public string? PistaRespaldo => EsRespaldo && NivelActual is { } n ? Presentacion.TextosRespaldo(estado, n).Pista : null;
    public string? TextoResultado => EsResultado && NivelActual is { } n ? n.Opciones[estado.UltimaOpcion!].Resultado : null;
    public DatoVM? Dato => EsResultado && NivelActual is { } n ? DatoVM.De(n, Tormenta) : null;

    // ---------- Tarjeta: victoria ----------
    public string FechaCertificado => TextosApp.Victoria.Fecha + " " + Presentacion.FechaLocal(DateTime.Now);
    public IReadOnlyList<int> Estrellas => Enumerable.Range(1, estado.Respaldos).ToArray();
    public string TextoRespaldosCertificado => Presentacion.TextoRespaldosCertificado(estado.Respaldos);
    public IReadOnlyList<string> MedidoresCertificado => Guion.InfoMedidores.Select(i => Presentacion.LineaMedidor(i, estado.Medidores)).ToArray();
    public IReadOnlyList<DatoVM> DatosCertificado => EsVictoria ? Guion.Niveles.Select(n => DatoVM.De(n, Tormenta)).ToArray() : [];

    // ---------- Decisiones ----------
    public string TextoBotonPrincipal => Presentacion.TextoBoton(Fase);
    public bool BotonReinicia => Fase is Fase.Victoria or Fase.MisionAbortada;

    // ---------- Acciones (las mismas de ui.js) ----------

    /// <summary>Elige la opción «A» o «B». Ignorada durante la transición.</summary>
    public void ElegirOpcion(string opcionId)
    {
        if (Bloqueado) return;
        Procesar(Motor.AplicarDecision(estado, opcionId));
    }

    /// <summary>El botón principal: comenzar, continuar, reintentar o jugar de nuevo.</summary>
    public void AccionPrincipal()
    {
        if (Bloqueado) return;
        var contexto = new ContextoPartida(ClaseX: Tormenta.Clase.StartsWith('X'));
        ResultadoMotor? resultado = Fase switch
        {
            Fase.Inicio => Motor.Empezar(estado, contexto),
            Fase.Resultado => Motor.Continuar(estado, contexto),
            Fase.Respaldo => Motor.Reintentar(estado),
            Fase.Victoria or Fase.MisionAbortada => new ResultadoMotor(Motor.Reiniciar(), []),
            _ => null,
        };
        if (resultado is not null) Procesar(resultado);
    }

    /// <summary>Aplica el resultado del motor: pinta, anima, anuncia y bloquea durante la transición.</summary>
    private void Procesar(ResultadoMotor resultado)
    {
        if (ReferenceEquals(resultado.Estado, estado)) return; // el motor ignoró la acción
        estado = resultado.Estado;
        var nuevaAyuda = resultado.Eventos.OfType<AyudaActivada>().FirstOrDefault();
        if (nuevaAyuda is not null || Fase != Fase.Nivel) ayuda = nuevaAyuda;
        Bloqueado = true;
        Pintar();
        Animar(resultado.Eventos);
        AnunciarEventos(resultado.Eventos);
        transicion.Stop();
        transicion.Start();
    }

    /// <summary>Fin de la transición: desbloquea y mueve el foco. Las pruebas lo llaman sin esperar.</summary>
    internal void TerminarTransicion()
    {
        transicion.Stop();
        if (!Bloqueado) return;
        Bloqueado = false;
        ActualizarBloqueo();
        Enfocar?.Invoke(this, Fase);
    }

    private void Pintar()
    {
        foreach (var medidor in Medidores) medidor.Actualizar(estado.Medidores[medidor.Clave]);
        foreach (var estacion in Estaciones) estacion.Actualizar(estado);
        if (EnNivel && NivelActual is { } nivel)
        {
            OpcionA.Actualizar(nivel.Opciones["A"]);
            OpcionB.Actualizar(nivel.Opciones["B"]);
        }
        AvisarTodo();
        ActualizarBloqueo();
    }

    private void ActualizarBloqueo()
    {
        Elegir.Refrescar();
        Principal.Refrescar();
        Avisar(nameof(Bloqueado));
    }

    private MedidorVM MedidorDe(Medidor clave) => Medidores.First(m => m.Clave == clave);

    private void Animar(IReadOnlyList<Evento> eventos)
    {
        var maximos = eventos.OfType<MedidorAlMaximo>().Select(e => e.Medidor).ToHashSet();
        foreach (var evento in eventos)
        {
            switch (evento)
            {
                case EfectosAplicados e:
                    foreach (var (clave, valor) in Presentacion.EfectosOrdenados(e.Efectos))
                        MedidorDe(clave).Flotar(maximos.Contains(clave) ? Textos.AlMaximo : Presentacion.ConIcono(valor, clave), valor > 0);
                    break;
                case PenalizacionX e:
                    MedidorDe(e.Medidor).Flotar(Presentacion.ConIcono(-e.Cantidad, e.Medidor), false);
                    break;
                case AyudaActivada e:
                    foreach (var (clave, valor) in e.Ajustes) MedidorDe(clave).Flotar(Presentacion.ConIcono(valor, clave), true);
                    break;
                case RespaldoActivado:
                    Temblor++;
                    Avisar(nameof(Temblor));
                    break;
                case NivelSuperado or Nucleo.Victoria:
                    Celebracion++;
                    Avisar(nameof(Celebracion));
                    break;
            }
        }
    }

    /// <summary>Cortés para resultados y avisos; urgente solo para «Respaldo activado» (5.5).</summary>
    private void AnunciarEventos(IReadOnlyList<Evento> eventos)
    {
        if (EsRespaldo && MensajeRespaldo is { } mensaje) Anunciar?.Invoke(this, new Anuncio(mensaje, Urgente: true));
        var mensajes = new List<string>();
        if (eventos.OfType<PenalizacionX>().Any()) mensajes.Add(Presentacion.TextoAvisoX());
        if (eventos.OfType<AyudaActivada>().FirstOrDefault() is { } activada) mensajes.Add(Presentacion.TextoAyuda(activada.Ajustes));
        if (EsResultado && NivelActual is { } nivel) mensajes.AddRange(Presentacion.MensajesResultado(estado, nivel, eventos));
        if (mensajes.Count > 0) Anunciar?.Invoke(this, new Anuncio(string.Join(" ", mensajes), Urgente: false));
    }
}
