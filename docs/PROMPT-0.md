# Base Lunar: El Escudo Cósmico — Prompt 0

> Copia del Prompt 0 recibido el 14-11-2026. Es la especificación de referencia del proyecto.
> Solo se restauró el formato Markdown (tablas, listas y bloques de código); el texto no se modificó.
> Las decisiones posteriores están en [DECISIONES.md](../DECISIONES.md) y la verificación de fuentes en [FUENTES.md](FUENTES.md).

## 0. Rol, prioridades y modo de trabajo

Actúa como líder técnico frontend senior con experiencia en juegos educativos para niños y en accesibilidad web. Vas a construir el prototipo funcional de este documento en un presupuesto de 8 horas, siguiendo la metodología MIDE-IA 2.0 (Definición → Diseño → Construcción → Validación → Operación).

Prioridades cuando haya un conflicto (en este orden):

1. Seguridad y bienestar infantil: lenguaje amable, nada de miedo ni culpa, ningún dato personal sale del navegador.
2. Honestidad científica: todo dato tiene su fuente, y «EN VIVO» se usa solo cuando de verdad lo es.
3. Jugabilidad sin bloqueos: nunca puede existir un estado del que el jugador no pueda salir.
4. Accesibilidad y responsividad.
5. Estética y extras.

Modo de trabajo:

- Trabaja por fases (sección 9). Cada fase termina con su Puerta de calidad: la lista de comprobación y las evidencias (qué probaste y cómo). No pases a la siguiente fase sin mi «Aprobado».
- Haz al menos un commit por fase, con mensajes en formato Conventional Commits.
- Si surge una decisión que este documento no resuelve, elige la opción más simple, anótala en DECISIONES.md (fecha · decisión · motivo) y sigue.
- No inventes datos científicos. Si no puedes verificar una cifra, márcala como PENDIENTE DE VERIFICAR y avísame.
- Si una fase se pasa de su tiempo, propón un recorte según las prioridades de la sección 2.

## 1. Contexto y objetivo · Ciclo I (Definición)

| Aspecto | Definición |
|---|---|
| Producto | Juego web educativo de decisiones: el jugador comanda una base en la superficie de la Luna durante 5 situaciones reales del espacio. |
| Objetivo pedagógico | Que el niño entienda que en el espacio todo recurso tiene un costo (energía, agua y oxígeno, protección contra la radiación) y aprenda 5 datos reales de NASA y CSA. |
| Público | Niños y niñas de 7 a 12 años (lectores en desarrollo) y jueces que lo abren desde un enlace, en móvil o computadora, sin instalar nada y quizá con internet inestable. |
| Duración de una partida | 3 a 5 minutos. |
| Narrativa | Base Lunar: la Propuesta 2 adaptada a la realidad de 2026. El 24 de marzo de 2026, la NASA pausó la estación Gateway y priorizó una base en la superficie lunar: unos 20 000 millones de dólares en 7 años, con el primer intento de alunizaje en Artemis IV (2028). La pantalla de Inicio lo dice en lenguaje infantil: «La NASA planea construir una base en la Luna. ¡En este juego tú ya la comandas!». No llamarla «Base Artemis» ni usar otro nombre de un programa oficial, porque parecería un producto de la NASA. |
| Presupuesto | 8 horas para una versión completa, responsiva, robusta y desplegada. |

## 2. Alcance y prioridades (MoSCoW)

**Must** (sin esto no se entrega):

- Pantalla única de 3 bloques, responsiva de 320 px a 1440 px.
- 5 niveles con 3 medidores, 3 Protocolos de Respaldo, reintento y Nivel 5 por condición.
- Pantallas de Inicio (briefing), Misión Abortada y Victoria con certificado.
- Nivel 4 con NASA DONKI más un respaldo histórico, en 3 s o menos y con etiqueta honesta.
- El juego funciona completo sin internet (salvo el dato en vivo).
- Despliegue público en GitHub Pages o Netlify.

**Should:**

- El 💡 Dato real aparece como recompensa después de cada decisión, con su fuente.
- Accesibilidad: teclado, lector de pantalla, contraste AA y movimiento reducido.
- Pruebas del motor y simulador de balance (tests/tests.html).
- Botón 🔊 «Escuchar»: lee la historia en voz alta con la API de voz del navegador, pensado para los lectores de 7 y 8 años. Se oculta si el navegador no tiene voz en español.
- Parámetros de demostración para los jueces: ?nivel=4 (saltar a un nivel), ?offline=1 (forzar el respaldo histórico) y ?debug=1 (ver el estado interno).
- Mini escala de llamaradas A · B · C · M · X en el Nivel 4.

**Could** (solo si sobra tiempo):

- Efectos de sonido generados por código (sin archivos), con botón de silencio.
- Estrellas en el certificado según los respaldos que queden, y botón «Imprimir certificado» con estilos de impresión.
- Recordar el silencio y el récord en localStorage, siempre dentro de try/catch.

