using System;
using Spectre.Console;
using SConsole = System.Console;

namespace SecretAdmin.Features.Console;

public static class Log 
{
    // Program Alerts
    public static void Intro()
    {
        SConsole.Clear();
        WriteLine(@" .--.                        .-.    .--.    .-.          _       
: .--'                      .' `.  : .; :   : :         :_;      
`. `.  .--.  .--. .--.  .--.`. .'  :    : .-' :,-.,-.,-..-.,-.,-.
 _`, :' '_.''  ..': ..'' '_.': :   : :: :' .; :: ,. ,. :: :: ,. :
`.__.'`.__.'`.__.':_;  `.__.':_;   :_;:_;`.__.':_;:_;:_;:_;:_;:_;
                                                                 ");
        Write($"[cyan]Secret Admin - Version v{SecretAdmin.Program.Version}[/]");
        WriteLine(" [lightgoldenrod1]by Jesus-QC[/]");
        WriteLine($"[thistle1]Released under MIT License Copyright © Jesus-QC 2021-{DateTime.Now.Year}[/]");
    }

    public static DateTime GetDateTimeWithOffset() => DateTime.Now.AddHours(SecretAdmin.Program.ConfigManager.SecretAdminConfig.TimeOffset);
    public static string GetTimeWithOffset() => GetDateTimeWithOffset().ToString("T");
    
    public static void Input(string message, string title = "SERVER")
    {
        Write($"[mistyrose1]{title} >>> [/]");
        WriteLine(message.EscapeMarkup());
    }
        
    public static void Alert(string message, bool showTimeStamp = true)
    {
        if (showTimeStamp)
            AddTimeStamp();
            
        Write("[[SecretAdmin]] ", ConsoleColor.Yellow);
        SConsole.Write("(Alert) ");
        WriteLine(message, ConsoleColor.Gray);
    }

    public static void ReadKey()
    {
        SConsole.WriteLine();
        AnsiConsole.Write(new Rule("[darkslategray3]Press any key to continue.[/]"));
        SConsole.ReadKey();
    }
        
    // Alerts

    public static void Path(string path)
    {
        TextPath p = new (path)
        {
            RootStyle = new Style(foreground: Color.Red),
            SeparatorStyle = new Style(foreground: Color.Green),
            StemStyle = new Style(foreground: Color.Blue),
            LeafStyle = new Style(foreground: Color.Yellow)
        };
        
        AnsiConsole.Write(p);
        SConsole.WriteLine();
    }

    public static void Raw(string message, ConsoleColor color = ConsoleColor.White, bool showTimeStamp = true)
    {
        if (showTimeStamp)
            AddTimeStamp();
        
        WriteLine(message.EscapeMarkup(), color);
    }

    public static void SpectreRaw(string message, string color = "white", bool showTimeStamp = false)
    {
        if (showTimeStamp)
            AddTimeStamp();
        
        WriteLine($"[{color}]{message}[/]");
    }

    private static void AddTimeStamp() => AnsiConsole.Markup($"[paleturquoise4][[[deepskyblue4_1]{GetTimeWithOffset()}[/]]][/] ");

    public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
    {
        SConsole.ForegroundColor = color;
        AnsiConsole.MarkupLine(message);
        SConsole.ForegroundColor = ConsoleColor.White;
    }
        
    public static void Write(string message, ConsoleColor color = ConsoleColor.White)
    {
        SConsole.ForegroundColor = color;
        AnsiConsole.Markup(message);
        SConsole.ForegroundColor = ConsoleColor.White;
    }

    public static int GetOption(string msg, int def)
    {
        AnsiConsole.MarkupLine($"[lightcyan3]{msg}[/] [springgreen3]({def})[/]:");
        
        while (true)
        {
            string input = System.Console.ReadLine();
            
            if (string.IsNullOrEmpty(input))
                return def;
            
            if (input is "exit")
                Environment.Exit(-1);

            if (int.TryParse(input, out int result))
                return result;
            
            AnsiConsole.MarkupLine($"[red]Invalid Input[/] Try again with a number:");
        }
    }

    public static bool GetConfirm(string msg, bool def)
    {
        AnsiConsole.MarkupLine($"[lightcyan3]{msg}[/] [dodgerblue1](y/n)[/] [springgreen3]({(def ? "Yes" : "No")})[/]:");
        
        while (true)
        {
            string input = System.Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                return def;
            
            if (input is "exit")
                Environment.Exit(-1);

            switch (input.Trim().ToLower())
            {
                case "yes" or "y":
                    return true;
                case "no" or "n":
                    return false;
            }
            
            AnsiConsole.MarkupLine($"[red]Invalid Input[/] Try again, it only accepts y or yes and n or no.");
        }
    }
        
    public static void HandleMessage(string message, ConsoleColor code)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;
            
        switch (code)
        {
            case ConsoleColor.Green:
                SpectreRaw(message.EscapeMarkup(), "springgreen3", true);
                break;
            case ConsoleColor.White:
                SpectreRaw(message.EscapeMarkup(), "mediumpurple4", true);
                break;
            case ConsoleColor.DarkYellow:
                SpectreRaw(message.EscapeMarkup(), "dodgerblue1", true);
                break;
            case ConsoleColor.Black:
            case ConsoleColor.DarkBlue:
            case ConsoleColor.DarkGreen:
            case ConsoleColor.DarkCyan:
            case ConsoleColor.DarkRed:
            case ConsoleColor.DarkMagenta:
            case ConsoleColor.Gray:
            case ConsoleColor.DarkGray:
            case ConsoleColor.Blue:
            case ConsoleColor.Cyan:
            case ConsoleColor.Red:
            case ConsoleColor.Magenta:
            case ConsoleColor.Yellow:
            default:
                Raw(message, code);
                break;
        }
    }
}