using System.Drawing;

namespace Engine.Entities.PanelTriggers;

public class PanelTrigger
{
    private Point _position = new(100, 100);
    private Size _size = new(180, 36);
    private bool _isVisible;
    private bool _isOpen;

    // --- Estado de Arrastre (Drag State) ---
    private bool _isDragging;
    private Point _dragOffset;

    // --- Estilos Visuales de la Entidad ---
    public Color BackColor { get; set; } = Color.FromArgb(32, 33, 36);
    public Color GripBackColor { get; set; } = Color.FromArgb(45, 48, 53);
    public Color GripDotColor { get; set; } = Color.FromArgb(120, 130, 140);
    public Color ButtonForeColor { get; set; } = Color.White;
    public Color ButtonHoverColor { get; set; } = Color.FromArgb(55, 58, 64);
    public Color DropdownForeColor { get; set; } = Color.Gainsboro;
    public Color DropdownHoverColor { get; set; } = Color.FromArgb(65, 68, 75);
    public Color MenuBackColor { get; set; } = Color.FromArgb(40, 42, 46);
    public Color MenuForeColor { get; set; } = Color.White;

    public Font Font { get; set; } = new("Segoe UI", 9f, FontStyle.Bold);
    public Font DropdownFont { get; set; } = new("Segoe UI", 7f, FontStyle.Regular);

    // --- Eventos ---
    public event Action<Point>? PositionChanged;
    public event Action<Size>? SizeChanged;
    public event Action<bool>? VisibilityChanged;
    public event Action<bool>? StateChanged;
    public event Action? StyleChanged;
    public event Action? Clicked;
    public event Action? Changed;

    // --- Propiedades Públicas ---
    public Point Position => _position;
    public Size Size => _size;
    public Rectangle Bounds => new(_position, _size);
    public bool IsVisible => _isVisible;
    public bool IsOpen => _isOpen;
    public bool IsDragging => _isDragging;
    public Point DragOffset => _dragOffset;

    // --- Posicionamiento y Arrastre ---

    public void SetPosition(int x, int y) => SetPosition(new Point(x, y));

    public void SetPosition(Point point)
    {
        if (_position == point) return;
        _position = point;
        PositionChanged?.Invoke(_position);
        Changed?.Invoke();
    }

    public void SetSize(Size size)
    {
        if (_size == size) return;
        _size = size;
        SizeChanged?.Invoke(_size);
        Changed?.Invoke();
    }

    public void BeginDrag(Point mouseLocation)
    {
        _isDragging = true;
        _dragOffset = mouseLocation;
    }

    public void DragTo(Point screenMousePosition)
    {
        if (!_isDragging) return;

        var targetLocation = new Point(
            screenMousePosition.X - _dragOffset.X,
            screenMousePosition.Y - _dragOffset.Y
        );

        SetPosition(targetLocation);
    }

    public void EndDrag()
    {
        _isDragging = false;
        _dragOffset = Point.Empty;
    }

    // --- Visibilidad y Estado ---

    public void SetVisibility(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;

        if (!_isVisible && _isOpen)
        {
            _isOpen = false;
            StateChanged?.Invoke(false);
        }

        VisibilityChanged?.Invoke(_isVisible);
        Changed?.Invoke();
    }

    public void Show() => SetVisibility(true);
    public void Hide() => SetVisibility(false);

    public void SetOpenState(bool isOpen)
    {
        if (_isOpen == isOpen) return;
        _isOpen = isOpen;
        StateChanged?.Invoke(_isOpen);
        Changed?.Invoke();
    }

    public void Open() => SetOpenState(true);
    public void Close() => SetOpenState(false);

    public void Toggle()
    {
        if (!_isVisible) return;
        SetOpenState(!_isOpen);
    }

    public void Click()
    {
        if (!_isVisible) return;
        Clicked?.Invoke();
        Toggle();
    }

    public void NotifyStyleChanged()
    {
        StyleChanged?.Invoke();
        Changed?.Invoke();
    }

    public void Reset()
    {
        _isVisible = false;
        _isOpen = false;
        _isDragging = false;
        Changed?.Invoke();
    }
}