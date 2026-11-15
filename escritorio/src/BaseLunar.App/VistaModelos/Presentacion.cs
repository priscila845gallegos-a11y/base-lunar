using System.Globalization;
using static BaseLunar.Nucleo.Configuracion;

namespace BaseLunar.App.VistaModelos;

/// <summary>
/// Traduce el estado del motor a textos de pantalla. Son las mismas funciones de js/ui.js (porcentaje,
/// conSigno, lineasEfecto, descripcion, textoAyuda, lineasExtra, fuentesDe, textoCambio…), sin WPF:
/// se prueban como funciones puras.
/// </summary>
public static class Presentacion
{
    private static readonly CultureInfo Espanol = CultureInfo.GetCultureInfo("es");
    private const string FormatoFecha = "d 'de' MMMM 'de' yyyy";

    /// <summary>«80 %», con espacio de no separación como en ui.js.</summary>
    public static string Porcentaje(int valor) => valor.ToString(CultureInfo.InvariantCulture) + " %";

    /// <summary>«+20» o «−35» (con el signo menos tipográfico).</summary>
    public static string ConSigno(int valor) => (valor > 0 ? "+" : "−") + Math.Abs(valor).ToString(CultureInfo.InvariantCulture);

    public static InfoMedidor Info(Medidor medidor) => Guion.InfoMedidores.First(i => i.Clave == medidor);

    /// <summary>De mayor a menor, conservando el orden del guion en los empates (como Array.sort).</summary>
    public static IEnumerable<KeyValuePair<Medidor, int>> EfectosOrdenados(IReadOnlyDictionary<Medidor, int> efectos) =>
        efectos.OrderByDescending(e => e.Value);

    /// <summary>«+20 ⚡»: número con signo y el ícono del medidor.</summary>
    public static string ConIcono(int valor, Medidor medidor) => ConSigno(valor) + " " + Info(medidor).Icono;

    private static string Mayuscula(string texto) =>
        texto.Length == 0 ? texto : char.ToUpper(texto[0], Espanol) + texto[1..];

    public static Nivel? NivelDe(Estado estado) =>
        estado.Nivel >= 1 && estado.Nivel <= Guion.Niveles.Count ? Guion.Niveles[estado.Nivel - 1] : null;

    public static string TextoProgreso(Estado estado) => estado.Fase switch
    {
        Fase.Inicio => Textos.Hud.Inicio,
        Fase.Victoria => Textos.Hud.Victoria,
        _ => Textos.Rellenar(Textos.Hud.Nivel, ("n", estado.Nivel), ("total", Guion.Niveles.Count)),
    };

    public static string TextoRespaldos(int respaldos) =>
        Textos.Rellenar(Textos.Hud.Respaldos, ("n", respaldos), ("total", Reglas.RespaldosIniciales));

    public static string TextoBoton(Fase fase) => fase switch
    {
        Fase.Inicio => Textos.Botones.Inicio,
        Fase.Resultado => Textos.Botones.Resultado,
        Fase.Respaldo => Textos.Botones.Respaldo,
        Fase.Victoria => Textos.Botones.Victoria,
        Fase.MisionAbortada => Textos.Botones.MisionAbortada,
        _ => string.Empty,
    };

    /// <summary>Estación superada en el mapa (renderProgreso de ui.js).</summary>
    public static bool EstacionHecha(Estado estado, int numero) =>
        estado.Fase == Fase.Victoria || numero < estado.Nivel || (numero == estado.Nivel && estado.Fase == Fase.Resultado);

    public static bool EstacionActual(Estado estado, int numero) => !EstacionHecha(estado, numero) && numero == estado.Nivel;

    /// <summary>Etiqueta 📡: la de dato histórico si el dato de la tormenta no es en vivo.</summary>
    public static string EtiquetaNivel(Nivel nivel, Tormenta tormenta) =>
        nivel.EtiquetaHistorico is { } historico && !tormenta.EnVivo ? historico : nivel.Etiqueta;

    public static string TextoAvisoX() => Textos.Rellenar(Textos.PenalizacionX, ("n", Reglas.PenalizacionX));

    public static string TextoAyuda(IReadOnlyDictionary<Medidor, int> ajustes) =>
        Textos.Rellenar(Textos.Ayuda, ("recurso", string.Join(" y ", ajustes.Keys.Select(k => Info(k).Recurso))));

    /// <summary>Mensaje del medidor que falló (💧 → 🛡️ → ⚡) o el del requisito del Nivel 5, con su pista.</summary>
    public static (string Mensaje, string Pista) TextosRespaldo(Estado estado, Nivel nivel)
    {
        if (estado.Fallidos.Count > 0) return (Textos.Respaldo.De(estado.Fallidos[0]), Textos.Respaldo.Pista);
        var opcion = nivel.Opciones[estado.UltimaOpcion!];
        return (Textos.Rellenar(Textos.Respaldo.Condicion, ("fallo", opcion.Fallo!)), Textos.Respaldo.PistaCondicion);
    }

