using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SecretAdmin.API.Events;
using SecretAdmin.API.Events.Arguments;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Server.Enums;
using SecretAdmin.Features.Server.Modules;
using Spectre.Console;

namespace SecretAdmin.Features.Server;

public class ScpServer
{
    private Process _process;

    private readonly int _port;
    private readonly string[] _args;
    
    public ServerStatus Status = ServerStatus.Offline;
    public SocketServer SocketServer;

    public MemoryManager MemoryManager;
    public SilentCrashHandler SilentCrashHandler;

    private bool _logStdErr;
    private bool _logStdOut;

    private Logger _serverLogger;
    private Logger _outputLogger;
    
    public ScpServer(int port = 7777, string[] args = null)
    {
        _port = port;
        _args = args;
    }
    
    public void Start()
    {
        StartingServerEventArgs args = new (this);
        Handler.OnStartingServer(args);
        
        if (!args.IsAllowed)
            return;

        SocketServer = new SocketServer();
        
        StartServerProcess();
    }

    private void StartServerProcess()
    {
        _process?.Dispose();
        
        if (!Utils.TryGetExecutable(out string executablePath))
        {
            Environment.Exit(-1);
            return;
        }
     
        Log.Alert($"Starting server on port: {_port}.\n");
        System.Console.Title = $"SecretAdmin [v{SecretAdmin.Program.Version}] - {_port}";

        DateTime now = Log.GetDateTimeWithOffset();
        string day = now.ToString("yyyy-MM-dd");
        
        Directory.CreateDirectory(Path.Combine(Paths.ServerLogsFolder, day));

        _serverLogger = new Logger(Path.Combine(Paths.ServerLogsFolder, day, $"{now:hh-mm-ss}-server.log"));
        _outputLogger = new Logger(Path.Combine(Paths.ServerLogsFolder, day, $"{now:hh-mm-ss}-output.log"));

        List<string> gameArgs = new() 
        {
            "-batchmode",
            "-nographics", 
            "-silent-crashes", 
            "-nodedicateddelete", 
            $"-id{Environment.ProcessId}", 
            $"-console{SocketServer.Port}", 
            $"-port{_port}",
        };

        if (SecretAdmin.Program.ConfigManager.SecretAdminConfig.RestartOnCrash)
        {
            gameArgs.Add("-heartbeat");
        }

        ProcessStartInfo startInfo = new(executablePath, string.Join(' ', gameArgs) + ' ' + string.Join(' ', _args ?? Array.Empty<string>()))
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        _process = Process.Start(startInfo);

        if (_process is null)
        {
            Log.Alert("An error occurred when starting the server process.");
            Log.ReadKey();
            Environment.Exit(-1);
        }
        
        _process.EnableRaisingEvents = true;
        
        _process.Exited += OnExited;
        _process.OutputDataReceived += StdOut;
        _process.ErrorDataReceived += StdErr;
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        
        Status = ServerStatus.Online;

        MemoryManager = new MemoryManager(_process);
        MemoryManager.Start();
        
        if (SecretAdmin.Program.ConfigManager.SecretAdminConfig.RestartOnCrash)
        {
            SilentCrashHandler = new SilentCrashHandler();
        }
    }
    
    private void KillMemoryManager()
    {
        if (MemoryManager is null) 
            return;
        
        MemoryManager.Stop();
        MemoryManager = null;
    }

    private void KillSilentCrashHandler()
    {
        if (SilentCrashHandler is null) 
            return;
        
        SilentCrashHandler.Stop();
        SilentCrashHandler = null;
    }

    private void KillSocket()
    {
        if (SocketServer is null)
            return;
        
        SocketServer.Stop();
        SocketServer = null;
    }
    
    public void Stop()
    {
        StoppingServerEventArgs args = new (this);
        Handler.OnStoppingServer(args);
        
        if(!args.IsAllowed)
            return;
        
        Status = ServerStatus.Offline;

        KillMemoryManager();
        KillSilentCrashHandler();
        KillSocket();
        
        _process.Kill();
    }

    public void Restart()
    {
        RestartingServerEventArgs args = new (this);
        Handler.OnRestartingServer(args);
        
        if (!args.IsAllowed)
            return;
            
        System.Console.Clear();

        Status = ServerStatus.Offline;
        
        KillMemoryManager();
        KillSilentCrashHandler();
        SocketServer.Reset();
        
        _process.Kill();
        
        StartServerProcess();
    }

    private void OnExited(object o, EventArgs e)
    {
        switch (Status)
        {
            case ServerStatus.RestartingNextRound:
            {
                Restart();
                break;
            }
            case ServerStatus.Offline:
            case ServerStatus.ExitingNextRound:
            {
                Stop();
                Environment.Exit(0);
                break;
            }
            case ServerStatus.Idle:
            case ServerStatus.Online:
            {
                Log.Raw(@"" + 
                        @"   █████████                              █████      ███"+
                        "  ███░░░░░███                            ░░███      ░███" +
                        " ███     ░░░  ████████   ██████    █████  ░███████  ░███" +
                        "░███         ░░███░░███ ░░░░░███  ███░░   ░███░░███ ░███" +
                        "░███          ░███ ░░░   ███████ ░░█████  ░███ ░███ ░███" +
                        "░░███     ███ ░███      ███░░███  ░░░░███ ░███ ░███ ░░░ " +
                        " ░░█████████  █████    ░░████████ ██████  ████ █████ ███" +
                        "  ░░░░░░░░░  ░░░░░      ░░░░░░░░ ░░░░░░  ░░░░ ░░░░░ ░░░ ", ConsoleColor.DarkYellow, false);
                
                if (SecretAdmin.Program.ConfigManager.SecretAdminConfig.RestartOnCrash)
                    Restart();
                else
                    Log.ReadKey();
                break;
            }
        }
    }

    public void ToggleStdOut() => _logStdOut = !_logStdOut;

    public void ToggleStdErr() => _logStdErr = !_logStdErr;
    

    private void StdOut(object _, DataReceivedEventArgs ev)
    {
        if (_logStdOut)
            Log.SpectreRaw("[[STDOUT]]" + ev.Data.EscapeMarkup(), "paleturquoise4");
        
        AddOutputLog(ev.Data, "STDOUT");
    }

    private void StdErr(object _, DataReceivedEventArgs ev)
    {
        if (_logStdErr)
            Log.SpectreRaw("[[STDERR]]" + ev.Data.EscapeMarkup(), "indianred");
        
        AddOutputLog(ev.Data, "STDERR");
    }

    public void AddLog(string message, string title = "")
    {
        if (string.IsNullOrEmpty(message))
            return;

        _serverLogger.AppendLog(string.IsNullOrWhiteSpace(title) ? message : $"[{title}] {message}");
    }

    private void AddOutputLog(string message, string title = "")
    {
        if (string.IsNullOrEmpty(message))
            return;

        _outputLogger.AppendLog(string.IsNullOrWhiteSpace(title) ? message : $"[{title}] {message}");
    }
}