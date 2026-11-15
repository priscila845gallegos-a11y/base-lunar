// Las pruebas de pantalla comparten un único hilo de interfaz y las de UI Automation abren ventanas:
// se ejecutan de una en una.
[assembly: DoNotParallelize]
