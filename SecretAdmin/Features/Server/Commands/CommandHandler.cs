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
    
    [ConsoleCommand("Quit")]
    private void QuitCommand()
    {
        ExitCommand();
    }
        
    [ConsoleCommand("Exit")]
    private void ExitCommand()
    {
        try
        {
            SecretAdmin.Program.Server.Status = ServerStatus.Exiting;
            SecretAdmin.Program.Server.SocketServer.SendMessage("exit");
            Log.SpectreRaw("Stopping the server safely.", "lightslateblue");
        }
        catch
        {
            Environment.Exit(-1);
        }
    }
        
    [ConsoleCommand("SR")]
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
                _commands.Add(query.Name.ToLower(), method);
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