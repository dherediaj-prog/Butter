namespace Engine.Entities.ResponseDisplays;

public class ResponseDisplay
{
    private string _responseText = "͡° ͜ʖ ͡°";
    private Point _position = new(100, 100);
    private Size _size = new(850, 650);
    private bool _isVisible;

    // --- Configuración Visual Integrada ---
    public Color TextColor { get; set; } = Color.FromArgb(210, 215, 220);
    public Color BackColor { get; set; } = Color.Black;
    public double Opacity { get; set; } = 0.80;
    public Font Font { get; set; } = new("Consolas", 10f, FontStyle.Regular);

    // --- Eventos de Dominio ---
    public event EventHandler<string>? OnTextChanged;
    public event EventHandler<Point>? OnPositionChanged;
    public event EventHandler<bool>? OnVisibilityChanged;
    public event EventHandler? OnStyleChanged;

    public string ResponseText => _responseText;
    public Point Position => _position;
    public Size Size => _size;
    public bool IsVisible => _isVisible;

    public void UpdateResponseText(string text)
    {
        if (_responseText == text) return;
        _responseText = text;
        OnTextChanged?.Invoke(this, _responseText);
    }

    public void SetPosition(int x, int y)
    {
        var newPos = new Point(x, y);
        if (_position == newPos) return;
        _position = newPos;
        OnPositionChanged?.Invoke(this, _position);
    }

    public void SetSize(int width, int height)
    {
        _size = new Size(width, height);
        OnStyleChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetVisibility(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;
        OnVisibilityChanged?.Invoke(this, _isVisible);
    }

    public void SetTheme(Color textColor, Color backColor, double opacity)
    {
        TextColor = textColor;
        BackColor = backColor;
        Opacity = opacity;
        OnStyleChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        _responseText = string.Empty;
        _isVisible = false;
        OnTextChanged?.Invoke(this, _responseText);
        OnVisibilityChanged?.Invoke(this, _isVisible);
    }
}