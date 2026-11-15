using System.Globalization;
using System.Text.RegularExpressions;

namespace BaseLunar.Nucleo;

/// <summary>
/// Textos de la interfaz con {marcadores}. Traslado fiel de BaseLunar.textos (js/levels.js);
/// PruebasParidad comprueba que son los mismos 34 textos.
/// </summary>
public static partial class Textos
{
    public static class Hud
    {
        public const string Inicio = "Antes de empezar";
        public const string Nivel = "Nivel {n} de {total}";
        public const string Victoria = "¡Misión cumplida!";
        public const string Respaldos = "Protocolos de Respaldo: quedan {n} de {total}";
    }

    public static class Botones
    {
        public const string Inicio = "Comenzar misión";
        public const string Resultado = "Continuar";
        public const string Respaldo = "Reintentar";
        public const string Victoria = "Jugar de nuevo";
        public const string MisionAbortada = "Jugar de nuevo";
    }

    public static class Opciones
    {
        public const string Requisito = "Necesita más de {umbral} % {icono}";
        public const string RequisitoAccesible = "Necesita más de {umbral} % de {nombre}.";
        public const string Gana = "gana {n} de {nombre}";
        public const string Cuesta = "cuesta {n} de {nombre}";
    }

    public static class Tarjeta
    {
        public const string PrefijoNivel = "Nivel {n}: ";
        public const string Fuente = "Fuente:";
        public const string Fuentes = "Fuentes:";
        public const string PestanaNueva = " (se abre en otra pestaña)";
    }

    /// <summary>Mensajes de Respaldo (3.6, con la corrección T-05). La prioridad está en Configuracion.</summary>
    public static class Respaldo
    {
        public const string Soporte = "¡Protocolo de Respaldo activado! Se acabaron el agua limpia y el oxígeno. La tripulación necesita soporte vital para estar sana. ¡Reintentemos este nivel!";
        public const string Proteccion = "¡Protocolo de Respaldo activado! La protección se agotó y la radiación entró a la base. La tripulación corrió al refugio. ¡Reintentemos este nivel!";
        public const string Bateria = "¡Protocolo de Respaldo activado! Nos quedamos sin energía para los calentadores. Sin electricidad, la base se enfría muchísimo. ¡Reintentemos este nivel!";
        public const string Condicion = "¡Protocolo de Respaldo activado! {fallo} ¡Reintentemos este nivel!";
        public const string Pista = "Pista: si un medidor llega a 0, se queda vacío. ¡Mira los números antes de elegir!";
        public const string PistaCondicion = "Pista: compara lo que necesita cada botón con tus medidores.";

        /// <summary>Mensaje del medidor que falló.</summary>
        public static string De(Medidor medidor) => medidor switch
        {
            Medidor.Soporte => Soporte,
            Medidor.Proteccion => Proteccion,
            Medidor.Bateria => Bateria,
            _ => throw new ArgumentOutOfRangeException(nameof(medidor)),
        };
    }

    public const string Ayuda = "¡Ayuda de Control de Misión! Te enviamos {recurso} extra para que puedas seguir.";
    public const string PenalizacionX = "Esta llamarada es de clase X, la más fuerte. La protección bajó {n} puntos.";
    public const string AlMaximo = "¡Al máximo!";

    public static class Llamarada
    {
        public const string Linea = "Llamarada real: clase {clase} del {fecha} (NASA DONKI).";
        public const string SerieX = "¡En solo 2 días, el Sol lanzó {n} llamaradas de clase X!";
        public const string Fuente = "NASA DONKI, registro de la llamarada";
    }

    public static class Anuncios
    {
        public const string Sube = "{nombre} sube a {valor}.";
        public const string Baja = "{nombre} baja a {valor}.";
        public const string AlMaximo = "{nombre} está al máximo.";
        public const string Dato = "Dato real: {texto}";
    }

    public static class Certificado
    {
        public const string Respaldos = "Respaldos sin usar: {n} de {total}";
    }

    /// <summary>Rellena los {marcadores} de una plantilla; los que no tienen valor quedan igual.</summary>
    public static string Rellenar(string plantilla, params (string Clave, object Valor)[] valores) =>
        Marcador().Replace(plantilla, m =>
        {
            var par = valores.FirstOrDefault(v => v.Clave == m.Groups[1].Value);
            return par.Clave is null ? m.Value : Convert.ToString(par.Valor, CultureInfo.InvariantCulture) ?? string.Empty;
        });

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex Marcador();
}
