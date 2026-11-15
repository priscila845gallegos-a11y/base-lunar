# Base Lunar: El Escudo Cósmico

Juego web educativo de decisiones para niñas y niños de 7 a 12 años. Quien juega comanda una base en la superficie de la Luna durante 5 situaciones reales del espacio y aprende que cada recurso tiene un costo: energía, agua y oxígeno, y protección contra la radiación.

> **Estado: F3 en curso. Se está rediseñando la interfaz como aplicación de escritorio (C# + WPF).**
>
> - Listos: el núcleo en C# (F3.1), la aplicación jugable de principio a fin (F3.2), el arte con las animaciones de cada consecuencia y el glosario (F3.3), y los efectos de sonido y la voz (F3.4).
> - Falta: el dato en vivo de NASA DONKI (F3.5).
>
> La versión web de F0–F2 está terminada y se puede jugar de principio a fin. Sigue siendo el acceso por enlace y desde el móvil. Ver [Versión de escritorio](#versión-de-escritorio-windows) y [Plan y estado](#plan-y-estado).

## Cómo se juega

- Hay 3 medidores: 🛡️ Protección, 💧 Soporte Vital y ⚡ Batería. Empiezan en 80 % y pasan de un nivel al siguiente.
- En los niveles 1 a 4 se elige entre dos opciones. Ninguna es gratis: cada botón muestra lo que gana y lo que cuesta.
- Si un medidor llega a 0, se usa un Protocolo de Respaldo (hay 3) y se reintenta el nivel.
- En el Nivel 5 gana quien sabe leer los medidores.
- Después de cada nivel superado aparece un 💡 Dato real con su fuente.
- Con teclado: A/B o 1/2 para elegir, y Enter para continuar.

## Cómo ejecutarlo

- Sin instalar nada: abre `index.html` con doble clic. Funciona también desde el ZIP, sin servidor y sin internet (salvo el dato en vivo del Nivel 4, que tiene respaldo histórico).
- Con un servidor local (opcional): `python -m http.server 8000` y abre <http://localhost:8000>.
- Pruebas del motor y simulador de balance: abre `tests/tests.html` en el navegador.
- Parámetros de demostración: `?nivel=4` (empezar en un nivel), `?offline=1` (forzar el respaldo histórico) y `?debug=1` (ver el estado interno).

Los parámetros de demostración llegan en F4.

## Versión de escritorio (Windows)

En construcción (F3). Es la misma partida con una interfaz nueva de videojuego, hecha en C# con WPF (.NET 10). Tiene un mapa de progreso por el que camina el astronauta, una escena ilustrada para cada nivel y una animación para cada una de las 10 elecciones. Los fallos se muestran con variantes amables, y hay barras animadas, partículas y un glosario «?» con fuentes de la NASA. Los efectos de sonido se generan con código, y «Escuchar» lee la tarjeta con una voz en español del propio equipo. Las reglas no cambian: el núcleo se trasladó a C# y se comprueba contra la versión web (ver [DECISIONES.md](DECISIONES.md), D-32 a D-53).

- Requisitos: Windows 10 u 11 y el SDK de .NET 10. Para «Escuchar» hace falta una voz de Windows en español (si no hay, el botón no aparece).
- Jugar: `dotnet run --project escritorio/src/BaseLunar.App`. Con teclado: A/B o 1/2 para elegir, Enter para continuar, F11 para la pantalla completa y Esc para salir de ella.
- Pruebas: `dotnet test escritorio/BaseLunar.sln`. Son 150 pruebas:
  - 60 del núcleo: las 39 de tests.html, la tabla de balance y 5 de paridad con js/.
  - 90 de la interfaz: textos, pantalla, accesibilidad, teclado, escena (10 consecuencias, fallos, victoria y abortada), glosario, sonido y voz (con medidas de calidad del audio), y 3 partidas con UI Automation.
- Las 3 partidas con UI Automation abren la ventana del juego unos segundos. Para omitirlas: `--filter "TestCategory!=UIAutomation"`.
- Las capturas de las pruebas quedan en `escritorio/capturas/`: 8 pantallas a 4 tamaños y, con el prefijo `f33-`, el estado final y un fotograma a mitad de cada animación.

```
escritorio/
├── BaseLunar.sln
├── src/BaseLunar.Nucleo/       # Núcleo sin interfaz                                     (F3.1 ✔)
│   ├── Configuracion.cs        # ← js/config.js
│   ├── Contenido/              # ← js/levels.js: Guion.cs, Textos.cs y Modelo.cs
│   ├── Motor/Motor.cs          # ← js/game.js: mismas funciones, reglas y eventos
│   ├── Modelos/                # Estado, eventos y medidores
│   └── Donki/                  # ← js/nasaData.js: el mismo JSON literal, embebido
├── src/BaseLunar.App/          # Interfaz WPF                                            (F3.2 ✔ · F3.3 ✔ · F3.4 ✔ · F3.5)
│   ├── VentanaPrincipal.xaml   # Ventana, atajos de teclado y pantalla completa
│   ├── VistaModelos/           # ← js/ui.js: JuegoVM (flujo), Presentacion (textos), Atajos
│   ├── Vistas/                 # Barra, medidores, escena, astronauta, mapa, tarjeta y decisiones
│   ├── Animacion/              # Director de escena, coreografías, consecuencias, partículas y ambiente
│   ├── Audio/                  # Sintetizador de efectos, salida de audio (winmm) y voz local (SAPI)
│   ├── Controles/              # Íconos, animaciones, emojis → íconos, disposición
│   └── Temas/                  # Paleta, tipografía, estilos e íconos (Lucide)
├── tests/BaseLunar.Pruebas/    # MSTest: motor, contenido, simulador, balance y paridad   (F3.1 ✔)
└── tests/BaseLunar.PruebasUI/  # MSTest: pantalla, capturas, accesibilidad y UI Automation (F3.2 ✔)
```

## Arquitectura

Es una copia de la sección 6 del [Prompt 0](docs/PROMPT-0.md), con el estado de cada archivo.

HTML5 + CSS3 (variables, Grid y Flexbox) + JavaScript ES6+ puro. Sin frameworks, sin paso de compilación y sin dependencias obligatorias. Los íconos (salvavidas, altavoz) son SVG en línea y los avatares son emojis nativos, así que no se depende de ningún CDN.

```
baseLunar/
├── index.html        # Pantalla única: 3 bloques + Inicio, Respaldo, Abortada y Victoria   (F2 ✔ · F4)
├── style.css         # Tokens, layout responsivo, estados, animaciones, impresión          (F2 ✔ · F4)
├── js/
│   ├── config.js     # Clave NASA, tiempos, banderas de demo y reglas del juego            (F2 ✔)
│   ├── levels.js     # Guion: 5 niveles, textos y fuentes (solo datos)                     (F2 ✔)
│   ├── nasaData.js   # DONKI: consulta con timeout, validación, caché y respaldo histórico (F2 respaldo · F3)
│   ├── game.js       # Motor puro: estado, reglas, respaldos, antibloqueo (sin DOM)        (F2 ✔)
│   └── ui.js         # Render, eventos, accesibilidad, voz y arranque (único con DOM)      (F2 ✔ · F4 voz)
├── tests/
│   └── tests.html    # Pruebas del motor + simulador de balance                            (F2 ✔)
├── assets/
│   ├── favicon.svg                                                                         (F1 ✔)
│   └── fonts/        # Atkinson Hyperlegible 400 y 700 (subconjunto latino) + OFL.txt      (F1 ✔)
├── docs/
│   ├── PROMPT-0.md   # Especificación completa                                             (F0 ✔)
│   └── FUENTES.md    # Verificación de cada dato científico                                (F0 ✔)
├── README.md
├── AGENTS.md         # Reglas para agentes de IA
└── DECISIONES.md     # Registro de decisiones (fecha · decisión · motivo)
```

- **Carga de scripts:** scripts clásicos con `defer`, en este orden: config → levels → nasaData → game → ui, todos bajo un único espacio de nombres global, `BaseLunar`. Los módulos ES no cargan al abrir `index.html` con doble clic (file://).
- **game.js:** solo funciones puras. Crea el estado inicial, aplica una decisión (devuelve el nuevo estado y una lista de eventos: `medidorAlMaximo`, `respaldoActivado`, `nivelSuperado`, `victoria`, `misionAbortada`), reintenta, reinicia, evalúa la condición del Nivel 5 y comprueba si un nivel es sobrevivible.
- **ui.js:** recibe la acción del usuario, llama al motor y dibuja el nuevo estado. Usa `textContent`, nunca `innerHTML` con datos externos.
- **nasaData.js:** expone una promesa que siempre se resuelve con el dato de la tormenta: clase, fecha, fuente, URL y si es en vivo o histórico.
- **levels.js:** solo datos. Ningún número mágico vive fuera de `levels.js` o `config.js`.

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

## Datos y fuentes

- **NASA DONKI** (llamaradas solares): dato en vivo en el Nivel 4, con respaldo histórico (la llamarada X8.7 del 14-05-2024).
- **NASA OSDR / RadLab**, **NASA** y **CSA**: textos curados y estáticos, con su fuente.
- La verificación completa, con enlaces, citas y fecha, está en [docs/FUENTES.md](docs/FUENTES.md).
- Atribución: «Datos de NASA / CSA». No se usan logos de la NASA ni de la CSA, y el juego no tiene respaldo oficial de ninguna agencia.

## Privacidad

Sin cuentas, sin cookies y sin analítica. El nombre del certificado solo se muestra en pantalla: nunca se guarda, nunca se envía y nunca se lee en voz alta. La única conexión externa es `api.nasa.gov`. En escritorio, la voz es local y lo único que se guarda es si los efectos de sonido están encendidos (`%LOCALAPPDATA%\BaseLunar\ajustes.json`).

## Créditos

- Íconos: [Lucide](https://lucide.dev/) (licencia ISC). En la web, el salvavidas y el altavoz; en escritorio, todos los íconos.
- Tipografía: [Atkinson Hyperlegible](https://github.com/googlefonts/atkinson-hyperlegible), del Braille Institute (SIL Open Font License).
- Datos: NASA DONKI, NASA OSDR / RadLab, NASA y CSA (ver [docs/FUENTES.md](docs/FUENTES.md)).

## Plan y estado

| Fase | Tiempo | Resultado | Estado |
|---|---|---|---|
| F0 · Preparación | 0:30 | Repositorio, documentación, fuentes verificadas o marcadas | Hecha (P0 aprobada) |
| F1 · Maquetación | 1:30 | index.html y style.css con datos de ejemplo | Hecha (P1 aprobada) |
| F2 · Motor y pruebas | 2:00 | levels.js, game.js, ui.js y tests.html en verde | Hecha (P2 aprobada) |
| F3 · Rediseño de escritorio (C# + WPF) | 6:30 | F3.1 núcleo en C# con paridad ✔ · F3.2 estructura jugable ✔ · F3.3 arte, animación y glosario ✔ · F3.4 sonido y voz ✔ · F3.5 NASA DONKI | En curso (F3.1 a F3.3 aprobadas; F3.4 en revisión) |
| F4 · Pulido y finales | 0:45 | Certificado imprimible, modo demo, alto contraste | Pendiente |
| F5 · Validación | 1:00 | Pruebas, partidas automatizadas, Narrador y prueba con usuarios | Pendiente |
| F6 · Despliegue | 0:30 | Web en GitHub Pages, ZIP portable de escritorio y tag v1.0.0 | Pendiente |

El rediseño amplía el presupuesto inicial de 8 horas (DECISIONES.md, D-37).

## Documentación del proyecto

- [docs/PROMPT-0.md](docs/PROMPT-0.md): especificación completa.
- [DECISIONES.md](DECISIONES.md): decisiones tomadas y su motivo.
- [docs/FUENTES.md](docs/FUENTES.md): verificación científica.
- [AGENTS.md](AGENTS.md): reglas para agentes de IA.