**Won't** (fuera de alcance): backend, cuentas, ranking en línea, analítica o cookies, varios idiomas, frameworks o paso de compilación.

## 3. Reglas del juego (especificación definitiva)

### 3.1 Medidores

| Medidor | Icono | Qué representa (explicado para niños) |
|---|---|---|
| Protección (antes «Blindaje») | 🛡️ | El blindaje de la base y la salud de la tripulación |
| Soporte Vital | 💧 | Agua y oxígeno |
| Batería | ⚡ | Energía eléctrica |

- La partida empieza con los 3 medidores al 80 %, y los valores pasan de un nivel al siguiente. Si se reiniciaran en cada nivel, ninguna opción podría bajar un medidor a 0 y las vidas no servirían de nada.
- El rango va de 0 a 100. Una ganancia que pase de 100 se recorta, y se muestra «¡Al máximo!».
- Estados visuales: normal (30 % o más) y bajo (menos de 30 %: color acento, ⚠ y un pulso suave).

### 3.2 Decisiones

- Los niveles 1 a 4 ofrecen 2 opciones y ninguna es gratis: cada botón muestra su ganancia y su costo con icono y número (por ejemplo «+20 ⚡ · −35 💧»).
- Al elegir: los botones se bloquean (así un doble clic no aplica dos decisiones), las barras se animan, aparece el resultado con el 💡 Dato real y un botón «Continuar ▶».

### 3.3 Protocolos de Respaldo (3 vidas) · regla corregida

- Icono: no usar 🛡️, porque se confunde con el medidor de Protección. Usar un salvavidas: el ícono life-buoy de Lucide copiado como SVG en línea (licencia ISC, citarlo en los créditos).
- Regla:
  - Al empezar cada nivel se guarda una foto del estado (los 3 medidores).
  - Si tras una decisión algún medidor queda en 0 % o menos: la tarjeta tiembla (shake, o un destello de borde si el sistema pide movimiento reducido), se pierde 1 respaldo y aparece el mensaje amable de ese medidor (3.6).
  - Reintento: se restaura la foto del inicio del nivel, y cada medidor que falló sube a al menos 30 % (el mayor entre su valor en la foto y 30).
  - Con 0 respaldos se pasa a Misión Abortada.

Por qué se corrige: con la regla original (solo el medidor caído vuelve a 30 % y se conservan los demás cambios) hay bloqueos garantizados. Ejemplo: N1-B → N2-A → N3-B deja Protección 100, Soporte 60 y Batería −5, que vuelve a 30. En el reintento del Nivel 3, las dos opciones cuestan 30 o más de batería, así que el niño pierde los 3 respaldos haga lo que haga. Una simulación de todas las rutas encontró 4 estados sin salida con la regla original y 0 con esta.

### 3.4 Nivel 5 (gana quien cumple la condición)

- La opción A gana si ⚡ > 35. La opción B gana si 🛡️ > 40. La comparación es estrictamente «mayor que».
- Cada botón muestra su requisito («Necesita más de 35 % ⚡»): el reto es leer los medidores.
- Si el niño elige una opción cuyo requisito no se cumple: pierde 1 respaldo, ve la explicación («La baliza necesitaba más energía») y reintenta con el mismo estado.

### 3.5 Antibloqueo (red de seguridad)

Al empezar o reintentar cualquier nivel, si ninguna opción es sobrevivible (o ganable, en el Nivel 5), se activa «Ayuda de Control de Misión»: el medidor más bajo sube lo justo para que alguna opción sea viable, y se muestra un mensaje. El simulador (sección 10) debe demostrar que con las cifras de la sección 4 esto nunca se activa; existe solo como defensa en profundidad.

### 3.6 Mensajes de Respaldo (tono amable, nunca de miedo)

- ⚡ «¡Protocolo de Respaldo activado! Nos quedamos sin energía para los calentadores. En el espacio, sin electricidad todo se congela en minutos. ¡Reintentemos este nivel!»
- 💧 «¡Protocolo de Respaldo activado! Se acabaron el agua limpia y el oxígeno. La tripulación necesita soporte vital para estar sana. ¡Reintentemos este nivel!»
- 🛡️ «¡Protocolo de Respaldo activado! La protección se agotó y la radiación entró a la base. La tripulación corrió al refugio. ¡Reintentemos este nivel!»
- Si fallan dos medidores a la vez, se muestra un solo mensaje, en este orden de prioridad: 💧 → 🛡️ → ⚡. La restauración se aplica a todos los que fallaron.

### 3.7 Finales

- Misión Abortada · Cápsula de Regreso: «¡La tripulación volvió a salvo a la Tierra en la cápsula! Aprendiste mucho en esta misión.» Debajo, un botón grande «Jugar de nuevo», que reinicia todo el estado sin recargar la página. La consulta a DONKI no se repite si ya está en caché.
- Victoria · Certificado de Comandante Junior de la Base Lunar, con:
  - un nombre opcional que escribe el niño (solo se muestra en pantalla; nunca se guarda ni se envía);
  - la fecha, las barras finales y los respaldos que quedan (⭐);
  - los 5 datos aprendidos y las fuentes (NASA DONKI, NASA OSDR / RadLab, CSA);
  - los botones «Jugar de nuevo» e «Imprimir» (este último es Could).
