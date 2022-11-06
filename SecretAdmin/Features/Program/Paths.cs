using System;
using System.IO;

namespace SecretAdmin.Features.Program;

public static class Paths
{
    public static string MainFolder;
    public static string LogsFolder;
    public static string ServerLogsFolder;
    public static string ProgramLogsFolder;
    public static string ProgramConfig;

    public static void Load(int port)
    {
        MainFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SecretAdmin", port.ToString());
        LogsFolder = Path.Combine(MainFolder, "Logs");
        ServerLogsFolder = Path.Combine(LogsFolder, "Server");
        ProgramLogsFolder = Path.Combine(LogsFolder, "SecretAdmin");
        ProgramConfig = Path.Combine(MainFolder, "config.yml");
        
        CreateIfNotExists();
    }
    
    private static void CreateIfNotExists()
    {
        Directory.CreateDirectory(MainFolder);
        Directory.CreateDirectory(LogsFolder);
        Directory.CreateDirectory(ServerLogsFolder);
        Directory.CreateDirectory(ProgramLogsFolder);
    }
}