using System.Windows;
using System.Windows.Controls;

namespace BaseLunar.App.Vistas;

/// <summary>Tarjeta de misión: Inicio, nivel (con resultado o respaldo), Misión Abortada y certificado.</summary>
public partial class TarjetaMision : UserControl
{
    public TarjetaMision()
    {
        InitializeComponent();
    }

    /// <summary>Al cambiar de nivel o de pantalla: la tarjeta vuelve arriba y el foco va al título (D-28).</summary>
    public void EnfocarTitulo(Fase fase)
    {
        Desplazamiento.ScrollToTop();
        TituloDe(fase).Focus();
    }

    /// <summary>Tras decidir: muestra el resultado o el aviso de respaldo.</summary>
    public void MostrarSeccion(Fase fase)
    {
        FrameworkElement seccion = fase == Fase.Respaldo ? AvisoRespaldo : Resultado;
        seccion.UpdateLayout();
        seccion.BringIntoView();
    }

    /// <summary>El título de la vista de cada fase.</summary>
    public TextBlock TituloDe(Fase fase) => fase switch
    {
        Fase.Inicio => TituloInicio,
        Fase.Victoria => TituloVictoria,
        Fase.MisionAbortada => TituloAbortada,
        _ => TituloNivel,
    };
}
