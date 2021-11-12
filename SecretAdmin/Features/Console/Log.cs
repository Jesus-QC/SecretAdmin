using System;
using System.Text.RegularExpressions;
using SConsole = System.Console;
using static SecretAdmin.Program;

namespace SecretAdmin.Features.Console
{
    public class Log
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
            Write($"Secret Admin - Version v{SecretAdmin.Program.Version}");
            WriteLine(" by Jesus-QC", ConsoleColor.Blue);
            WriteLine("Released under MIT License Copyright © Jesus-QC 2021", ConsoleColor.Red);

            if (!ConfigManager.SecretAdminConfig.ManualStart)
                return;
            
            WriteLine("Press any key to continue.", ConsoleColor.Green);
            SConsole.ReadKey();
        }
        
        public static void Input(string message, string title = "SERVER")
        {
            SConsole.ForegroundColor = ConsoleColor.Yellow;
            SConsole.Write($"{title} >>> ");
            Raw(message, ConsoleColor.Magenta, false);
        }
        
        public static void Alert(string message, bool showTimeStamp = true)
        {
            if (showTimeStamp)
                Write($"[{DateTime.Now:T}] ", ConsoleColor.DarkRed);
            
            Write("[SecretAdmin] ", ConsoleColor.Yellow);
            SConsole.Write("(Alert) ");
            WriteLine(message, ConsoleColor.Gray);
        }
        
        // Alerts
        
        public static void Raw(string message, ConsoleColor color = ConsoleColor.White, bool showTimeStamp = true) => WriteLine(showTimeStamp ? $"[{DateTime.Now:T}] {message}" : message, color);
        
        private static void Info(string title, string message)
        {
            Write($"[{DateTime.Now:T}] ", ConsoleColor.Magenta);
            Write("[INFO] ", ConsoleColor.DarkCyan);
            Write($"{title} ", ConsoleColor.Yellow);
            WriteLine(message);
        }

        private static void Error(string title, string message)
        {
            Write($"[{DateTime.Now:T}] ", ConsoleColor.DarkYellow);
            Write("[ERROR] ", ConsoleColor.Red);
            Write($"{title} ", ConsoleColor.Yellow);
            WriteLine(message, ConsoleColor.Red);
        }

        private static void Debug(string title, string message)
        {
            Write($"[{DateTime.Now:T}] ", ConsoleColor.DarkGray);
            Write("[DEBUG] ", ConsoleColor.Blue);
            Write($"{title} ", ConsoleColor.DarkGray);
            WriteLine(message, ConsoleColor.Cyan);
        }

        private static void Warn(string title, string message)
        {
            Write($"[{DateTime.Now:T}] ", ConsoleColor.Yellow);
            Write("[WARN] ", ConsoleColor.Magenta);
            Write($"{title} ", ConsoleColor.DarkYellow);
            WriteLine(message, ConsoleColor.Yellow);
        }

        public static void WriteLine(string message = "", ConsoleColor color = ConsoleColor.White)
        {
            SConsole.ForegroundColor = color;
            SConsole.WriteLine(message);
            ProgramLogger?.AppendLog(message, true);
        }
        
        public static void Write(string message = "", ConsoleColor color = ConsoleColor.White)
        {
            SConsole.ForegroundColor = color;
            SConsole.Write(message);
            ProgramLogger?.AppendLog(message);
        }
        
        public static void HandleMessage(string message, byte code)
        {
            if(message == null)
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
                Raw(message, (ConsoleColor)code);
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