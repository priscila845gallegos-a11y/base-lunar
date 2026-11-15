using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BaseLunar.App.VistaModelos;

/// <summary>Base de los modelos de vista: avisa a la pantalla cuando cambia una propiedad.</summary>
public abstract class Observable : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void Avisar([CallerMemberName] string? propiedad = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));

    /// <summary>Avisa de que cambiaron todas las propiedades (es el «render()» de ui.js).</summary>
    protected void AvisarTodo() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}

/// <summary>Comando para los botones. <see cref="Refrescar"/> vuelve a consultar si se puede ejecutar.</summary>
public sealed class Comando(Action<object?> accion, Func<bool> puede) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => puede();

    public void Execute(object? parameter) => accion(parameter);

    public void Refrescar() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
