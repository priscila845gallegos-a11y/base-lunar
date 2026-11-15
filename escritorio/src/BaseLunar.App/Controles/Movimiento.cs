using System.Windows;

namespace BaseLunar.App.Controles;

/// <summary>
/// Preferencia de movimiento reducido: la opción de Windows «Mostrar animaciones» (5.4). Con movimiento
/// reducido, los cambios se aplican sin desplazamientos ni temblores.
/// </summary>
public static class Movimiento
{
    /// <summary>Las pruebas de pantalla lo fuerzan para capturar el estado final sin esperar animaciones.</summary>
    internal static bool Forzado { get; set; }

    public static bool Reducido => Forzado || !SystemParameters.ClientAreaAnimation;
}
