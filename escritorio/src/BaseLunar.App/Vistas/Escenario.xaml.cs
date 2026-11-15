using System.Windows.Controls;
using BaseLunar.App.Animacion;
using BaseLunar.App.VistaModelos;

namespace BaseLunar.App.Vistas;

/// <summary>La zona de la escena: mapa y escena del nivel, dirigidos por <see cref="Animacion.Director"/>.</summary>
public partial class Escenario : UserControl
{
    public Escenario()
    {
        InitializeComponent();
        Director = new Director(this);
    }

    internal Director Director { get; }

    /// <summary>Sin animación: coloca mapa, escena y ficha según el estado.</summary>
    public void Sincronizar(Estado estado) => Director.Sincronizar(estado);

    /// <summary>Reproduce el momento (consecuencia, fallo, entrada al nivel…).</summary>
    public void Representar(Momento momento) => Director.Representar(momento);
}
