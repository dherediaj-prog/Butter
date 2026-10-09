using System.Drawing;
using System.Windows.Forms;

namespace Engine.Entities.Commands.Model;

// 1. Soporte con tipos independientes para CanExecute y Execute
public abstract class AsyncAppCommand<TCanExecute, TExecute>(
    string title,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null) : AppCommand<TCanExecute, TExecute>(title, description, icon, shortcut)
{
    private bool _isExecuting;

    public abstract Task ExecuteAsync(TExecute parameter, CancellationToken ct = default);

    protected virtual bool CanExecuteAsync() => true;
    protected virtual bool CanExecuteAsync(TCanExecute parameter) => CanExecuteAsync();

    public sealed override bool CanExecute()
        => !_isExecuting && CanExecuteAsync();

    public sealed override bool CanExecute(TCanExecute parameter)
        => !_isExecuting && CanExecuteAsync(parameter);

    public sealed override async void Execute(TExecute parameter)
    {
        if (_isExecuting) return;

        try
        {
            _isExecuting = true;
            NotifyCanExecuteChanged();
            await ExecuteAsync(parameter);
        }
        finally
        {
            _isExecuting = false;
            NotifyCanExecuteChanged();
        }
    }
}

// 2. Variante estándar donde CanExecute y Execute comparten el mismo tipo
public abstract class AsyncAppCommand<TParameter>(
    string title,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null)
    : AsyncAppCommand<TParameter, TParameter>(title, description, icon, shortcut);

// 3. Variante sin parámetros
public abstract class AsyncAppCommand(
    string title,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null)
    : AsyncAppCommand<object?, object?>(title, description, icon, shortcut)
{
    public abstract Task ExecuteAsync(CancellationToken ct = default);

    public sealed override Task ExecuteAsync(object? parameter, CancellationToken ct = default)
        => ExecuteAsync(ct);
}