using System.Drawing;

namespace Engine.Entities.Selections;

public class Selection
{
    public Point StartPoint { get; private set; }
    public Point CurrentPoint { get; private set; }
    public bool IsSelecting { get; private set; }

    public Rectangle Bounds => CalculateBounds(StartPoint, CurrentPoint);

    public event Action? Changed;
    public event Action<Rectangle>? Completed;
    public event Action? Cancelled;
    
    // Evento y método para desencadenar la orden de captura
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

    private static Rectangle CalculateBounds(Point p1, Point p2)
    {
        int x = Math.Min(p1.X, p2.X);
        int y = Math.Min(p1.Y, p2.Y);
        int width = Math.Abs(p1.X - p2.X);
        int height = Math.Abs(p1.Y - p2.Y);

        return new Rectangle(x, y, width, height);
    }
}