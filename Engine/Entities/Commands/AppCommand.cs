using System.Windows.Input;
using Engine.Entities.Commands.Interfaces;

namespace Engine.Entities.Commands;

// 1. Base principal con soporte para tipos independientes en CanExecute y Execute
public abstract class AppCommand<TCanExecute, TExecute>(
    string title,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null) : IAppCommand
{
    public string Title { get; protected set; } = title;
    public string Description { get; protected set; } = description;
    public Image? Icon { get; protected set; } = icon;
    public Keys? Shortcut { get; protected set; } = shortcut;

    public event EventHandler? CanExecuteChanged;

    public virtual bool CanExecute() => true;
    public virtual bool CanExecute(TCanExecute parameter) => CanExecute();
    public abstract void Execute(TExecute parameter);

    bool ICommand.CanExecute(object? parameter)
    {
        if (parameter is TCanExecute typedParam)
            return CanExecute(typedParam);

        return CanExecute();
    }

    void ICommand.Execute(object? parameter)
    {
        if (parameter is TExecute typedParam)
        {
            Execute(typedParam);
        }
        else if (parameter is null && default(TExecute) is null)
        {
            Execute(default!);
        }
    }

    public void NotifyCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public void NotifyCanExecuteChanged(object? _)
        => NotifyCanExecuteChanged();

    public void NotifyCanExecuteChanged(object? _1, object? _2)
        => NotifyCanExecuteChanged();
}

// 2. Variante estándar donde CanExecute y Execute comparten el mismo tipo de parámetro
public abstract class AppCommand<TParameter>(
    string title,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null)
    : AppCommand<TParameter, TParameter>(title, description, icon, shortcut);

// 3. Variante sin parámetros
public abstract class AppCommand(
    string title,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null)
    : AppCommand<object?, object?>(title, description, icon, shortcut)
{
    public abstract void Execute();
    public sealed override void Execute(object? parameter) => Execute();
}