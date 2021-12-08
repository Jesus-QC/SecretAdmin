using System;
using System.IO;
using SecretAdmin.API;
using Spectre.Console;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Server;
using SecretAdmin.Features.Server.Commands;
using ConfigManager = SecretAdmin.Features.Program.Config.ConfigManager;

namespace SecretAdmin
{
    class Program
    {
        public static Version Version { get; } = new (0, 0, 0,3);
        public static ScpServer Server { get; private set; }
        public static ConfigManager ConfigManager { get; private set; }
        public static CommandHandler CommandHandler { get; private set; }

        private static bool _exceptionalExit = true;
        
        static void Main(string[] args)
        {
            Console.Title = $"SecretAdmin [v{Version}]";
            
            AnsiConsole.Record();
            
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            AppDomain.CurrentDomain.UnhandledException += OnError;
            
            Start(args);
        }

        private static void Start(string[] args)
        {
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
            ModuleManager.LoadAll(ConfigManager.GetServerConfig(arguments.Config).Port);
            
            Server = new ScpServer(ConfigManager.GetServerConfig(arguments.Config));
            Server.Start();
            
            InputManager.Start();
        }

        private static void OnError(object obj, UnhandledExceptionEventArgs ev)
        {
            _exceptionalExit = false;
            AnsiConsole.WriteException((Exception)ev.ExceptionObject);
            File.WriteAllText(Path.Combine(Paths.ProgramLogsFolder, $"{DateTime.Now:MM.dd.yyyy-hh.mm.ss}-exception.log"), AnsiConsole.ExportText());
        }
        
        private static void OnExit(object obj, EventArgs ev)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nExit Detected. Killing game process.");

            if (ConfigManager.SecretAdminConfig.SafeShutdown)
                Server?.Kill();

            foreach (var module in ModuleManager.Modules)
                module?.OnDisabled();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Everything seems good to go! Bye :)");

            if(!_exceptionalExit)
                File.WriteAllText(Path.Combine(Paths.ProgramLogsFolder, $"{DateTime.Now:MM.dd.yyyy-hh.mm.ss}.log"), AnsiConsole.ExportText());
        }
    }
}