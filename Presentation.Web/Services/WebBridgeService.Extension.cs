using System.IO.Compression;

namespace Presentation.Web.Services;

public partial class WebBridgeService
{
    public IResult HandleExtensionDownload()
    {
        var zipPath = Path.Combine(_env.ContentRootPath, "ButterKnife-Extension.zip");
        if (File.Exists(zipPath))
        {
            return Results.File(zipPath, "application/zip", "ButterKnife-Extension.zip");
        }

        var extensionPath = Path.Combine(_env.WebRootPath, "extension");
        if (!Directory.Exists(extensionPath))
        {
            extensionPath = Path.Combine(_env.ContentRootPath, "extension");
        }

        if (!Directory.Exists(extensionPath))
        {
            return Results.NotFound(new { error = "No se encontró el directorio de la extensión." });
        }

        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in Directory.GetFiles(extensionPath, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(extensionPath, file);
                archive.CreateEntryFromFile(file, relativePath);
            }
        }

        memoryStream.Position = 0;
        return Results.File(memoryStream, "application/zip", "ButterKnife-Extension.zip");
    }
}