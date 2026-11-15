# Base Lunar: El Escudo Cósmico

Juego educativo de decisiones para niñas y niños de 7 a 12 años, en la web y para Windows. Quien juega comanda una base en la superficie de la Luna durante 5 situaciones reales del espacio y aprende que cada recurso tiene un costo: energía, agua y oxígeno, y protección contra la radiación.

> **Versión 1.0.0** ([nota de versión](docs/NOTAS-DE-VERSION.md)). Hay dos formas de jugar:
>
> - **Web**: el acceso por enlace y desde el móvil, sin instalar nada. **Jugar: <https://priscila845gallegos-a11y.github.io/base-lunar/>**
> - **Windows** (C# + WPF): la experiencia completa, con escena animada, sonido, voz, glosario y el dato en vivo de NASA DONKI. **Descarga:** `BaseLunar-escritorio-v1.0.0-win-x64.zip` en la [versión v1.0.0](https://github.com/priscila845gallegos-a11y/base-lunar/releases/tag/v1.0.0).
>
> La validación (F5) está en [docs/VALIDACION.md](docs/VALIDACION.md). Quedan pendientes la prueba con niños, la escucha con el Narrador y Lighthouse. Ver [Plan y estado](#plan-y-estado).

## Cómo se juega

- Hay 3 medidores: 🛡️ Protección, 💧 Soporte Vital y ⚡ Batería. Empiezan en 80 % y pasan de un nivel al siguiente.
- En los niveles 1 a 4 se elige entre dos opciones. Ninguna es gratis: cada botón muestra lo que gana y lo que cuesta.
- Si un medidor llega a 0, se usa un Protocolo de Respaldo (hay 3) y se reintenta el nivel.
- En el Nivel 5 gana quien sabe leer los medidores.
- Después de cada nivel superado aparece un 💡 Dato real con su fuente.
- Con teclado: A/B o 1/2 para elegir, y Enter para continuar.

## Cómo ejecutarlo

Web:

- En línea: <https://priscila845gallegos-a11y.github.io/base-lunar/>. Se publica con GitHub Pages desde `main`, y cada push la actualiza.
- Sin instalar nada: abre `index.html` con doble clic. Funciona también desde el ZIP del repositorio, sin servidor y sin internet. En el Nivel 4 muestra siempre la llamarada X8.7 real del 14-05-2024, con la etiqueta «HISTÓRICO REAL» (D-55).
- Con un servidor local (opcional): `python -m http.server 8000` y abre <http://localhost:8000>.
- Pruebas del motor y simulador de balance: abre `tests/tests.html` en el navegador.
- Las opciones de demostración (empezar en un nivel, sin red, depuración) están en la versión de Windows (D-56).

Windows: ver la sección siguiente.

## Versión de escritorio (Windows)

Es la misma partida con una interfaz nueva de videojuego, hecha en C# con WPF (.NET 10). Tiene un mapa de progreso por el que camina el astronauta, una escena ilustrada para cada nivel y una animación para cada una de las 10 elecciones. Los fallos se muestran con variantes amables, y hay barras animadas, partículas y un glosario «?» con fuentes de la NASA. Los efectos de sonido se generan con código, y «Escuchar» lee la tarjeta con una voz en español del propio equipo. Las reglas no cambian: el núcleo se trasladó a C# y se comprueba contra la versión web (ver [DECISIONES.md](DECISIONES.md), D-32 a D-62). El Nivel 4 usa la llamarada más intensa del último mes, consultada en vivo a NASA DONKI, con respaldo histórico si no hay red.

- Versión portable (sin instalar nada, con .NET incluido): `BaseLunar-escritorio-v1.0.0-win-x64.zip`. Se descomprime y se abre `BaseLunar.exe`; las instrucciones están en [escritorio/LEEME.txt](escritorio/LEEME.txt). El ZIP se genera con `powershell -ExecutionPolicy Bypass -File escritorio/publicar.ps1 -Version 1.0.0` (D-63).
- Requisitos: Windows 10 u 11 de 64 bits. Para compilar, el SDK de .NET 10. Para «Escuchar» hace falta una voz de Windows en español (si no hay, el botón no aparece).
- Jugar desde el código: `dotnet run --project escritorio/src/BaseLunar.App`. Opciones de demostración: `--nivel N` (empezar en el nivel N), `--offline` (siempre el respaldo histórico), `--debug` (ver el estado interno), `--silencio` (sin sonido ni voz) y `--alto-contraste`. Si Windows tiene activado el alto contraste, se usa solo. Con teclado: A/B o 1/2 para elegir, Enter para continuar, F11 para la pantalla completa y Esc para salir de ella.
- Pruebas: `dotnet test escritorio/BaseLunar.sln`. Son 202 pruebas:
  - 76 del núcleo: las 39 de tests.html, la tabla de balance, el simulador (con medidores siempre en 0–100), 5 de paridad con js/ y 14 de NASA DONKI. Una de ellas consulta la red real; para omitirla: `--filter "TestCategory!=Red"`.
  - 126 de la interfaz:
    - textos, pantalla (también con el zoom de Windows al 125, 150 y 200 %) y accesibilidad (contraste AA y alto contraste);
    - teclado (una partida entera solo con teclado) y escena (10 consecuencias, fallos, victoria y abortada);
    - glosario, sonido y voz (con medidas de calidad del audio), dato en vivo, modo demo, certificado y «Control de Misión»;
    - 3 pruebas con UI Automation;
    - las de validación de F5: duración, cero errores, privacidad y fuentes;
    - 13 de la web en Edge, Chrome y Firefox.
- Las pruebas con UI Automation abren la ventana del juego unos segundos. Para omitirlas: `--filter "TestCategory!=UIAutomation"`.
- Las de la web abren los navegadores sin ventana (D-61), y si falta alguno quedan como «Inconclusive». Para omitirlas: `--filter "TestCategory!=Navegador"`.
- Para probar lo publicado:
  - `BASELUNAR_WEB=https://priscila845gallegos-a11y.github.io/base-lunar/` hace que las pruebas de la web usen el enlace público;
  - `BASELUNAR_EXE=<ruta>\BaseLunar.exe` hace que las partidas de UI Automation usen el .exe portable.
- Las capturas de las pruebas quedan en `escritorio/capturas/`:
  - 8 pantallas a 4 tamaños y con zoom;
  - con el prefijo `f33-`, el estado final y un fotograma a mitad de cada animación;
  - con `f5-`, los informes de validación;
  - en `web/`, las de la web en cada navegador.

```
escritorio/
├── BaseLunar.sln
├── src/BaseLunar.Nucleo/       # Núcleo sin interfaz                                     (F3.1 ✔)
│   ├── Configuracion.cs        # ← js/config.js
│   ├── Contenido/              # ← js/levels.js: Guion.cs, Textos.cs y Modelo.cs
│   ├── Motor/Motor.cs          # ← js/game.js: mismas funciones, reglas y eventos
│   ├── Modelos/                # Estado, eventos y medidores
│   └── Donki/                  # ← js/nasaData.js (el mismo JSON literal) + consulta en vivo, validación y caché (F3.5 ✔)
├── src/BaseLunar.App/          # Interfaz WPF                                            (F3.2 ✔ · F3.3 ✔ · F3.4 ✔ · F3.5 ✔)
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
├── index.html        # Pantalla única: 3 bloques + Inicio, Respaldo, Abortada y Victoria   (F2 ✔)
├── style.css         # Tokens, layout responsivo, estados, animaciones, impresión          (F2 ✔)
├── js/
│   ├── config.js     # Clave NASA, tiempos, banderas de demo y reglas del juego            (F2 ✔)
│   ├── levels.js     # Guion: 5 niveles, textos y fuentes (solo datos)                     (F2 ✔)
│   ├── nasaData.js   # DONKI: respaldo histórico real (la consulta en vivo está en escritorio, D-55) (F2 ✔)
│   ├── game.js       # Motor puro: estado, reglas, respaldos, antibloqueo (sin DOM)        (F2 ✔)
│   └── ui.js         # Render, eventos, accesibilidad y arranque (único con DOM)           (F2 ✔)
├── tests/
│   └── tests.html    # Pruebas del motor + simulador de balance                            (F2 ✔)
├── assets/
│   ├── favicon.svg                                                                         (F1 ✔)
│   ├── og-imagen.png # Vista previa del enlace al compartirlo (Open Graph)                 (F6 ✔)
│   ├── lucide-LICENSE.txt # Licencia de los íconos de Lucide                               (F6 ✔)
│   └── fonts/        # Atkinson Hyperlegible 400 y 700 (subconjunto latino) + OFL.txt      (F1 ✔)
├── docs/
│   ├── PROMPT-0.md   # Especificación completa                                             (F0 ✔)
│   ├── FUENTES.md    # Verificación de cada dato científico                                (F0 ✔)
│   ├── VALIDACION.md # Matriz de validación (P5) y protocolos con personas                 (F5 ✔)
│   └── NOTAS-DE-VERSION.md # Qué trae cada versión                                         (F6 ✔)
├── escritorio/       # Versión de Windows en C# + WPF (ver «Versión de escritorio»)         (F3–F6 ✔)
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

Fuentes que el juego enlaza (las mismas que la pantalla de Victoria):

| Nivel | Fuentes |
|---|---|
| 1 · La Noche Lunar | [NASA, Moon Facts](https://science.nasa.gov/moon/facts/) · [NASA, LuSEE-Night](https://www.nasa.gov/solar-system/nasa-department-of-energy-join-forces-on-innovative-lunar-experiment/) |
| 2 · El Huerto Espacial | [NASA, blog de la Estación (10-08-2015)](https://blogs.nasa.gov/spacestation/2015/08/10/cosmonauts-complete-russian-spacewalk/) · [CSA, Growing healthy food in space](https://www.asc-csa.gc.ca/eng/sciences/food-production/growing-healthy-food-in-space-and-remote-areas.asp) |
| 3 · Rayos Invisibles | [NASA, RadWorks (NTRS)](https://ntrs.nasa.gov/citations/20150001237) · [NASA, protección en la Luna (NTRS)](https://ntrs.nasa.gov/citations/20050215115) · [NASA OSDR / RadLab](https://www.nasa.gov/reference/osdr-environmental-data) |
| 4 · Tormenta Solar | [NASA DONKI, llamarada X8.7 del 14-05-2024](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30849/-1) (respaldo histórico; en vivo, el enlace de la llamarada consultada) · [NASA, plan de protección de Orión](https://www.nasa.gov/missions/artemis/orion/scientists-and-engineers-evaluate-orion-radiation-protection-plan/) |
| 5 · El Relevo Final | [NASA, Strong Solar Flare](https://www.nasa.gov/image-article/strong-solar-flare/) · [NASA, ¿Qué es un día luz?](https://science.nasa.gov/mission/voyager/voyager-1/voyager-1-what-is-a-light-day/) |
| Glosario (escritorio) | [NASA, ECLSS](https://www.nasa.gov/reference/environmental-control-and-life-support-systems-eclss/) · [NASA, Cosmic Rays](https://imagine.gsfc.nasa.gov/science/toolbox/cosmic_rays1.html) · [NASA, Earth's Magnetosphere](https://science.nasa.gov/science-research/planetary-science/earths-magnetosphere/) · [NASA, la mayor llamarada del ciclo 25](https://svs.gsfc.nasa.gov/14592) · [NASA, X-Class: A Guide to Solar Flares](https://svs.gsfc.nasa.gov/10109) |
- Atribución: «Datos de NASA / CSA». No se usan logos de la NASA ni de la CSA, y el juego no tiene respaldo oficial de ninguna agencia.

## Privacidad

Sin cuentas, sin cookies y sin analítica. El nombre del certificado solo se muestra en pantalla: nunca se guarda, nunca se envía y nunca se lee en voz alta. La web no se conecta a nada: muestra el respaldo histórico (D-55). En escritorio, la voz es local y en `%LOCALAPPDATA%\BaseLunar` solo se guardan si los efectos están encendidos (`ajustes.json`) y la última consulta a DONKI durante 6 h (`donki.json`). Las únicas conexiones son a la NASA: `api.nasa.gov` y, si falla, `ccmc.gsfc.nasa.gov`, con las fechas de la consulta y la clave de la API.

## Créditos

- Íconos: [Lucide](https://lucide.dev/) (licencia ISC; MIT para los derivados de Feather, ver [assets/lucide-LICENSE.txt](assets/lucide-LICENSE.txt)). En la web, el salvavidas y el altavoz; en escritorio, todos los íconos.
- Tipografía: [Atkinson Hyperlegible](https://github.com/googlefonts/atkinson-hyperlegible), del Braille Institute (SIL Open Font License).
- Datos: NASA DONKI, NASA OSDR / RadLab, NASA y CSA (ver [docs/FUENTES.md](docs/FUENTES.md)).

## Plan y estado

| Fase | Tiempo | Resultado | Estado |
|---|---|---|---|
| F0 · Preparación | 0:30 | Repositorio, documentación, fuentes verificadas o marcadas | Hecha (P0 aprobada) |
| F1 · Maquetación | 1:30 | index.html y style.css con datos de ejemplo | Hecha (P1 aprobada) |
| F2 · Motor y pruebas | 2:00 | levels.js, game.js, ui.js y tests.html en verde | Hecha (P2 aprobada) |
| F3 · Rediseño de escritorio (C# + WPF) | 6:30 | F3.1 núcleo en C# con paridad ✔ · F3.2 estructura jugable ✔ · F3.3 arte, animación y glosario ✔ · F3.4 sonido y voz ✔ · F3.5 NASA DONKI en vivo ✔ | Hecha (aprobada) |
| F4 · Pulido y finales | 0:45 | Certificado con nombre e «Imprimir», modo demo, alto contraste y «Control de Misión» | Hecha (P4 aprobada) |
| F5 · Validación | 1:00 | Matriz de P5 con evidencias, web en 3 navegadores, zoom de Windows y protocolos para la prueba con usuarios, el Narrador y Lighthouse | Hecha (P5 en revisión; la prueba con niños, el Narrador y Lighthouse necesitan al equipo) |
| F6 · Despliegue | 0:30 | Web en GitHub Pages, ZIP portable de escritorio y tag v1.0.0 | Hecha (P6 en revisión: falta probar el enlace en un móvil real) |

El rediseño amplía el presupuesto inicial de 8 horas (DECISIONES.md, D-37).

## Documentación del proyecto

- [docs/PROMPT-0.md](docs/PROMPT-0.md): especificación completa.
- [DECISIONES.md](DECISIONES.md): decisiones tomadas y su motivo.
- [docs/FUENTES.md](docs/FUENTES.md): verificación científica.
- [docs/VALIDACION.md](docs/VALIDACION.md): matriz de validación (P5) y protocolos de la prueba con usuarios, del Narrador y de Lighthouse.
- [docs/NOTAS-DE-VERSION.md](docs/NOTAS-DE-VERSION.md): qué trae cada versión.
- [AGENTS.md](AGENTS.md): reglas para agentes de IA.