- ⚠ No titularlo «de la NASA» ni usar logos de NASA o CSA. Daría a entender que es un certificado oficial, y las pautas de marca de la NASA no permiten usar su insignia sin autorización. Los datos se citan como fuente («Datos: NASA DONKI…»).

## 4. Guion por niveles (versión 2, revisada)

Reglas de texto para todo el guion: español neutro, frases de 15 palabras o menos, historia de 3 líneas como máximo en móvil (unos 150 caracteres), y cualquier palabra técnica explicada entre paréntesis.

### Nivel 1 · La Noche Lunar 🌑

- Etiqueta: 📡 Dato NASA: Ciencia Lunar
- Historia: ¡Empezó la noche lunar! Dura casi 15 días y los paneles solares no reciben luz. La base se está enfriando rápido.
- A (azul): Apagar recicladores de agua → +20 ⚡ · −35 💧
- B (naranja): Mantener soporte al máximo → +15 💧 · −40 ⚡
- 💡 Dato: «La noche en la Luna dura casi 15 días de la Tierra. El suelo se enfría a más de 130 grados bajo cero. ¡Por eso la base necesita calefacción!»
- Fuentes: NASA, Lunar Surface Technology (noches cerca de −130 °C) y Moon Facts (hasta unos −173 °C). Las cifras de la NASA varían según la zona; por eso el texto dice «más de 130 bajo cero».
- Cambio frente a la v1: el eclipse en órbita pasa a ser la noche lunar en la base, un evento real y mucho más largo; las cifras de las opciones no cambian. Además, el dato original atribuía los −130 °C a «la órbita lunar», pero la cifra de la NASA es la de la superficie de la Luna de noche. La etiqueta «APOD» se quita: APOD es la Foto Astronómica del Día, y solo tendría sentido si se mostrara una imagen de APOD.

### Nivel 2 · El Huerto Espacial 🌱

- Etiqueta: 📡 Dato NASA/CSA: Nutrición espacial
- Historia: Las plantas del invernadero de la base necesitan agua purificada para crecer y dar vitaminas frescas a los astronautas.
- A: Regar los cultivos → +25 🛡️ · −35 💧 (resultado: «¡Vitaminas frescas! La tripulación está más fuerte.»)
- B: Guardar agua para oxígeno → +25 💧 · −25 🛡️ (resultado: «En la Estación Espacial Internacional, el oxígeno se fabrica separando el agua. ¡En la base también!»)
- 💡 Dato: «El 10 de agosto de 2015, astronautas de la Estación Espacial Internacional comieron por primera vez lechuga cultivada en el espacio (experimento Veggie de la NASA).» Segunda línea, de CSA: «La Agencia Espacial Canadiense estudia plantas que, además de alimentar, ayudan a limpiar el agua y a producir oxígeno.»
- Fuentes: NASA, blog de Kennedy (11-08-2015) y CSA, Growing healthy food in space.
- Cambio: el dato ahora es concreto y verificable. Como 🛡️ pasa a ser «Protección (blindaje y salud)», que regar las plantas suba ese medidor tiene sentido para el niño.

### Nivel 3 · Rayos Invisibles ☢️

- Etiqueta: 📡 Dato NASA: OSDR / RadLab
- Historia: El dosímetro (medidor de radiación) pita fuerte. La Luna no tiene aire ni campo magnético que nos protejan de los rayos cósmicos.
- A: Rodear los dormitorios con agua → +30 🛡️ · −30 ⚡ (las bombas que mueven el agua gastan energía)
- B: Cubrir el módulo con regolito (polvo lunar) → +40 🛡️ · −45 ⚡ (los robots excavadores gastan mucha energía)
- 💡 Dato: «El agua bloquea la radiación espacial porque tiene mucho hidrógeno. ¡Y unos metros de polvo lunar sobre la base también sirven de escudo!»
- Fuentes: la NASA explica que la Luna no tiene un campo magnético global y que su capa de gases no protege de la radiación (Solar Wind on the Moon, Weather on the Moon). Sus estudios proponen cubrir los hábitats con 3 a 5 metros de regolito (Radiation Protection for Lunar Mission Scenarios, NTRS). También están el estudio de la NASA sobre refugios con paredes de agua (RadWorks, NTRS 20150001237) y el portal OSDR RadLab, que reúne mediciones de radiación de la Estación Espacial y, desde mayo de 2026, de Artemis I. Solo se cita y enlaza: la NASA pone condiciones para redistribuir esos datos (OSDR Environmental Data).
- Cambio: en la v1, la opción B (+20 / −45) era peor en todo que la A (+30 / −30). Era una opción dominada: no había dilema. Ahora B protege más, pero cuesta más. En la base lunar, el escudo magnético experimental se cambia por regolito, una técnica que la NASA estudia de verdad.

