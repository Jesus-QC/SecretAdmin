using System;
using System.Linq;
using System.Reflection;
using SecretAdmin.API;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Server;
using SecretAdmin.Features.Server.Commands;
using Spectre.Console;
using ConfigManager = SecretAdmin.Features.Program.Config.ConfigManager;

namespace SecretAdmin;

class Program
{
    public static Version Version { get; private set; }
    public static ScpServer Server { get; private set; }
    public static ConfigManager ConfigManager { get; private set; }
    public static CommandHandler CommandHandler { get; private set; }

    private static bool _exceptionalExit;
        
    static void Main(string[] args)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version;
        
        Console.Title = $"SecretAdmin [v{Version}]";
        
        AppDomain.CurrentDomain.ProcessExit += OnExit;
        AppDomain.CurrentDomain.UnhandledException += OnError;
        
        AnsiConsole.Record();

        Start(args);
    }

    private static void Start(string[] args)
    {
        if (Console.WindowWidth is 0) // Pterodactyl
            AnsiConsole.Console.Profile.Width = 1000;

        if (args.Length == 0 || !int.TryParse(args[0], out int port))
            port = Log.GetOption("Please introduce the port you want to start the server on", 7777);
        else
            args = args.Skip(1).ToArray();
        
        Log.Intro();

        Paths.Load(port);

        ConfigManager = new ConfigManager();
        ConfigManager.LoadConfig();

        if (ConfigManager.SecretAdminConfig.AutoUpdater)
            AutoUpdater.CheckForUpdates().Wait();

        ModuleLoader.Load();
        
        Utils.RemoveOldLogs(ConfigManager.SecretAdminConfig.DeleteLogsDays);
        Utils.ArchiveOldLogs(ConfigManager.SecretAdminConfig.ArchiveLogsDays);
        
        CommandHandler = new CommandHandler();

        Server = new ScpServer(port, args);
        Server.Start();
        
        InputManager.Start();
    }

    private static void OnError(object obj, UnhandledExceptionEventArgs ev)
    {
        _exceptionalExit = true;
        
        if (Paths.MainFolder is null)
            Paths.Load(0);

        try
        {
            AnsiConsole.WriteException((Exception)ev.ExceptionObject);
            Utils.SaveCrashLogs();
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }
        
    private static void OnExit(object obj, EventArgs ev)
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[yellow]Exiting safely.[/]");
        
        if ((ConfigManager?.SecretAdminConfig?.SafeShutdown ?? false) && Server is not null)
            Server.Stop();
        
        if (Paths.MainFolder is null)
            Paths.Load(0);

        if (!_exceptionalExit)
        {
            try { Utils.SaveLogs(); }
            catch { Utils.SaveCrashLogs(); }
        }

        AnsiConsole.MarkupLine("[green]Exited safely! Program can be killed.[/]");
    }
}