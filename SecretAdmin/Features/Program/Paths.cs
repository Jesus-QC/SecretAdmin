using System.IO;

namespace SecretAdmin.Features.Program
{
    public static class Paths
    {
        public static string MainFolder { get; private set; }
        public static string LogsFolder { get; private set; }
        public static string ServerLogsFolder { get; private set; }
        public static string ProgramLogsFolder { get; private set; }
        public static string ServerConfigsFolder { get; private set; }
        public static string ProgramConfig { get; private set; }

        public static void Load()
        {
            MainFolder = "SecretAdmin";
            LogsFolder = Path.Combine(MainFolder, "Logs");
            ServerLogsFolder = Path.Combine(LogsFolder, "Server");
            ProgramLogsFolder = Path.Combine(LogsFolder, "SecretAdmin");
            ServerConfigsFolder = Path.Combine(MainFolder, "Configs");
            ProgramConfig = Path.Combine(MainFolder, "config.yml");
            CreateIfNotExists();
        }

        public static void CreateIfNotExists()
        {
            Directory.CreateDirectory(MainFolder);
            Directory.CreateDirectory(LogsFolder);
            Directory.CreateDirectory(ServerLogsFolder);
            Directory.CreateDirectory(ProgramLogsFolder);
            Directory.CreateDirectory(ServerConfigsFolder);
        }
    }
}