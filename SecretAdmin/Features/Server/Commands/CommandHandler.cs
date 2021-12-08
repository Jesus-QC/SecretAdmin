using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Server.Enums;
using Spectre.Console;

namespace SecretAdmin.Features.Server.Commands
{
    public class CommandHandler
    {
        private readonly Dictionary<string, MethodInfo> _commands = new();
        
        [ConsoleCommand("Files")]
        private void FilesCommand()
        {
            var root = new Tree($"\n[white]{Paths.MainFolder}[/]");
            var logs = root.AddNode("[yellow]Logs[/]");
            root.AddNode("[yellow]Configs[/]");
            root.AddNode("[yellow]Modules[/]");
            root.AddNode("[blue]config.yml[/]");
            logs.AddNode("[lime]SecretAdmin[/]");
            logs.AddNode("[lime]Server[/]");

            AnsiConsole.Write(root);
            Log.WriteLine();
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start("explorer.exe", Paths.MainFolder);
        }
        
        [ConsoleCommand("sacreate")]
        private void CreateServerFileCommand()
        {
            
        }
        
        [ConsoleCommand("Ram")]
        private void ShowRamUsage()
        {
            Log.Alert($"RAM USAGE: {SecretAdmin.Program.Server.MemoryManager.GetMemory()}MB");
        }

        #region Console

        [ConsoleCommand("StdErr")]
        private void StdErr()
        {
            SecretAdmin.Program.Server.LogStdErr = !SecretAdmin.Program.Server.LogStdErr;
            Log.Raw($"Log StdErr: {SecretAdmin.Program.Server.LogStdErr}", ConsoleColor.DarkCyan);
        }
        
        [ConsoleCommand("StdOut")]
        private void StdOut()
        {
            SecretAdmin.Program.Server.LogStdOut = !SecretAdmin.Program.Server.LogStdOut;
            Log.Raw($"Log StdOut: {SecretAdmin.Program.Server.LogStdOut}", ConsoleColor.DarkCyan);
        }

        #endregion
        
        #region Exiled

        [ConsoleCommand("ExiledFolder")]
        private void ExiledFolder()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start("explorer.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED"));
        }

        [ConsoleCommand("exiled")]
        private void ExiledInstall()
        {
            ExiledInstaller.InstallExiled();
        }

        #endregion

        #region BaseCommandEnhances

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
        
        [ConsoleCommand("SR")]
        private void SoftRestartCommand()
        {
            Log.SpectreRaw("Restarting the server...", "lightslateblue");
            SecretAdmin.Program.Server.Socket.SendMessage("sr");
        }

        #endregion

        #region Manager

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

        #endregion
    }
}