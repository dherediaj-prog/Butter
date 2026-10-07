using System.Drawing.Imaging;
using Engine.Services;

namespace Presentation.Windows.Services;

public class Win32ScreenCaptureService : IScreenCaptureService
{
    public byte[] CaptureRegion(Rectangle bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return Array.Empty<byte>();

        using var bitmap = new Bitmap(bounds.Width, bounds.Height);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}