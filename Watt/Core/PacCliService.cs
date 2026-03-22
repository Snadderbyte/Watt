using System.Diagnostics;

namespace Watt.Tools.Dataverse;

internal static class PacCliService
{
    public static string RunPacCommand(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "pac",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }
}