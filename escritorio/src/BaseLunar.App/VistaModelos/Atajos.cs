using System.Windows.Input;

namespace BaseLunar.App.VistaModelos;

public enum Atajo
{
    Ninguno,
    OpcionA,
    OpcionB,
    Principal,
    PantallaCompleta,
    SalirPantallaCompleta,
}

/// <summary>
/// Atajos de teclado (5.5): A/B o 1/2 para elegir y Enter para continuar, como alPulsarTecla de ui.js.
/// En escritorio se añaden F11 (pantalla completa) y Esc (salir de ella).
/// </summary>
public static class Atajos
{
    /// <param name="enCampoDeTexto">El foco está en un campo de texto: las letras se escriben, no eligen.</param>
    public static Atajo Interpretar(Key tecla, ModifierKeys modificadores, bool repeticion, bool enCampoDeTexto)
    {
        if (repeticion || enCampoDeTexto) return Atajo.Ninguno;
        if ((modificadores & (ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Windows)) != 0) return Atajo.Ninguno;
        return tecla switch
        {
            Key.A or Key.D1 or Key.NumPad1 => Atajo.OpcionA,
            Key.B or Key.D2 or Key.NumPad2 => Atajo.OpcionB,
            Key.Enter => Atajo.Principal,
            Key.F11 => Atajo.PantallaCompleta,
            Key.Escape => Atajo.SalirPantallaCompleta,
            _ => Atajo.Ninguno,
        };
    }
}
