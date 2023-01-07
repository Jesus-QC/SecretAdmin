using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.Features.Server.Commands;

public class CommandHandler
{
    public readonly Dictionary<string, MethodInfo> Commands = new();
    
    [ConsoleCommand("secretadmin-reconfigure")]
    private static void ReconfigureSecretAdmin()
    {
        ProgramIntroduction.ShowIntroduction();
        System.Console.WriteLine("Restart to apply the changes!");
    }
    
    [ConsoleCommand("Ram")]
    private static void ShowRamUsage()
    {
        Log.Alert($"RAM USAGE: {SecretAdmin.Program.Server.MemoryManager.GetMemory()}MB");
    }
    
    [ConsoleCommand("StdErr")]
    private static void StdErr()
    {
        Log.Alert("StdErr logs toggled.");
        SecretAdmin.Program.Server.ToggleStdErr();
    }
        
    [ConsoleCommand("StdOut")]
    private static void StdOut()
    {
        Log.Alert("StdOut logs toggled.");
        SecretAdmin.Program.Server.ToggleStdOut();
    }

    [ConsoleCommand("Exit", new[] { "Quit" })]
    private static void ExitCommand()
    {
        SecretAdmin.Program.Server.Status = ServerStatus.ExitingNextRound;
        SecretAdmin.Program.Server.SocketServer.SendMessage("exit");
        System.Console.Clear();
        Environment.Exit(0);
    }
        
    [ConsoleCommand("SR", new [] { "serverrestart" })]
    private static void SoftRestartCommand()
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
                Commands.Add(query.Name.ToLower(), method);

                foreach (string alias in query.Aliases)
                {
                    Commands.Add(alias.ToLower(), method);
                }
            }
        }
    }

    public bool SendCommand(string name)
    {
        name = name.ToLower();

        if (!Commands.ContainsKey(name)) return false;
            
        Log.Input(name, "SecretAdmin");
        Commands[name].Invoke(null, Array.Empty<object>());
        return true;
    }
}