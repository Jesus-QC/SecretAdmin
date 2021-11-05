using System;
using System.IO;
using SecretAdmin.Features.Console;

namespace SecretAdmin.Features.Program
{
    public class ProgramIntroduction
    {
        public static bool FirstTime => !Directory.Exists("SecretAdmin");
        
        public static void ShowIntroduction()
        {
            System.Console.WriteLine();
            Log.Alert("Hi, welcome to SecretAdmin!");
            Log.Alert("It seems like your first time using it, so we have to configure some things before!");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
            
            // Options
            
            Log.Alert("Ok, thats all! Time to enjoy the server :)");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
            Directory.CreateDirectory("SecretAdmin");
        }
    }
}