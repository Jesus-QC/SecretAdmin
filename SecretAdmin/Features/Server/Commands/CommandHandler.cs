using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.Features.Server.Commands;

public class CommandHandler
{
    private readonly Dictionary<string, MethodInfo> _commands = new();
    
    [ConsoleCommand("Ram")]
    private void ShowRamUsage()
    {
        Log.Alert($"RAM USAGE: {SecretAdmin.Program.Server.MemoryManager.GetMemory()}MB");
    }
    
    [ConsoleCommand("StdErr")]
    private void StdErr()
    {
        Log.Alert("StdErr logs toggled.");
        SecretAdmin.Program.Server.ToggleStdErr();
    }
        
    [ConsoleCommand("StdOut")]
    private void StdOut()
    {
        Log.Alert("StdOut logs toggled.");
        SecretAdmin.Program.Server.ToggleStdOut();
    }

    [ConsoleCommand("Exit", new[] { "Quit" })]
    private void ExitCommand()
    {
        try
        {
            SecretAdmin.Program.Server.Status = ServerStatus.ExitingNextRound;
            SecretAdmin.Program.Server.SocketServer.SendMessage("exit");
            System.Console.Clear();
            
            Log.WriteLine(@".   ___     _ _   _             .", ConsoleColor.Red);
            Log.WriteLine(@"|  | __|_ _(_) |_(_)_ _  __ _   |", ConsoleColor.DarkCyan);
            Log.WriteLine(@"|  | _|\ \ / |  _| | ' \/ _` |  |", ConsoleColor.Yellow);
            Log.WriteLine(@"|  |___/_\_\_|\__|_|_||_\__, |  |", ConsoleColor.DarkMagenta);
            Log.WriteLine(@".                       |___/   .", ConsoleColor.Red);

            Log.SpectreRaw("Stopping the server safely.", "lightslateblue");
        }
        catch
        {
            Environment.Exit(-1);
        }
    }
        
    [ConsoleCommand("SR", new [] { "serverrestart" })]
    private void SoftRestartCommand()
    {
        Log.SpectreRaw("Restarting the server...", "lightslateblue");
        SecretAdmin.Program.Server.SocketServer.SendMessage("sr");
    }
    
    public CommandHandler()
    {
        TypeInfo ti = typeof(CommandHandler).GetTypeInfo();
            
        foreach (MethodInfo method in ti.DeclaredMethods)
        {
            IEnumerable<Attribute> attributes = method.GetCustomAttributes();

            if (attributes.FirstOrDefault() is ConsoleCommandAttribute query)
            {
                _commands.Add(query.Name.ToLower(), method);

                foreach (string alias in query.Aliases)
                {
                    _commands.Add(alias.ToLower(), method);
                }
            }
        }
    }

    public bool SendCommand(string name)
    {
        name = name.ToLower();

        if (!_commands.ContainsKey(name)) return false;
            
        Log.Input(name, "SecretAdmin");
        _commands[name].Invoke(this, Array.Empty<object>());
        return true;
    }
}