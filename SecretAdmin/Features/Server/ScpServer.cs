using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program.Config;
using SecretAdmin.Features.Server.Enums;
using static System.String;

namespace SecretAdmin.Features.Server
{
    public class ScpServer
    {
        public SocketServer Socket { get; private set; }
        public ServerConfig Config { get; }
        public DateTime StartedTime;
        public ServerStatus Status;
        public int Rounds;
        
        
        private Process _serverProcess;
        
        
        public ScpServer(ServerConfig config) => Config = config;

        public void Start()
        {
            var fileName = "SCPSL.x86_64";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                fileName = "SCPSL.exe";

            if (!File.Exists(fileName))
            {
                System.Console.WriteLine();
                Log.Alert("Executable not found, make sure this file is on the same folder as LocalAdmin.");
                System.Console.ReadLine();
                Environment.Exit(-1);
            }
            
            _serverProcess?.Dispose();
            Socket = new SocketServer(this);
            
            var gameArgs = new List<string> { "-batchmode", "-nographics", "-silent-crashes", "-nodedicateddelete", $"-id{Process.GetCurrentProcess().Id}", $"-console{Socket.Port}", $"-port{Config.Port}" };
            var startInfo = new ProcessStartInfo(fileName, Join(' ', gameArgs)) { CreateNoWindow = true, UseShellExecute = false };
            
            System.Console.WriteLine();
            Log.Alert("Starting server on port 7777.");
            System.Console.WriteLine();
            
            _serverProcess = Process.Start(startInfo);
            _serverProcess!.Exited += OnExited;
            _serverProcess.EnableRaisingEvents = true;
            
            Status = ServerStatus.Online;
            StartedTime = DateTime.Now;
            Rounds = 0;
        }

        private void OnExited(object o, EventArgs e)
        {
            switch (Status)
            {
                case ServerStatus.Restarting:
                    Restart();
                    break;
                case ServerStatus.Exiting:
                    Kill();
                    Environment.Exit(0);
                    break;
                case ServerStatus.Online:
                    Log.Raw(@"
   █████████                              █████      ███
  ███░░░░░███                            ░░███      ░███
 ███     ░░░  ████████   ██████    █████  ░███████  ░███
░███         ░░███░░███ ░░░░░███  ███░░   ░███░░███ ░███
░███          ░███ ░░░   ███████ ░░█████  ░███ ░███ ░███
░░███     ███ ░███      ███░░███  ░░░░███ ░███ ░███ ░░░ 
 ░░█████████  █████    ░░████████ ██████  ████ █████ ███
  ░░░░░░░░░  ░░░░░      ░░░░░░░░ ░░░░░░  ░░░░ ░░░░░ ░░░ ", ConsoleColor.DarkYellow, false);
                    Restart();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void Kill()
        {
            Socket?.Dispose();
            Socket = null;
            _serverProcess?.Kill();
        }

        public void Restart()
        {
            Kill();
            Start();
        }

        public void ForceRestart()
        {
            Status = ServerStatus.Restarting;
            Restart();
        }
    }
}