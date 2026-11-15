# Validación (F5 · Ciclo IV)

Matriz de la puerta **P5**: cada criterio de la sección 10 del [Prompt 0](PROMPT-0.md), con su evidencia. Fecha: 15-11-2026.

Hay dos versiones del juego (D-32):

- **Escritorio** (C# + WPF): la experiencia principal.
- **Web** (F0–F2): se conserva sin cambios y es el acceso por enlace y desde el móvil.

Cada criterio se comprueba en las dos.

Cómo repetir las pruebas:

- Todo: `dotnet test escritorio/BaseLunar.sln`. Son 202 pruebas y tardan unos 6 minutos.
- Sin abrir navegadores: `--filter "TestCategory!=Navegador"`.
- Sin red: `--filter "TestCategory!=Red"`.
- Capturas e informes: quedan en `escritorio/capturas/`, que no se sube al repositorio. La carpeta `web/` tiene las de la web y los archivos `f5-*`, los de esta fase.

Leyenda:

- ✅ Cumplido, con evidencia automática.
- 🧑 Necesita a una persona (ver la sección 5).
- ⏳ Se hace en F6.

## 1. Criterios de aceptación (Definición de Terminado)

| # | Criterio | Escritorio | Web | Estado |
|---|---|---|---|---|
| 1 | Una partida dura de 3 a 5 minutos y se termina en móvil y en escritorio | Medido con la voz local leyendo cada pantalla en voz alta, más las animaciones, sin contar el tiempo de pensar: 3,9 min sin perder respaldos, 4,5 min perdiendo 1 y 4,2 min hasta Misión Abortada (`f5-duracion.txt`). Con UI Automation se juegan dos partidas completas en la ventana real: una hasta la Victoria y otra hasta Misión Abortada. | Partida completa en un móvil emulado de 360×640, táctil, en Edge, Chrome y Firefox. | ✅ Se puede terminar. 🧑 El tiempo real se mide en la prueba con usuarios. ⏳ Móvil real en P6. |
| 2 | Cada opción muestra su ganancia y su costo; ningún medidor sale de 0–100 | Botón con «+20 ⚡ · −35 💧» y nombre accesible «Gana … y cuesta …». El simulador recorre las 59 rutas (con y sin clase X) y confirma que ningún medidor sale de 0–100 en ningún estado. | En cada nivel de la partida en el móvil: una ganancia (+) y un costo (−) en cada opción de los niveles 1 a 4, y su requisito en el 5. | ✅ |
| 3 | Al perder un respaldo hay shake (o su alternativa), el mensaje es el del medidor correcto y el reintento usa el estado restaurado | La tarjeta tiembla. Con movimiento reducido destella sin moverse (D-60). La escena muestra el fallo de ese medidor. Hay un mensaje por medidor, y el reintento restaura la foto con el medidor en 30 o más. | Animación `temblor`; con movimiento reducido, `destello`. Mensaje visible, y en el reintento el medidor que falló vuelve con 30 o más. | ✅ |
| 4 | Con 0 respaldos aparece Misión Abortada y «Jugar de nuevo» reinicia sin recargar | UI Automation: 3 respaldos en el Nivel 4 llevan a Misión Abortada y «Jugar de nuevo» vuelve al Inicio con 80/80/80. | La misma ruta en el móvil. Una marca en `window` comprueba que la página no se recargó. | ✅ |
| 5 | El Nivel 4 muestra un dato real en 3 s o menos, con la etiqueta EN VIVO o HISTÓRICO correcta | La consulta empieza al abrir el juego. Hace 2 intentos de 3 s como máximo y, si fallan, usa el respaldo. Al llegar al Nivel 4 ya hay un dato, y si no, se ve el respaldo al instante: la pantalla nunca espera. Un nivel nunca cambia su dato a medias. Se probaron los 6 escenarios de red y la red real. | Siempre el respaldo histórico, al instante y con la etiqueta «HISTÓRICO REAL» (D-55). | ✅ |
| 6 | Sin internet, el juego funciona completo | `--offline` y las partidas de UI Automation. Sin red, cada consulta falla y se usa el respaldo, sin errores. | Las dos partidas en el móvil se juegan con la red cortada en Edge y Chrome. También se juega abierta con doble clic (`file://`, como el ZIP). | ✅ |
| 7 | El teclado, el lector de pantalla y el movimiento reducido funcionan | Una partida entera solo con teclado. Tab llega a todo. Nombres, valores, encabezados y regiones en vivo de UI Automation. Movimiento reducido sin desplazamientos. Alto contraste a 7:1. | En los 3 navegadores, Enter comienza, A elige y el resultado llega a la región `aria-live`. El movimiento reducido se emuló en Edge y Chrome. | ✅ 🧑 Falta escuchar el Narrador de verdad (sección 5.2). |
| 8 | Cero errores en consola y ningún dato del niño sale del equipo | Cero errores de enlace de datos en 18 partidas: 3 tamaños, con y sin alto contraste, en vivo, glosario y nombre. «Control de Misión» no aparece nunca. El nombre no está en ningún archivo y no se lee en voz alta. La auditoría del código encuentra red solo en `ClienteDonki` y escritura solo en los ajustes y la caché. A la NASA solo van pedidos GET con fechas y la clave. | Cero errores de consola (excepciones, `console.error`, CSP y recursos) en las 3 pruebas de cada navegador. Ningún pedido sale de la propia web. | ✅ |
| 9 | Todas las fuentes enlazadas en la pantalla de Victoria y en el README | Las 11 fuentes de los niveles y de DONKI están enlazadas en el certificado. El README tiene esas 11 más las 5 del glosario. | Los 11 enlaces en la Victoria de la web, en los 3 navegadores. | ✅ |

## 2. Pruebas automáticas del motor y simulador

| Lo que pide la sección 10 | Evidencia |
|---|---|
| Aplicar efectos suma, resta y recorta a 0–100 | `tests/tests.html` (39 de 39 en verde en Edge, Chrome y Firefox) y sus 39 pruebas trasladadas a C# (`PruebasMotor`) |
| Medidor en 0 o menos: respaldo, −1 y foto con el medidor en 30 o más | Ídem |
| 0 respaldos: Misión Abortada | Ídem |
| Nivel 5: condiciones estrictas; elegir mal resta 1 y no cambia los medidores | Ídem |
| Clase X: −5 🛡️ una sola vez, también al reintentar | Ídem |
| Reiniciar devuelve exactamente el estado inicial | Ídem |
| Simulador: desde cualquier estado hay una opción que no falla | 0 estados sin salida, con y sin clase X |
| Simulador: el Nivel 5 siempre tiene una opción ganadora | ✅ |
| Simulador: existe una ruta sin perder respaldos y otra que pierde | ✅ |
| Simulador: ninguna opción está dominada | `PruebasContenido` |
| Tabla resumen | 59 rutas · 45 victorias (16 con 3 respaldos, 13 con 2 y 16 con 1) · 14 Misiones Abortadas, igual con y sin clase X |

## 3. Matriz de pruebas manuales, ahora automatizadas

| Lo que pide la sección 10 | Cómo se comprobó | Estado |
|---|---|---|
| Navegadores: Chrome y Firefox de escritorio | Edge, Chrome y Firefox sin ventana, manejados desde C# (D-61): tests.html, partidas completas, tamaños y robustez | ✅ |
| Navegadores: Safari en iOS y Chrome en Android | Necesitan el enlace público y móviles reales | ⏳ P6 |
| Red: normal, sin conexión, 3G lenta (más de 3 s), 403, 429 y respuesta vacía | `PruebasDonki`: 404 y 503, 429 y 403, JSON inválido, lista vacía, sin llamaradas válidas, sin red y espera agotada. Todos terminan en el respaldo X8.7, nunca en un error. La red real, el 15-11-2026: `api.nasa.gov` no respondió y el CCMC dio una llamarada M3.5 del 05-11-2026, marcada EN VIVO. | ✅ |
| Robustez: doble clic rápido | Escritorio: UI Automation y modelo de vista. Web: doble toque en los 3 navegadores. En todos, una sola decisión. | ✅ |
| Robustez: recargar a mitad de partida | Web: vuelve al Inicio con 80/80/80 y sin errores. Escritorio: no guarda la partida, así que al volver a abrir siempre empieza en el Inicio. | ✅ |
| Robustez: rotar el móvil | Web: de 360×640 a 640×360 a mitad de partida; el nivel y los medidores siguen igual y no hay scroll horizontal. Escritorio: cambiar de tamaño pasa de la disposición ancha a la compacta. | ✅ |
| Robustez: zoom al 200 % | Web: 1280×800 al 200 %, sin scroll horizontal. Escritorio: zoom de Windows al 125, 150 y 200 %, con captura de los píxeles reales (D-59). | ✅ |
| P1: sin scroll horizontal a 320, 360×640, 768, 1280 y 1440; botones visibles en 360×640 | Web, en los 3 navegadores y en Inicio, Nivel 1, Resultado, Nivel 4 y Respaldo. Los botones se ven sin desplazar la página. | ✅ |
| Lighthouse en móvil: Accesibilidad ≥ 95, Rendimiento ≥ 90, Buenas prácticas ≥ 90 | No se ejecutó: la versión de línea de comandos exige instalar un paquete externo (D-61). Se hace desde las DevTools de Edge o Chrome (sección 5.3). | 🧑 |
| Aceptación de usuario: 1 o 2 niños de 7 a 12 años | Protocolo en la sección 5.1 | 🧑 |

## 4. Correcciones hechas en F5

La validación encontró 4 problemas, ya corregidos y con su prueba:

1. **Zoom de Windows (D-59).**
   - El problema: con zoom al 200 % en una pantalla de 1920×1080 (o al 125 % en una de 1366×768), la ventana mínima de 800×600 era más alta que la pantalla y la parte de abajo quedaba tapada. Además, la tarjeta se quedaba en 117 DIP de alto.
   - La corrección: el mínimo nunca pasa del área de trabajo. Con poca altura, la escena decorativa se oculta y la tarjeta recupera ese espacio.
   - La prueba: capturas al 125, 150 y 200 %.
2. **Movimiento reducido en escritorio (D-60).** El temblor no tenía alternativa. Ahora la tarjeta destella en naranja y se apaga en 600 ms, como `destello` en la web.
3. **Fuentes en el README.** El README remitía a docs/FUENTES.md sin enlazar cada fuente. Ahora tiene una tabla con las 16.
4. **Privacidad en el README.** Decía que la web se conecta a `api.nasa.gov`, pero no se conecta a nada (D-55). Se corrigió.

## 5. Lo que necesita a una persona

### 5.1 Prueba con usuarios (aceptación)

Participan 1 o 2 niñas o niños de 7 a 12 años, con el permiso de su adulto responsable. Si no es posible, un adulto lee el juego en voz alta.

**Antes de empezar**

- El adulto responsable está presente y da su permiso.
- No se graba, no se fotografía y no se anota el nombre: solo «Participante 1, 9 años».
- En el certificado, el nombre es opcional y no se guarda en ningún sitio.

**La sesión** (unos 15 minutos)

1. Abre el juego (escritorio o web) y explica solo esto: «Es un juego sobre una base en la Luna. Juega como quieras; yo solo miro».
2. No ayudes salvo que lo pida. Si se atasca más de 30 segundos, pregunta: «¿Qué crees que hace este botón?».
3. Cronometra desde «Comenzar misión» hasta el certificado o la Misión Abortada.
4. Al terminar, haz 4 preguntas y anota las respuestas tal cual:
   - «¿Qué pasaba cuando elegías una opción?» (¿entiende que cada opción gana algo y cuesta algo?)
   - «¿Qué es un Protocolo de Respaldo?»
   - «¿Te dio miedo o te hizo sentir mal algo del juego?»
   - «¿Qué aprendiste de la Luna o del Sol?»

**Hoja de observación** (una por participante)

| Momento | ¿Dudó? (dónde y cuánto) | ¿Pidió ayuda? | Nota |
|---|---|---|---|
| Inicio y «Comenzar misión» | | | |
| Nivel 1: leer y elegir | | | |
| Resultado y Dato real | | | |
| Nivel 3: glosario «?» (escritorio) | | | |
| Nivel 4: tormenta y etiqueta del dato | | | |
| Protocolo de Respaldo (si ocurre) | | | |
| Nivel 5: requisito | | | |
| Certificado o Misión Abortada | | | |

Duración: ___ min. Respaldos perdidos: ___.

**Se aprueba si:**

- termina la partida sin ayuda o con una sola pista;
- dura de 3 a 5 minutos;
- explica con sus palabras que cada opción tiene un costo;
- nada le da miedo ni le hace sentir culpa.

Lo que no se cumpla se anota como corrección en DECISIONES.md.

### 5.2 Revisión con el Narrador de Windows

Las pruebas comprueban lo que la aplicación entrega al lector de pantalla (nombres, valores, encabezados y regiones en vivo), pero no lo que se oye.

1. Activa el Narrador (Windows + Ctrl + Enter) y abre el juego.
2. Juega con Tab, A/B y Enter, y marca cada punto:
   - [ ] Al abrir se lee «¡Hola, comandante!».
   - [ ] Cada medidor se lee con su nombre y su valor, por ejemplo «Soporte Vital, 80 %».
   - [ ] Cada opción se lee completa: «Opción A: … Gana … y cuesta …».
   - [ ] Al decidir, se lee el resultado con los cambios y el dato real.
   - [ ] Al perder un respaldo, el mensaje se lee enseguida (anuncio urgente).
   - [ ] Al abrir una palabra del glosario, se lee su explicación.
   - [ ] En el certificado se llega al nombre, a «Imprimir», a las fuentes y a «Jugar de nuevo».
   - [ ] El nombre escrito no se lee como anuncio.
3. Apaga el Narrador con Windows + Ctrl + Enter.

### 5.3 Lighthouse (web)

1. Abre la web en Edge o Chrome: el enlace público (P6), o `index.html` servido en localhost.
2. Pulsa F12, ve a la pestaña Lighthouse, elige «Mobile» y las categorías Accesibilidad, Rendimiento y Buenas prácticas, y pulsa «Analyze page load».
3. Objetivo: Accesibilidad ≥ 95, Rendimiento ≥ 90 y Buenas prácticas ≥ 90. Anota las cifras aquí.

| Fecha | Accesibilidad | Rendimiento | Buenas prácticas |
|---|---|---|---|
| | | | |

## 6. Pruebas añadidas en F5

| Prueba | Qué demuestra |
|---|---|
| `PruebasValidacion` (7) | Duración, temblor o destello, cero errores, privacidad en disco y en el código, fuentes enlazadas y ventana que cabe con zoom |
| `PruebasWeb` (13) | La web en Edge, Chrome y Firefox: tests.html, partidas en el móvil, tamaños, rotación, zoom, doble toque, recarga, teclado y anuncios, sin conexión, movimiento reducido y doble clic (`file://`) |
| `PruebasPantalla` (+3) | Zoom de Windows al 125, 150 y 200 % |
| `PruebasSimulador` (+2) | Ningún medidor sale de 0–100 en ninguna ruta |
| `PruebasDonki` (+1) | Los pedidos a la NASA son GET, sin cuerpo, sin cookies y sin cabeceras con datos |
