/*
 * Base Lunar · js/levels.js
 * Guion del juego: solo datos. Sección 4 del Prompt 0, con las correcciones T-01 a T-08
 * de docs/FUENTES.md. Ningún número mágico vive fuera de este archivo o de config.js.
 */
'use strict';

window.BaseLunar = window.BaseLunar || {};

/** Los 3 medidores, en el orden en que se muestran. */
window.BaseLunar.medidores = Object.freeze([
  Object.freeze({ clave: 'proteccion', nombre: 'Protección', icono: '🛡️', recurso: 'protección' }),
  Object.freeze({ clave: 'soporte', nombre: 'Soporte Vital', icono: '💧', recurso: 'agua y oxígeno' }),
  Object.freeze({ clave: 'bateria', nombre: 'Batería', icono: '⚡', recurso: 'energía' }),
]);

/** Los 5 niveles. Modelo de datos: sección 6 del Prompt 0 (fuentes en lista: DECISIONES.md, D-25). */
window.BaseLunar.niveles = Object.freeze([
  {
    id: 1,
    titulo: 'La Noche Lunar',
    etiqueta: '📡 Dato NASA: Ciencia Lunar',
    avatar: '🌑',
    historia: '¡Empezó la noche lunar! Dura casi 15 días y los paneles solares no reciben luz. La base se está enfriando rápido.',
    tema: 'normal',
    tipo: 'decision',
    opciones: {
      A: {
        texto: 'Apagar recicladores de agua',
        efectos: { bateria: 20, soporte: -35 },
        resultado: 'Ahorraste energía para los calentadores, pero queda menos agua limpia.',
      },
      B: {
        texto: 'Mantener soporte al máximo',
        efectos: { soporte: 15, bateria: -40 },
        resultado: 'El agua y el aire están perfectos, pero los calentadores gastan mucha batería.',
      },
    },
    dato: {
      texto: 'La noche en la Luna dura casi 15 días de la Tierra. El suelo se enfría a más de 130 grados bajo cero. ¡Por eso la base necesita calefacción!',
      fuentes: [
        { nombre: 'NASA, Moon Facts', url: 'https://science.nasa.gov/moon/facts/' },
        { nombre: 'NASA, LuSEE-Night', url: 'https://www.nasa.gov/solar-system/nasa-department-of-energy-join-forces-on-innovative-lunar-experiment/' },
      ],
      verificado: true,
    },
  },
  {
    id: 2,
    titulo: 'El Huerto Espacial',
    etiqueta: '📡 Dato NASA/CSA: Nutrición espacial',
    avatar: '🌱',
    historia: 'Las plantas del invernadero necesitan agua purificada para crecer. Así dan vitaminas frescas a los astronautas.',
    tema: 'normal',
    tipo: 'decision',
    opciones: {
      A: {
        texto: 'Regar los cultivos',
        efectos: { proteccion: 25, soporte: -35 },
        resultado: '¡Vitaminas frescas! La tripulación está más fuerte.',
      },
      B: {
        texto: 'Guardar agua para oxígeno',
        efectos: { soporte: 25, proteccion: -25 },
        resultado: 'En la Estación Espacial Internacional, el oxígeno se fabrica separando el agua. ¡En la base también!',
      },
    },
    dato: {
      texto: 'El 10 de agosto de 2015, la tripulación de la Estación Espacial comió su lechuga. La habían cultivado allí, en el experimento Veggie de la NASA. ¡Fue la primera vez que la NASA lo permitió!',
      extra: ['La Agencia Espacial Canadiense también estudia plantas para el espacio. Además de alimentar, pueden dar oxígeno y ayudar a reciclar el agua.'],
      fuentes: [
        { nombre: 'NASA, blog de la Estación (10-08-2015)', url: 'https://blogs.nasa.gov/spacestation/2015/08/10/cosmonauts-complete-russian-spacewalk/' },
        { nombre: 'CSA, Growing healthy food in space', url: 'https://www.asc-csa.gc.ca/eng/sciences/food-production/growing-healthy-food-in-space-and-remote-areas.asp' },
      ],
      verificado: true,
    },
  },
  {
    id: 3,
    titulo: 'Rayos Invisibles',
    etiqueta: '📡 Dato NASA: OSDR / RadLab',
    avatar: '☢️',
    historia: 'El dosímetro (medidor de radiación) pita fuerte. La Luna no tiene aire ni campo magnético que nos protejan de los rayos cósmicos.',
    tema: 'normal',
    tipo: 'decision',
    opciones: {
      A: {
        texto: 'Rodear los dormitorios con agua',
        efectos: { proteccion: 30, bateria: -30 },
        resultado: 'Las bombas rodearon los dormitorios con agua. Gastaron energía, ¡pero ahora protegen mejor!',
      },
      B: {
        texto: 'Cubrir el módulo con regolito (polvo lunar)',
        efectos: { proteccion: 40, bateria: -45 },
        resultado: 'Los robots excavadores cubrieron el módulo con polvo lunar. ¡Trabajaron mucho y gastaron mucha energía!',
      },
    },
    dato: {
      texto: 'El agua ayuda a frenar la radiación espacial porque tiene mucho hidrógeno. ¡Y una capa gruesa de polvo lunar sobre la base también sirve de escudo!',
      fuentes: [
        { nombre: 'NASA, RadWorks (NTRS)', url: 'https://ntrs.nasa.gov/citations/20150001237' },
        { nombre: 'NASA, protección en la Luna (NTRS)', url: 'https://ntrs.nasa.gov/citations/20050215115' },
        { nombre: 'NASA OSDR / RadLab', url: 'https://www.nasa.gov/reference/osdr-environmental-data' },
      ],
      verificado: true,
    },
  },
  {
    id: 4,
    titulo: 'Tormenta Solar',
    etiqueta: '📡 Dato EN VIVO: NASA DONKI',
    etiquetaHistorico: '📡 Dato HISTÓRICO REAL: NASA DONKI',
    avatar: '☀️',
    historia: '¡ALERTA ROJA! El Sol lanzó una llamarada gigante hacia la Luna. La radiación está subiendo al límite peligroso.',
    tema: 'alerta',
    tipo: 'decision',
    /** Si la llamarada real es de clase X, este nivel resta protección al empezar (antes de la foto). */
    penalizacionX: true,
    /** Este nivel añade al dato la línea de la llamarada real (NASA DONKI). */
    datoTormenta: true,
    opciones: {
      A: {
        texto: 'Refugiarse rodeados de provisiones',
        efectos: { proteccion: 35, soporte: -30 },
        resultado: 'En el refugio estamos protegidos, pero el aire y el agua se gastan más rápido.',
      },
      B: {
        texto: 'Cargar las baterías antes del golpe',
        efectos: { bateria: 30, proteccion: -45 },
        resultado: '¡Baterías llenas! Pero la radiación desgastó parte de la protección.',
      },
    },
    dato: {
      texto: 'En la nave Orión, los astronautas pueden armar un refugio con sus bolsas de provisiones. ¡Pueden quedarse dentro hasta 24 horas!',
      fuentes: [
        { nombre: 'NASA, plan de protección de Orión', url: 'https://www.nasa.gov/missions/artemis/orion/scientists-and-engineers-evaluate-orion-radiation-protection-plan/' },
      ],
      verificado: true,
    },
  },
  {
    id: 5,
    titulo: 'El Relevo Final',
    etiqueta: '📡 Dato NASA: Comunicaciones',
    avatar: '🚀',
    historia: '¡Llega el módulo de aterrizaje con el relevo! Pero la tormenta solar dejó estática en la radio y no ve dónde alunizar.',
    tema: 'normal',
    tipo: 'condicion',
    opciones: {
      A: {
        texto: 'Encender la baliza de la base a máxima potencia',
        requisito: { medidor: 'bateria', umbral: 35 },
        resultado: '¡La baliza brilló fuerte y el módulo alunizó junto a la base!',
        fallo: 'La baliza necesitaba más energía.',
      },
      B: {
        texto: 'Guiado automático por computadora',
        requisito: { medidor: 'proteccion', umbral: 40 },
        resultado: '¡Los sensores protegidos guiaron al módulo hasta la base!',
        fallo: 'Los sensores del guiado necesitaban más protección.',
      },
    },
    dato: {
      texto: 'Las llamaradas solares pueden interrumpir las comunicaciones por radio. ¡Una señal tarda más de 1 segundo en ir de la Luna a la Tierra!',
      fuentes: [
        { nombre: 'NASA, Strong Solar Flare', url: 'https://www.nasa.gov/image-article/strong-solar-flare/' },
        { nombre: 'NASA, ¿Qué es un día luz?', url: 'https://science.nasa.gov/mission/voyager/voyager-1/voyager-1-what-is-a-light-day/' },
      ],
      verificado: true,
    },
  },
]);

