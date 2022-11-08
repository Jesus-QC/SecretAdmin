using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Server;
using SecretAdmin.Features.Server.Commands;
using SecretAdmin.Features.Server.Enums;
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
        // ArgumentsManager.Args arguments = ArgumentsManager.GetArgs(args);
 
        // ConfigManager.LoadConfig();
 
        // if(ConfigManager.SecretAdminConfig.AutoUpdater)
        //     AutoUpdater.CheckForUpdates();
        
        // Utils.ArchiveControlLogs();
        
        if (args.Length == 0 || !int.TryParse(args[0], out int port))
            port = Log.GetOption("Please introduce the port you want to start the server on", 7777);
        else
            args = args.Skip(1).ToArray();
        
        Log.Intro();
        
        Paths.Load(port);

        ConfigManager = new ConfigManager();
        ConfigManager.LoadConfig();

        // if (ConfigManager.SecretAdminConfig.AutoUpdater)
        //     AutoUpdater.CheckForUpdates();

        Utils.RemoveOldLogs(ConfigManager.SecretAdminConfig.DeleteLogsDays);
        Utils.ArchiveOldLogs(ConfigManager.SecretAdminConfig.ArchiveLogsDays);
        
        CommandHandler = new CommandHandler();

        Server = new ScpServer(port, args);
        Server.Start();
        
        InputManager.Start();
    }

    private static async void OnError(object obj, UnhandledExceptionEventArgs ev)
    {
        _exceptionalExit = true;
        
        if (Paths.MainFolder is null)
            Paths.Load(0);

        try
        {
            AnsiConsole.WriteException((Exception)ev.ExceptionObject);
            await File.WriteAllTextAsync(Path.Combine(Paths.ProgramLogsFolder, $"{DateTime.Now:MM.dd.yyyy-hh.mm.ss}-crash.log"), AnsiConsole.ExportText());
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
        
        await Task.Delay(1000);
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
            Task.Delay(0);
            File.WriteAllText(Path.Combine(Paths.ProgramLogsFolder, $"{DateTime.Now:MM.dd.yyyy-hh.mm.ss}.log"), AnsiConsole.ExportText());
        }

        AnsiConsole.MarkupLine("[green]Exited safely! Program can be killed.[/]");
    }
}