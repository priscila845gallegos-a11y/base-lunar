using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using BaseLunar.Nucleo;
using static BaseLunar.Nucleo.Configuracion;

namespace BaseLunar.Pruebas;

/// <summary>
/// Paridad con la versión web (js/*.js), que no se modifica: cada valor y cada texto del núcleo en C#
/// tiene que aparecer tal cual en el archivo original. Así el traslado no puede desviarse sin que falle una prueba.
/// </summary>
[TestClass]
public sealed partial class PruebasParidad
{
    private static readonly string LevelsJs = LeerJs("js/levels.js");
    private static readonly string ConfigJs = LeerJs("js/config.js");
    private static readonly string NasaDataJs = LeerJs("js/nasaData.js");

    /// <summary>Lee un archivo JS; si tuviera escapes  , los convierte en el carácter (los archivos actuales usan el carácter).</summary>
    private static string LeerJs(string ruta) => Utilidades.LeerRepositorio(ruta).Replace("\\u00A0", " ");

    /// <summary>Cómo se escribe un texto en los archivos JS: entre comillas simples.</summary>
    private static string Js(string texto) => "'" + texto.Replace("\\", "\\\\").Replace("'", "\\'") + "'";

    private static string NombreJs(Medidor medidor) => medidor.ToString().ToLowerInvariant();

    private static string Numero(long valor) => valor.ToString(CultureInfo.InvariantCulture);

    private static void Contiene(string archivo, string fragmento, string contexto) =>
        Assert.IsTrue(archivo.Contains(fragmento, StringComparison.Ordinal), $"{contexto}: no aparece «{fragmento}»");

    [TestMethod(DisplayName = "Paridad con js/config.js: reglas, tiempos, NASA y parámetros de demo")]
    public void ParidadConfiguracion()
    {
        string[] fragmentos =
        [
            $"VALOR_INICIAL: {Numero(Reglas.ValorInicial)},",
            $"VALOR_MINIMO: {Numero(Reglas.ValorMinimo)},",
            $"VALOR_MAXIMO: {Numero(Reglas.ValorMaximo)},",
            $"UMBRAL_BAJO: {Numero(Reglas.UmbralBajo)},",
            $"MINIMO_REINTENTO: {Numero(Reglas.MinimoReintento)},",
            $"RESPALDOS_INICIALES: {Numero(Reglas.RespaldosIniciales)},",
            $"PENALIZACION_X: {Numero(Reglas.PenalizacionX)},",
            "PRIORIDAD_FALLO: Object.freeze([" + string.Join(", ", Reglas.PrioridadFallo.Select(m => Js(NombreJs(m)))) + "])",
            $"TRANSICION_MS: {Numero(Tiempos.TransicionMs)},",
            $"TEMBLOR_MS: {Numero(Tiempos.TemblorMs)},",
            $"ANUNCIO_MS: {Numero(Tiempos.AnuncioMs)},",
            $"NASA_CLAVE: {Js(Nasa.Clave)},",
            $"NASA_CLAVE_RESERVA: {Js(Nasa.ClaveReserva)},",
            $"NASA_URL_FLR: {Js(Nasa.UrlFlr)},",
            $"NASA_DIAS_CONSULTA: {Numero(Nasa.DiasConsulta)},",
            $"NASA_TIMEOUT_MS: {Numero(Nasa.TimeoutMs)},",
            "NASA_CACHE_MS: 6 * 60 * 60 * 1000,",
            $"NASA_CACHE_CLAVE: {Js(Nasa.CacheClave)},",
            $"FLR_RESPALDO_ID: {Js(Nasa.FlrRespaldoId)},",
            $"PARAMETROS_DEMO: Object.freeze({{ NIVEL: {Js(ParametrosDemo.Nivel)}, OFFLINE: {Js(ParametrosDemo.Offline)}, DEBUG: {Js(ParametrosDemo.Debug)} }})",
        ];
        foreach (var fragmento in fragmentos) Contiene(ConfigJs, fragmento, "config.js");
        Assert.AreEqual(Nasa.CacheMs, (long)TimeSpan.FromHours(6).TotalMilliseconds, "la caché dura 6 horas");
    }

    [TestMethod(DisplayName = "Paridad con js/levels.js: los 3 medidores")]
    public void ParidadMedidores()
    {
        Assert.HasCount(3, Guion.InfoMedidores);
        foreach (var m in Guion.InfoMedidores)
        {
            var fragmento = $"{{ clave: {Js(NombreJs(m.Clave))}, nombre: {Js(m.Nombre)}, icono: {Js(m.Icono)}, recurso: {Js(m.Recurso)} }}";
            Contiene(LevelsJs, fragmento, "medidores");
        }
    }

    [TestMethod(DisplayName = "Paridad con js/levels.js: los 5 niveles, con sus textos y cifras")]
    public void ParidadNiveles()
    {
        Assert.HasCount(5, NivelJs().Matches(LevelsJs), "niveles en levels.js");
        Assert.HasCount(5, Guion.Niveles);
        foreach (var nivel in Guion.Niveles) ComprobarNivel(nivel);
    }

