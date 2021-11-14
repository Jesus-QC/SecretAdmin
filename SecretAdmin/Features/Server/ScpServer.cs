using System;
using System.Collections.Generic;
using System.Diagnostics;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program.Config;
using SecretAdmin.Features.Server.Enums;
using SEvents = SecretAdmin.API.Events.Handlers.Server;

namespace SecretAdmin.Features.Server
{
    public class ScpServer
    {
        public MemoryManager MemoryManager { get; private set; }
        public SocketServer Socket { get; private set; }
        public ServerConfig Config { get; }

        public ServerStatus Status;
        public DateTime StartedTime; //TODO: .
        public int Rounds; //TODO: .
        
        private Process _serverProcess;
        private Logger _logger;
        private Logger _outputLogger;
        
        public ScpServer(ServerConfig config) => Config = config;

        public void Start()
        {
            if (!Utils.GetExecutable(out var fileName))
            {
                System.Console.ReadKey();
                Environment.Exit(-1);
                return;
            }
            
            Utils.ArchiveServerLogs();
            _logger = new Logger(Utils.GetLogsName(Config.Port));
            _outputLogger = new Logger(Utils.GetOutputLogsName(Config.Port));

            _serverProcess?.Dispose();
            Socket = new SocketServer(this);
            
            var gameArgs = new List<string> { "-batchmode", "-nographics", "-silent-crashes", "-nodedicateddelete", $"-id{Process.GetCurrentProcess().Id}", $"-console{Socket.Port}", $"-port{Config.Port}" };
            var startInfo = new ProcessStartInfo(fileName, string.Join(' ', gameArgs)) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            
            Log.WriteLine();
            Log.Alert($"Starting server on port {Config.Port}.");
            Log.WriteLine();

            _serverProcess = Process.Start(startInfo);
            _serverProcess!.Exited += OnExited;
            _serverProcess.EnableRaisingEvents = true;
            _serverProcess.ErrorDataReceived += (_, args)  => AddOutputLog(args.Data, "[STDERR]");
            _serverProcess.OutputDataReceived += (_, args) => AddOutputLog(args.Data, "[STDOUT]");
            _serverProcess.BeginErrorReadLine();
            _serverProcess.BeginOutputReadLine();

            Status = ServerStatus.Online;
            StartedTime = DateTime.Now;
            Rounds = 0;

            MemoryManager = new MemoryManager(_serverProcess);
            MemoryManager.Start();
        }

        private void OnExited(object o, EventArgs e)
        {
            switch (Status)
            {
                case ServerStatus.Restarting:
                    Restart();
                    return;
                
                case ServerStatus.Exiting:
                    Kill();
                    Environment.Exit(0);
                    return;
                
                case ServerStatus.Online:
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
                        Log.Raw("Server crashed, press any key to close SecretAdmin."); System.Console.ReadKey();
                    return;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void Kill()
        {
            MemoryManager.Dispose();
            MemoryManager = null;
            Socket?.Dispose();
            Socket = null;
            _serverProcess?.Kill();
        }

        public void Restart()
        {
            SEvents.OnRestarted();
            Kill();
            Start();
        }

        public void ForceRestart()
        {
            Status = ServerStatus.Restarting;
            Restart();
        }

        public void AddLog(string message, string title = null)
        {
            if(string.IsNullOrEmpty(message))
                return;
            
            _logger.AppendLog(title == null ? message : $"{title} {message}", true);
        }
        
        public void AddOutputLog(string message, string title = null)
        {
            if(string.IsNullOrEmpty(message))
                return;
            
            _outputLogger.AppendLog(title == null ? message : $"{title} {message}", true);
        }
    }
}