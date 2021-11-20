using System;
using System.IO;
using SecretAdmin.API;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Program.Config;
using SecretAdmin.Features.Server;
using SecretAdmin.Features.Server.Commands;
using Spectre.Console;

namespace SecretAdmin
{
    class Program
    {
        public static Version Version { get; } = new (0, 0, 0,1);
        public static ScpServer Server { get; private set; }
        public static ConfigManager ConfigManager { get; private set; }
        public static CommandHandler CommandHandler { get; private set; }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            Console.Title = $"SecretAdmin [v{Version}]";

            AnsiConsole.Record();
            
            var arguments = ArgumentsManager.GetArgs(args);
            
            Paths.Load();
            ConfigManager = new ConfigManager();
            if (ProgramIntroduction.FirstTime || arguments.Reconfigure)
                ProgramIntroduction.ShowIntroduction();
            ConfigManager.LoadConfig();

            if(ConfigManager.SecretAdminConfig.AutoUpdater)
                AutoUpdater.CheckForUpdates();
            
            Log.Intro();

            Utils.ArchiveControlLogs();

            CommandHandler = new CommandHandler();
            ModuleManager.LoadAll();
            
            Server = new ScpServer(ConfigManager.GetServerConfig(arguments.Config));
            Server.Start();
            
            InputManager.Start();
        }

        private static void OnExit(object obj, EventArgs ev)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nExit Detected. Killing game process.");

            if (ConfigManager.SecretAdminConfig.SafeShutdown)
                Server?.Kill();

            foreach (var module in ModuleManager.Modules)
                module.OnDisabled();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Everything seems good to go! Bye :)");
            File.WriteAllText(Path.Combine(Paths.ProgramLogsFolder, $"{DateTime.Now:MM.dd.yyyy-hh.mm.ss}.log"), AnsiConsole.ExportText());
        }
    }
}