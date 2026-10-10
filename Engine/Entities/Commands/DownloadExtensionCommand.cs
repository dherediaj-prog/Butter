using System.Diagnostics;
using System.Windows.Forms;
using Engine.Entities.Commands.Model;

namespace Engine.Entities.Commands;

public class DownloadExtensionCommand : AppCommand
{
    private readonly string _baseUrl;

    public DownloadExtensionCommand(string baseUrl = "http://localhost:5000/download/extension")
        : base(
            title: "Descargar Extensión (.zip)", // Cambiado a .zip
            description: "Abre el navegador para descargar el paquete ZIP de la extensión.")
    {
        _baseUrl = baseUrl;
    }

    public override void Execute()
    {
        try
        {
            // Agregamos un timestamp único a la URL para ROMPER LA CACHÉ de Chrome.
            // Esto hace que Chrome piense que es una URL totalmente nueva y no aplique
            // su bloqueo de seguridad en caché de las descargas anteriores.
            var cacheBusterUrl = $"{_baseUrl}?t={DateTime.UtcNow.Ticks}";

            // Abre la URL en el navegador predeterminado
            Process.Start(new ProcessStartInfo
            {
                FileName = cacheBusterUrl,
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