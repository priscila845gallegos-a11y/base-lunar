using System.Windows;

namespace BaseLunar.App.Controles;

/// <summary>
/// Disposición compacta (ventanas estrechas), como propiedad heredada: la pantalla la fija una vez y
/// los estilos de Temas/Controles.xaml reaccionan con un Trigger (insignias y títulos más pequeños).
/// </summary>
public static class Disposicion
{
    public static readonly DependencyProperty CompactaProperty = DependencyProperty.RegisterAttached(
        "Compacta", typeof(bool), typeof(Disposicion),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    public static bool GetCompacta(DependencyObject elemento) => (bool)elemento.GetValue(CompactaProperty);

    public static void SetCompacta(DependencyObject elemento, bool valor) => elemento.SetValue(CompactaProperty, valor);
}
