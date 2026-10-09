using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Engine.Services.IconRenderer;

public static class IconRendererService
{
    // Caché en memoria para parsear cada SVG Path solo una vez
    private static readonly ConcurrentDictionary<string, GraphicsPath> PathCache = new();

    /// <summary>
    /// Dibuja un ícono vectorial SVG escalado y centrado automáticamente en los límites especificados.
    /// </summary>
    public static void DrawIcon(this Graphics g, string svgPathData, Rectangle targetBounds, Color color,
        float margin = 6f)
    {
        if (string.IsNullOrWhiteSpace(svgPathData)) return;

        var path = PathCache.GetOrAdd(svgPathData, ParseSvgPath);
        if (path.PointCount == 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Bounding Box real del icono
        RectangleF bounds = path.GetBounds();
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        // Cálculo dinámico de escala y centrado
        float availableWidth = targetBounds.Width - (margin * 2);
        float availableHeight = targetBounds.Height - (margin * 2);

        float scale = Math.Min(availableWidth / bounds.Width, availableHeight / bounds.Height);
        float offsetX = targetBounds.X + (targetBounds.Width - (bounds.Width * scale)) / 2f - (bounds.X * scale);
        float offsetY = targetBounds.Y + (targetBounds.Height - (bounds.Height * scale)) / 2f - (bounds.Y * scale);

        var state = g.Save();
        g.TranslateTransform(offsetX, offsetY);
        g.ScaleTransform(scale, scale);

        using var pen = new Pen(color, 1.5f / scale)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        g.DrawPath(pen, path);
        g.Restore(state);
    }

    /// <summary>
    /// Dibuja la textura Grip (puntos de arrastre).
    /// </summary>
    public static void DrawGripTexture(this Graphics g, Color color)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(color);

        int startX = 8;
        int startY = 10;
        for (int i = 0; i < 3; i++)
        {
            g.FillEllipse(brush, startX, startY + (i * 6), 3, 3);
            g.FillEllipse(brush, startX + 5, startY + (i * 6), 3, 3);
        }
    }

    private static GraphicsPath ParseSvgPath(string pathData)
    {
        var path = new GraphicsPath();
        var matches = Regex.Matches(pathData, @"[a-zA-Z]|-?\d+(?:\.\d+)?");
        PointF current = PointF.Empty;
        char cmd = '\0';
        int idx = 0;

        while (idx < matches.Count)
        {
            string token = matches[idx].Value;
            if (char.IsLetter(token[0]))
            {
                cmd = token[0];
                idx++;
            }

            switch (char.ToUpper(cmd))
            {
                case 'M':
                    if (idx + 1 >= matches.Count) break;
                    float mx = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    float my = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    current = new PointF(mx, my);
                    path.StartFigure();
                    break;

                case 'L':
                    if (idx + 1 >= matches.Count) break;
                    float lx = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    float ly = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    path.AddLine(current, new PointF(lx, ly));
                    current = new PointF(lx, ly);
                    break;

                case 'C':
                    if (idx + 5 >= matches.Count) break;
                    float x1 = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    float y1 = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    float x2 = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    float y2 = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    float x = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    float y = float.Parse(matches[idx++].Value, CultureInfo.InvariantCulture);
                    path.AddBezier(current, new PointF(x1, y1), new PointF(x2, y2), new PointF(x, y));
                    current = new PointF(x, y);
                    break;

                case 'Z':
                    path.CloseFigure();
                    break;

                default:
                    idx++;
                    break;
            }
        }

        return path;
    }
}