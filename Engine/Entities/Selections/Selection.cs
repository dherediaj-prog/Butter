using System.Drawing.Drawing2D;

namespace Engine.Entities.Selections;

public class Selection
{
    public Point StartPoint { get; private set; }
    public Point CurrentPoint { get; private set; }
    public bool IsSelecting { get; private set; }

    public Rectangle Bounds => CalculateBounds(StartPoint, CurrentPoint);

    // --- Propiedades de Estilo y Transparencia de la Entidad ---

    /// <summary>
    /// Color clave que el Form utiliza para recortar la transparencia nativa en WinForms.
    /// </summary>
    public Color TransparentColor { get; set; } = Color.Magenta;

    /// <summary>
    /// Color del borde del rectángulo de selección.
    /// </summary>
    public Color BorderColor { get; set; } = Color.FromArgb(255, 0, 120, 215);

    /// <summary>
    /// Color de relleno interno del área seleccionada.
    /// </summary>
    public Color FillColor { get; set; } = Color.FromArgb(45, 0, 120, 215);

    /// <summary>
    /// Ancho del trazo del borde en píxeles.
    /// </summary>
    public float BorderWidth { get; set; } = 2f;

    /// <summary>
    /// Estilo de la línea del borde (Sólido, Discontinuo, etc.).
    /// </summary>
    public DashStyle BorderStyle { get; set; } = DashStyle.Solid;

    /// <summary>
    /// Color de fondo para toda la pantalla. Por defecto es transparente (Color.Transparent).
    /// </summary>
    public Color OverlayColor { get; set; } = Color.Transparent;

    // --- Eventos ---
    public event Action? Changed;
    public event Action<Rectangle>? Completed;
    public event Action? Cancelled;
    public event Action? SelectionRequested;

    public void RequestSelection() => SelectionRequested?.Invoke();

    public void Start(Point startPoint)
    {
        StartPoint = startPoint;
        CurrentPoint = startPoint;
        IsSelecting = true;
        Changed?.Invoke();
    }

    public void Update(Point currentPoint)
    {
        if (!IsSelecting) return;

        CurrentPoint = currentPoint;
        Changed?.Invoke();
    }

    public Rectangle Complete()
    {
        if (!IsSelecting) return Rectangle.Empty;

        var finalBounds = Bounds;
        IsSelecting = false;
        Completed?.Invoke(finalBounds);
        return finalBounds;
    }

    public void Cancel()
    {
        IsSelecting = false;
        Cancelled?.Invoke();
    }

    public void NotifyStyleChanged()
    {
        Changed?.Invoke();
    }

    private static Rectangle CalculateBounds(Point p1, Point p2)
    {
        int x = Math.Min(p1.X, p2.X);
        int y = Math.Min(p1.Y, p2.Y);
        int width = Math.Abs(p1.X - p2.X);
        int height = Math.Abs(p1.Y - p2.Y);

        return new Rectangle(x, y, width, height);
    }
}