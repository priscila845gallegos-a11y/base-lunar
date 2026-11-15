using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace BaseLunar.App.Controles;

/// <summary>
/// Texto solo visual, como aria-hidden en la web: el número flotante o las cifras dentro de un botón cuyo
/// nombre accesible ya lo dice todo. No aparece en el árbol de accesibilidad.
/// </summary>
public sealed class TextoDecorativo : TextBlock
{
    protected override AutomationPeer? OnCreateAutomationPeer() => null;
}
