namespace BaseLunar.Nucleo;

/// <summary>
/// Configuración global: solo datos. Traslado fiel de js/config.js;
/// PruebasParidad comprueba que cada valor coincide con el original.
/// </summary>
public static class Configuracion
{
    /// <summary>Reglas del juego (sección 3 del Prompt 0).</summary>
    public static class Reglas
    {
        public const int ValorInicial = 80;
        public const int ValorMinimo = 0;
        public const int ValorMaximo = 100;

        /// <summary>Por debajo de este valor, el medidor se muestra en estado «bajo».</summary>
        public const int UmbralBajo = 30;

        /// <summary>Valor mínimo de un medidor que falló, al reintentar el nivel.</summary>
        public const int MinimoReintento = 30;

        public const int RespaldosIniciales = 3;

        /// <summary>Protección que resta una llamarada de clase X al empezar el Nivel 4.</summary>
        public const int PenalizacionX = 5;

        /// <summary>Si fallan varios medidores a la vez, se muestra el mensaje del primero de esta lista (3.6).</summary>
        public static readonly IReadOnlyList<Medidor> PrioridadFallo = [Medidor.Soporte, Medidor.Proteccion, Medidor.Bateria];
    }

    /// <summary>NASA DONKI (sección 7 del Prompt 0).</summary>
    public static class Nasa
    {
        /// <summary>Clave personal de api.nasa.gov (D-10). Es visible en el ejecutable: no reutilizarla en otros servicios.</summary>
        public const string Clave = "gCcCt7wbkbQJNb6PLrqFPNttPwgwjRFGdWkjNDQj";

        /// <summary>Último recurso (D-10).</summary>
        public const string ClaveReserva = "DEMO_KEY";

        public const string UrlFlr = "https://api.nasa.gov/DONKI/FLR";

        /// <summary>Segunda vía (D-19): el servicio DONKI del CCMC de la NASA, sin clave. Solo escritorio.</summary>
        public const string UrlFlrCcmc = "https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR";
        public const int DiasConsulta = 30;
        public const int TimeoutMs = 3000;
        public const long CacheMs = 6L * 60 * 60 * 1000;
        public const string CacheClave = "baseLunar.donki.v1";

        /// <summary>Respaldo histórico: llamarada X8.7 del 14-05-2024.</summary>
        public const string FlrRespaldoId = "2024-05-14T16:46:00-FLR-001";
    }

    /// <summary>Tiempos de la interfaz, en milisegundos.</summary>
    public static class Tiempos
    {
        /// <summary>Controles bloqueados tras cada acción (D-30).</summary>
        public const int TransicionMs = 450;

        public const int TemblorMs = 700;
        public const int AnuncioMs = 60;
    }

    /// <summary>Parámetros de demostración (en la web, ?nivel=4; en escritorio, --nivel 4).</summary>
    public static class ParametrosDemo
    {
        public const string Nivel = "nivel";
        public const string Offline = "offline";
        public const string Debug = "debug";
    }
}
