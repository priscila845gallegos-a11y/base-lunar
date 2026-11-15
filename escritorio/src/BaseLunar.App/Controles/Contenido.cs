using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.App.Controles;

/// <summary>
/// Propiedad adjunta para TextBlock: <c>c:Contenido.Texto="{Binding …}"</c> pinta el texto con sus emojis
/// cambiados por íconos vectoriales y pone como nombre accesible el texto sin emojis.
/// </summary>
public static class Contenido
{
    public static readonly DependencyProperty TextoProperty = DependencyProperty.RegisterAttached(
        "Texto", typeof(string), typeof(Contenido), new PropertyMetadata(null, AlCambiarTexto));

    public static string? GetTexto(DependencyObject elemento) => (string?)elemento.GetValue(TextoProperty);

    public static void SetTexto(DependencyObject elemento, string? valor) => elemento.SetValue(TextoProperty, valor);

    private static void AlCambiarTexto(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock bloque) return;
        var texto = e.NewValue as string ?? string.Empty;
        bloque.Inlines.Clear();
        foreach (var trozo in Iconografia.Partir(texto))
        {
            if (trozo.Icono is null)
            {
                bloque.Inlines.Add(new Run(trozo.Texto));
                continue;
            }
            var icono = new Icono();
            icono.SetResourceReference(Icono.GeometriaProperty, trozo.Icono);
            if (trozo.Pincel is not null) icono.SetResourceReference(Icono.ForegroundProperty, trozo.Pincel);
            bloque.Inlines.Add(new InlineUIContainer(icono) { BaselineAlignment = BaselineAlignment.Center });
        }
        AutomationProperties.SetName(bloque, Iconografia.SinIconos(texto));
    }

    /// <summary>
    /// <c>c:Contenido.Fuentes="{Binding Dato}"</c> pinta «Fuentes: A · B» con un enlace por fuente, en un
    /// solo párrafo que se parte en líneas como en la web (pintarFuentes de ui.js).
    /// </summary>
    public static readonly DependencyProperty FuentesProperty = DependencyProperty.RegisterAttached(
        "Fuentes", typeof(DatoVM), typeof(Contenido), new PropertyMetadata(null, AlCambiarFuentes));

    public static DatoVM? GetFuentes(DependencyObject elemento) => (DatoVM?)elemento.GetValue(FuentesProperty);

    public static void SetFuentes(DependencyObject elemento, DatoVM? valor) => elemento.SetValue(FuentesProperty, valor);

    private static void AlCambiarFuentes(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock bloque) return;
        bloque.Inlines.Clear();
        if (e.NewValue is not DatoVM dato) return;
        bloque.Inlines.Add(new Run(dato.EtiquetaFuentes + " "));
        for (var i = 0; i < dato.Fuentes.Count; i++)
        {
            if (i > 0) bloque.Inlines.Add(new Run(" · "));
            var fuente = dato.Fuentes[i];
            var enlace = new Hyperlink(new Run(fuente.Nombre)) { NavigateUri = new Uri(fuente.Url) };
            AutomationProperties.SetName(enlace, fuente.NombreAccesible);
            bloque.Inlines.Add(enlace);
        }
    }
}
