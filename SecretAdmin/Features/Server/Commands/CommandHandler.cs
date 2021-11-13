using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SecretAdmin.API;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.Features.Server.Commands
{
    public class CommandHandler
    {
        private readonly Dictionary<string, MethodInfo> _commands = new();

        [ConsoleCommand("Ram")]
        private void ShowRamUsage()
        {
            Log.Alert($"RAM USAGE: (X)MB"); // TODO: calculate this
        }
        
        [ConsoleCommand("exiled")]
        private void ExiledInstall()
        {
            ExiledInstaller.InstallExiled();
        }
        
        [ConsoleCommand("Quit")]
        private void QuitCommand()
        {
            ExitCommand();
        }
        
        [ConsoleCommand("Exit")]
        private void ExitCommand()
        {
            SecretAdmin.Program.Server.Status = ServerStatus.Exiting;
            SecretAdmin.Program.Server.Socket.SendMessage("exit");
        }

        public CommandHandler()
        {
            var ti = typeof(CommandHandler).GetTypeInfo();
            
            foreach (var method in ti.DeclaredMethods)
            {
                var attributes = method.GetCustomAttributes();
                
                if (attributes.FirstOrDefault() is ConsoleCommandAttribute query)
                    _commands.Add(query.Name.ToLower(), method);
            }
        }

        public void RegisterCommands(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetTypeInfo().DeclaredMethods)
                {
                    var attributes = method.GetCustomAttributes();

                    if (attributes.FirstOrDefault() is not ConsoleCommandAttribute query) 
                        continue;
                    
                    if (!method.IsStatic)
                    {
                        Log.Raw($"[Warn] The command {query.Name} couldn't be registered due to not being static.", ConsoleColor.DarkYellow);
                        continue;
                    }

                    var cmd = query.Name.ToLower();
                    
                    if (_commands.ContainsKey(cmd))
                    {
                        Log.Raw($"[Error] The command \"{query.Name}\" already exists inside the module {_commands[cmd].Module.Assembly.GetName().Name}.", ConsoleColor.Red);
                        continue;
                    }

                    _commands.Add(cmd, method);
                }
            }
        }

        public bool SendCommand(string name)
        {
            name = name.ToLower();

            if (!_commands.ContainsKey(name)) return false;
            
            Log.Input(name, "SecretAdmin");
            _commands[name].Invoke(this, new object[]{});
            return true;
        }
    }
}