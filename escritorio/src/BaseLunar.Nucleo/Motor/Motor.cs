using static BaseLunar.Nucleo.Configuracion;

namespace BaseLunar.Nucleo;

/// <summary>
/// Motor puro: estado, reglas, respaldos y antibloqueo (sección 3 del Prompt 0 y DECISIONES.md).
/// Traslado fiel de js/game.js: mismas funciones, mismas reglas y mismos eventos.
/// No toca la interfaz, la red ni el disco. Cada función devuelve un estado nuevo y los eventos que
/// produjo; si la acción no corresponde a la fase, devuelve el mismo estado y ningún evento
/// (así un doble clic no aplica dos decisiones).
/// </summary>
public static class Motor
{
    private static readonly IReadOnlyList<Medidor> SinFallos = Array.Empty<Medidor>();
    private static readonly IReadOnlyList<Evento> SinEventos = Array.Empty<Evento>();

    private static int Recortar(int valor) => Math.Clamp(valor, Reglas.ValorMinimo, Reglas.ValorMaximo);

    private static int ValorDe(IReadOnlyDictionary<Medidor, int> efectos, Medidor medidor) =>
        efectos.TryGetValue(medidor, out var valor) ? valor : 0;

    private static ResultadoMotor SinCambios(Estado estado) => new(estado, SinEventos);

    private static Medidores MedidoresIniciales() => new(Reglas.ValorInicial, Reglas.ValorInicial, Reglas.ValorInicial);

    /// <summary>Estado inicial: pantalla de Inicio, medidores al 80 % y 3 respaldos.</summary>
    public static Estado CrearEstadoInicial() =>
        new(Fase.Inicio, 0, MedidoresIniciales(), MedidoresIniciales(), Reglas.RespaldosIniciales, false, SinFallos, null);

    /// <summary>
    /// Suma los efectos y recorta a 0–100. Falla todo medidor que quede en 0 o menos antes de recortar
    /// (D-02); «al máximo» es toda ganancia que pasa de 100.
    /// </summary>
    public static ResultadoEfectos AplicarEfectos(Medidores medidores, IReadOnlyDictionary<Medidor, int> efectos)
    {
        var crudos = medidores;
        var nuevos = medidores;
        foreach (var medidor in Medidores.Todos)
        {
            var crudo = medidores[medidor] + ValorDe(efectos, medidor);
            crudos = crudos.Con(medidor, crudo);
            nuevos = nuevos.Con(medidor, Recortar(crudo));
        }
        var fallidos = Reglas.PrioridadFallo.Where(m => crudos[m] <= Reglas.ValorMinimo).ToArray();
        var alMaximo = Medidores.Todos.Where(m => ValorDe(efectos, m) > 0 && crudos[m] > Reglas.ValorMaximo).ToArray();
        return new ResultadoEfectos(nuevos, crudos, fallidos, alMaximo);
    }

    /// <summary>Requisito del Nivel 5: estrictamente «mayor que» (3.4).</summary>
    public static bool CumpleRequisito(Medidores medidores, Requisito requisito) =>
        medidores[requisito.Medidor] > requisito.Umbral;

    private static bool OpcionViable(Medidores medidores, Opcion opcion) =>
        opcion.Requisito is not null
            ? CumpleRequisito(medidores, opcion.Requisito)
            : AplicarEfectos(medidores, opcion.Efectos!).Fallidos.Count == 0;

    /// <summary>¿Hay al menos una opción que no falla (o que gana, en el Nivel 5)?</summary>
    public static bool EsSobrevivible(Medidores medidores, Nivel nivel) =>
        nivel.Opciones.Values.Any(opcion => OpcionViable(medidores, opcion));

    /// <summary>Subida mínima por medidor para que una opción sea viable.</summary>
    private static IReadOnlyDictionary<Medidor, int> SubidaNecesaria(Medidores medidores, Opcion opcion)
    {
        var subida = new OrderedDictionary<Medidor, int>();
        if (opcion.Requisito is { } requisito)
        {
            if (medidores[requisito.Medidor] <= requisito.Umbral) subida.Add(requisito.Medidor, requisito.Umbral + 1 - medidores[requisito.Medidor]);
            return subida;
        }
        foreach (var medidor in Medidores.Todos)
        {
            var crudo = medidores[medidor] + ValorDe(opcion.Efectos!, medidor);
            if (crudo <= Reglas.ValorMinimo) subida.Add(medidor, Reglas.ValorMinimo + 1 - crudo);
        }
        return subida;
    }

