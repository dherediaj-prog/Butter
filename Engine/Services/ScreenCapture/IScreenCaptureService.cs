namespace Engine.Services.ScreenCapture;

public interface IScreenCaptureService
{
    byte[] CaptureRegion(Rectangle bounds);
}