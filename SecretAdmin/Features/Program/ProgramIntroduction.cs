using System;
using System.IO;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program.Config;
using Spectre.Console;

namespace SecretAdmin.Features.Program;

public static class ProgramIntroduction
{
    public static void ShowIntroduction()
    {
        Log.Alert("\nHi, welcome to SecretAdmin!");
        Log.Alert("We have to configure some things before starting!\n");

        AnsiConsole.WriteLine("[red]Configuration[/]");

        // Program Options
        
        MainConfig cfg = new ()
        {
            SafeShutdown = Log.GetConfirm("Do you want to safe shutdown the game processes?", true),
            ArchiveLogsDays = Log.GetOption("In how many days the logs should be archived?", 1),
            DeleteLogsDays = Log.GetOption("In how many days the logs should be deleted?", 2),
            RestartOnCrash = Log.GetConfirm("Should the server automatically restart itself when it crashes?", true),
            RestartWithLowMemory = Log.GetConfirm("Should the server restart itself when it has low memory?", true),
            MaxDefaultMemory = Log.GetOption("Max memory the server can use, in MB.", 2048),
            EnableModules = Log.GetConfirm("Do you want to use Modules? Found in our discord.", false),
            TimeOffset = Log.GetOption($"{DateTime.Now:T} In hours, write how many hours should be added/removed (For example -2).", 0),
        };
        
        SecretAdmin.Program.ConfigManager.SaveConfig(cfg);
        
        Log.SpectreRaw("\nThat were all the program configs! You can edit them always in:","skyblue2");
        Log.Path(Paths.ProgramConfig);

        Directory.CreateDirectory(Paths.MainFolder);
    }
}