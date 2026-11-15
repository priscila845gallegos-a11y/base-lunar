using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BaseLunar.App.Vistas;

/// <summary>El certificado preparado para papel A4: lo usa «Imprimir» (Could de 3.7).</summary>
public partial class CertificadoImpresion : UserControl
{
    /// <summary>Tamaño de una hoja A4 a 96 DIP.</summary>
    public static readonly Size Hoja = new(794, 1123);

    public CertificadoImpresion()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Abre el diálogo de impresión de Windows (impresora o «Microsoft Print to PDF») e imprime el
    /// certificado ajustado al área imprimible. Si se cancela o falla, no pasa nada.
    /// </summary>
    public static void Imprimir(object modelo, string titulo)
    {
        try
        {
            var dialogo = new PrintDialog();
            if (dialogo.ShowDialog() != true) return;
            var area = new Size(dialogo.PrintableAreaWidth, dialogo.PrintableAreaHeight);
            var escala = Math.Min(area.Width / Hoja.Width, area.Height / Hoja.Height);
            var hoja = new CertificadoImpresion { DataContext = modelo, LayoutTransform = new ScaleTransform(escala, escala) };
            hoja.Measure(area);
            hoja.Arrange(new Rect(area));
            hoja.UpdateLayout();
            dialogo.PrintVisual(hoja, titulo);
        }
        catch (Exception error)
        {
            System.Diagnostics.Debug.WriteLine("Base Lunar · no se pudo imprimir: " + error.Message);
        }
    }
}
