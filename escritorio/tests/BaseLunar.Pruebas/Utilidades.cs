using BaseLunar.Nucleo;

namespace BaseLunar.Pruebas;

/// <summary>Ayudas compartidas por las pruebas (las mismas que en tests/tests.html).</summary>
internal static class Utilidades
{
    public static Medidores M(int proteccion, int soporte, int bateria) => new(proteccion, soporte, bateria);

    /// <summary>Estado en la fase Nivel con los medidores dados.</summary>
    public static Estado EnNivel(int nivel, Medidores medidores) =>
        Motor.CrearEstadoInicial() with { Fase = Fase.Nivel, Nivel = nivel, Medidores = medidores, Foto = medidores };

    /// <summary>Juega una ruta de decisiones (por ejemplo, «ABAA») desde el Inicio y devuelve el último resultado.</summary>
    public static ResultadoMotor Jugar(string ruta, bool claseX = false)
    {
        var contexto = new ContextoPartida(ClaseX: claseX);
        var resultado = Motor.Empezar(Motor.CrearEstadoInicial(), contexto);
        foreach (var opcion in ruta)
        {
            if (resultado.Estado.Fase == Fase.Resultado) resultado = Motor.Continuar(resultado.Estado, contexto);
            resultado = Motor.AplicarDecision(resultado.Estado, opcion.ToString());
        }
        return resultado;
    }

    /// <summary>¿Hay algún evento del tipo T que cumpla la condición?</summary>
    public static bool Hay<T>(this ResultadoMotor resultado, Func<T, bool>? condicion = null) where T : Evento =>
        resultado.Eventos.OfType<T>().Any(e => condicion is null || condicion(e));

    /// <summary>Nivel de prueba con dos opciones de efectos.</summary>
    public static Nivel NivelDePrueba(int id, IReadOnlyDictionary<Medidor, int> efectosA, IReadOnlyDictionary<Medidor, int> efectosB) =>
        new(id, "Prueba", "", "", "", Tema.Normal, TipoNivel.Decision,
            Guion.Opciones(new Opcion("A", "", efectosA), new Opcion("B", "", efectosB)),
            new Dato("", [], true));

    /// <summary>Carpeta raíz del repositorio: la que contiene js/levels.js.</summary>
    public static string RaizRepositorio()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null && !File.Exists(Path.Combine(carpeta.FullName, "js", "levels.js"))) carpeta = carpeta.Parent;
        return carpeta?.FullName ?? throw new DirectoryNotFoundException("No se encontró la raíz del repositorio (js/levels.js).");
    }

    /// <summary>Lee un archivo del repositorio con finales de línea LF.</summary>
    public static string LeerRepositorio(string rutaRelativa) =>
        File.ReadAllText(Path.Combine(RaizRepositorio(), rutaRelativa)).Replace("\r\n", "\n");
}
