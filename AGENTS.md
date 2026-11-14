# AGENTS.md — Reglas para agentes de IA

Este archivo es para cualquier agente de IA que trabaje en este repositorio. Léelo entero antes de tocar nada.

## Antes de empezar

1. Lee la especificación: [docs/PROMPT-0.md](docs/PROMPT-0.md). Manda sobre cualquier otra idea.
2. Lee [DECISIONES.md](DECISIONES.md). Las decisiones registradas ya están tomadas: no las reabras sin que el equipo lo pida.
3. Lee [docs/FUENTES.md](docs/FUENTES.md) antes de escribir o cambiar cualquier texto con datos.

## Prioridades ante un conflicto (en este orden)

1. Seguridad y bienestar infantil: lenguaje amable, nada de miedo ni culpa, ningún dato personal sale del navegador.
2. Honestidad científica: todo dato tiene su fuente, y «EN VIVO» se usa solo cuando de verdad lo es.
3. Jugabilidad sin bloqueos: nunca puede existir un estado del que el jugador no pueda salir.
4. Accesibilidad y responsividad.
5. Estética y extras.

## Forma de trabajo

- Se trabaja por fases (F0 a F6), cada una con su puerta de calidad (P0 a P6). No se pasa a la siguiente fase sin el «Aprobado» del equipo.
- Al menos un commit por fase, en formato Conventional Commits y en español: `feat`, `fix`, `docs`, `test`, `style`, `refactor`, `chore`.
- Si surge una decisión que la especificación no resuelve, elige la opción más simple y anótala en DECISIONES.md (fecha · decisión · motivo).
- Nunca inventes datos científicos. Lo que no puedas verificar se marca como «PENDIENTE DE VERIFICAR» en docs/FUENTES.md, no se muestra en el juego y se avisa al equipo.
- No hagas push, ni publiques, ni crees repositorios remotos sin permiso explícito.

## Arquitectura (resumen de la sección 6)

- HTML + CSS + JavaScript ES6+ puros. Sin frameworks, sin paso de compilación y sin dependencias obligatorias.
- Scripts clásicos con `defer`, en este orden: config → levels → nasaData → game → ui. Nada de módulos ES: el juego tiene que abrir con doble clic (file://) y desde el ZIP.
- Un único global: `window.BaseLunar`. Ninguna otra variable global.
- `js/game.js`: solo funciones puras. Nada de DOM, red ni almacenamiento.
- `js/ui.js`: el único archivo que toca el DOM. Usa `textContent` y nunca `innerHTML` con datos externos.
- `js/nasaData.js`: expone una promesa que siempre se resuelve, nunca falla.
- `js/levels.js` y `js/config.js`: solo datos. Ningún número mágico vive fuera de ellos.

## Estándares de código (sección 8)

- `'use strict'`, `const`/`let`, funciones de 30 líneas como máximo.
- Nombres en español y en camelCase (`tomarDecision`); constantes en MAYÚSCULAS; JSDoc breve en las funciones públicas del motor.
- CSP en una etiqueta meta: `connect-src` limitado a `https://api.nasa.gov`. Nada de Google Fonts: la fuente está en el proyecto (D-12).
- HTML que pase el validador del W3C. Cero errores en consola, tanto en el flujo normal como en el respaldo.
- `localStorage` siempre dentro de try/catch, y solo para el silencio, el récord y la caché de DONKI.

## Contenido para niños

- Español neutro, frases de 15 palabras o menos, historia de 150 caracteres como máximo y palabras técnicas explicadas entre paréntesis.
- Tono amable: nada de miedo ni culpa. Al fallar, la tripulación siempre está a salvo.
- «📡 Dato EN VIVO» solo con datos de la API o de su caché de menos de 6 h. Con el respaldo, «📡 Dato HISTÓRICO REAL». Nunca se muestran errores técnicos al niño.
- Sin logos de NASA ni de CSA, sin títulos «de la NASA» y sin frases que sugieran un respaldo oficial. La atribución es «Datos de NASA / CSA».
- No llamar a la base «Base Artemis» ni usar nombres de programas oficiales.

## Privacidad

- Ningún dato del niño sale del navegador. Sin cuentas, analítica, cookies ni servicios de terceros; la única conexión externa es api.nasa.gov.
- El nombre del certificado vive solo en memoria y nunca se lee en voz alta (D-13, D-17).

## Pruebas

- `tests/tests.html` tiene que estar al 100 % en verde antes de cualquier commit que toque `js/`.
- El simulador de balance tiene que confirmar 0 estados sin salida. Referencia con las cifras v2, idéntica con y sin clase X: 59 rutas, 45 victorias (16 con 3 respaldos, 13 con 2 y 16 con 1) y 14 Misiones Abortadas.
- Si cambias alguna cifra de `js/levels.js`, vuelve a ejecutar el simulador y anota el cambio en DECISIONES.md.

## Fuera de alcance

Backend, cuentas, ranking en línea, analítica, cookies, varios idiomas, frameworks y paso de compilación.
