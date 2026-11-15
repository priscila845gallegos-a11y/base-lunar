using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BaseLunar.App.Controles;

/// <summary>
/// Visible si el valor es «verdadero»: true, un texto no vacío, una lista con elementos o cualquier objeto
/// no nulo. Con <see cref="Invertir"/>, al revés.
/// </summary>
public sealed class Visible : IValueConverter
{
    public bool Invertir { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var verdadero = value switch
        {
            null => false,
            bool b => b,
            string s => s.Length > 0,
            ICollection c => c.Count > 0,
            _ => true,
        };
        return verdadero != Invertir ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Emoji → geometría del ícono vectorial (ver <see cref="Iconografia"/>).</summary>
public sealed class IconoDeEmoji : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Iconografia.GeometriaDe(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Clave de recurso → recurso (un pincel, una geometría…).</summary>
public sealed class Recurso : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string clave ? Application.Current?.TryFindResource(clave) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
