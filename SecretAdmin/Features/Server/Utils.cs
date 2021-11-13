using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using static SecretAdmin.Program;

namespace SecretAdmin.Features.Server
{
    public static class Utils
    {
        public static bool GetExecutable(out string executable)
        {
            executable = "SCPSL.x86_64";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                executable = @"SCPSL.exe";

#if DEBUG
            executable = @"C:\Program Files (x86)\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL.exe";
#endif

            if (File.Exists(executable)) 
                return true;
            
            Log.Alert("\nExecutable not found, make sure this file is on the same folder as LocalAdmin.");
            return false;
        }

        public static string GetLogsName(uint port) => Path.Combine(Paths.ServerLogsFolder, $"[{DateTime.Now:MM-dd-yyyy HH.mm}]-{port}.log");
        public static string GetOutputLogsName(uint port) => Path.Combine(Paths.ServerLogsFolder, $"[{DateTime.Now:MM-dd-yyyy HH.mm}]-{port}-output.log");

        public static void ArchiveServerLogs()
        {
            var filesToArchive = (from fileName in Directory.GetFiles(Paths.ServerLogsFolder) let reg = new Regex(@"\[(.*?)\]") let match = reg.Match(fileName) where match.Success && match.Groups[1].Value.Length > 10 && DateTime.TryParse(match.Groups[1].Value[..10], out var d) && d <= DateTime.Today.AddDays(-ConfigManager.SecretAdminConfig.ArchiveLogsDays) select fileName).ToList();
            var zipName = Path.Combine(Paths.ServerLogsFolder, $"{DateTime.Now:MM-dd-yyyy}-archive.zip");
            
            if(!File.Exists(zipName))
                File.WriteAllText(zipName, "");
            
            using var archive = ZipFile.Open(zipName, ZipArchiveMode.Update);

            foreach (var file in filesToArchive)
            {
                archive.CreateEntryFromFile(file, new FileInfo(file).Name);
                File.Delete(file);
            }
        }
        
        public static void ArchiveControlLogs()
        {
            var filesToArchive = (from fileName in Directory.GetFiles(Paths.ProgramLogsFolder) where DateTime.TryParse(fileName.Replace(".log", ""), out var d) && d <= DateTime.Today.AddDays(-ConfigManager.SecretAdminConfig.ArchiveLogsDays) select fileName).ToList();
            var zipName = Path.Combine(Paths.ServerLogsFolder, $"{DateTime.Now:MM-dd-yyyy}-archive.zip");
            
            if(!File.Exists(zipName))
                File.WriteAllText(zipName, "");
            
            using var archive = ZipFile.Open(zipName, ZipArchiveMode.Update);

            foreach (var file in filesToArchive)
            {
                archive.CreateEntryFromFile(file, new FileInfo(file).Name);
                File.Delete(file);
            }
        }
    }
}