### Nivel 4 · Tormenta Solar ☀️🔥 (el fondo cambia a #450A0A)

- Etiqueta: 📡 Dato EN VIVO: NASA DONKI. Si se usa el respaldo, la etiqueta es «📡 Dato HISTÓRICO REAL: NASA DONKI».
- Historia: ¡ALERTA ROJA! El Sol lanzó una llamarada gigante hacia la Luna. La radiación está subiendo al límite peligroso.
- Al empezar el nivel: se muestra la clase real de la llamarada junto a la mini escala A · B · C · M · X. Si es de clase X, se resta −5 🛡️ extra, avisado en pantalla. Esta penalización se aplica antes de tomar la foto del estado, así que no se repite en los reintentos.
- A: Refugiarse rodeados de provisiones → +35 🛡️ · −30 💧 («En el refugio estamos protegidos, pero el aire y el agua se gastan más rápido.»)
- B: Cargar las baterías antes del golpe → +30 ⚡ · −45 🛡️
- 💡 Dato: «En la nave Orión, los astronautas arman un refugio con sus bolsas de provisiones. ¡Pueden quedarse dentro hasta 24 horas!» Debajo, una línea dinámica: «Llamarada real: clase {clase} del {fecha} (NASA DONKI).» En modo respaldo se añade: «¡En solo 2 días, el Sol lanzó 5 llamaradas de clase X!» (sección 7).
- Fuente: NASA, Crew Systems (Orion), actualizada el 8-09-2026. La tripulación vacía dos compartimentos bajo los asientos y se protege con las bolsas de provisiones; tiene hasta una hora para prepararlo y puede quedarse dentro hasta 24 h. No afirmar que se probó en vuelo: en Artemis II (regresó el 10-04-2026) esa demostración se recortó y no hubo tormentas fuertes.
- Cambio: «Girar los paneles de costado» no protege a la tripulación de la radiación; refugiarse rodeados de provisiones es el procedimiento real. Además, en la v1 el 💧 Soporte Vital dejaba de importar después del Nivel 2. Con este costo vuelve a ser decisivo.

### Nivel 5 · El Relevo Final 🚀

- Etiqueta: 📡 Dato NASA: Comunicaciones
- Historia: ¡Llega el módulo de aterrizaje con el relevo! Pero la tormenta solar dejó estática en la radio y no ve dónde alunizar.
- A: Encender la baliza de la base a máxima potencia → gana si ⚡ > 35
- B: Guiado automático por computadora → gana si 🛡️ > 40 («los sensores del guiado necesitan estar protegidos de la radiación»)
- 💡 Dato: «Las llamaradas solares pueden interrumpir las comunicaciones por radio. ¡Y una señal tarda más de 1 segundo en viajar de la Luna a la Tierra!»
- Cambio: «retrasa» pasa a «interrumpe», que es más preciso.

### Balance verificado (el simulador de la sección 10 debe reproducirlo)

Se recorrieron todas las rutas, incluidos los fallos y los reintentos, con y sin la penalización de clase X. Con las cifras de la v2:

- No hay ningún estado sin salida, y el Nivel 5 siempre tiene al menos una opción ganadora.
- Existen rutas perfectas, sin perder ningún respaldo (por ejemplo N1-A → N2-B → N3-A → N4-A).
- Existen rutas con riesgo real. Por ejemplo, N1-A → N2-A deja 💧 en 10 %, y en el Nivel 4 el refugio falla.
- Ninguna opción está dominada.

La adaptación a base lunar solo cambió textos: las cifras, y por lo tanto este balance, siguen iguales.

## 5. Diseño visual: pantalla única en 3 bloques · Ciclo II (Diseño)

### 5.1 Los 3 bloques

- Panel de estado (HUD): los 3 medidores, cada uno con icono, nombre, barra y número; los 3 respaldos; el progreso «Nivel 2 de 5» con 5 puntos; y botones pequeños de 🔊/🔇 y «?» (cómo se juega).
- Tarjeta de misión: etiqueta 📡, avatar grande (emoji), título e historia. Tras decidir, muestra el resultado y el 💡 Dato real con su fuente. En esta misma tarjeta se muestran también el Inicio, los avisos de Respaldo, Misión Abortada y Victoria: la pantalla es siempre la misma y solo cambia el contenido.
- Panel de decisiones: botón A (azul) y botón B (naranja), con ganancia y costo. Después de decidir, se convierte en «Continuar ▶».

### 5.2 Layout responsivo (mobile-first)

- Móvil vertical: HUD compacto arriba, tarjeta al centro (con desplazamiento interno solo si hiciera falta) y decisiones fijas abajo, en la zona del pulgar. Usar la altura dinámica del viewport (dvh, con vh de respaldo). En 360×640, los botones de decisión se ven sin hacer scroll.
- Móvil horizontal: dos columnas compactas.
- 900 px o más: el HUD como columna izquierda; la tarjeta y las decisiones a la derecha; ancho máximo de unos 1100 px, centrado.
- Tipografía: fluida con clamp(); base de 18 px o más e historia de 20 px o más. Fuente legible (Atkinson Hyperlegible o Lexend, con font-display: swap) y system-ui de respaldo. El juego tiene que verse bien aunque no cargue la fuente externa.

