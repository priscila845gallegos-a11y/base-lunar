namespace BaseLunar.Nucleo;

/// <summary>
/// Guion del juego: solo datos. Traslado fiel de js/levels.js, con los mismos textos y cifras.
/// PruebasParidad lo comprueba leyendo el archivo original, que no se modifica.
/// </summary>
public static class Guion
{
    /// <summary>Crea los efectos de una opción en el orden en que se escriben (el mismo que en levels.js).</summary>
    public static IReadOnlyDictionary<Medidor, int> Efectos(params (Medidor Medidor, int Valor)[] efectos)
    {
        var resultado = new OrderedDictionary<Medidor, int>();
        foreach (var (medidor, valor) in efectos) resultado.Add(medidor, valor);
        return resultado;
    }

    /// <summary>Crea las opciones A y B, en ese orden.</summary>
    public static IReadOnlyDictionary<string, Opcion> Opciones(Opcion a, Opcion b) =>
        new OrderedDictionary<string, Opcion> { ["A"] = a, ["B"] = b };

    /// <summary>Los 3 medidores, en el orden en que se muestran.</summary>
    public static readonly IReadOnlyList<InfoMedidor> InfoMedidores =
    [
        new(Medidor.Proteccion, "Protección", "🛡️", "protección"),
        new(Medidor.Soporte, "Soporte Vital", "💧", "agua y oxígeno"),
        new(Medidor.Bateria, "Batería", "⚡", "energía"),
    ];

