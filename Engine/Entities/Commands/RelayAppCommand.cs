namespace Engine.Entities.Commands;

// 1. Soporte completo con tipos independientes
public class RelayAppCommand<TCanExecute, TExecute>(
    string title,
    Action<TExecute> execute,
    Func<TCanExecute, bool>? canExecute = null,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null) : AppCommand<TCanExecute, TExecute>(title, description, icon, shortcut)
{
    public override bool CanExecute(TCanExecute parameter) => canExecute == null || canExecute(parameter);
    public override void Execute(TExecute parameter) => execute(parameter);
}

// 2. Variante estándar con tipo único
public class RelayAppCommand<TParameter>(
    string title,
    Action<TParameter> execute,
    Func<TParameter, bool>? canExecute = null,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null)
    : RelayAppCommand<TParameter, TParameter>(title, execute, canExecute, description, icon, shortcut);

// 3. Variante limpia sin parámetros
public class RelayAppCommand(
    string title,
    Action execute,
    Func<bool>? canExecute = null,
    string description = "",
    Image? icon = null,
    Keys? shortcut = null) : AppCommand(title, description, icon, shortcut)
{
    public override bool CanExecute() => canExecute == null || canExecute();
    public override void Execute() => execute();
}