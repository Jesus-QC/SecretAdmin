using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;

namespace SecretAdmin.Features.Server;

public static class Utils
{
    public static bool TryGetExecutable(out string executable)
    {
        executable = "SCPSL.x86_64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            executable = "SCPSL.exe";

#if DEBUG
        executable = @"C:\Program Files (x86)\Steam\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL.exe";
#endif

        if (File.Exists(executable)) 
            return true;
            
        Log.Alert("\nExecutable not found, make sure this file is on the same folder as LocalAdmin.");
        Log.ReadKey();
        return false;
    }

    private static void Archive(HashSet<FileInfo> paths, string zip)
    {
        using FileStream zipFile = File.Open(zip, FileMode.Create);
        using ZipArchive archive = new(zipFile, ZipArchiveMode.Create);
        
        foreach (FileInfo fileInfo in paths)
        {
            archive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name);
            fileInfo.Delete();
        }
    }
    
    public static void ArchiveOldLogs(int days)
    {
        DateTime now = DateTime.Today.AddDays(-days);
        HashSet<FileInfo> toArchive = new();

        foreach (string outputLog in Directory.GetFiles(Paths.ProgramLogsFolder, "*.log"))
        {
            FileInfo file = new (outputLog);

            if (!DateTime.TryParseExact(file.Name[..19], "MM.dd.yyyy-hh.mm.ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d) || d > now) 
                continue;

            toArchive.Add(file);
        }
        
        if (toArchive.Count != 0)
            Archive(toArchive, Path.Combine(Paths.ProgramLogsFolder, $"{now:MM.dd.yyyy-hh.mm.ss}.zip"));
        
        toArchive.Clear();
        
        foreach (string outputLog in Directory.GetFiles(Paths.ServerLogsFolder, "*.log"))
        {
            FileInfo file = new (outputLog);

            if (!DateTime.TryParseExact(file.Name[..19], "MM.dd.yyyy-hh.mm.ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d) || d > now) 
                continue;

            toArchive.Add(file);
        }
        
        if(toArchive.Count == 0)
            return;
        
        Archive(toArchive, Path.Combine(Paths.ProgramLogsFolder, $"{now:MM.dd.yyyy-hh.mm.ss}.zip"));
    }

    public static void RemoveOldLogs(int days)
    {
        DateTime now = DateTime.Today.AddDays(-days);

        foreach (string outputLog in Directory.GetFiles(Paths.ProgramLogsFolder))
        {
            FileInfo file = new (outputLog);

            if (!DateTime.TryParseExact(file.Name[..19], "MM.dd.yyyy-hh.mm.ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d) || d > now) 
                continue;

            File.Delete(outputLog);
        }
        
        foreach (string outputLog in Directory.GetFiles(Paths.ServerLogsFolder))
        {
            FileInfo file = new (outputLog);

            if (!DateTime.TryParseExact(file.Name[..19], "MM.dd.yyyy-hh.mm.ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d) || d > now) 
                continue;

            File.Delete(outputLog);
        }
    }
}