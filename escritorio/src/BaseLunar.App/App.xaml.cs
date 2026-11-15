using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using BaseLunar.App.Temas;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.App;

/// <summary>Aplicación de escritorio de Base Lunar (D-32).</summary>
public partial class App : Application
{
    private ResourceDictionary? contraste;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Solo al ejecutar el juego: en las pruebas, un error tiene que verse.
        DispatcherUnhandledException += AlFallar;
        // Alto contraste (D-56): al abrir y cada vez que Windows lo active o lo desactive.
        AplicarContraste();
        SystemParameters.StaticPropertyChanged += (_, cambio) =>
        {
            if (cambio.PropertyName == nameof(SystemParameters.HighContrast)) Dispatcher.BeginInvoke(AplicarContraste);
        };
        base.OnStartup(e);
    }

    /// <summary>Colores de alto contraste de Windows (o los de --alto-contraste), o la paleta normal.</summary>
    private void AplicarContraste()
    {
        if (contraste is not null) AltoContraste.Restaurar(Resources, contraste);
        contraste = SystemParameters.HighContrast ? AltoContraste.Aplicar(Resources, ColoresContraste.DelSistema())
            : Opciones.DelProceso().AltoContraste ? AltoContraste.Aplicar(Resources, ColoresContraste.Noche)
            : null;
    }

    /// <summary>
    /// Un error inesperado nunca debe dejar al niño atascado (prioridad 3, D-57): se registra, aparece
    /// «Control de Misión» y la partida sigue desde el último estado seguro.
    /// </summary>
    private void AlFallar(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        ManejarError(e.Exception, MainWindow?.DataContext as JuegoVM);
    }

    /// <summary>Registra el error (sin mostrar detalles al niño) y muestra «Control de Misión». Nunca lanza.</summary>
    internal static void ManejarError(Exception error, JuegoVM? juego)
    {
        Debug.WriteLine("Base Lunar · error inesperado: " + error);
        try
        {
            juego?.Registrar("Error: " + error.GetType().Name + ": " + error.Message);
            juego?.MostrarControlMision();
        }
        catch (Exception otro)
        {
            Debug.WriteLine("Base Lunar · no se pudo mostrar Control de Misión: " + otro.Message);
        }
    }
}
