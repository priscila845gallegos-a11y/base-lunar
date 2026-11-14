# Fuentes y verificación científica

Verificación del **14-11-2026** (fase F0). Para cada dato se buscó la fuente, se comprobó que el enlace responde (HTTP 200) y se leyó la frase que lo respalda en la página o en el PDF. Las pruebas se hicieron desde la red de desarrollo; cuando una página no abrió, se indica.

Leyenda:

- ✔ — frase leída en la propia fuente.
- ✔ᵢ — confirmado con el contenido indexado por el buscador; la frase no se pudo leer directamente en la fuente.
- ⚠ PENDIENTE DE VERIFICAR — no se usa en el juego hasta confirmarlo.

## Resumen

| # | Dato | Dónde se usa | Estado |
|---|---|---|---|
| F-01 | La NASA planea una base en la superficie lunar | Inicio | ✔ |
| F-02 | La noche lunar dura casi 15 días terrestres | Nivel 1 | ✔ |
| F-03 | De noche, el suelo baja de 130 °C bajo cero | Nivel 1 | ✔ |
| F-04 | Veggie: la tripulación de la EEI comió lechuga cultivada allí (10-08-2015) | Nivel 2 | ✔ · texto a corregir |
| F-05 | CSA: plantas que alimentan, dan oxígeno y ayudan a reciclar el agua | Nivel 2 | ✔ᵢ |
| F-06 | En la EEI, el oxígeno se fabrica separando el agua | Nivel 2 (opción B) | ✔ |
| F-07 | La Luna no tiene campo magnético global ni atmósfera que proteja | Nivel 3 | ✔ |
| F-08 | El agua protege de la radiación por su hidrógeno | Nivel 3 | ✔ · texto a corregir |
| F-09 | Una capa de regolito sobre la base sirve de escudo | Nivel 3 | ✔ · texto a corregir · «3 a 5 m» ⚠ |
| F-10 | OSDR RadLab reúne mediciones de radiación, también de Artemis I | Nivel 3 (etiqueta) | ✔ |
| F-11 | Refugio de Orión con bolsas de provisiones, hasta 24 h dentro | Nivel 4 | ✔ · texto a corregir |
| F-12 | Llamarada X8.7 del 14-05-2024 (respaldo histórico) | Nivel 4 | ✔ clase, fecha y pico · copia literal de DONKI ⚠ |
| F-13 | 5 llamaradas de clase X en 2 días (14 y 15-05-2024) | Nivel 4 (respaldo) | ⚠ |
| F-14 | Las llamaradas pueden interrumpir las comunicaciones por radio | Nivel 5 | ✔ |
| F-15 | Una señal tarda más de 1 s en ir de la Luna a la Tierra | Nivel 5 | ✔ |
| F-16 | Límites y CORS de api.nasa.gov | Nivel 4 (técnico) | ✔ |
| F-17 | La insignia y el logotipo de la NASA no se pueden usar libremente | Certificado | ✔ |
| F-18 | Licencias: Lucide (ISC) y Atkinson Hyperlegible (SIL OFL) | Créditos | ✔ |

## Detalle

### F-01 · La NASA planea una base en la superficie lunar — ✔

