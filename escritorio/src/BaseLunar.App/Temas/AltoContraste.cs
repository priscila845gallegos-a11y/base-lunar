using System.Windows;
using System.Windows.Media;

namespace BaseLunar.App.Temas;

/// <summary>Los cinco colores de un tema de alto contraste de Windows.</summary>
public sealed record ColoresContraste(Color Fondo, Color Texto, Color Resalte, Color TextoResalte, Color Enlace)
{
    /// <summary>Los colores del tema de alto contraste activo en Windows.</summary>
    public static ColoresContraste DelSistema() =>
        new(SystemColors.WindowColor, SystemColors.WindowTextColor, SystemColors.HighlightColor, SystemColors.HighlightTextColor, SystemColors.HotTrackColor);

    /// <summary>Un tema fijo (negro, blanco y amarillo, como «Noche» de Windows) para --alto-contraste y las pruebas.</summary>
    public static ColoresContraste Noche { get; } = new(Colors.Black, Colors.White, Color.FromRgb(0xFF, 0xFF, 0x00), Colors.Black, Color.FromRgb(0x75, 0xE9, 0xFC));
}

/// <summary>
/// Modo de alto contraste (D-56): si Windows lo tiene activado, la interfaz usa sus colores. Todas las vistas
/// toman los pinceles de la paleta con DynamicResource, así que basta con añadir un diccionario que los
/// redefine: fondos al color de ventana, textos y bordes al de texto, botones al de resalte y enlaces al de
/// hipervínculo. La escena es decorativa y conserva sus colores. Quitar el diccionario devuelve la paleta.
/// </summary>
public static class AltoContraste
{
    private static readonly string[] Fondos = ["Pincel.Fondo", "Pincel.Superficie", "Pincel.SuperficieTranslucida", "Pincel.FondoEfecto", "Pincel.Pista",
        "Pincel.Espacio", "Pincel.CieloAlerta"];
    private static readonly string[] Textos = ["Pincel.Texto", "Pincel.TextoSuave", "Pincel.Borde", "Pincel.BordeControl", "Pincel.BordeBoton",
        "Pincel.Exito", "Pincel.Costo", "Pincel.Estrella", "Pincel.Proteccion", "Pincel.Soporte", "Pincel.Bateria"];
    private static readonly string[] Resaltes = ["Pincel.Primario", "Pincel.Acento"];

    /// <summary>Añade los colores de alto contraste encima de la paleta y devuelve el diccionario añadido.</summary>
    public static ResourceDictionary Aplicar(ResourceDictionary recursos, ColoresContraste colores)
    {
        var contraste = new ResourceDictionary();
        foreach (var clave in Fondos) contraste[clave] = Pincel(colores.Fondo);
        foreach (var clave in Textos) contraste[clave] = Pincel(colores.Texto);
        foreach (var clave in Resaltes) contraste[clave] = Pincel(colores.Resalte);
        contraste["Pincel.TextoOscuro"] = Pincel(colores.TextoResalte);
        contraste["Pincel.Enlace"] = Pincel(colores.Enlace);
        recursos.MergedDictionaries.Add(contraste);
        return contraste;
    }

    /// <summary>Vuelve a la paleta normal.</summary>
    public static void Restaurar(ResourceDictionary recursos, ResourceDictionary contraste) => recursos.MergedDictionaries.Remove(contraste);

    private static SolidColorBrush Pincel(Color color)
    {
        var pincel = new SolidColorBrush(color);
        pincel.Freeze();
        return pincel;
    }
}
