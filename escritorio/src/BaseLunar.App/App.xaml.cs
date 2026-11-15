using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace BaseLunar.App;

/// <summary>Aplicación de escritorio de Base Lunar (D-32).</summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Solo al ejecutar el juego: en las pruebas, un error tiene que verse.
        DispatcherUnhandledException += AlFallar;
        base.OnStartup(e);
    }

    /// <summary>
    /// Un error inesperado nunca debe dejar al niño atascado (prioridad 3): se registra y la partida sigue.
    /// F5 añade la pantalla «Control de Misión» para volver al último estado seguro.
    /// </summary>
    private void AlFallar(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Debug.WriteLine("Base Lunar · error inesperado: " + e.Exception);
        e.Handled = true;
    }
}
