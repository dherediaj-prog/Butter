using Engine.Entities.PanelTriggers.Enums;

namespace Engine.Entities.PanelTriggers;

public class PanelTrigger
{
    private Point _position = new(100, 100);
    private Size _size = new(32, 32);
    private bool _isVisible;
    private bool _isOpen;

    // --- Eventos Livianos (Mismo patrón que Overlay y Selection) ---
    public event Action<Point>? PositionChanged;
    public event Action<Size>? SizeChanged;
    public event Action<bool>? VisibilityChanged;
    public event Action<bool>? StateChanged; // Indica si el menú/panel objetivo se desplegó u ocultó
    public event Action? Clicked;
    public event Action? Changed;

    // --- Propiedades Públicas ---
    public Point Position => _position;
    public Size Size => _size;
    public Rectangle Bounds => new(_position, _size);
    public bool IsVisible => _isVisible;

    /// <summary>
    /// Indica si el panel o menú controlado por este trigger está abierto/activo.
    /// </summary>
    public bool IsOpen => _isOpen;

    // --- Posicionamiento y Dimensión ---

    public void SetPosition(int x, int y) => SetPosition(new Point(x, y));

    public void SetPosition(Point point)
    {
        if (_position == point) return;
        _position = point;
        PositionChanged?.Invoke(_position);
        Changed?.Invoke();
    }

    public void SetSize(int width, int height) => SetSize(new Size(width, height));

    public void SetSize(Size size)
    {
        if (_size == size) return;
        _size = size;
        SizeChanged?.Invoke(_size);
        Changed?.Invoke();
    }

    /// <summary>
    /// Ancla automáticamente la posición del trigger respecto a un área (ej: un Selection.Bounds o la esquina de la pantalla).
    /// </summary>
    public void AnchorTo(Rectangle targetArea, AnchorAlignment alignment = AnchorAlignment.TopRight, int offset = 6)
    {
        if (targetArea.IsEmpty) return;

        int x = alignment switch
        {
            AnchorAlignment.TopRight or AnchorAlignment.BottomRight => targetArea.Right - _size.Width + offset,
            AnchorAlignment.TopLeft or AnchorAlignment.BottomLeft => targetArea.Left - offset,
            AnchorAlignment.TopCenter or AnchorAlignment.BottomCenter => targetArea.Left +
                                                                         (targetArea.Width - _size.Width) / 2,
            _ => targetArea.Right
        };

        int y = alignment switch
        {
            AnchorAlignment.TopRight or AnchorAlignment.TopLeft or AnchorAlignment.TopCenter => targetArea.Top -
                _size.Height - offset,
            AnchorAlignment.BottomRight or AnchorAlignment.BottomLeft or AnchorAlignment.BottomCenter => targetArea
                .Bottom + offset,
            _ => targetArea.Top
        };

        SetPosition(x, y);
    }

    // --- Visibilidad y Estado de Disparo ---

    public void SetVisibility(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;

        // Si el trigger se oculta, se cierra automáticamente el menú/panel abierto
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

    public void Reset()
    {
        _isVisible = false;
        _isOpen = false;
        Changed?.Invoke();
    }
}