- Fuentes: [SpacePolicyOnline, «NASA Rolls Out New Moon Plan»](https://spacepolicyonline.com/news/nasa-rolls-out-new-moon-plan/) (24-03-2026) y [Al Jazeera, «NASA to spend $20bn on moon base, nuclear-powered Mars spacecraft»](https://www.aljazeera.com/news/2026/3/24/nasa-to-spend-20bn-on-moon-base-nuclear-powered-mars-spacecraft) (24-03-2026).
- Qué dicen: «Isaacman said $20 billion would be spent over the next seven years». Artemis III pasa a ser «an earth-orbiting test flight in 2027, with two lunar landings in 2028, Artemis IV and V». La pausa de Gateway «en su forma actual» la recogen varios medios (✔ᵢ).
- Nota: el Congreso no ha garantizado todo el presupuesto, por eso el Inicio dice «planea». El documento citaba Space.com y The Register; se usan estas dos porque se pudieron leer.

### F-02 · La noche lunar dura casi 15 días terrestres — ✔

- Fuentes: [NASA, «NASA, Department of Energy Join Forces on Innovative Lunar Experiment»](https://www.nasa.gov/solar-system/nasa-department-of-energy-join-forces-on-innovative-lunar-experiment/) y [NASA Science, «Moon Phases»](https://science.nasa.gov/moon/moon-phases/).
- Qué dicen: «LuSEE-Night will operate during the cold temperatures of the 14-day lunar night». El ciclo completo de día y noche «repeats about once a month (every 29.5 days)», así que la noche dura unos 14,8 días: «casi 15» es correcto.

### F-03 · De noche, el suelo baja de 130 °C bajo cero — ✔

- Fuente: [NASA Science, «Moon Facts»](https://science.nasa.gov/moon/facts/).
- Qué dice: «in darkness, the temperatures plummet to about -280 degrees Fahrenheit (-173 degrees Celsius)».
- Nota: −173 °C está por debajo de −130 °C, así que «más de 130 grados bajo cero» es cierto y prudente. La página «Lunar Surface Technology» que citaba el documento no hace falta.

### F-04 · Veggie: lechuga cultivada y comida en la EEI — ✔ · texto a corregir

- Fuentes: [Blog de la Estación Espacial de la NASA (10-08-2015)](https://blogs.nasa.gov/spacestation/2015/08/10/cosmonauts-complete-russian-spacewalk/) y [Live Science (10-08-2015)](https://www.livescience.com/51811-astronauts-eat-space-lettuce.html).
- Qué dicen: Scott Kelly, Kjell Lindgren y Kimiya Yui probaron la lechuga romana roja «Outredgeous» del sistema Veggie el 10-08-2015. La NASA: «This is the first time a station-grown crop has officially been on the menu for station crew members». Live Science: fue la primera vez que la NASA autorizó a sus astronautas a comer lo cultivado, «following in the farming footsteps of earlier Russian cosmonauts, who first ate the leafy greens they cultivated aboard the space station more than a decade ago».
- Corrección: el texto del guion («comieron por primera vez lechuga cultivada en el espacio») exagera, porque los cosmonautas rusos ya lo habían hecho antes. Ver la propuesta T-02 más abajo. El documento citaba el «blog de Kennedy (11-08-2015)»; la fuente localizada es el blog de la Estación del 10-08-2015.

### F-05 · CSA: plantas que alimentan, dan oxígeno y ayudan a reciclar el agua — ✔ᵢ

- Fuentes: [CSA, «Growing healthy food in space and in remote areas»](https://www.asc-csa.gc.ca/eng/sciences/food-production/growing-healthy-food-in-space-and-remote-areas.asp) (la página abrió) y [CSA, actividad «Interplanetary farming» (PDF)](https://www.asc-csa.gc.ca/pdf/eng/youth-educators/activities/fitness-and-nutrition/interplanetary-farming.pdf).
- Qué dicen: la página principal trata de cómo cultivar alimentos sanos en el espacio y en zonas remotas, pero no menciona el agua ni el oxígeno. El PDF de la CSA dice que las plantas de futuras naves podrían dar a la tripulación oxígeno, agua reciclada y comida fresca. Ese texto se leyó en el índice del buscador porque la descarga directa del PDF falló (conexión reiniciada por el servidor de la CSA).
- Nota: es el mismo nivel de evidencia que registró el equipo. Si se quiere ✔ pleno, hay que abrir el PDF desde otra red.

### F-06 · En la EEI, el oxígeno se fabrica separando el agua — ✔ (antes 🔍)

- Fuente: [NASA NTRS 20150016494 (PDF)](https://ntrs.nasa.gov/api/citations/20150016494/downloads/20150016494.pdf), artículo técnico de la NASA sobre el soporte vital de la EEI.
- Qué dice: «The OGA utilizes cathode feed electrolysis to produce both oxygen for the crew to breathe, and hydrogen». También dice que el sistema de agua de la estación entrega agua potable a la tripulación «and the OGS for electrolysis».

### F-07 · La Luna no tiene campo magnético global ni atmósfera que proteja — ✔

- Fuente: [NASA Science, «Solar Wind on the Moon»](https://science.nasa.gov/moon/solar-wind/).
- Qué dice: «Our planet has two layers of protection that the Moon lacks—an atmosphere and a global magnetic field».
- Nota: «La Luna no tiene aire» simplifica que su capa de gases es tan tenue que no protege. Es adecuado para el público. «Weather on the Moon» no hace falta.

### F-08 · El agua protege de la radiación por su hidrógeno — ✔ · texto a corregir

- Fuente: [NASA NTRS 20150001237, «Design of Two RadWorks Storm Shelters for Solar Particle Event Shielding» (PDF)](https://ntrs.nasa.gov/api/citations/20150001237/downloads/20150001237.pdf).
- Qué dice: «shielding materials (particularly those with high concentrations of hydrogen such as polyethylene and water) are effective at reducing astronaut exposure». También: «The majority of the shielding materials: water, packaged food, and hygiene and housekeeping supplies, have significant hydrogen content».
- Corrección: la fuente dice que el agua *reduce* la exposición, no que la *bloquee*. Ver la propuesta T-06.

### F-09 · Una capa de regolito sobre la base sirve de escudo — ✔ · texto a corregir · «3 a 5 m» ⚠

- Fuente: [NASA NTRS 20050215115, «Radiation Protection for Lunar Mission Scenarios»](https://ntrs.nasa.gov/citations/20050215115) ([PDF](https://ntrs.nasa.gov/api/citations/20050215115/downloads/20050215115.pdf)).
- Qué dice: una misión larga «might require using the lunar regolith as a shielding material. This could be accomplished by building the habitat inside a lava tube or by putting regolith on top of the habitat».
- ⚠ PENDIENTE DE VERIFICAR: la cifra «3 a 5 metros» que da el documento **no aparece** en esta fuente, que expresa el blindaje en g/cm². No se usa en el juego. El texto «unos metros de polvo lunar» se cambia por «una capa gruesa» (propuesta T-07).

### F-10 · OSDR RadLab — ✔

- Fuentes: [NASA Science Data, «Data & Software Highlights: May 2026»](https://science.data.nasa.gov/features-events/data-software-highlights-may-2026) y [NASA, «OSDR Environmental Data»](https://www.nasa.gov/reference/osdr-environmental-data).
- Qué dicen: «NASA's Open Science Data Repository (OSDR) released updates to its RadLab radiation visualization portal to include radiation telemetry from the Artemis I mission».
- Nota: el portal visualization.osdr.nasa.gov/radlab devolvió error 500 desde aquí el 14-11-2026. En el juego se enlaza la página «OSDR Environmental Data». Los datos solo se citan y enlazan; no se redistribuyen.

### F-11 · Refugio de Orión — ✔ · texto a corregir

- Fuente: [NASA, «Scientists and Engineers Evaluate Orion Radiation Protection Plan»](https://www.nasa.gov/missions/artemis/orion/scientists-and-engineers-evaluate-orion-radiation-protection-plan/).
- Qué dice: «the crew would create the shelter within an hour and in some cases would need to stay inside for as long as 24 hours. Using the stowage bags on board that will contain supplies, food and water, in combination with Orion's seats…».
- Corrección: es un plan («would»), no un procedimiento ya usado en una tormenta real. Por eso el texto pasa de «arman» a «pueden armar» (propuesta T-08). La página «Crew Systems (Orion)» actualizada el 8-09-2026 que citaba el documento no se localizó; esta fuente de la NASA cubre todo lo que dice el juego.
- Contexto (no se usa en el juego): Artemis II amerizó el 10-04-2026 a las 20:07 EDT ([blog de la NASA](https://www.nasa.gov/blogs/missions/2026/04/10/artemis-ii-flight-day-10-crew-sets-for-final-burn-splashdown/)). Que la demostración del refugio se recortara en esa misión no se verificó, y el juego no lo afirma.

### F-12 · Llamarada X8.7 del 14-05-2024 — ✔ clase, fecha y pico · copia literal ⚠

- Fuentes: [NASA Scientific Visualization Studio, «Largest Flare yet from Solar Cycle 25»](https://svs.gsfc.nasa.gov/14592) y [NOAA NESDIS, «Sun Releases Strong X9.0 Solar Flare»](https://www.nesdis.noaa.gov/news/sun-releases-strong-x90-solar-flare) (04-10-2024).
- Qué dicen: la del 14-05-2024 fue «the largest of Solar Cycle 25», clase X8.7, con pico a las 12:51 p. m. EDT (16:51 UTC). El 03-10-2024 una X9.0 la superó; por eso la sección 7 dice «la más intensa del ciclo solar 25 hasta esa fecha», y está bien dicho.
- ⚠ Pendiente: la copia literal del registro DONKI (ID `2024-05-14T16:46:00-FLR-001`, horas de inicio y fin, S19W90, región 13664, GOES-P EXIS, CME asociada y `link`). El 14-11-2026 no se pudo obtener:
  - `kauai.ccmc.gsfc.nasa.gov` y `webtools.ccmc.gsfc.nasa.gov` (servicio DONKI original, sin clave): el nombre resuelve, pero la conexión al puerto 443 se agota desde esta red.
  - `api.nasa.gov` con `DEMO_KEY`: `429 Too Many Requests` en la primera llamada y otra vez en un segundo intento, minutos después (`X-Ratelimit-Limit: 10`, `X-Ratelimit-Remaining: 0`).
  - Se completa en cuanto haya una clave personal de api.nasa.gov (DECISIONES.md, D-10).

### F-13 · 5 llamaradas de clase X en 2 días — ⚠ PENDIENTE DE VERIFICAR

- Los 5 IDs (X1.7, X1.2, X8.7, X3.4 y X2.9) los aportó el equipo. Se confirman con la misma copia literal de DONKI que F-12. Hasta entonces, la línea «¡En solo 2 días, el Sol lanzó 5 llamaradas de clase X!» no se muestra.

### F-14 · Las llamaradas pueden interrumpir las comunicaciones por radio — ✔ (antes 🔍)

- Fuente: [NOAA SWPC, «Solar Flares (Radio Blackouts)»](https://www.swpc.noaa.gov/phenomena/solar-flares-radio-blackouts).
- Qué dice: la página de la NOAA trata los apagones de radio causados por llamaradas y su efecto en las comunicaciones de radio de alta frecuencia (HF). Ocurre en el lado de la Tierra iluminado por el Sol.
- Nota: en el juego, la «estática» de la radio en la Luna es licencia narrativa inspirada en este efecto. El dato real solo dice que las llamaradas pueden interrumpir la radio.

### F-15 · Una señal tarda más de 1 s en ir de la Luna a la Tierra — ✔ (antes 🔍)

- Fuente: [ESA, «Ten things you didn't know about the Moon – Moon calling Earth»](https://www.esa.int/ESA_Multimedia/Images/2021/09/Ten_things_you_didn_t_know_about_the_Moon_-_Moon_calling_Earth).
- Qué dice: «It takes on average 1.27 seconds for a radio signal to travel from Moon to Earth».
- Apoyo: el [Apollo Lunar Surface Journal de la NASA (Apolo 12)](https://www.nasa.gov/history/alsj/a12/A12Audio_Moon_Houston.html) usa un retraso de unos 1,3 s (✔ᵢ). La ESA no es la NASA ni la NOAA, como pedía el documento, pero es una agencia espacial y da la cifra exacta.

### F-16 · api.nasa.gov — ✔

- Fuente: [api.nasa.gov](https://api.nasa.gov/) (documentación de autenticación y límites).
- Qué dice: 1 000 solicitudes por hora con clave personal; DEMO_KEY, 30 por hora y 50 al día por IP.
- Prueba real (14-11-2026): DEMO_KEY respondió `429` en la primera llamada, con `X-Ratelimit-Limit: 10`, y la respuesta incluía `Access-Control-Allow-Origin: *`.

### F-17 · Marca de la NASA — ✔

- Fuente: [NASA, «Images and Media» (pautas de uso)](https://www.nasa.gov/nasa-brand-center/images-and-media/).
- Qué dice: «The NASA Insignia, Logotype, identifiers, and imagery are not in the public domain. The use of the Insignia, Logotype and NASA identifiers is protected by law».
- Consecuencia: sin logos de la NASA ni de la CSA y sin títulos «de la NASA». Atribución: «Datos de NASA / CSA».

### F-18 · Licencias — ✔

- [Lucide, licencia ISC](https://lucide.dev/license): ícono salvavidas (life-buoy) copiado como SVG en línea.
- [Atkinson Hyperlegible (Braille Institute)](https://github.com/googlefonts/atkinson-hyperlegible): «free for anyone to use, under the SIL Open Font License».

## Fuentes del documento que no se usan

| Fuente citada en el Prompt 0 | Motivo |
|---|---|
| Space.com, The Register (base lunar) | Sustituidas por SpacePolicyOnline y Al Jazeera, que se pudieron leer. |
| NASA Lunar Surface Technology (−130 °C) | Basta con NASA Moon Facts (−173 °C). |
| NASA Weather on the Moon | Basta con «Solar Wind on the Moon». |
| Blog de Kennedy (11-08-2015) | Se usa el blog de la Estación del 10-08-2015, que tiene la frase exacta. |
| NASA Crew Systems (Orion), 8-09-2026 | No se localizó; la fuente de la NASA de F-11 cubre el dato. |
| SpaceWeatherLive (X8.7) | Se usan la NASA y la NOAA; los IDs se confirmarán con DONKI. |

## Textos del guion que se corrigen en F2

Propuesta para revisar en la puerta P2. Se corrige lo que rompe las reglas de texto del guion (frases de 15 palabras o menos) o dice más que su fuente. Las cuentas de palabras incluyen los paréntesis.

| # | Dónde | Texto actual | Problema | Propuesta |
|---|---|---|---|---|
| T-01 | Historia N2 | «Las plantas del invernadero de la base necesitan agua purificada para crecer y dar vitaminas frescas a los astronautas.» | 19 palabras en una frase | «Las plantas del invernadero necesitan agua purificada para crecer. Así dan vitaminas frescas a los astronautas.» (9 + 7) |
| T-02 | Dato N2 | «El 10 de agosto de 2015, astronautas de la Estación Espacial Internacional comieron por primera vez lechuga cultivada en el espacio (experimento Veggie de la NASA).» | 26 palabras; «por primera vez» exagera (F-04) | «El 10 de agosto de 2015, la tripulación de la Estación Espacial comió su lechuga. La habían cultivado allí, en el experimento Veggie de la NASA. ¡Fue la primera vez que la NASA lo permitió!» (15 + 11 + 9) |
| T-03 | Dato N2 (CSA) | «La Agencia Espacial Canadiense estudia plantas que, además de alimentar, ayudan a limpiar el agua y a producir oxígeno.» | 19 palabras | «La Agencia Espacial Canadiense también estudia plantas para el espacio. Además de alimentar, pueden dar oxígeno y ayudar a reciclar el agua.» (10 + 12) |
| T-04 | Dato N5 | «¡Y una señal tarda más de 1 segundo en viajar de la Luna a la Tierra!» | 16 palabras | «¡Una señal tarda más de 1 segundo en ir de la Luna a la Tierra!» (15) |
| T-05 | Mensaje ⚡ (3.6) | «En el espacio, sin electricidad todo se congela en minutos.» | «en minutos» no tiene fuente | «Sin electricidad, la base se enfría muchísimo.» |
| T-06 | Dato N3 | «El agua bloquea la radiación espacial porque tiene mucho hidrógeno.» | La fuente dice «reduce» (F-08) | «El agua ayuda a frenar la radiación espacial porque tiene mucho hidrógeno.» (12) |
| T-07 | Dato N3 | «¡Y unos metros de polvo lunar sobre la base también sirven de escudo!» | «Unos metros» no está en la fuente (F-09) | «¡Y una capa gruesa de polvo lunar sobre la base también sirve de escudo!» (14) |
| T-08 | Dato N4 | «En la nave Orión, los astronautas arman un refugio con sus bolsas de provisiones.» | Es un plan, no un procedimiento ya usado (F-11) | «En la nave Orión, los astronautas pueden armar un refugio con sus bolsas de provisiones.» (15) |
