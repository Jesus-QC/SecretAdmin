using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Program.Config;
using SecretAdmin.Features.Server;
using SecretAdmin.Features.Server.Commands;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin
{
    class Program
    {
        public static Version Version { get; } = new (0, 0, 0,1);
        public static ScpServer Server { get; private set; }
        public static CommandHandler CommandHandler { get; private set; }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            
            Console.Title = $"SecretAdmin [v{Version}]";

            Log.Intro();
            Console.ReadKey();
            
            if (ProgramIntroduction.FirstTime)
                ProgramIntroduction.ShowIntroduction();

            Paths.Load();
            
            CommandHandler = new CommandHandler();
            
            Server = new ScpServer(new ServerConfig());
            Server.Start();

            InputManager.Start();
        }

        private static void OnExit(object obj, EventArgs ev)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nExit Detected. Killing game process.");

            Server?.Kill();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Everything seems good to go! Bye :)");
        }
    }
}