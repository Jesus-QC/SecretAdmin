using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SecretAdmin.Features.Console;
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
            var methods = ti.DeclaredMethods;
            
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes();
                
                if (attributes.FirstOrDefault() is ConsoleCommandAttribute query)
                    _commands.Add(query.Name.ToLower(), method);
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