using Engine.Entities.Overlays.EventArgs;

namespace Engine.Entities.Overlays;

public class Overlay
{
    private string _responseText = "͡° ͜ʖ ͡°";
    private Point _position = new Point(100, 100);
    private bool _isVisible;

    // Eventos específicos y globales
    public event EventHandler<string>? OnTextChanged;
    public event EventHandler<Point>? OnPositionChanged;
    public event EventHandler<bool>? OnVisibilityChanged;
    public event EventHandler<OverlayStateChangedEventArgs>? OnStateChanged;

    public string ResponseText => _responseText;
    public Point Position => _position;
    public bool IsVisible => _isVisible;

    public void UpdateResponseText(string text)
    {
        if (_responseText == text) return;
        _responseText = text;
        OnTextChanged?.Invoke(this, _responseText);
        NotifyState();
    }

    public void SetPosition(int x, int y)
    {
        var newPos = new Point(x, y);
        if (_position == newPos) return;
        _position = newPos;
        OnPositionChanged?.Invoke(this, _position);
        NotifyState();
    }

    public void SetVisibility(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;
        OnVisibilityChanged?.Invoke(this, _isVisible);
        NotifyState();
    }

    public void Reset()
    {
        _responseText = string.Empty;
        _isVisible = false;
        NotifyState();
    }

    private void NotifyState()
    {
        OnStateChanged?.Invoke(this, new OverlayStateChangedEventArgs(_responseText, _position, _isVisible));
    }
}