/** Textos de la interfaz que cambian durante la partida. Plantillas con {marcadores}. */
window.BaseLunar.textos = Object.freeze({
  hud: {
    inicio: 'Antes de empezar',
    nivel: 'Nivel {n} de {total}',
    victoria: '¡Misión cumplida!',
    respaldos: 'Protocolos de Respaldo: quedan {n} de {total}',
  },
  botones: {
    INICIO: 'Comenzar misión',
    RESULTADO: 'Continuar',
    RESPALDO: 'Reintentar',
    VICTORIA: 'Jugar de nuevo',
    MISION_ABORTADA: 'Jugar de nuevo',
  },
  opciones: {
    requisito: 'Necesita más de {umbral} % {icono}',
    requisitoAccesible: 'Necesita más de {umbral} % de {nombre}.',
    gana: 'gana {n} de {nombre}',
    cuesta: 'cuesta {n} de {nombre}',
  },
  tarjeta: {
    prefijoNivel: 'Nivel {n}: ',
    fuente: 'Fuente:',
    fuentes: 'Fuentes:',
    pestanaNueva: ' (se abre en otra pestaña)',
  },
  /** Mensajes de Respaldo (3.6, con la corrección T-05). Prioridad en config.js. */
  respaldo: {
    soporte: '¡Protocolo de Respaldo activado! Se acabaron el agua limpia y el oxígeno. La tripulación necesita soporte vital para estar sana. ¡Reintentemos este nivel!',
    proteccion: '¡Protocolo de Respaldo activado! La protección se agotó y la radiación entró a la base. La tripulación corrió al refugio. ¡Reintentemos este nivel!',
    bateria: '¡Protocolo de Respaldo activado! Nos quedamos sin energía para los calentadores. Sin electricidad, la base se enfría muchísimo. ¡Reintentemos este nivel!',
    condicion: '¡Protocolo de Respaldo activado! {fallo} ¡Reintentemos este nivel!',
    pista: 'Pista: si un medidor llega a 0, se queda vacío. ¡Mira los números antes de elegir!',
    pistaCondicion: 'Pista: compara lo que necesita cada botón con tus medidores.',
  },
  ayuda: '¡Ayuda de Control de Misión! Te enviamos {recurso} extra para que puedas seguir.',
  penalizacionX: 'Esta llamarada es de clase X, la más fuerte. La protección bajó {n} puntos.',
  alMaximo: '¡Al máximo!',
  llamarada: {
    linea: 'Llamarada real: clase {clase} del {fecha} (NASA DONKI).',
    serieX: '¡En solo 2 días, el Sol lanzó {n} llamaradas de clase X!',
    fuente: 'NASA DONKI, registro de la llamarada',
  },
  anuncios: {
    sube: '{nombre} sube a {valor}.',
    baja: '{nombre} baja a {valor}.',
    alMaximo: '{nombre} está al máximo.',
    dato: 'Dato real: {texto}',
  },
  certificado: {
    respaldos: 'Respaldos sin usar: {n} de {total}',
  },
});
