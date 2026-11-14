# Base Lunar: El Escudo Cósmico

Juego web educativo de decisiones para niñas y niños de 7 a 12 años. Quien juega comanda una base en la superficie de la Luna durante 5 situaciones reales del espacio y aprende que cada recurso tiene un costo: energía, agua y oxígeno, y protección contra la radiación.

> **Estado: fase F0 (Preparación).** Todavía no se puede jugar. Ver [Plan y estado](#plan-y-estado).

## Cómo se juega

- Hay 3 medidores: 🛡️ Protección, 💧 Soporte Vital y ⚡ Batería. Empiezan en 80 % y pasan de un nivel al siguiente.
- En los niveles 1 a 4 se elige entre dos opciones. Ninguna es gratis: cada botón muestra lo que gana y lo que cuesta.
- Si un medidor llega a 0, se usa un Protocolo de Respaldo (hay 3) y se reintenta el nivel.
- En el Nivel 5 gana quien sabe leer los medidores.
- Después de cada nivel superado aparece un 💡 Dato real con su fuente.

## Cómo ejecutarlo

- Sin instalar nada: abre `index.html` con doble clic. Funciona también desde el ZIP, sin servidor y sin internet (salvo el dato en vivo del Nivel 4, que tiene respaldo histórico).
- Con un servidor local (opcional): `python -m http.server 8000` y abre <http://localhost:8000>.
- Pruebas del motor y simulador de balance: abre `tests/tests.html` en el navegador.
- Parámetros de demostración: `?nivel=4` (empezar en un nivel), `?offline=1` (forzar el respaldo histórico) y `?debug=1` (ver el estado interno).

`index.html` llega en F1 y `tests/tests.html` en F2.

## Arquitectura

Es una copia de la sección 6 del [Prompt 0](docs/PROMPT-0.md), con el estado de cada archivo.

HTML5 + CSS3 (variables, Grid y Flexbox) + JavaScript ES6+ puro. Sin frameworks, sin paso de compilación y sin dependencias obligatorias. Los íconos (salvavidas, altavoz) son SVG en línea y los avatares son emojis nativos, así que no se depende de ningún CDN.

```
baseLunar/
├── index.html        # Pantalla única: 3 bloques + Inicio, Respaldo, Abortada y Victoria   (F1)
├── style.css         # Tokens, layout responsivo, estados, animaciones, impresión          (F1)
├── js/
│   ├── config.js     # Clave NASA, tiempos (timeout 3 s, caché 6 h), banderas de demo      (F0 ✔)
│   ├── levels.js     # Guion: 5 niveles, mensajes de respaldo, fuentes (solo datos)        (F2)
│   ├── nasaData.js   # DONKI: consulta con timeout, validación, caché y respaldo histórico (F0 ✔ datos · F3)
│   ├── game.js       # Motor puro: estado, reglas, respaldos, antibloqueo (sin DOM)        (F2)
│   └── ui.js         # Render, eventos, accesibilidad, voz y arranque (único con DOM)      (F2)
├── tests/
│   └── tests.html    # Pruebas del motor + simulador de balance                            (F2)
├── assets/
│   └── favicon.svg                                                                         (F1)
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

Sin cuentas, sin cookies y sin analítica. El nombre del certificado solo se muestra en pantalla: nunca se guarda, nunca se envía y nunca se lee en voz alta. La única conexión externa es `api.nasa.gov`.

## Créditos

- Ícono salvavidas: [Lucide](https://lucide.dev/) (licencia ISC).
- Tipografía: [Atkinson Hyperlegible](https://github.com/googlefonts/atkinson-hyperlegible), del Braille Institute (SIL Open Font License).
- Datos: NASA DONKI, NASA OSDR / RadLab, NASA y CSA (ver [docs/FUENTES.md](docs/FUENTES.md)).

## Plan y estado

| Fase | Tiempo | Resultado | Estado |
|---|---|---|---|
| F0 · Preparación | 0:30 | Repositorio, documentación, fuentes verificadas o marcadas | En revisión (P0) |
| F1 · Maquetación | 1:30 | index.html y style.css con datos de ejemplo | Pendiente |
| F2 · Motor y pruebas | 2:00 | levels.js, game.js, ui.js y tests.html en verde | Pendiente |
| F3 · NASA DONKI | 1:00 | nasaData.js con timeout, validación, caché y respaldo | Pendiente |
| F4 · Pulido y finales | 1:00 | Inicio, certificado, Misión Abortada, voz, demo | Pendiente |
| F5 · Validación | 1:00 | Matriz de pruebas, Lighthouse, prueba con usuarios | Pendiente |
| F6 · Despliegue | 0:30 | Enlace público, ZIP offline, tag v1.0.0 | Pendiente |

## Documentación del proyecto

- [docs/PROMPT-0.md](docs/PROMPT-0.md): especificación completa.
- [DECISIONES.md](DECISIONES.md): decisiones tomadas y su motivo.
- [docs/FUENTES.md](docs/FUENTES.md): verificación científica.
- [AGENTS.md](AGENTS.md): reglas para agentes de IA.