    /// <summary>Los 5 niveles (sección 4 del Prompt 0, con las correcciones T-01 a T-08).</summary>
    public static readonly IReadOnlyList<Nivel> Niveles =
    [
        new(
            Id: 1,
            Titulo: "La Noche Lunar",
            Etiqueta: "📡 Dato NASA: Ciencia Lunar",
            Avatar: "🌑",
            Historia: "¡Empezó la noche lunar! Dura casi 15 días y los paneles solares no reciben luz. La base se está enfriando rápido.",
            Tema: Tema.Normal,
            Tipo: TipoNivel.Decision,
            Opciones: Opciones(
                new Opcion(
                    Texto: "Apagar recicladores de agua",
                    Resultado: "Ahorraste energía para los calentadores, pero queda menos agua limpia.",
                    Efectos: Efectos((Medidor.Bateria, 20), (Medidor.Soporte, -35))),
                new Opcion(
                    Texto: "Mantener soporte al máximo",
                    Resultado: "El agua y el aire están perfectos, pero los calentadores gastan mucha batería.",
                    Efectos: Efectos((Medidor.Soporte, 15), (Medidor.Bateria, -40)))),
            Dato: new Dato(
                Texto: "La noche en la Luna dura casi 15 días de la Tierra. El suelo se enfría a más de 130 grados bajo cero. ¡Por eso la base necesita calefacción!",
                Fuentes:
                [
                    new("NASA, Moon Facts", "https://science.nasa.gov/moon/facts/"),
                    new("NASA, LuSEE-Night", "https://www.nasa.gov/solar-system/nasa-department-of-energy-join-forces-on-innovative-lunar-experiment/"),
                ],
                Verificado: true)),
        new(
            Id: 2,
            Titulo: "El Huerto Espacial",
            Etiqueta: "📡 Dato NASA/CSA: Nutrición espacial",
            Avatar: "🌱",
            Historia: "Las plantas del invernadero necesitan agua purificada para crecer. Así dan vitaminas frescas a los astronautas.",
            Tema: Tema.Normal,
            Tipo: TipoNivel.Decision,
            Opciones: Opciones(
                new Opcion(
                    Texto: "Regar los cultivos",
                    Resultado: "¡Vitaminas frescas! La tripulación está más fuerte.",
                    Efectos: Efectos((Medidor.Proteccion, 25), (Medidor.Soporte, -35))),
                new Opcion(
                    Texto: "Guardar agua para oxígeno",
                    Resultado: "En la Estación Espacial Internacional, el oxígeno se fabrica separando el agua. ¡En la base también!",
                    Efectos: Efectos((Medidor.Soporte, 25), (Medidor.Proteccion, -25)))),
            Dato: new Dato(
                Texto: "El 10 de agosto de 2015, la tripulación de la Estación Espacial comió su lechuga. La habían cultivado allí, en el experimento Veggie de la NASA. ¡Fue la primera vez que la NASA lo permitió!",
                Fuentes:
                [
                    new("NASA, blog de la Estación (10-08-2015)", "https://blogs.nasa.gov/spacestation/2015/08/10/cosmonauts-complete-russian-spacewalk/"),
                    new("CSA, Growing healthy food in space", "https://www.asc-csa.gc.ca/eng/sciences/food-production/growing-healthy-food-in-space-and-remote-areas.asp"),
                ],
                Verificado: true,
                Extra: ["La Agencia Espacial Canadiense también estudia plantas para el espacio. Además de alimentar, pueden dar oxígeno y ayudar a reciclar el agua."])),
        new(
            Id: 3,
            Titulo: "Rayos Invisibles",
            Etiqueta: "📡 Dato NASA: OSDR / RadLab",
            Avatar: "☢️",
            Historia: "El dosímetro (medidor de radiación) pita fuerte. La Luna no tiene aire ni campo magnético que nos protejan de los rayos cósmicos.",
            Tema: Tema.Normal,
            Tipo: TipoNivel.Decision,
            Opciones: Opciones(
                new Opcion(
                    Texto: "Rodear los dormitorios con agua",
                    Resultado: "Las bombas rodearon los dormitorios con agua. Gastaron energía, ¡pero ahora protegen mejor!",
                    Efectos: Efectos((Medidor.Proteccion, 30), (Medidor.Bateria, -30))),
                new Opcion(
                    Texto: "Cubrir el módulo con regolito (polvo lunar)",
                    Resultado: "Los robots excavadores cubrieron el módulo con polvo lunar. ¡Trabajaron mucho y gastaron mucha energía!",
                    Efectos: Efectos((Medidor.Proteccion, 40), (Medidor.Bateria, -45)))),
            Dato: new Dato(
                Texto: "El agua ayuda a frenar la radiación espacial porque tiene mucho hidrógeno. ¡Y una capa gruesa de polvo lunar sobre la base también sirve de escudo!",
                Fuentes:
                [
                    new("NASA, RadWorks (NTRS)", "https://ntrs.nasa.gov/citations/20150001237"),
                    new("NASA, protección en la Luna (NTRS)", "https://ntrs.nasa.gov/citations/20050215115"),
                    new("NASA OSDR / RadLab", "https://www.nasa.gov/reference/osdr-environmental-data"),
                ],
                Verificado: true)),
        new(
            Id: 4,
            Titulo: "Tormenta Solar",
            Etiqueta: "📡 Dato EN VIVO: NASA DONKI",
            Avatar: "☀️",
            Historia: "¡ALERTA ROJA! El Sol lanzó una llamarada gigante hacia la Luna. La radiación está subiendo al límite peligroso.",
            Tema: Tema.Alerta,
            Tipo: TipoNivel.Decision,
            Opciones: Opciones(
                new Opcion(
                    Texto: "Refugiarse rodeados de provisiones",
                    Resultado: "En el refugio estamos protegidos, pero el aire y el agua se gastan más rápido.",
                    Efectos: Efectos((Medidor.Proteccion, 35), (Medidor.Soporte, -30))),
                new Opcion(
                    Texto: "Cargar las baterías antes del golpe",
                    Resultado: "¡Baterías llenas! Pero la radiación desgastó parte de la protección.",
                    Efectos: Efectos((Medidor.Bateria, 30), (Medidor.Proteccion, -45)))),
            Dato: new Dato(
                Texto: "En la nave Orión, los astronautas pueden armar un refugio con sus bolsas de provisiones. ¡Pueden quedarse dentro hasta 24 horas!",
                Fuentes:
                [
                    new("NASA, plan de protección de Orión", "https://www.nasa.gov/missions/artemis/orion/scientists-and-engineers-evaluate-orion-radiation-protection-plan/"),
                ],
                Verificado: true),
            EtiquetaHistorico: "📡 Dato HISTÓRICO REAL: NASA DONKI",
            AplicaPenalizacionX: true,
            DatoTormenta: true),
        new(
            Id: 5,
            Titulo: "El Relevo Final",
            Etiqueta: "📡 Dato NASA: Comunicaciones",
            Avatar: "🚀",
            Historia: "¡Llega el módulo de aterrizaje con el relevo! Pero la tormenta solar dejó estática en la radio y no ve dónde alunizar.",
            Tema: Tema.Normal,
            Tipo: TipoNivel.Condicion,
            Opciones: Opciones(
                new Opcion(
                    Texto: "Encender la baliza de la base a máxima potencia",
                    Resultado: "¡La baliza brilló fuerte y el módulo alunizó junto a la base!",
                    Requisito: new Requisito(Medidor.Bateria, 35),
                    Fallo: "La baliza necesitaba más energía."),
                new Opcion(
                    Texto: "Guiado automático por computadora",
                    Resultado: "¡Los sensores protegidos guiaron al módulo hasta la base!",
                    Requisito: new Requisito(Medidor.Proteccion, 40),
                    Fallo: "Los sensores del guiado necesitaban más protección.")),
            Dato: new Dato(
                Texto: "Las llamaradas solares pueden interrumpir las comunicaciones por radio. ¡Una señal tarda más de 1 segundo en ir de la Luna a la Tierra!",
                Fuentes:
                [
                    new("NASA, Strong Solar Flare", "https://www.nasa.gov/image-article/strong-solar-flare/"),
                    new("NASA, ¿Qué es un día luz?", "https://science.nasa.gov/mission/voyager/voyager-1/voyager-1-what-is-a-light-day/"),
                ],
                Verificado: true)),
    ];
}
