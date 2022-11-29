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
        if (!args.IsAllowed)
            return;
        
        if (!Utils.TryGetExecutable(out string executablePath))
        {
            Environment.Exit(-1);
            return;
        }

        _process?.Dispose();
        SocketServer = new SocketServer();

        _serverLogger = new Logger(Path.Combine(Paths.ServerLogsFolder, $"{DateTime.Now:MM.dd.yyyy-hh.mm.ss}-server.log"));
        _outputLogger = new Logger(Path.Combine(Paths.ServerLogsFolder, $"{DateTime.Now:MM.dd.yyyy-hh.mm.ss}-output.log"));

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
        
        Log.Alert($"Starting server on port: {_port}.\n");
        
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

    public void Stop()
    {
        StoppingServerEventArgs args = new (this);
        if(!args.IsAllowed)
            return;
        
        Status = ServerStatus.Offline;

        if (MemoryManager is not null)
        {
            MemoryManager.Stop();
            MemoryManager = null;
        }

        if (SilentCrashHandler is not null)
        {
            SilentCrashHandler.Stop();
            SilentCrashHandler = null;
        }

        if (SocketServer is not null)
        {
            SocketServer.Stop();
            SocketServer = null;
        }
        
        _process.Kill();
    }

    public void Restart()
    {
        RestartingServerEventArgs args = new (this);
        Handler.OnRestartingServer(args);
        
        if (!args.IsAllowed)
            return;
            
        System.Console.Clear();
        Stop();
        Start();
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
                Log.Raw(@"
   █████████                              █████      ███
  ███░░░░░███                            ░░███      ░███
 ███     ░░░  ████████   ██████    █████  ░███████  ░███
░███         ░░███░░███ ░░░░░███  ███░░   ░███░░███ ░███
░███          ░███ ░░░   ███████ ░░█████  ░███ ░███ ░███
░░███     ███ ░███      ███░░███  ░░░░███ ░███ ░███ ░░░ 
 ░░█████████  █████    ░░████████ ██████  ████ █████ ███
  ░░░░░░░░░  ░░░░░      ░░░░░░░░ ░░░░░░  ░░░░ ░░░░░ ░░░ ",
                    ConsoleColor.DarkYellow, false);
                
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