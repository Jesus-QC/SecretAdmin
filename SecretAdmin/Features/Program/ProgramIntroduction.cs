using System;
using System.IO;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program.Config;

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
                AutoUpdater = GetOption("Do you want to enable the auto updater?"),
                ManualStart = GetOption("Do you want to manually have to enter a key to start the server?"),
                SafeShutdown = GetOption("Do you want to safe shutdown the game processes?"),
                ArchiveLogsDays = GetOption("In how many days the logs should be archived?", "1"),
                RestartOnCrash = GetOption("Should the server automatically restart itself when it crashes?"),
                RestartWithLowMemory = GetOption("Should the server restart itself when it has low memory?"),
                MaxDefaultMemory = GetOption("Max memory the server can use, in MB.", "2048")
            };

            Paths.Load();
            SecretAdmin.Program.ConfigManager.SaveConfig(cfg);
            
            Log.WriteLine();
            Log.Raw("That were all the program configs! You can edit them always in /SecretAdmin/config.yml.", ConsoleColor.Cyan);
            Log.Alert("Time to edit the default server configs.");

            // Server Options

            var srvConfig = new ServerConfig()
            {
                Port = (uint)GetOption("Which should be the default server port?", "7777"),
                RoundsToRestart = GetOption("In how many rounds the server should restart itself. -1 disable, 0 every round", "-1")
            };
            
            SecretAdmin.Program.ConfigManager.SaveServerConfig(srvConfig);

            // Start the server
            
            Log.Alert("Ok, thats all! Time to enjoy the server :)");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
            Directory.CreateDirectory("SecretAdmin");
        }

        private static bool GetOption(string msg)
        {
            START:
            Log.Alert($"{msg} yes (y) / no (n)");
            var opt = System.Console.ReadLine()?.ToLower();
            if (!string.IsNullOrWhiteSpace(opt) && (opt[0] == 'y' || opt[0] == 'n'))
                return opt[0] == 'y';
            Log.Alert("An error occurred parsing the input, please try again!");
            goto START;
        }
        
        private static int GetOption(string msg, string def)
        {
            START:
            Log.Alert($"{msg} introduce a number. (default = {def})");
            var opt = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(opt))
                return int.Parse(def);
            if (int.TryParse(opt, out var z))
                return z;
            Log.Alert("An error occurred parsing the input, please try again!");
            goto START;
        }
    }
}