    /// <summary>
    /// Antibloqueo (3.5, D-03): la menor ayuda que hace viable alguna opción (en empate, la A).
    /// Devuelve null si no hace falta ayuda.
    /// </summary>
    public static Ayuda? CalcularAyuda(Medidores medidores, Nivel nivel)
    {
        if (EsSobrevivible(medidores, nivel)) return null;
        IReadOnlyDictionary<Medidor, int>? mejor = null;
        foreach (var opcion in nivel.Opciones.Values)
        {
            var subida = SubidaNecesaria(medidores, opcion);
            if (mejor is null || subida.Values.Sum() < mejor.Values.Sum()) mejor = subida;
        }
        var nuevos = medidores;
        foreach (var (medidor, cantidad) in mejor!) nuevos = nuevos.Con(medidor, Recortar(nuevos[medidor] + cantidad));
        return new Ayuda(nuevos, mejor);
    }

    /// <summary>Aplica el antibloqueo si hace falta y anota el evento.</summary>
    private static Medidores ConAyuda(Medidores medidores, Nivel nivel, List<Evento> eventos)
    {
        var ayuda = CalcularAyuda(medidores, nivel);
        if (ayuda is null) return medidores;
        eventos.Add(new AyudaActivada(ayuda.Ajustes));
        return ayuda.Medidores;
    }

    /// <summary>Entra en un nivel: penalización X (antes de la foto), antibloqueo y foto.</summary>
    private static ResultadoMotor EntrarNivel(Estado estado, int numero, ContextoPartida contexto, IReadOnlyList<Nivel> niveles)
    {
        var nivel = niveles[numero - 1];
        var eventos = new List<Evento>();
        var medidores = estado.Medidores;
        if (nivel.AplicaPenalizacionX && contexto.ClaseX)
        {
            medidores = medidores.Con(Medidor.Proteccion, Recortar(medidores.Proteccion - Reglas.PenalizacionX));
            eventos.Add(new PenalizacionX(Medidor.Proteccion, Reglas.PenalizacionX));
        }
        medidores = ConAyuda(medidores, nivel, eventos);
        var nuevo = estado with
        {
            Fase = Fase.Nivel, Nivel = numero, Medidores = medidores, Foto = medidores,
            ClaseX = contexto.ClaseX, Fallidos = SinFallos, UltimaOpcion = null,
        };
        return new ResultadoMotor(nuevo, eventos);
    }

    /// <summary>Empieza la partida desde Inicio, en el nivel 1 o en el que pida el contexto (D-08).</summary>
    public static ResultadoMotor Empezar(Estado estado, ContextoPartida? contexto = null) =>
        Empezar(estado, contexto ?? new ContextoPartida(), Guion.Niveles);

    /// <summary>Empieza la partida con un guion concreto (para las pruebas).</summary>
    public static ResultadoMotor Empezar(Estado estado, ContextoPartida contexto, IReadOnlyList<Nivel> niveles)
    {
        if (estado.Fase != Fase.Inicio) return SinCambios(estado);
        var numero = contexto.Nivel is int pedido && pedido >= 1 && pedido <= niveles.Count ? pedido : 1;
        return EntrarNivel(CrearEstadoInicial(), numero, contexto, niveles);
    }

    /// <summary>Resta un respaldo; con 0 respaldos, Misión Abortada (D-04).</summary>
    private static ResultadoMotor PerderRespaldo(Estado estado, IReadOnlyList<Medidor> fallidos, Medidor? motivo, List<Evento> eventos)
    {
        var respaldos = estado.Respaldos - 1;
        eventos.Add(new RespaldoActivado(motivo, fallidos, respaldos));
        if (respaldos > 0) return new ResultadoMotor(estado with { Respaldos = respaldos, Fallidos = fallidos, Fase = Fase.Respaldo }, eventos);
        eventos.Add(new MisionAbortada());
        return new ResultadoMotor(estado with { Respaldos = 0, Fallidos = fallidos, Fase = Fase.MisionAbortada }, eventos);
    }

