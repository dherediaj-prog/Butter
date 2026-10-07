using System.Diagnostics;

namespace Presentation.Windows.Services;

public static class FirewallService
{
    private const string RuleNamePrefix = "Butter Kestrel Port";

    public static void EnsurePortOpen(int port = 5000)
    {
        string ruleName = $"{RuleNamePrefix} {port}";

        if (IsRuleActive(ruleName))
            return;

        // Si la regla no existe, invoca netsh pidiendo elevación UAC (runas)
        var startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=TCP localport={port}",
            Verb = "runas", // Solicita elevación UAC en pantalla solo para esta acción
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        try
        {
            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
        catch
        {
            // Ocurre si el usuario rechaza la ventana emergente de Administrador (UAC)
        }
    }

    private static bool IsRuleActive(string ruleName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"advfirewall firewall show rule name=\"{ruleName}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null) return false;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains(ruleName);
        }
        catch
        {
            return false;
        }
    }
}