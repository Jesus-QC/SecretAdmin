using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using static SecretAdmin.Program;

namespace SecretAdmin.Features.Server
{
    public class Utils
    {
        public static bool GetExecutable(out string executable)
        {
            executable = "SCPSL.x86_64";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                executable = @"C:\Program Files (x86)\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL.exe";

            if (File.Exists(executable)) 
                return true;
            
            Log.Alert("\nExecutable not found, make sure this file is on the same folder as LocalAdmin.");
            return false;
        }

        public static string GetLogsName(uint port) => Path.Combine(Paths.ServerLogsFolder, $"[{DateTime.Now:MM-dd-yyyy hh.mm}]-{port}.log");
        public static string GetOutputLogsName(uint port) => Path.Combine(Paths.ServerLogsFolder, $"[{DateTime.Now:MM-dd-yyyy hh.mm}]-{port}-output.log");

        public static void ArchiveServerLogs(DateTime date)
        {
            List<string> filesToArchive = new();

            foreach (var fileName in Directory.GetFiles(Paths.ServerLogsFolder))
            {
                var reg = new Regex(@"\[(.*?)\]");
                var match = reg.Match(fileName);
                
                if (match.Success && DateTime.Parse(match.Groups[1].Value[..10]) <= DateTime.Today.AddDays(-ConfigManager.SecretAdminConfig.ArchiveLogsDays))
                {
                    filesToArchive.Add(fileName);
                }
            }
            
            // TODO: archive the files
        }
    }
}