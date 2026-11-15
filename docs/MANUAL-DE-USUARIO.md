# Manual de usuario · Base Lunar: El Escudo Cósmico

Versión 1.0.0

Este manual explica cómo abrir el juego, cómo se juega y qué hacer si algo no funciona. Está pensado para familias, docentes y quien presente el juego, y también para leerlo junto a la niña o el niño que va a jugar.

También está en PDF, listo para imprimir: [MANUAL-DE-USUARIO.pdf](pdf/MANUAL-DE-USUARIO.pdf).

## Índice

1. [¿Qué es Base Lunar?](#1-qué-es-base-lunar)
2. [Dónde jugar](#2-dónde-jugar)
3. [Cómo abrir el juego](#3-cómo-abrir-el-juego)
4. [La pantalla](#4-la-pantalla)
5. [Cómo se juega](#5-cómo-se-juega)
6. [Los 5 niveles](#6-los-5-niveles)
7. [Cómo termina la misión](#7-cómo-termina-la-misión)
8. [Controles](#8-controles)
9. [Funciones de la versión de Windows](#9-funciones-de-la-versión-de-windows)
10. [Accesibilidad](#10-accesibilidad)
11. [Para familias y docentes](#11-para-familias-y-docentes)
12. [Modo demostración (para presentar el juego)](#12-modo-demostración-para-presentar-el-juego)
13. [Preguntas frecuentes y solución de problemas](#13-preguntas-frecuentes-y-solución-de-problemas)
14. [Privacidad](#14-privacidad)
15. [Créditos y fuentes](#15-créditos-y-fuentes)

---

## 1. ¿Qué es Base Lunar?

Es un juego educativo de decisiones para niñas y niños de **7 a 12 años**. Quien juega es la comandante o el comandante de una base en la Luna. Tiene que superar **5 situaciones reales del espacio**: la larga noche lunar, un huerto espacial, la radiación, una tormenta solar y la llegada del relevo.

En cada situación hay que elegir entre dos opciones, y **ninguna es gratis**: lo que se gana por un lado se gasta por otro. Así se aprende que en el espacio la energía, el agua, el oxígeno y la protección son recursos que hay que cuidar. Al superar cada nivel aparece un **💡 Dato real** de la NASA o de la Agencia Espacial Canadiense (CSA), con su fuente.

- Una partida dura **unos 4 o 5 minutos**: es una estimación, y cada niño tiene su ritmo.
- Se puede jugar las veces que se quiera.
- No hay cuentas, anuncios ni compras.

## 2. Dónde jugar

| | Web | Windows |
|---|---|---|
| Dónde | En el navegador, del móvil, la tableta o el ordenador | En un ordenador con Windows 10 u 11 de 64 bits |
| Instalación | Ninguna: se abre un enlace | Ninguna: se descomprime un ZIP |
| Qué incluye | El juego completo, con las mismas reglas | Además: escena animada, mapa con astronauta, sonidos, lectura en voz alta, glosario, certificado con nombre para imprimir y dato solar en vivo |
| Internet | Hace falta para abrir el enlace | No hace falta; con internet, el Nivel 4 trae un dato en vivo |

## 3. Cómo abrir el juego

### 3.1 En la web

1. Abre **<https://priscila845gallegos-a11y.github.io/base-lunar/>** en el navegador.
2. Pulsa **«Comenzar misión»**.

Una vez abierta la página, el juego sigue funcionando aunque se corte internet.

También se puede jugar sin internet desde el ZIP del repositorio (`BaseLunar-v1.0.0-repositorio.zip`, en la [versión v1.0.0](https://github.com/priscila845gallegos-a11y/base-lunar/releases/tag/v1.0.0)): descomprímelo y abre `index.html` con doble clic.

### 3.2 En Windows

1. Descarga `BaseLunar-escritorio-v1.0.0-win-x64.zip` desde la [versión v1.0.0](https://github.com/priscila845gallegos-a11y/base-lunar/releases/tag/v1.0.0).
2. Haz clic derecho sobre el archivo y elige **«Extraer todo»**. Si lo abres sin descomprimir, Windows no encuentra los demás archivos del juego.
3. Abre la carpeta **BaseLunar** y haz doble clic en **`BaseLunar.exe`**.
4. Si Windows muestra un aviso azul de SmartScreen, pulsa **«Más información»** y después **«Ejecutar de todas formas»**. Aparece porque el programa no tiene firma digital.

Para quitar el juego, borra la carpeta BaseLunar. Si quieres borrar también las preferencias, borra la carpeta `%LOCALAPPDATA%\BaseLunar`.

## 4. La pantalla

La pantalla tiene siempre las mismas partes:

| Parte | Qué muestra |
|---|---|
| **Barra superior** | El nombre del juego, el progreso («Nivel 3 de 5»), los **Protocolos de Respaldo** que quedan (salvavidas) y, en Windows, los botones de **Escuchar** y de **sonido** |
| **Medidores** | 🛡️ **Protección**, 💧 **Soporte Vital** y ⚡ **Batería**, con su porcentaje. Si uno baja de 30 %, se pone naranja y muestra ⚠ |
| **Escena** (solo Windows) | El mapa de la misión, con el astronauta caminando de estación en estación, y la escena ilustrada de cada nivel |
| **Tarjeta de misión** | La etiqueta 📡 del dato, el título y la historia del nivel, los avisos, el resultado de cada decisión y el 💡 Dato real con su fuente |
| **Botones** | Las dos opciones, **A** (azul) y **B** (naranja), o el botón principal: «Comenzar misión», «Continuar», «Reintentar» o «Jugar de nuevo» |

En un móvil en vertical, los botones quedan siempre abajo, al alcance del pulgar. En una pantalla ancha, la tarjeta y los botones van a la derecha.

## 5. Cómo se juega

### 5.1 Los medidores

| Medidor | Qué representa |
|---|---|
| 🛡️ Protección | El escudo de la base contra la radiación del espacio |
| 💧 Soporte Vital | El agua limpia y el oxígeno para respirar |
| ⚡ Batería | La energía para los calentadores, las máquinas y las comunicaciones |

Los tres empiezan en **80 %** y pasan de un nivel al siguiente. Nunca bajan de 0 ni suben de 100: si una ganancia pasaría de 100, el medidor se queda lleno y aparece «¡Al máximo!».

### 5.2 Elegir

En los niveles 1 a 4, cada botón muestra **lo que gana y lo que cuesta**. Por ejemplo, en el Nivel 1:

- **A · Apagar recicladores de agua**: `+20 ⚡  −35 💧`
- **B · Mantener soporte al máximo**: `+15 💧  −40 ⚡`

Antes de elegir conviene mirar los medidores: **¿alguno quedaría en 0?**

### 5.3 Después de elegir

- La tarjeta cuenta qué pasó, y los medidores suben o bajan con números flotantes como «+20 ⚡».
- Si el nivel se supera, aparece el **💡 Dato real** con su fuente; en Windows, la escena muestra la consecuencia animada.
- Con **«Continuar»** se pasa al siguiente nivel.

### 5.4 Protocolos de Respaldo

Si un medidor llega a **0**, se activa un **Protocolo de Respaldo**.

- La tarjeta tiembla (o destella, si el equipo tiene reducido el movimiento) y un mensaje amable explica qué pasó. **La tripulación siempre está a salvo.**
- Con **«Reintentar»** el nivel vuelve a empezar como estaba, y el medidor que se vació vuelve con al menos un 30 %.
- Hay **3 Protocolos de Respaldo** por partida. Los que quedan se ven como salvavidas en la barra superior. Si se pierde el tercero, la misión termina (sección 7).

### 5.5 Ayuda de Control de Misión

Si alguna vez ninguna de las dos opciones fuera posible, Control de Misión envía un poco del recurso que falta, para que siempre se pueda seguir. Es una red de seguridad: con las cifras del juego no debería aparecer nunca.

## 6. Los 5 niveles

| Nivel | Situación | Pista para pensar |
|---|---|---|
| 1 · La Noche Lunar 🌑 | Empieza la noche lunar, que dura casi 15 días: los paneles solares no reciben luz y la base se enfría | ¿Qué pasa con el agua si se ahorra energía? |
| 2 · El Huerto Espacial 🌱 | Las plantas del invernadero necesitan agua purificada | El agua también sirve para fabricar oxígeno |
| 3 · Rayos Invisibles ☢️ | El dosímetro (medidor de radiación) pita fuerte: la Luna no tiene aire ni campo magnético que proteja | El agua y el polvo lunar pueden servir de escudo, pero cuestan energía |
| 4 · Tormenta Solar ☀️ | ¡Alerta roja! El Sol lanzó una llamarada. El fondo se vuelve rojo | Si la llamarada es de clase X, la más fuerte, la protección baja 5 puntos al empezar |
| 5 · El Relevo Final 🚀 | Llega el módulo con el relevo, pero la tormenta dejó estática en la radio | Aquí los botones no gastan: cada uno **necesita** un mínimo. Por ejemplo, «Necesita más de 35 % ⚡» |

En el Nivel 5 gana quien sabe leer los medidores. Si se elige una opción cuyo requisito no se cumple, se pierde un Protocolo de Respaldo, pero los medidores no cambian.

**El dato del Nivel 4** tiene una etiqueta que dice de dónde viene:

- **«📡 Dato EN VIVO»** (solo en Windows y con internet): la llamarada solar más fuerte del último mes, consultada a la NASA en ese momento.
- **«📡 Dato HISTÓRICO REAL»**: una llamarada real y muy fuerte del 14 de mayo de 2024 (clase X8.7). Aparece en la web y en Windows sin internet.

Los dos datos son reales. La etiqueta solo dice si se consultó ahora o es un registro guardado.

## 7. Cómo termina la misión

### 🏅 Misión cumplida

Si se supera el Nivel 5, el relevo llega a la base y aparece el **Certificado de Comandante Junior de la Base Lunar**. Muestra:

- la fecha;
- una estrella por cada Protocolo de Respaldo sin usar («Respaldos sin usar: 3 de 3»);
- cómo quedaron los medidores;
- **lo que aprendiste**: los 5 datos reales, con sus fuentes.

En Windows, además, se puede escribir un nombre y pulsar «Imprimir» (ver la sección 9.4).

### 🪂 Misión Abortada

Si se pierde el tercer Protocolo de Respaldo, la tripulación **vuelve a salvo a la Tierra en la cápsula**: «¡La tripulación volvió a salvo a la Tierra en la cápsula! Aprendiste mucho en esta misión.»

En los dos casos, **«Jugar de nuevo»** empieza una partida nueva desde el principio.

## 8. Controles

| Acción | Ratón o pantalla táctil | Teclado |
|---|---|---|
| Elegir la opción A | Pulsar el botón azul | **A** o **1** |
| Elegir la opción B | Pulsar el botón naranja | **B** o **2** |
| Comenzar, continuar, reintentar o jugar de nuevo | Pulsar el botón principal | **Enter** |
| Moverse por botones y enlaces | — | **Tab** (y **Mayús + Tab** para volver) |
| Pantalla completa (Windows) | — | **F11**; **Esc** para salir |

Después de cada decisión, los botones se bloquean un momento (menos de medio segundo). Así un doble clic nunca elige dos veces.

## 9. Funciones de la versión de Windows

### 9.1 Escuchar 🗣️

El botón **«Escuchar»** de la barra superior lee en voz alta la tarjeta que se ve, con una voz de Windows en español y a ritmo tranquilo. Si se toma una decisión, la voz se calla.

Si el ordenador no tiene ninguna voz en español, el botón no aparece (ver la sección 13).

### 9.2 Sonido 🔊

El botón del altavoz enciende o apaga los efectos de sonido: elegir, ganar, gastar, superar un nivel, la llamarada, la victoria… El juego recuerda la elección para la próxima vez.

### 9.3 Glosario «?»

Bajo la historia de cada nivel hay botones con un **?** y una palabra difícil. Al pulsarlo aparece una explicación corta, con su fuente de la NASA. Las palabras son: soporte vital, rayos cósmicos, campo magnético, regolito, llamarada solar y clase X.

### 9.4 Certificado con nombre e «Imprimir»

- En el certificado hay un campo **«Tu nombre (opcional)»**, de hasta 24 caracteres. El nombre aparece como «Comandante: …».
- **«Imprimir»** abre el diálogo de impresión de Windows con el certificado en una hoja A4 blanca, que ahorra tinta. Para guardarlo como archivo, elige la impresora **«Microsoft Print to PDF»**.
- El nombre **no se guarda en ningún sitio**: al pulsar «Jugar de nuevo» o cerrar el juego, se borra.

### 9.5 Control de Misión

Si alguna vez algo falla dentro del programa, aparece el aviso «¡Hubo un pequeño fallo en la base! Ya lo arreglamos y tu misión sigue igual.». Basta con pulsar **«Seguir la misión»**: la partida continúa donde estaba.

## 10. Accesibilidad

| Necesidad | Web | Windows |
|---|---|---|
| Jugar sin ratón | Teclado completo (sección 8) | Teclado completo, también en el certificado |
| Lector de pantalla | Preparada para lectores de pantalla: botones con nombre completo, y resultados y avisos anunciados | Preparada para el **Narrador** (Windows + Ctrl + Enter) y otros lectores: nombres, valores de los medidores y anuncios |
| Menos movimiento | Activa «Reducir movimiento» en el móvil o el ordenador | Desactiva «Efectos de animación» en Configuración → Accesibilidad → Efectos visuales |
| Más contraste | — | Activa un tema de contraste en Configuración → Accesibilidad → Temas de contraste. El juego lo usa enseguida |
| Letra más grande | Zoom del navegador (Ctrl y +), hasta 200 % | Escala de Windows (Configuración → Sistema → Pantalla), hasta 200 % |
| Leer en voz alta | — | Botón «Escuchar» (sección 9.1) |

La tipografía es **Atkinson Hyperlegible**, diseñada para leerse con facilidad. Las palabras técnicas se explican entre paréntesis o en el glosario.

## 11. Para familias y docentes

- **Antes de jugar**: basta con decir «Es un juego sobre una base en la Luna. Cada decisión gana algo y cuesta algo».
- **Durante**: deja que elija sola o solo. Si duda mucho, una buena pregunta es «¿Qué medidor bajaría?».
- **Equivocarse forma parte del juego**: los Protocolos de Respaldo están para eso, y los mensajes nunca culpan ni asustan.
- **Después**, algunas preguntas para conversar:
  - ¿Qué pasaba cuando elegías una opción?
  - ¿Por qué ninguna opción era gratis?
  - ¿Qué aprendiste de la Luna o del Sol?
- **Para ampliar**: cada 💡 Dato real enlaza a su fuente de la NASA o de la CSA. En el certificado aparecen los 5 juntos.
- **Tiempo**: una partida dura unos 4 o 5 minutos; con conversación, una sesión de 15 minutos es cómoda.

## 12. Modo demostración (para presentar el juego)

En Windows, el juego acepta opciones para presentarlo en público. Una forma de usarlas:

1. Abre la carpeta BaseLunar.
2. Escribe `cmd` en la barra de direcciones del Explorador y pulsa Enter.
3. Escribe el comando que necesites:

| Comando | Qué hace |
|---|---|
| `BaseLunar.exe --nivel 4` | Cada partida empieza en el nivel indicado, del 1 al 5 |
| `BaseLunar.exe --offline` | No consulta internet: usa siempre el dato histórico |
| `BaseLunar.exe --silencio` | Sin sonido ni voz |
| `BaseLunar.exe --alto-contraste` | Colores de alto contraste, aunque Windows no lo tenga activado |
| `BaseLunar.exe --debug` | Muestra el estado interno, para quien prueba el juego. No es para niños |

Se pueden combinar, por ejemplo: `BaseLunar.exe --nivel 4 --offline`.

## 13. Preguntas frecuentes y solución de problemas

| Pregunta o problema | Respuesta |
|---|---|
| Windows dice que «protegió el equipo» | Pulsa «Más información» y luego «Ejecutar de todas formas». El programa no tiene firma digital |
| `BaseLunar.exe` no se abre | Descomprime el ZIP con «Extraer todo» y abre el .exe desde la carpeta extraída. Hace falta Windows 10 u 11 de 64 bits |
| No aparece el botón «Escuchar» | El ordenador no tiene voces de Windows en español. Se pueden añadir instalando el idioma español con su opción de texto a voz (Configuración → Hora e idioma). El resto del juego funciona igual |
| No se oye nada | Revisa el botón del altavoz del juego y el volumen de Windows. Comprueba también que no se abrió con `--silencio` |
| El Nivel 4 dice «HISTÓRICO REAL» en Windows | No había internet o la NASA no respondió a tiempo. Es normal y el dato es real. Al volver a abrir el juego, lo intenta de nuevo |
| ¿Se guarda la partida? | No. Cada vez que se abre el juego, la misión empieza desde el principio |
| ¿Por qué no puedo imprimir? | Hace falta una impresora instalada, o elegir «Microsoft Print to PDF» para guardar un PDF |
| La ventana no cabe en la pantalla | Pulsa F11 para la pantalla completa, o maximiza la ventana. El juego se adapta al tamaño y al zoom de Windows |
| La web dice «Para jugar a Base Lunar hay que activar JavaScript» | El navegador tiene JavaScript desactivado. Actívalo en su configuración |
| La web se ve rara en un navegador antiguo | Actualiza el navegador. El juego se probó en Edge, Chrome y Firefox actuales y en un móvil real |

## 14. Privacidad

- **Sin cuentas, sin cookies, sin anuncios y sin analítica.**
- **El nombre del certificado** solo se muestra en pantalla: nunca se guarda, nunca se envía y nunca se lee en voz alta.
- **La web** no se conecta a ningún servicio externo.
- **La versión de Windows** solo se conecta a la NASA (`api.nasa.gov` y `ccmc.gsfc.nasa.gov`) para pedir el dato de la tormenta solar. Solo envía las fechas de la consulta, nada de quien juega.
- **Lo único que se guarda** en Windows está en `%LOCALAPPDATA%\BaseLunar`: si los efectos de sonido están encendidos y la última consulta a la NASA, durante 6 horas.

## 15. Créditos y fuentes

- **Datos**: NASA DONKI, NASA OSDR / RadLab, NASA y CSA. Cada dato tiene su fuente en el juego y en [FUENTES.md](FUENTES.md).
- **Tipografía**: Atkinson Hyperlegible, del Braille Institute (licencia SIL OFL).
- **Íconos**: Lucide (licencia ISC; MIT para los derivados de Feather).

Base Lunar es un juego educativo independiente. Usa datos reales de NASA y CSA, pero no tiene respaldo oficial de ninguna agencia, y el certificado no es oficial.

¿Quieres saber cómo está hecho? Consulta la [documentación técnica](DOCUMENTACION.md).
