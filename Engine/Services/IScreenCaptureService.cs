using System.Drawing;

namespace Engine.Services;

public interface IScreenCaptureService
{
    byte[] CaptureRegion(Rectangle bounds);
}