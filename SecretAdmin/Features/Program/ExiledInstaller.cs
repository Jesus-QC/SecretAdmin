using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using SecretAdmin.Features.Console;

namespace SecretAdmin.Features.Program
{
    public static class ExiledInstaller
    {
        public static void InstallExiled()
        {
            var platformSpecificString = "Linux";
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                platformSpecificString = "Win.exe";

            Log.Alert("Downloading EXILED...");
            
            using (var client = new WebClient())
                client.DownloadFile($"https://github.com/Exiled-Team/EXILED/releases/latest/download/Exiled.Installer-{platformSpecificString}", $"Exiled.Installer-{platformSpecificString}");
            
            Log.Alert("Running installer...");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Log.Alert("Marking installer as executable...");
                
                using var p = new Process();
                
                p.StartInfo.FileName = "/bin/bash";
                p.StartInfo.Arguments = "-c \" chmod +x ./Exiled.Installer-Linux\" ";
                p.StartInfo.CreateNoWindow = true;

                p.Start();
                p.WaitForExit();
            }

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = $"Exiled.Installer-{platformSpecificString}";
                p.Start();
                p.WaitForExit();
            }

            Log.Alert("Done! Exiled was installed correctly.");
        }
    }
}