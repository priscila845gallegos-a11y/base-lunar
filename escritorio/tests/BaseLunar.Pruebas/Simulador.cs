using BaseLunar.Nucleo;

namespace BaseLunar.Pruebas;

/// <summary>Resumen de una simulación: rutas, victorias por respaldos restantes, abortadas y comprobaciones.</summary>
internal sealed class ResumenSimulacion
{
    public int Rutas { get; set; }
    public Dictionary<int, int> Victorias { get; } = new() { [3] = 0, [2] = 0, [1] = 0 };
    public int Abortadas { get; set; }
    public int Estados { get; set; }
    public int SinSalida { get; set; }
    public int N5SinGanadora { get; set; }
    public int Ayudas { get; set; }
    public int FueraDeRango { get; set; }
    public bool Perfecta { get; set; }
    public bool ConPerdida { get; set; }

    public override string ToString() =>
        $"rutas {Rutas} | victorias 3/2/1: {Victorias[3]}/{Victorias[2]}/{Victorias[1]} | abortadas {Abortadas} | " +
        $"estados de nivel {Estados} | sin salida {SinSalida} | ayudas {Ayudas}";
}

/// <summary>
/// Simulador de balance (sección 10 del Prompt 0): recorre todas las rutas, con fallos y reintentos,
/// usando solo la API pública del motor. Es el mismo recorrido que el de tests/tests.html.
/// </summary>
internal static class Simulador
{
    public static ResumenSimulacion Simular(bool claseX)
    {
        var resumen = new ResumenSimulacion();
        var contexto = new ContextoPartida(ClaseX: claseX);
        Seguir(Motor.Empezar(Motor.CrearEstadoInicial(), contexto), resumen, contexto);
        return resumen;
    }

    private static void Seguir(ResultadoMotor resultado, ResumenSimulacion resumen, ContextoPartida contexto)
    {
        if (resultado.Eventos.OfType<AyudaActivada>().Any()) resumen.Ayudas++;
        var estado = resultado.Estado;
        // Sección 10 (F5): ningún medidor sale de 0–100, ni en la partida ni en la foto del nivel.
        foreach (var medidor in Medidores.Todos)
        {
            if (estado.Medidores[medidor] is < 0 or > 100 || estado.Foto[medidor] is < 0 or > 100) resumen.FueraDeRango++;
        }
        switch (estado.Fase)
        {
            case Fase.Resultado:
                Seguir(Motor.Continuar(estado, contexto), resumen, contexto);
                return;
            case Fase.Respaldo:
                Seguir(Motor.Reintentar(estado), resumen, contexto);
                return;
            case Fase.Victoria or Fase.MisionAbortada:
                Terminar(resumen, estado);
                return;
        }
        resumen.Estados++;
        var nivel = Guion.Niveles[estado.Nivel - 1];
        if (!Motor.EsSobrevivible(estado.Medidores, nivel))
        {
            resumen.SinSalida++;
            if (nivel.Tipo == TipoNivel.Condicion) resumen.N5SinGanadora++;
        }
        foreach (var opcion in nivel.Opciones.Keys) Seguir(Motor.AplicarDecision(estado, opcion), resumen, contexto);
    }

    private static void Terminar(ResumenSimulacion resumen, Estado estado)
    {
        resumen.Rutas++;
        if (estado.Fase == Fase.MisionAbortada)
        {
            resumen.Abortadas++;
            resumen.ConPerdida = true;
            return;
        }
        resumen.Victorias[estado.Respaldos]++;
        if (estado.Respaldos == Configuracion.Reglas.RespaldosIniciales) resumen.Perfecta = true;
        else resumen.ConPerdida = true;
    }
}