    /// <summary>Lo que se ve en el botón: «+20 ⚡», «−35 💧» o el requisito del Nivel 5.</summary>
    public static IReadOnlyList<string> LineasEfecto(Opcion opcion)
    {
        if (opcion.Requisito is { } requisito)
            return [Textos.Rellenar(Textos.Opciones.Requisito, ("umbral", requisito.Umbral), ("icono", Info(requisito.Medidor).Icono))];
        return EfectosOrdenados(opcion.Efectos!).Select(e => ConIcono(e.Value, e.Key)).ToArray();
    }

    /// <summary>Lo que oye el lector de pantalla: «. Gana 30 de Protección y cuesta 30 de Batería.».</summary>
    public static string Descripcion(Opcion opcion)
    {
        if (opcion.Requisito is { } requisito)
            return ". " + Textos.Rellenar(Textos.Opciones.RequisitoAccesible, ("umbral", requisito.Umbral), ("nombre", Info(requisito.Medidor).Nombre));
        var partes = EfectosOrdenados(opcion.Efectos!).Select(e =>
            Textos.Rellenar(e.Value > 0 ? Textos.Opciones.Gana : Textos.Opciones.Cuesta, ("n", Math.Abs(e.Value)), ("nombre", Info(e.Key).Nombre)));
        return ". " + Mayuscula(string.Join(" y ", partes)) + ".";
    }

    /// <summary>Nombre accesible del botón: «Opción A: texto. Gana … y cuesta ….».</summary>
    public static string NombreOpcion(string id, Opcion opcion) =>
        Textos.Rellenar(TextosApp.PrefijoOpcion, ("id", id)) + opcion.Texto + Descripcion(opcion);

    /// <summary>Líneas extra del 💡: la de CSA y, en el Nivel 4, la llamarada real (sección 7).</summary>
    public static IReadOnlyList<string> LineasExtra(Nivel nivel, Tormenta tormenta)
    {
        var lineas = new List<string>(nivel.Dato.Extra ?? []);
        if (!nivel.DatoTormenta) return lineas;
        lineas.Add(Textos.Rellenar(Textos.Llamarada.Linea, ("clase", tormenta.Clase), ("fecha", FechaUtc(tormenta.Fecha))));
        if (!tormenta.EnVivo) lineas.Add(Textos.Rellenar(Textos.Llamarada.SerieX, ("n", tormenta.LlamaradasX)));
        return lineas;
    }

    public static IReadOnlyList<Fuente> FuentesDe(Nivel nivel, Tormenta tormenta) =>
        nivel.DatoTormenta ? [.. nivel.Dato.Fuentes, new Fuente(Textos.Llamarada.Fuente, tormenta.Url)] : nivel.Dato.Fuentes;

    public static string EtiquetaFuentes(int cantidad) => cantidad > 1 ? Textos.Tarjeta.Fuentes : Textos.Tarjeta.Fuente;

    /// <summary>Fecha de la llamarada en UTC: «14 de mayo de 2024».</summary>
    public static string FechaUtc(string iso) =>
        DateTimeOffset.Parse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime.ToString(FormatoFecha, Espanol);

    /// <summary>Fecha del certificado, en la hora local.</summary>
    public static string FechaLocal(DateTime fecha) => fecha.ToString(FormatoFecha, Espanol);

    /// <summary>«Batería sube a 100 %.», «Protección está al máximo.»…</summary>
    public static string TextoCambio(Estado estado, Medidor medidor, int delta, IReadOnlyList<Evento> eventos)
    {
        var nombre = Info(medidor).Nombre;
        if (eventos.OfType<MedidorAlMaximo>().Any(e => e.Medidor == medidor))
            return Textos.Rellenar(Textos.Anuncios.AlMaximo, ("nombre", nombre));
        var forma = delta > 0 ? Textos.Anuncios.Sube : Textos.Anuncios.Baja;
        return Textos.Rellenar(forma, ("nombre", nombre), ("valor", Porcentaje(estado.Medidores[medidor])));
    }

    /// <summary>Lo que se anuncia al superar un nivel: resultado, cambios de los medidores y dato.</summary>
    public static IReadOnlyList<string> MensajesResultado(Estado estado, Nivel nivel, IReadOnlyList<Evento> eventos)
    {
        var efectos = eventos.OfType<EfectosAplicados>().FirstOrDefault();
        var cambios = efectos is null
            ? Enumerable.Empty<string>()
            : EfectosOrdenados(efectos.Efectos).Select(e => TextoCambio(estado, e.Key, e.Value, eventos));
        return [nivel.Opciones[estado.UltimaOpcion!].Resultado, .. cambios, Textos.Rellenar(Textos.Anuncios.Dato, ("texto", nivel.Dato.Texto))];
    }

    /// <summary>«🛡️ Protección: 80 %» para el certificado.</summary>
    public static string LineaMedidor(InfoMedidor info, Medidores medidores) =>
        $"{info.Icono} {info.Nombre}: {Porcentaje(medidores[info.Clave])}";

    public static string TextoRespaldosCertificado(int respaldos) =>
        Textos.Rellenar(Textos.Certificado.Respaldos, ("n", respaldos), ("total", Reglas.RespaldosIniciales));
}
