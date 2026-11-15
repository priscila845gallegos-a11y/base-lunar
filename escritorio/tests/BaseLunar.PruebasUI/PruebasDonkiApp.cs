using System.Globalization;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.PruebasUI;

/// <summary>
/// El dato en vivo en la pantalla (F3.5): etiqueta honesta, líneas del dato, mini escala y cuándo se aplica.
/// Las reglas no cambian: la penalización X solo existe si la llamarada es de clase X.
/// </summary>
[TestClass]
public sealed class PruebasDonkiApp
{
    private static readonly DateTimeOffset Consulta = new(2026, 11, 15, 10, 30, 0, TimeSpan.Zero);

    private static Tormenta EnVivo(string clase) =>
        new(clase, "2026-11-05T18:33Z", "NASA DONKI", "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/49391/-1", true, clase.StartsWith('X') ? 1 : 0, Consulta);

    private static string Hora(DateTimeOffset momento) => momento.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture);

    [TestMethod(DisplayName = "En vivo antes del Nivel 4: etiqueta EN VIVO; una M no es X, así que no hay aviso ni −5 de protección")]
    public void EnVivoAntesDelNivel4() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM();
        juego.ActualizarTormenta(EnVivo("M3.5"));
        Assert.AreEqual("M3.5", juego.Tormenta.Clase);
        foreach (var paso in Partida.HastaNivel4) Partida.Paso(juego, paso);
        Assert.AreEqual("📡 Dato EN VIVO: NASA DONKI", juego.Etiqueta);
        Assert.IsNull(juego.AvisoX);
        Assert.AreEqual(100, juego.Estado.Medidores.Proteccion, "Sin penalización X (con la X8.7 serían 95)");
        Assert.AreEqual(new Momento(TipoMomento.EntrarNivel, 4, Desde: 3), juego.UltimoMomento, "Sin destello de llamarada X");
    });

    [TestMethod(DisplayName = "Un nivel nunca cambia su dato a medias: si llega en el Nivel 4 o después, espera a la siguiente partida")]
    public void NoCambiaAMedias() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM();
        foreach (var paso in Partida.HastaNivel4) Partida.Paso(juego, paso);
        juego.ActualizarTormenta(EnVivo("M3.5"));
        Assert.AreEqual("X8.7", juego.Tormenta.Clase, "Sigue el respaldo durante esta partida");
        Assert.IsNotNull(juego.AvisoX);
        foreach (var paso in new[] { "B", "P", "A", "P" }) Partida.Paso(juego, paso);
        Assert.AreEqual(Fase.Victoria, juego.Fase);
        Assert.AreEqual("X8.7", juego.Tormenta.Clase);
        Partida.Paso(juego, "P");
        Assert.AreEqual("M3.5", juego.Tormenta.Clase, "Al jugar de nuevo ya se usa el dato en vivo");
    });

    [TestMethod(DisplayName = "Líneas honestas: la más fuerte del mes, si no es X lo dice, y la hora de la consulta")]
    public void LineasHonestas()
    {
        var nivel4 = Guion.Niveles[3];
        CollectionAssert.AreEqual(new[]
        {
            "Llamarada real: clase M3.5 del 5 de noviembre de 2026 (NASA DONKI).",
            "Es la más fuerte de los últimos 30 días. Las de clase X son todavía más fuertes.",
            $"Dato consultado hoy a las {Hora(Consulta)}.",
        }, Presentacion.LineasExtra(nivel4, EnVivo("M3.5"), Consulta).ToArray());
        var conX = Presentacion.LineasExtra(nivel4, EnVivo("X2.1"), Consulta);
        Assert.AreEqual("Es la más fuerte de los últimos 30 días.", conX[1], "Si es X, no hace falta la aclaración");
        var otroDia = Consulta.AddDays(1);
        var fecha = Consulta.ToLocalTime().ToString("d 'de' MMMM", CultureInfo.GetCultureInfo("es"));
        Assert.AreEqual($"Dato consultado el {fecha} a las {Hora(Consulta)}.", Presentacion.TextoConsulta(Consulta, otroDia));
        Assert.Contains("¡En solo 2 días", string.Join(" ", Presentacion.LineasExtra(nivel4, DatosDonki.RespaldoHistorico())),
            "Con el respaldo, la línea de la serie de 5 llamaradas X");
    }

    [TestMethod(DisplayName = "Mini escala B · C · M · X con la clase resaltada y «X es la más fuerte» (captura)")]
    public void MiniEscala() => HiloUI.Ejecutar(() =>
    {
        var juego = new JuegoVM();
        juego.ActualizarTormenta(EnVivo("M3.5"));
        foreach (var paso in Partida.HastaVictoria[..8]) Partida.Paso(juego, paso);
        Assert.AreEqual(Fase.Resultado, juego.Fase);
        CollectionAssert.AreEqual(new[] { new LetraEscala("B", false), new LetraEscala("C", false), new LetraEscala("M", true), new LetraEscala("X", false) },
            juego.EscalaLlamarada.ToArray());
        Assert.AreEqual("Clases de llamarada, de menor a mayor: B, C, M y X. Esta es de clase M. X es la más fuerte.", juego.EscalaAccesible);
        var pantalla = Capturas.Montar(juego, 1280, 720);
        pantalla.Tarjeta.MostrarSeccion(Fase.Resultado);
        HiloUI.Vaciar();
        Capturas.Guardar(pantalla, "f35-nivel4-envivo");
        Assert.IsEmpty(Partida.Jugar(Partida.HastaResultado1).EscalaLlamarada, "Solo en el dato de la llamarada");
    });
}
