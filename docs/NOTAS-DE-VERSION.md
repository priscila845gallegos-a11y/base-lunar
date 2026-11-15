# Notas de versión

## v1.0.0 · 15-11-2026

Primera versión completa de **Base Lunar: El Escudo Cósmico**. Es un juego educativo de decisiones para niñas y niños de 7 a 12 años: comandan una base en la Luna durante 5 situaciones reales del espacio y aprenden que cada recurso tiene un costo.

### Cómo jugar

- **Web** (enlace y móvil): <https://priscila845gallegos-a11y.github.io/base-lunar/>. También se puede abrir `index.html` con doble clic en el ZIP del repositorio, sin servidor ni internet. En el Nivel 4 muestra siempre la llamarada X8.7 real del 14-05-2024, con la etiqueta «HISTÓRICO REAL».
- **Windows** (versión completa): se descomprime `BaseLunar-escritorio-v1.0.0-win-x64.zip` y se abre `BaseLunar.exe`. No hace falta instalar nada.
  - Funciona en Windows 10 u 11 de 64 bits.
  - Windows puede avisar con SmartScreen porque el programa no está firmado: hay que pulsar «Más información» y luego «Ejecutar de todas formas».

### Qué trae

- **Las mismas reglas en las dos versiones.**
  - Tres medidores: 🛡️ Protección, 💧 Soporte Vital y ⚡ Batería.
  - Cinco niveles. En los cuatro primeros, cada opción muestra lo que gana y lo que cuesta.
  - Tres Protocolos de Respaldo con mensajes amables. El Nivel 5 se gana leyendo los medidores.
  - Un 💡 Dato real con su fuente al superar cada nivel, y un certificado al final.
- **Windows:**
  - Arte y escena:
    - mapa de progreso con el astronauta;
    - una escena ilustrada por nivel y una animación para cada una de las 10 elecciones;
    - fallos amables, confeti en la Victoria y cápsula de regreso en Misión Abortada.
  - Sonido y voz:
    - efectos de sonido generados por código, con su nivel medido;
    - «Escuchar» con una voz local en español.
  - Contenido:
    - glosario «?» con 6 palabras explicadas y su fuente de la NASA;
    - en el Nivel 4, la llamarada más intensa del último mes, consultada **en vivo** a NASA DONKI, con respaldo histórico si no hay red.
  - Certificado:
    - nombre opcional, que nunca se guarda ni se envía;
    - botón «Imprimir», en una hoja A4 que ahorra tinta.
  - Accesibilidad:
    - partida completa solo con teclado y lector de pantalla (UI Automation);
    - movimiento reducido, alto contraste de Windows y zoom de Windows hasta el 200 %.
  - «Control de Misión»: un error inesperado nunca deja al niño atascado.
  - Opciones de demostración: `--nivel N`, `--offline`, `--debug`, `--silencio` y `--alto-contraste`.

### Privacidad

- Sin cuentas, sin cookies y sin analítica.
- La web no se conecta a nada.
- La versión de Windows solo consulta a la NASA (`api.nasa.gov` y `ccmc.gsfc.nasa.gov`), con las fechas de la consulta y la clave de la API.
- Solo guarda dos cosas en `%LOCALAPPDATA%\BaseLunar`: si los efectos están encendidos y la última consulta a DONKI.

### Calidad

- 202 pruebas automáticas:
  - el motor, el simulador de balance (59 rutas, 0 estados sin salida) y la paridad entre la web y Windows;
  - pantallas, accesibilidad, sonido, voz y NASA DONKI;
  - partidas reales con UI Automation;
  - la web en Edge, Chrome y Firefox.
- Matriz de validación de la sección 10, con evidencias: [docs/VALIDACION.md](VALIDACION.md).
- Cada dato científico tiene su fuente verificada: [docs/FUENTES.md](FUENTES.md).

### Pendiente

Estas tareas necesitan personas (protocolos en [docs/VALIDACION.md](VALIDACION.md)):

- la prueba con 1 o 2 niños;
- la escucha con el Narrador de Windows;
- Lighthouse.

El enlace público se probó en un móvil real (P6 aprobada el 15-11-2026).

### Descargas

| Archivo | Contenido |
|---|---|
| `BaseLunar-escritorio-v1.0.0-win-x64.zip` | El juego para Windows, con .NET incluido, el LEEME y las licencias |
| `BaseLunar-v1.0.0-repositorio.zip` | El repositorio en el tag v1.0.0. La web funciona con doble clic en `index.html` |
| `SHA256SUMS.txt` | La huella SHA-256 de cada ZIP |

Juego educativo independiente, con datos reales de NASA y CSA. No tiene respaldo oficial de ninguna agencia.
