using System.Windows.Input;

namespace Engine.Entities.Commands.Interfaces;

public interface IAppCommand : ICommand
{
    string Title { get; }
    string Description { get; }
    Image? Icon { get; }
    Keys? Shortcut { get; }

    /// <summary>
    /// Notifica a la UI o controles vinculados que deben reevaluar la ejecutabilidad del comando.
    /// </summary>
    void NotifyCanExecuteChanged();
}