### 5.3 Paleta (tokens CSS)

| Token | Color | Uso |
|---|---|---|
| --color-fondo | #0F172A | Fondo general |
| --color-superficie | #1E293B | Tarjeta y HUD |
| --color-primario | #3B82F6 | Opción A y barras en estado normal |
| --color-acento | #F97316 | Opción B y medidor bajo |
| --color-alerta | #450A0A | Fondo del Nivel 4 (.alert-storm) |

Para el texto se usan dos neutros, #F8FAFC y #CBD5E1, que no cuentan como parte de la paleta.

Contraste (WCAG AA), con valores calculados:

- Texto blanco sobre #F97316: 2,9:1 ❌. Usar texto oscuro #0F172A sobre el naranja (6,1:1 ✔).
- Texto blanco sobre #3B82F6: 3,7:1, que solo es válido para texto grande. Usar negrita de 19 px o más en el botón, o texto oscuro (4,9:1).
- No depender solo del color: medidores y botones llevan siempre icono, texto y número. El par azul/naranja ya es seguro para personas con daltonismo.

### 5.4 Animaciones

- Barras con transition: width 0.4s ease, y números flotantes («+20 ⚡», «−35 💧») junto a cada barra.
- shake de 0,4 s en la tarjeta al perder un respaldo.
- .alert-storm: transición del fondo a #450A0A y pulso suave del borde.
- Con prefers-reduced-motion: sin shake ni pulsos; en su lugar, un destello del borde o un cambio de color.

### 5.5 Accesibilidad

- HTML semántico, lang="es", botones nativos, foco visible y objetivos táctiles de 48 px o más.
- Cada medidor como barra de progreso accesible, con su valor anunciado («Batería 45 %»).
- Una región live (polite) para resultados y mensajes. Usar assertive solo para «Respaldo activado».
- Atajos de teclado: A/B o 1/2 para elegir, y Enter para continuar. Al cambiar de nivel, el foco pasa a la tarjeta.

## 6. Arquitectura técnica · Ciclo II (Diseño)

Stack: HTML5 + CSS3 (variables, Grid y Flexbox) + JavaScript ES6+ puro. Sin frameworks, sin paso de compilación y sin dependencias obligatorias. Lucide es opcional: los pocos íconos que hacen falta (salvavidas, altavoz) se copian como SVG en línea, y los avatares son emojis nativos. Así no se depende de ningún CDN durante la evaluación.

Estructura. Es una evolución de la propuesta original: mismo stack, pero game.js se divide en contenido, motor y vista para que el motor se pueda probar.

```
base-lunar/
├── index.html        # Pantalla única: 3 bloques + contenidos de Inicio, Respaldo, Abortada y Victoria
├── style.css         # Tokens, layout responsivo, estados, animaciones, movimiento reducido, impresión
├── js/
│   ├── config.js     # Clave NASA, tiempos (timeout 3 s, caché 6 h), banderas de demo
│   ├── levels.js     # Guion: 5 niveles, mensajes de respaldo, fuentes (solo datos)
│   ├── nasaData.js   # DONKI: consulta con timeout, validación, caché y respaldo histórico
│   ├── game.js       # Motor puro: estado, reglas, respaldos, antibloqueo (sin DOM)
│   └── ui.js         # Render, eventos, accesibilidad, voz y arranque (único que toca el DOM)
├── tests/
│   └── tests.html    # Pruebas del motor + simulador de balance (se abre en el navegador)
├── assets/
│   └── favicon.svg
├── README.md         # Cómo jugar, cómo ejecutarlo, arquitectura, fuentes y créditos
├── AGENTS.md         # Reglas para agentes de IA que trabajen en el repositorio
└── DECISIONES.md     # Registro de decisiones (fecha · decisión · motivo)
```

