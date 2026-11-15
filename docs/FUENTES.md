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
| F-12 | Llamarada X8.7 del 14-05-2024 (respaldo histórico) | Nivel 4 | ✔ |
| F-13 | 5 llamaradas de clase X en 2 días (14 y 15-05-2024) | Nivel 4 (respaldo) | ✔ |
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

### F-12 · Llamarada X8.7 del 14-05-2024 — ✔

- Fuentes: [NASA DONKI, registro de la llamarada](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30849/-1), con copia literal en `js/nasaData.js` descargada el 14-11-2026 del [servicio web de DONKI](https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR?startDate=2024-05-14&endDate=2024-05-15); [NASA Scientific Visualization Studio, «Largest Flare yet from Solar Cycle 25»](https://svs.gsfc.nasa.gov/14592); y [NOAA NESDIS, «Sun Releases Strong X9.0 Solar Flare»](https://www.nesdis.noaa.gov/news/sun-releases-strong-x90-solar-flare) (04-10-2024).
- Qué dice el registro: `2024-05-14T16:46:00-FLR-001`, clase X8.7, inicio 16:46Z, pico 16:51Z, fin 17:02Z, ubicación S19W90, región activa 13664, instrumento «GOES-P: EXIS 1.0-8.0» y CME asociada `2024-05-14T17:00:00-CME-001`. Todo coincide con la sección 7 del Prompt 0.
- La NASA la llamó «the largest of Solar Cycle 25». El 03-10-2024 una X9.0 la superó; por eso la sección 7 dice «la más intensa del ciclo solar 25 hasta esa fecha», y está bien dicho.
- Nota técnica del 14-11-2026, que afecta a F3:
  - Con la clave personal, `api.nasa.gov/DONKI/FLR` responde `404`. La pasarela reenvía a `/DONKI-API/get/FLR/` y el servidor de origen contesta 404. Los endpoints CME y notifications sí funcionan.
  - DONKI se mudó a `ccmc.gsfc.nasa.gov/DONKI`; el servidor antiguo, kauai, no responde. El servicio nuevo entrega el mismo JSON sin clave y con `Access-Control-Allow-Origin: *`.
  - Ver DECISIONES.md, D-18 y D-19.

### F-13 · 5 llamaradas de clase X en 2 días — ✔

- Fuente: el mismo registro de DONKI que F-12. El 14 y 15-05-2024 hubo 11 llamaradas: 5 de clase X, 5 de clase M y 1 de clase C.
- Las 5 de clase X coinciden con los IDs del Prompt 0:
  - X1.7 · `2024-05-14T02:03:00-FLR-001`
  - X1.2 · `2024-05-14T12:40:00-FLR-001`
  - X8.7 · `2024-05-14T16:46:00-FLR-001`
  - X3.4 · `2024-05-15T08:18:00-FLR-001`
  - X2.9 · `2024-05-15T14:20:00-FLR-001`
- La línea «¡En solo 2 días, el Sol lanzó 5 llamaradas de clase X!» es correcta y se puede mostrar en modo respaldo.

### F-14 · Las llamaradas pueden interrumpir las comunicaciones por radio — ✔ (antes 🔍)

- Fuentes: [NASA, «Strong Solar Flare»](https://www.nasa.gov/image-article/strong-solar-flare/), que es la que enlaza el juego, y [NOAA SWPC, «Solar Flares (Radio Blackouts)»](https://www.swpc.noaa.gov/phenomena/solar-flares-radio-blackouts).
- Qué dicen: la NASA, «Solar flares are powerful bursts of energy that can, along with other types of solar eruptions, can impact radio communications». La NOAA trata los apagones de radio de alta frecuencia (HF) que causan las llamaradas en el lado de la Tierra iluminado por el Sol.
- Nota: en el juego, la «estática» de la radio en la Luna es licencia narrativa inspirada en este efecto. El dato real solo dice que las llamaradas pueden interrumpir la radio.

### F-15 · Una señal tarda más de 1 s en ir de la Luna a la Tierra — ✔ (antes 🔍)

- Fuentes: [NASA, «Voyager 1: What Is a Light-Day»](https://science.nasa.gov/mission/voyager/voyager-1/voyager-1-what-is-a-light-day/), que es la que enlaza el juego, y [NASA, «How Does Webb See Back in Time?»](https://science.nasa.gov/mission/webb/science-overview/science-explainers/how-does-webb-see-back-in-time/).
- Qué dicen: la luz recorre la distancia Luna–Tierra «in 1.3 seconds»; la luz de la Luna llega «1.3 seconds old».
- Apoyo: [ESA, «Moon calling Earth»](https://www.esa.int/ESA_Multimedia/Images/2021/09/Ten_things_you_didn_t_know_about_the_Moon_-_Moon_calling_Earth): «It takes on average 1.27 seconds for a radio signal to travel from Moon to Earth».
- Nota: con las dos fuentes de la NASA (F-14 y F-15), la etiqueta «📡 Dato NASA: Comunicaciones» del Nivel 5 es correcta.

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
| SpaceWeatherLive (X8.7) | Se usan el registro de DONKI, la NASA y la NOAA. |

## Textos del guion corregidos en F2

Aplicados en `js/levels.js` en F2, pendientes de revisión en la puerta P2. Se corrige lo que rompe las reglas de texto del guion (frases de 15 palabras o menos) o dice más que su fuente. Las cuentas de palabras incluyen los paréntesis. `tests/tests.html` comprueba en cada ejecución que todos los textos del juego cumplen las reglas.

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

## Glosario de escritorio (F3.3)

Verificación del **15-11-2026**. Se leyó cada página (HTTP 200) y se copió la frase que respalda la explicación. Las explicaciones están en `escritorio/src/BaseLunar.App/VistaModelos/Glosario.cs` (D-45).

| # | Palabra | Explicación en el juego | Niveles |
|---|---|---|---|
| G-01 | soporte vital | Son las máquinas que dan agua limpia y oxígeno para respirar. | 1 y 2 |
| G-02 | rayos cósmicos | No son rayos de luz: son partículas con mucha energía. Viajan por el espacio casi a la velocidad de la luz. | 3 |
| G-03 | campo magnético | Es un escudo invisible de la Tierra. Desvía casi todo el material que llega del Sol. El de la Luna es muy débil. | 3 |
| G-04 | regolito | Es el polvo gris y las piedritas que cubren casi toda la Luna. | 3 |
| G-05 | llamarada solar | Es un destello muy fuerte de luz y radiación en el Sol. | 4 y 5 |
| G-06 | clase X | Así se llaman las llamaradas más fuertes. De menor a mayor, las clases son B, C, M y X. | 4 |

### G-01 · Soporte vital — ✔

- Fuente: [NASA, «Environmental Control and Life Support Systems (ECLSS)»](https://www.nasa.gov/reference/environmental-control-and-life-support-systems-eclss/).
- Qué dice: «The Water Recovery System provides clean water by reclaiming wastewater» y «The Oxygen Generation System produces oxygen for the crew to breathe».

### G-02 · Rayos cósmicos — ✔

- Fuente: [NASA Goddard, Imagine the Universe, «Cosmic Rays - Introduction»](https://imagine.gsfc.nasa.gov/science/toolbox/cosmic_rays1.html).
- Qué dice: «They are high energy particles that move through space at nearly the speed of light.»

### G-03 · Campo magnético — ✔

- Fuentes: [NASA Science, «Earth's Magnetosphere»](https://science.nasa.gov/science-research/planetary-science/earths-magnetosphere/) y [NASA Science, «Moon Facts»](https://science.nasa.gov/moon/facts/).
- Qué dicen: «It deflects most of the solar material sweeping towards us from our star» y «The magnetosphere is a permeable shield». Sobre la Luna: «today, the Moon has a very weak magnetic field».
- Nota: por eso la explicación dice «desvía casi todo», no «bloquea». Coincide con F-07.

### G-04 · Regolito — ✔

- Fuente: [NASA Science, «Moon Facts»](https://science.nasa.gov/moon/facts/).
- Qué dice: «Nearly the entire Moon is covered by a rubble pile of charcoal-gray, powdery dust, and rocky debris» … «called the lunar regolith».

### G-05 · Llamarada solar — ✔

- Fuente: [NASA SVS, «Largest Flare yet from Solar Cycle 25»](https://svs.gsfc.nasa.gov/14592).
- Qué dice: «A solar flare is an intense burst of radiation, or light, on the Sun.»

### G-06 · Clase X — ✔

- Fuente: [NASA SVS, «X-Class: A Guide to Solar Flares»](https://svs.gsfc.nasa.gov/10109).
- Qué dice: «The smallest ones are B-class, followed by C, M and X, the largest.» La página 14592 añade: «X-class denotes the most intense flares».

### Palabras que no se usan

- «alunizar» y «baliza»: el Diccionario de la RAE (dle.rae.es) respondió HTTP 403 a la consulta automática. No se pudieron leer las definiciones, así que no se muestran en el juego (⚠ PENDIENTE DE VERIFICAR).

## Consulta en vivo de NASA DONKI (F3.5)

Prueba real del **15-11-2026** desde la aplicación de escritorio (D-54), con el rango del 16-10-2026 al 15-11-2026:

- `https://api.nasa.gov/DONKI/FLR` (clave personal): HTTP 404, como en F0 (D-19).
- `https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR` (sin clave): lista válida. La más intensa es una **M3.5** con pico el 05-11-2026 a las 18:33 UTC ([registro DONKI](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/49391/-1)). No hubo ninguna de clase X, así que el juego no aplica la penalización X y el 💡 lo aclara con «Las de clase X son todavía más fuertes.».
- Coincide con lo anotado en D-09 el 14-11-2026: la más intensa del mes era una M3.5 del 05-11.
