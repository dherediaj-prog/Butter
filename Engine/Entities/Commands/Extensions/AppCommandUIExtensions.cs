using Engine.Entities.Commands.Interfaces;

namespace Engine.Entities.Commands.Extensions;

public static class AppCommandUIExtensions
{
    public static ToolStripMenuItem ToMenuItem(
        this IAppCommand command,
        Func<object?>? parameterSupplier = null)
    {
        ArgumentNullException.ThrowIfNull(command);

        var item = new ToolStripMenuItem
        {
            Text = command.Title,
            Image = command.Icon,
            ShortcutKeys = command.Shortcut ?? Keys.None,
            ToolTipText = command.Description
        };

        command.BindTo(item, parameterSupplier);
        return item;
    }

    public static ToolStripButton ToToolStripButton(
        this IAppCommand command,
        Func<object?>? parameterSupplier = null)
    {
        ArgumentNullException.ThrowIfNull(command);

        var button = new ToolStripButton
        {
            Text = command.Title,
            Image = command.Icon,
            ToolTipText = command.Description,
            DisplayStyle = command.Icon != null
                ? ToolStripItemDisplayStyle.ImageAndText
                : ToolStripItemDisplayStyle.Text
        };

        command.BindTo(button, parameterSupplier);
        return button;
    }

    public static Button ToButton(
        this IAppCommand command,
        Func<object?>? parameterSupplier = null)
    {
        ArgumentNullException.ThrowIfNull(command);

        var button = new Button
        {
            Text = command.Title,
            Image = command.Icon,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };

        command.BindTo(button, parameterSupplier);
        return button;
    }

    public static void BindTo(
        this IAppCommand command,
        ToolStripItem item,
        Func<object?>? parameterSupplier = null)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(item);

        void UpdateState()
        {
            var param = parameterSupplier?.Invoke();
            item.Enabled = command.CanExecute(param);
        }

        UpdateState();

        EventHandler onCanExecuteChanged = (_, _) =>
        {
            if (item.IsDisposed) return;

            if (item.Owner?.InvokeRequired == true)
            {
                item.Owner.BeginInvoke((Action)UpdateState);
            }
            else
            {
                UpdateState();
            }
        };

        EventHandler onClick = (_, _) =>
        {
            var param = parameterSupplier?.Invoke();
            if (command.CanExecute(param))
            {
                command.Execute(param);
            }
        };

        command.CanExecuteChanged += onCanExecuteChanged;
        item.Click += onClick;

        item.Disposed += (_, _) =>
        {
            command.CanExecuteChanged -= onCanExecuteChanged;
            item.Click -= onClick;
        };
    }

    public static void BindTo(
        this IAppCommand command,
        Button button,
        Func<object?>? parameterSupplier = null)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(button);

        void UpdateState()
        {
            var param = parameterSupplier?.Invoke();
            button.Enabled = command.CanExecute(param);
        }

        UpdateState();

        EventHandler onCanExecuteChanged = (_, _) =>
        {
            if (button.IsDisposed) return;

            if (button.InvokeRequired)
            {
                button.BeginInvoke((Action)UpdateState);
            }
            else
            {
                UpdateState();
            }
        };

        EventHandler onClick = (_, _) =>
        {
            var param = parameterSupplier?.Invoke();
            if (command.CanExecute(param))
            {
                command.Execute(param);
            }
        };

        command.CanExecuteChanged += onCanExecuteChanged;
        button.Click += onClick;

        button.Disposed += (_, _) =>
        {
            command.CanExecuteChanged -= onCanExecuteChanged;
            button.Click -= onClick;
        };
    }
}