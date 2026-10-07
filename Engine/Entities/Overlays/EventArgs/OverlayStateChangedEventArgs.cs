using System.Drawing;

namespace Engine.Entities.Overlays.EventArgs;

public class OverlayStateChangedEventArgs : System.EventArgs
{
    public string Text { get; }
    public Point Position { get; }
    public bool IsVisible { get; }

    public OverlayStateChangedEventArgs(string text, Point position, bool isVisible)
    {
        Text = text;
        Position = position;
        IsVisible = isVisible;
    }
}