using System;
using System.IO;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program.Config;

namespace SecretAdmin.Features.Program
{
    public class ProgramIntroduction
    {
        public static bool FirstTime => !File.Exists(Paths.ProgramConfig);
        
        public static void ShowIntroduction()
        {
            Log.Intro();
            Log.WriteLine("");
            Log.Alert("Hi, welcome to SecretAdmin!");
            Log.Alert("It seems like your first time using it, so we have to configure some things before!");
            Log.WriteLine("Press any key to continue.", ConsoleColor.Green);
            System.Console.ReadKey();
            
            // Program Options

            var cfg = new MainConfig();

            cfg.AutoUpdater = GetOption("Do you want to enable the auto updater?");

            Paths.Load();
            SecretAdmin.Program.ConfigManager.SaveConfig(cfg);

            // Server Options

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
    }
}