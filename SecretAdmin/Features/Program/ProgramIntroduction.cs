using System;
using System.IO;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program.Config;

namespace SecretAdmin.Features.Program
{
    public class ProgramIntroduction
    {
        public static bool FirstTime => !Directory.Exists("SecretAdmin");
        
        public static void ShowIntroduction()
        {
            Log.WriteLine("");
            Log.Alert("Hi, welcome to SecretAdmin!");
            Log.Alert("It seems like your first time using it, so we have to configure some things before!");
            Log.WriteLine("Press any key to continue.", ConsoleColor.Green);
            System.Console.ReadKey();
            
            // Program Options

            var cfg = new MainConfig();
            
            Log.Alert("Do you want to enable the auto updater? (Y) yes (y) / no (n)");
            var au = System.Console.ReadLine()?.ToLower();
            if (au != null && (au[0] == 'y' || au[0] == 'n'))
                cfg.AutoUpdater = au[0] == 'y';

            Paths.Load();
            SecretAdmin.Program.ConfigManager.SaveConfig(cfg);

            // Server Options
            
            Log.Alert("Do you want to auto-install EXILED? (y/n)");
            var ai = System.Console.ReadLine()?.ToLower();
            if (ai != null && (ai[0] == 'y' || ai[0] == 'n'))
                ExiledInstaller.InstallExiled();
            
            Log.Alert("Ok, thats all! Time to enjoy the server :)");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
            Directory.CreateDirectory("SecretAdmin");
        }
    }
}