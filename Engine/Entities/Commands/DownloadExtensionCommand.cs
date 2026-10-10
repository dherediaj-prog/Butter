using System.Diagnostics;

namespace Engine.Entities.Commands;

public class DownloadExtensionCommand : AppCommand
{
    private readonly string _downloadUrl;

    public DownloadExtensionCommand(string downloadUrl = "http://localhost:5000/download/extension")
        : base(
            title: "Descargar Extensión (.crx)",
            description: "Abre el navegador predeterminado para descargar el paquete de la extensión.")
    {
        _downloadUrl = downloadUrl;
    }

    public override void Execute()
    {
        try
        {
            // Abre la URL en el navegador predeterminado de forma instantánea sin bloquear la UI
            Process.Start(new ProcessStartInfo
            {
                FileName = _downloadUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo abrir la URL de descarga: {ex.Message}",
                "Error de Descarga",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}