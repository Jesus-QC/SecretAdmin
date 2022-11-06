using System;
using System.Text.RegularExpressions;
using Spectre.Console;
using SConsole = System.Console;

namespace SecretAdmin.Features.Console;

public static class Log 
{
    private static readonly Regex FrameworksRegex = new (@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);

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
        WriteLine($"[thistle1]Released under MIT License Copyright © Jesus-QC {DateTime.Now.Year}[/]");
    }
        
    public static void Input(string message, string title = "SERVER")
    {
        Write($"[mistyrose1]{title} >>> [/]");
        WriteLine(message.EscapeMarkup());
    }
        
    public static void Alert(object message, bool showTimeStamp = true)
    {
        if (showTimeStamp)
            Write($"[[{DateTime.Now:T}]] ", ConsoleColor.DarkRed);
            
        Write("[[SecretAdmin]] ", ConsoleColor.Yellow);
        SConsole.Write("(Alert) ");
        WriteLine(message, ConsoleColor.Gray);
    }

    public static void ReadKey()
    {
        SConsole.ForegroundColor = ConsoleColor.White;
        WriteLine();
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
        
    public static void Raw(object message, ConsoleColor color = ConsoleColor.White, bool showTimeStamp = true) => WriteLine(showTimeStamp ? $"[[{DateTime.Now:T}]] {message.ToString().EscapeMarkup()}" : message, color);
    public static void SpectreRaw(object message, string color = "white", bool showTimeStamp = false, string timestampColor = "white") => WriteLine(showTimeStamp ? $"[{timestampColor}][[{DateTime.Now:T}]][/] [{color}]{message}[/]" : $"[{color}]{message}[/]");

    private static void Info(string title, string message)
    {
        Write($"[[{DateTime.Now:T}]] ", ConsoleColor.Magenta);
        Write("[[INFO]] ", ConsoleColor.Cyan);
        Write($"[{title}] ", ConsoleColor.Yellow);
        WriteLine(message.EscapeMarkup());
    }

    private static void Error(string title, string message)
    {
        Write($"[[{DateTime.Now:T}]] ", ConsoleColor.Magenta);
        Write("[deeppink2][[ERROR]][/] ");
        Write($"[{title}] ", ConsoleColor.Yellow);
        WriteLine($"[deeppink2]{message.EscapeMarkup()}[/]");
    }

    private static void Debug(string title, string message)
    {
        Write($"[grey58][[{DateTime.Now:T}]][/] ");
        Write("[[DEBUG]] ", ConsoleColor.Blue);
        Write($"[grey58][{title}][/] ");
        WriteLine(message.EscapeMarkup(), ConsoleColor.Cyan);
    }

    private static void Warn(string title, string message)
    {
        Write($"[[{DateTime.Now:T}]] ", ConsoleColor.Magenta);
        Write("[gold1][[WARN]][/] ");
        Write($"[plum2][{title}][/] ", ConsoleColor.DarkYellow);
        WriteLine(message.EscapeMarkup(), ConsoleColor.Yellow);
    }

    public static void WriteLine(object message = null, ConsoleColor color = ConsoleColor.White)
    {
        SConsole.ForegroundColor = color;
        AnsiConsole.MarkupLine(message?.ToString() ?? "");
        SConsole.ForegroundColor = ConsoleColor.White;
    }
        
    public static void Write(object message = null, ConsoleColor color = ConsoleColor.White)
    {
        SConsole.ForegroundColor = color;
        AnsiConsole.Markup(message?.ToString() ?? "");
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
        
    public static void HandleMessage(string message, byte code)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;
            
        Match match = FrameworksRegex.Match(message);

        if (match.Success)
        {
            if (match.Groups.Count > 3)
            {
                switch (match.Groups[1].Value.Trim())
                {
                    case "INFO":
                        Info(match.Groups[2].Value, match.Groups[3].Value);
                        break;
                    case "WARN":
                        Warn(match.Groups[2].Value, match.Groups[3].Value);
                        break;
                    case "DEBUG":
                        Debug(match.Groups[2].Value, match.Groups[3].Value);
                        break;
                    case "ERROR":
                        Error(match.Groups[2].Value, match.Groups[3].Value);
                        break;
                }
            }
        }
        else
        {
            switch (code)
            {
                case 10:
                    SpectreRaw(message.EscapeMarkup(), "springgreen3", true, "slateblue1");
                    break;
                case 15:
                    SpectreRaw(message.EscapeMarkup(), "mediumpurple4", true, "mediumpurple4");
                    break;
                case 6:
                    SpectreRaw(message.EscapeMarkup(), "dodgerblue1", true, "mediumpurple4");
                    break;
                default:
                    Raw(message, (ConsoleColor)code);
                    break;
            }
        }
    }
}