Carga de scripts: scripts clásicos con defer, en este orden: config → levels → nasaData → game → ui, todos bajo un único espacio de nombres global, BaseLunar. Motivo: los módulos ES no cargan al abrir index.html con doble clic (file://), y el juego tiene que funcionar también desde un ZIP, sin servidor.

Responsabilidades y contratos:

- game.js contiene solo funciones puras: crear el estado inicial, aplicar una decisión (devuelve el nuevo estado y una lista de eventos: medidorAlMaximo, respaldoActivado, nivelSuperado, victoria, misionAbortada), reintentar, reiniciar, evaluar la condición del Nivel 5 y comprobar si un nivel es sobrevivible. Nunca toca el DOM, la red ni el almacenamiento.
- ui.js recibe la acción del usuario, llama al motor y dibuja el nuevo estado. Usa textContent y nunca innerHTML con datos externos.
- nasaData.js expone una promesa que siempre se resuelve (nunca falla) con el dato de la tormenta: clase, fecha, fuente, URL y si es en vivo o histórico.
- levels.js solo contiene datos: ningún número mágico vive fuera de levels.js o config.js.

Modelo de datos de un nivel:

| Campo | Contenido |
|---|---|
| id | 1 a 5 |
| titulo, etiqueta, avatar | Texto del título, texto del 📡 y emoji |
| historia | 150 caracteres como máximo |
| tema | normal o alerta |
| tipo | decision (niveles 1–4) o condicion (nivel 5) |
| opciones A y B | Texto; efectos {proteccion, soporte, bateria} o requisito {medidor, umbral}; y resultado (1 frase) |
| dato | Texto, fuente, URL y verificado (sí/no) |

Máquina de estados:

```
INICIO → NIVEL → RESULTADO → NIVEL (siguiente) … → VICTORIA
NIVEL → RESPALDO → NIVEL (reintento) o bien → MISION_ABORTADA
VICTORIA / MISION_ABORTADA → INICIO (con «Jugar de nuevo»)
```

Durante las transiciones, los controles están bloqueados.

## 7. Integración con NASA DONKI (Nivel 4, sin fallos en vivo)

- Clave (Must): usar una clave personal gratuita de api.nasa.gov (1 000 solicitudes por hora por defecto). DEMO_KEY queda solo como último recurso. La documentación oficial le da 30 solicitudes por hora y 50 al día por IP, pero en una prueba real del 14-11-2026 respondió 429 desde la primera llamada (límite informado en la cabecera: 10). En un evento con mucha gente en la misma red no hay que contar con ella. La clave va en config.js y queda visible en el navegador. Eso es aceptable para esta API pública de datos abiertos, pero la clave no debe reutilizarse en otros servicios. CORS verificado: api.nasa.gov responde con Access-Control-Allow-Origin: *, incluso en los errores.
- Consulta: el endpoint FLR (llamaradas solares), con el rango de los últimos 30 días (startDate y endDate calculados). La v1 decía «hoy», pero casi nunca hay llamaradas registradas en un solo día.
- La consulta se lanza al cargar la página, en paralelo, con un timeout de 3 s (AbortController). Cuando el niño llega al Nivel 4, ya está resuelta.
- Validación: la respuesta tiene que ser una lista; classType tiene que ser una letra (A, B, C, M o X) seguida de un número; la fecha tiene que ser válida. Se elige la llamarada más reciente (por peakTime, o por beginTime si falta).
- Caché: un resultado válido se guarda 6 h en localStorage (con try/catch). Ahorra cuota y aguanta cortes de red entre ensayos.
- Respaldo automático ante timeout, falta de conexión, 403, 429, 5xx, error de CORS, JSON inválido o lista vacía: se usa un registro histórico real, la llamarada de clase X8.7 del 14 de mayo de 2024, la más intensa del ciclo solar 25 hasta esa fecha. Datos verificados del registro DONKI: inicio 16:46 UTC, pico 16:51 UTC, fin 17:02 UTC, ubicación S19W90, región activa 13664, instrumento GOES-P EXIS y CME asociada 2024-05-14T17:00:00-CME-001, e ID confirmado: 2024-05-14T16:46:00-FLR-001.
- Llamaradas de clase X registradas en DONKI el 14 y 15 de mayo de 2024:
  - 2024-05-14T02:03:00-FLR-001 · X1.7
  - 2024-05-14T12:40:00-FLR-001 · X1.2
  - 2024-05-14T16:46:00-FLR-001 · X8.7 (respaldo principal)
  - 2024-05-15T08:18:00-FLR-001 · X3.4
  - 2024-05-15T14:20:00-FLR-001 · X2.9
- Uso: la X8.7 es siempre el respaldo, para que la demostración ante los jueces sea predecible. Las 5 juntas aportan una línea extra al 💡 en modo respaldo: «¡En solo 2 días, el Sol lanzó 5 llamaradas de clase X!».
- Tarea de F0: con la clave personal, consultar FLR del 14 al 15 de mayo de 2024 y guardar en nasaData.js los 5 objetos JSON tal como los devuelve la API (con flrID, horas y link). Así el respaldo es una copia literal del dato real.
- Etiqueta honesta: «📡 Dato EN VIVO» solo si el dato viene de la API (o de su caché de menos de 6 h, indicando la hora de la consulta). Con el respaldo: «📡 Dato HISTÓRICO REAL». Nunca mostrar errores técnicos al niño; registrarlos en la consola solo con ?debug=1.
- Para que el niño entienda la clase, la mini escala A · B · C · M · X la resalta con el texto «X es la más fuerte».
- Los datos de OSDR / RadLab y de CSA son textos curados y estáticos, con su fuente y su URL. No se consultan en vivo: añadiría riesgo sin aportar nada al niño.

## 8. Estándares de código

- ES6+, modo estricto, const/let, funciones cortas (30 líneas como máximo) y ninguna variable global fuera de BaseLunar.
- Nombres en español y en camelCase (tomarDecision, aplicarEfectos); constantes en MAYÚSCULAS. Comentarios JSDoc breves en las funciones públicas del motor.
- Una Content Security Policy en una etiqueta meta: connect-src limitado a api.nasa.gov, y Google Fonts permitido solo si se usa.
- HTML que pase el validador del W3C; cero errores en consola, tanto en el flujo normal como en el respaldo.
- Sin analítica, sin cookies y sin servicios de terceros más allá de la NASA (y la fuente tipográfica, si se usa).

## 9. Plan de construcción en 8 horas (MIDE-IA 2.0)

| Fase | Ciclo | Tiempo | Acumulado | Resultado |
|---|---|---|---|---|
| F0 · Preparación | I–II | 0:30 | 0:30 | Repositorio y estructura; README, AGENTS y DECISIONES iniciales; clave NASA; fuentes verificadas o marcadas |
| F1 · Maquetación de los 3 bloques | III | 1:30 | 2:00 | index.html y style.css con datos de ejemplo, responsivo y accesible |
| F2 · Motor y pruebas | III | 2:00 | 4:00 | levels.js, game.js, ui.js y tests.html en verde; juego completo de inicio a fin sin NASA |
| F3 · NASA DONKI | III | 1:00 | 5:00 | nasaData.js con timeout, validación, caché y respaldo |
| F4 · Pulido y finales | III | 1:00 | 6:00 | Inicio, certificado, Misión Abortada, voz, modo demo, movimiento reducido |
| F5 · Validación | IV | 1:00 | 7:00 | Matriz de pruebas, Lighthouse, prueba con usuarios y correcciones |
| F6 · Despliegue | V | 0:30 | 7:30 | Enlace público, ZIP offline, tag v1.0.0 |
| Colchón | — | 0:30 | 8:00 | Imprevistos |

Reglas de recorte:

- Si a las 5:00 la F3 no está lista, se entrega solo con el respaldo histórico, etiquetado honestamente.
- Si a las 6:00 hay retraso, se quitan primero los Could, luego la voz y por último la escala de llamaradas.

Puertas de calidad (sin cumplirlas no se avanza):

- P0: las reglas de la sección 3 no tienen ambigüedades; cada dato tiene su fuente o está marcado PENDIENTE DE VERIFICAR.
- P1: sin scroll horizontal a 320, 360×640, 768, 1280 y 1440 px; botones visibles sin scroll en 360×640; contraste AA.
- P2: tests.html 100 % en verde; el simulador confirma 0 estados sin salida; una partida completa se juega de inicio a fin.
- P3: los 6 escenarios de fallo de red muestran el dato en 3 s o menos y con la etiqueta correcta.
- P4: todo se puede jugar solo con el teclado; el lector de pantalla anuncia los cambios; se respeta el movimiento reducido.
- P5: se cumplen los criterios de aceptación de la sección 10, con evidencias.
- P6: el enlace público se probó en un móvil real; el README está completo; el ZIP de respaldo existe.

## 10. Pruebas y criterios de aceptación · Ciclo IV (Validación)

Pruebas automáticas del motor (tests/tests.html):

- Aplicar efectos suma y resta bien, y recorta los valores a 0–100.
- Un medidor en 0 o menos activa el respaldo, resta 1 y restaura la foto con el medidor que falló en 30 o más.
- Con 0 respaldos se llega a misión abortada.
- Nivel 5: las condiciones son estrictas (>, no ≥); elegir mal resta 1 respaldo y no cambia los medidores.
- La clase X aplica −5 🛡️ una sola vez, también si el nivel se reintenta.
- Reiniciar devuelve exactamente el estado inicial.

Simulador de balance. Recorre todas las rutas, incluidos los fallos y los reintentos, con y sin clase X, y debe confirmar que:

- Desde cualquier estado alcanzable hay al menos una opción que no falla.
- El Nivel 5 siempre tiene al menos una opción ganadora.
- Existe al menos una ruta sin perder respaldos.
- Existe al menos una ruta que pierde respaldos (el riesgo es real).
- Ninguna opción está dominada.

Al terminar, imprime una tabla resumen: número de rutas, victorias por respaldos restantes y número de Misiones Abortadas.

Matriz de pruebas manuales:

- Navegadores: Chrome y Firefox de escritorio, Safari en iOS y Chrome en Android.
- Red (con DevTools): normal, sin conexión, 3G lenta (más de 3 s), clave inválida (403), límite de solicitudes (429) y respuesta vacía.
- Robustez: doble clic rápido, recarga a mitad de partida, rotar el móvil y zoom al 200 %.
- Lighthouse en móvil: Accesibilidad 95 o más, Rendimiento 90 o más, Buenas prácticas 90 o más.
- Aceptación de usuario: 1 o 2 niños de 7 a 12 años, con permiso de su adulto responsable. Si no es posible, un adulto que lea el juego en voz alta. Anotar dónde dudan.

Criterios de aceptación (Definición de Terminado):

- [ ] Una partida completa dura de 3 a 5 minutos y se puede terminar en móvil y en escritorio.
- [ ] Cada opción muestra su ganancia y su costo, y ningún medidor sale del rango 0–100.
- [ ] Al perder un respaldo hay shake (o su alternativa), el mensaje es el del medidor correcto y el reintento usa el estado restaurado.
- [ ] Con 0 respaldos aparece Misión Abortada, y «Jugar de nuevo» reinicia sin recargar la página.
- [ ] El Nivel 4 muestra un dato real en 3 s o menos, con la etiqueta EN VIVO o HISTÓRICO correcta.
- [ ] Sin internet, el juego funciona completo.
- [ ] El teclado, el lector de pantalla y el movimiento reducido funcionan.
- [ ] Cero errores en consola, y ningún dato del niño sale del navegador.
- [ ] Todas las fuentes están enlazadas en la pantalla de Victoria y en el README.

## 11. Despliegue y entregables · Ciclo V (Operación)

- Publicar en GitHub Pages (o Netlify) desde main y probar el enlace en un móvil real. Incluir metaetiquetas Open Graph (para que el enlace se vea bien al compartirlo), favicon y un mensaje noscript.
- Preparar un ZIP offline del repositorio como respaldo para los jueces. Funciona con doble clic gracias a los scripts clásicos.
- Crear el tag v1.0.0 con una nota de versión.

Mapa de entregables de MIDE-IA 2.0:

| Entregable | Dónde queda |
|---|---|
| Contexto y Requisitos | Este Prompt 0 y el README |
| Arquitectura y Modelo de datos | README (copia de la sección 6) y levels.js |
| Roadmap | Sección 9 |
| README + AGENTS | En la raíz del repositorio |
| Código + pruebas | js/ y tests/tests.html |
| CI/CD | Despliegue automático de Pages en cada push a main |
| Documentación | README, DECISIONES.md, créditos y fuentes |
| Backups | Repositorio remoto, tag v1.0.0 y ZIP offline |

## 12. Verificación científica y de marca (antes de entregar)

Verificación hecha el 14-11-2026:

| Dato | Estado | Fuente |
|---|---|---|
| Base lunar como plan de la NASA | ✔ Anunciada el 24-03-2026, al pausar Gateway: unos 20 000 millones de dólares en 7 años; primer intento de alunizaje en Artemis IV (2028) | Space.com, The Register |
| Noche lunar | ✔ Dura casi 15 días terrestres y el suelo baja de 130 °C bajo cero (las cifras de la NASA van de −130 a −173 °C) | NASA Lunar Surface Technology, NASA Moon Facts |
| Lechuga Veggie | ✔ 10-08-2015, lechuga romana roja | Blog NASA Kennedy |
| Plantas que limpian agua y dan oxígeno (CSA) | ✔ Confirmado con el contenido indexado de la página; el acceso directo falló durante la prueba | CSA |
| Luna sin campo magnético global ni atmósfera protectora | ✔ | NASA Solar Wind on the Moon, NASA Weather on the Moon |
| Regolito como escudo | ✔ Capa de 3 a 5 m sobre el hábitat (estudios de la NASA de los años 1990–2000) | NTRS 20050215115 |
| Agua e hidrógeno como escudo | ✔ | NASA RadWorks (NTRS) |
| OSDR RadLab | ✔ Mediciones de radiación de la EEI y, desde mayo de 2026, de Artemis I. Solo citar y enlazar. | RadLab |
| Refugio de Orión | ✔ Bolsas de provisiones, hasta 1 h para armarlo, hasta 24 h dentro. No probado en vuelo en Artemis II. | NASA Crew Systems |
| Llamarada X8.7 del 14-05-2024 | ✔ Horas, región, instrumento e ID 2024-05-14T16:46:00-FLR-001, junto con las otras 4 llamaradas X del 14 y 15 de mayo (IDs de DONKI aportados por el equipo) | SpaceWeatherLive y el registro DONKI (visto con el buscador; la página no abrió directamente) |
| Oxígeno a partir del agua en la EEI | 🔍 Dato conocido; falta enlazar una fuente de la NASA en F0 | — |
| Llamaradas que interrumpen la radio; señal Luna–Tierra de más de 1 s | 🔍 Falta una fuente de la NASA o la NOAA en F0 | — |

Marca: sin logos de NASA ni de CSA y sin frases que sugieran un respaldo oficial. Se usa la atribución «Datos de NASA / CSA».

## 13. Tu primera respuesta (todavía sin código)

Responde solo con esto:

1. Un resumen de lo que vas a construir (10 líneas como máximo).
2. Una tabla de balance con el estado de los 3 medidores tras cada una de las 16 combinaciones de decisiones de los niveles 1 a 4, y qué opción gana en el Nivel 5, con y sin clase X. Indica si confirma o refuta la sección 4.
3. Los riesgos que veas y tus preguntas bloqueantes (5 como máximo). Si no tienes ninguna, dilo.
4. La confirmación del plan de fases.

Después, espera mi mensaje «Aprobado, inicia la fase 0».