    private static void ComprobarNivel(Nivel nivel)
    {
        var bloque = BloqueNivel(nivel.Id);
        var contexto = $"Nivel {nivel.Id}";
        foreach (var fragmento in FragmentosNivel(nivel)) Contiene(bloque, fragmento, contexto);
        Assert.AreEqual(nivel.AplicaPenalizacionX, bloque.Contains("penalizacionX: true", StringComparison.Ordinal), $"{contexto}: penalizacionX");
        Assert.AreEqual(nivel.DatoTormenta, bloque.Contains("datoTormenta: true", StringComparison.Ordinal), $"{contexto}: datoTormenta");
        Assert.AreEqual(nivel.EtiquetaHistorico is not null, bloque.Contains("etiquetaHistorico:", StringComparison.Ordinal), $"{contexto}: etiquetaHistorico");
        Assert.AreEqual(nivel.Dato.Extra is not null, bloque.Contains("extra:", StringComparison.Ordinal), $"{contexto}: extra");
        Assert.HasCount(nivel.Dato.Fuentes.Count, FuenteJs().Matches(bloque), $"{contexto}: número de fuentes");
        Assert.HasCount(2, nivel.Opciones, $"{contexto}: opciones");
    }

    /// <summary>Texto de un nivel dentro de levels.js: desde «id: N,» hasta el siguiente nivel o el fin de la lista.</summary>
    private static string BloqueNivel(int id)
    {
        var inicio = LevelsJs.IndexOf($"\n    id: {id},\n", StringComparison.Ordinal);
        Assert.AreNotEqual(-1, inicio, $"no se encontró el nivel {id} en levels.js");
        var siguiente = LevelsJs.IndexOf($"\n    id: {id + 1},\n", inicio, StringComparison.Ordinal);
        var fin = siguiente >= 0 ? siguiente : LevelsJs.IndexOf("\n]);", inicio, StringComparison.Ordinal);
        return LevelsJs[inicio..fin];
    }

    private static IEnumerable<string> FragmentosNivel(Nivel nivel)
    {
        yield return $"titulo: {Js(nivel.Titulo)},";
        yield return $"etiqueta: {Js(nivel.Etiqueta)},";
        if (nivel.EtiquetaHistorico is not null) yield return $"etiquetaHistorico: {Js(nivel.EtiquetaHistorico)},";
        yield return $"avatar: {Js(nivel.Avatar)},";
        yield return $"historia: {Js(nivel.Historia)},";
        yield return $"tema: {Js(nivel.Tema.ToString().ToLowerInvariant())},";
        yield return $"tipo: {Js(nivel.Tipo.ToString().ToLowerInvariant())},";
        foreach (var fragmento in nivel.Opciones.SelectMany(o => FragmentosOpcion(o.Key, o.Value))) yield return fragmento;
        yield return $"texto: {Js(nivel.Dato.Texto)},";
        if (nivel.Dato.Extra is not null) yield return "extra: [" + string.Join(", ", nivel.Dato.Extra.Select(Js)) + "],";
        foreach (var fuente in nivel.Dato.Fuentes) yield return $"{{ nombre: {Js(fuente.Nombre)}, url: {Js(fuente.Url)} }}";
        yield return $"verificado: {(nivel.Dato.Verificado ? "true" : "false")},";
    }

    private static IEnumerable<string> FragmentosOpcion(string id, Opcion opcion)
    {
        yield return $"{id}: {{";
        yield return $"texto: {Js(opcion.Texto)},";
        yield return $"resultado: {Js(opcion.Resultado)},";
        if (opcion.Efectos is not null)
            yield return "efectos: { " + string.Join(", ", opcion.Efectos.Select(e => $"{NombreJs(e.Key)}: {Numero(e.Value)}")) + " },";
        if (opcion.Requisito is not null)
            yield return $"requisito: {{ medidor: {Js(NombreJs(opcion.Requisito.Medidor))}, umbral: {Numero(opcion.Requisito.Umbral)} }},";
        if (opcion.Fallo is not null) yield return $"fallo: {Js(opcion.Fallo)},";
    }

    [TestMethod(DisplayName = "Paridad con js/levels.js: los 34 textos de la interfaz")]
    public void ParidadTextos()
    {
        var textos = TextosCs().ToList();
        var bloque = LevelsJs[LevelsJs.IndexOf("window.BaseLunar.textos", StringComparison.Ordinal)..];
        Assert.HasCount(LiteralJs().Matches(bloque).Count, textos, "mismo número de textos");
        foreach (var (nombre, valor) in textos) Contiene(bloque, Js(valor), nombre);
    }

    /// <summary>Todas las constantes de texto de Textos y sus clases anidadas.</summary>
    private static IEnumerable<(string Nombre, string Valor)> TextosCs() =>
        new[] { typeof(Textos) }.Concat(typeof(Textos).GetNestedTypes())
            .SelectMany(tipo => tipo.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (tipo.Name + "." + f.Name, (string)f.GetRawConstantValue()!)));

    [TestMethod(DisplayName = "Paridad con js/nasaData.js: la copia literal de DONKI es idéntica")]
    public void ParidadRespaldoDonki()
    {
        const string marca = "LLAMARADAS_X_MAYO_2024: ";
        var inicio = NasaDataJs.IndexOf(marca, StringComparison.Ordinal) + marca.Length;
        var fin = NasaDataJs.IndexOf("\n};", inicio, StringComparison.Ordinal);
        var web = JsonNode.Parse(NasaDataJs[inicio..fin].TrimEnd().TrimEnd(','));
        var escritorio = JsonNode.Parse(DatosDonki.LeerRespaldoLiteral());
        Assert.IsTrue(JsonNode.DeepEquals(web, escritorio), "el JSON embebido difiere del de js/nasaData.js");
        Assert.HasCount(5, escritorio!.AsArray());
    }

    [GeneratedRegex(@"\n    id: \d+,\n")]
    private static partial Regex NivelJs();

    [GeneratedRegex(@"\{ nombre: ")]
    private static partial Regex FuenteJs();

    [GeneratedRegex(@"^\s+\w+: '", RegexOptions.Multiline)]
    private static partial Regex LiteralJs();
}
