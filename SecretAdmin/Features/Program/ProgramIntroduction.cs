using System;
using System.IO;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program.Config;
using Spectre.Console;

namespace SecretAdmin.Features.Program
{
    public static class ProgramIntroduction
    {
        public static bool FirstTime => !File.Exists(Paths.ProgramConfig);
        
        public static void ShowIntroduction()
        {
            Log.Intro();
            Log.WriteLine();
            Log.Alert("Hi, welcome to SecretAdmin!");
            Log.Alert("It seems like your first time using it, so we have to configure some things before!");
            Log.WriteLine("Press any key to continue.", ConsoleColor.Green);
            System.Console.ReadKey();
            
            // Program Options

            var cfg = new MainConfig
            {
                AutoUpdater = Log.GetConfirm("Do you want to enable the auto updater?", true),
                ManualStart = Log.GetConfirm("Do you want to manually have to enter a key to start the server?", false),
                SafeShutdown = Log.GetConfirm("Do you want to safe shutdown the game processes?", true),
                ArchiveLogsDays = Log.GetOption("In how many days the logs should be archived?", 1),
                RestartOnCrash = Log.GetConfirm("Should the server automatically restart itself when it crashes?", true),
                RestartWithLowMemory = Log.GetConfirm("Should the server restart itself when it has low memory?", true),
                MaxDefaultMemory = Log.GetOption("Max memory the server can use, in MB.", 2048)
            };

            Paths.Load();
            SecretAdmin.Program.ConfigManager.SaveConfig(cfg);
            
            Log.WriteLine();
            Log.SpectreRaw($"That were all the program configs! You can edit them always in {Paths.ProgramConfig}.","skyblue2");
            Log.Alert("Time to edit the default server configs.\n");

            // Server Options

            var srvConfig = new ServerConfig()
            {
                Port = Log.GetOption("Which should be the default server port?", (uint)7777),
                RoundsToRestart = Log.GetOption("In how many rounds the server should restart itself. -1 disable, 0 every round", -1)
            };
            
            SecretAdmin.Program.ConfigManager.SaveServerConfig(srvConfig);

            // Start the server
            
            Log.Alert("Ok, thats all! Time to enjoy the server :)");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
            Directory.CreateDirectory(Paths.MainFolder);
        }
    }
}