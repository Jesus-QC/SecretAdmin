using System;
using System.Text.RegularExpressions;
using SecretAdmin.API.Events.EventArgs;
using Spectre.Console;
using SConsole = System.Console;
using static SecretAdmin.Program;
using SEvents = SecretAdmin.API.Events.Handlers.Server;

namespace SecretAdmin.Features.Console
{
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
            WriteLine("[thistle1]Released under MIT License Copyright © Jesus-QC 2021[/]");

            if (ConfigManager.SecretAdminConfig.ManualStart)
                ReadKey();
        }
        
        public static void Input(string message, string title = "SERVER")
        {
            Write($"\n[mistyrose1]{title} >>> [/]");
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
        
        public static void Raw(object message, ConsoleColor color = ConsoleColor.White, bool showTimeStamp = true) => WriteLine(showTimeStamp ? $"[[{DateTime.Now:T}]] {message.ToString().EscapeMarkup()}" : message, color);
        public static void SpectreRaw(object message, string color = "white", bool showTimeStamp = false, string timestampColor = "white")
        {
            WriteLine(showTimeStamp ? $"[{timestampColor}][[{DateTime.Now:T}]][/] [{color}]{message.ToString().EscapeMarkup()}[/]" : $"[{color}]{message.ToString().EscapeMarkup()}[/]");
        }

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
        }
        
        public static void Write(object message = null, ConsoleColor color = ConsoleColor.White)
        {
            SConsole.ForegroundColor = color;
            AnsiConsole.Markup(message?.ToString() ?? "");
        }

        public static T GetOption<T>(string msg, T def) => AnsiConsole.Ask($"[lightcyan3]{msg}[/]", def);
        public static bool GetConfirm(string msg, bool def) => AnsiConsole.Confirm($"[lightcyan3]{msg}[/]", def);
        
        public static void HandleMessage(string message, byte code)
        {
            var ev = new ReceivedMessageEventArgs(message, code);
            SEvents.OnReceivedMessage(ev);

            message = ev.Message;
            code = ev.Code;
            
            if(!ev.IsAllowed|| string.IsNullOrWhiteSpace(message))
                return;
            
            var match = FrameworksRegex.Match(message);

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
                        SpectreRaw(message, "springgreen3", true, "slateblue1");
                        break;
                    case 15:
                        SpectreRaw(message, "mediumpurple4", true, "mediumpurple4");
                        break;
                    case 6:
                        SpectreRaw(message, "dodgerblue1", true, "mediumpurple4");
                        break;
                    default:
                        Raw(message, (ConsoleColor)code);
                        break;
                }
            }
        }
        
        public static void DeletePrevConsoleLine()
        {
            if (SConsole.CursorTop == 0) return;
            SConsole.SetCursorPosition(0, SConsole.CursorTop - 1);
            SConsole.Write(new string(' ', SConsole.WindowWidth));
            SConsole.SetCursorPosition(0, SConsole.CursorTop - 1);
        }
    }
}