    private static ResultadoMotor DecidirConEfectos(Estado estado, Nivel nivel, string opcionId)
    {
        var opcion = nivel.Opciones[opcionId];
        var r = AplicarEfectos(estado.Medidores, opcion.Efectos!);
        var eventos = new List<Evento> { new EfectosAplicados(opcion.Efectos!) };
        eventos.AddRange(r.AlMaximo.Select(m => new MedidorAlMaximo(m)));
        var siguiente = estado with { Medidores = r.Medidores, UltimaOpcion = opcionId };
        if (r.Fallidos.Count > 0) return PerderRespaldo(siguiente, r.Fallidos, r.Fallidos[0], eventos);
        eventos.Add(new NivelSuperado(nivel.Id));
        return new ResultadoMotor(siguiente with { Fase = Fase.Resultado }, eventos);
    }

    private static ResultadoMotor DecidirConCondicion(Estado estado, Nivel nivel, string opcionId)
    {
        var siguiente = estado with { UltimaOpcion = opcionId };
        if (!CumpleRequisito(estado.Medidores, nivel.Opciones[opcionId].Requisito!)) return PerderRespaldo(siguiente, SinFallos, null, []);
        return new ResultadoMotor(siguiente with { Fase = Fase.Resultado }, [new NivelSuperado(nivel.Id)]);
    }

    /// <summary>
    /// Aplica la opción elegida («A» o «B»). Solo actúa en la fase Nivel. Eventos posibles:
    /// EfectosAplicados, MedidorAlMaximo, NivelSuperado, RespaldoActivado y MisionAbortada.
    /// </summary>
    public static ResultadoMotor AplicarDecision(Estado estado, string opcionId) =>
        AplicarDecision(estado, opcionId, Guion.Niveles);

    /// <summary>Aplica la opción elegida con un guion concreto (para las pruebas).</summary>
    public static ResultadoMotor AplicarDecision(Estado estado, string opcionId, IReadOnlyList<Nivel> niveles)
    {
        if (estado.Fase != Fase.Nivel) return SinCambios(estado);
        var nivel = niveles[estado.Nivel - 1];
        if (!nivel.Opciones.ContainsKey(opcionId)) return SinCambios(estado);
        return nivel.Tipo == TipoNivel.Condicion
            ? DecidirConCondicion(estado, nivel, opcionId)
            : DecidirConEfectos(estado, nivel, opcionId);
    }

    /// <summary>
    /// Reintento (3.3, D-01): restaura la foto del nivel; cada medidor que falló sube a máx(foto, 30).
    /// El estado restaurado pasa a ser la nueva foto. La penalización X ya está en la foto: no se repite.
    /// </summary>
    public static ResultadoMotor Reintentar(Estado estado) => Reintentar(estado, Guion.Niveles);

    /// <summary>Reintento con un guion concreto (para las pruebas).</summary>
    public static ResultadoMotor Reintentar(Estado estado, IReadOnlyList<Nivel> niveles)
    {
        if (estado.Fase != Fase.Respaldo) return SinCambios(estado);
        var medidores = estado.Foto;
        foreach (var medidor in estado.Fallidos) medidores = medidores.Con(medidor, Math.Max(estado.Foto[medidor], Reglas.MinimoReintento));
        var eventos = new List<Evento> { new Reintento(estado.Fallidos.ToArray()) };
        var finales = ConAyuda(medidores, niveles[estado.Nivel - 1], eventos);
        var nuevo = estado with { Fase = Fase.Nivel, Medidores = finales, Foto = finales, Fallidos = SinFallos, UltimaOpcion = null };
        return new ResultadoMotor(nuevo, eventos);
    }

    /// <summary>Tras el resultado: siguiente nivel o, después del Nivel 5, la Victoria (D-07).</summary>
    public static ResultadoMotor Continuar(Estado estado, ContextoPartida? contexto = null) =>
        Continuar(estado, contexto ?? new ContextoPartida(), Guion.Niveles);

    /// <summary>Continuar con un guion concreto (para las pruebas).</summary>
    public static ResultadoMotor Continuar(Estado estado, ContextoPartida contexto, IReadOnlyList<Nivel> niveles)
    {
        if (estado.Fase != Fase.Resultado) return SinCambios(estado);
        if (estado.Nivel < niveles.Count) return EntrarNivel(estado, estado.Nivel + 1, contexto, niveles);
        return new ResultadoMotor(estado with { Fase = Fase.Victoria }, [new Victoria(estado.Respaldos)]);
    }

    /// <summary>«Jugar de nuevo»: devuelve exactamente el estado inicial.</summary>
    public static Estado Reiniciar() => CrearEstadoInicial();
}
