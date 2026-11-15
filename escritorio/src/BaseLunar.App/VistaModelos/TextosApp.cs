namespace BaseLunar.App.VistaModelos;

/// <summary>
/// Textos propios de la pantalla. Los que ya existen en index.html se copian literalmente (lo comprueba
/// PruebasTextosApp); los de escritorio, que la web no tiene, están marcados.
/// </summary>
public static class TextosApp
{
    public const string TituloVentana = "Base Lunar: El Escudo Cósmico";
    public const string Titulo = "Base Lunar";
    public const string Subtitulo = "El Escudo Cósmico";
    public const string RegionMedidores = "Medidores de la base";
    public const string RegionTarjeta = "Tarjeta de misión";
    public const string RegionDecisiones = "Decisiones";
    public const string NivelBajo = ", nivel bajo";

    /// <summary>Escritorio: rótulo visible bajo los salvavidas (en la web solo lo oye el lector de pantalla).</summary>
    public const string Respaldos = "Respaldos";
    public const string PrefijoOpcion = "Opción {id}: ";
    public const string DatoReal = "💡 Dato real";

    /// <summary>Escritorio: el enlace se abre en el navegador, no en otra pestaña.</summary>
    public const string EnlaceExterno = " (se abre en el navegador)";

    public static class Inicio
    {
        public const string Etiqueta = "🚀 Misión: Base Lunar";
        public const string Avatar = "🌕";
        public const string Titulo = "¡Hola, comandante!";
        public const string Historia = "La NASA planea construir una base en la Luna. ¡En este juego tú ya la comandas!";
        public const string Nota = "Juego educativo independiente, con datos reales de NASA y CSA.";

        public static readonly IReadOnlyList<string> Instrucciones =
        [
            "Cuida los 3 medidores: 🛡️ Protección, 💧 Soporte Vital y ⚡ Batería.",
            "Son 5 niveles. En cada uno eliges A o B: ¡ninguna opción es gratis!",
            "Si un medidor llega a 0, usas un Protocolo de Respaldo. Tienes 3.",
            "Al superar cada nivel, ganas un 💡 Dato real con su fuente.",
        ];
    }

    public static class Abortada
    {
        public const string Etiqueta = "🪂 Cápsula de Regreso";
        public const string Titulo = "Misión Abortada";
        public const string Historia = "¡La tripulación volvió a salvo a la Tierra en la cápsula! Aprendiste mucho en esta misión.";
    }

    public static class Victoria
    {
        public const string Etiqueta = "🏅 ¡Misión cumplida!";
        public const string Avatar = "🏅";
        public const string Titulo = "Certificado de Comandante Junior de la Base Lunar";
        public const string Historia = "El relevo llegó y la base sigue funcionando. ¡Gracias por comandarla!";
        public const string Fecha = "Fecha:";
        public const string Aprendiste = "Lo que aprendiste";
        public const string Nota = "Datos: NASA DONKI, NASA OSDR / RadLab, NASA y CSA. Es un juego educativo independiente: este certificado no es oficial.";
    }

    /// <summary>Escritorio: el mapa de progreso no existe en la web, que solo tiene 5 puntos.</summary>
    public static class Mapa
    {
        public const string Nombre = "Mapa de la misión";
        public const string Base = "Base";
        public const string Meta = "Relevo";
        public const string Hecha = "superado";
        public const string Actual = "en curso";
        public const string Pendiente = "por jugar";
        public const string Estacion = "Nivel {n}: {titulo}, {situacion}";
    }
}
