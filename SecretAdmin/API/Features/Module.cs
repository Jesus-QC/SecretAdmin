using System;
using System.Reflection;
using SecretAdmin.Features.Console;

namespace SecretAdmin.API.Features
{
    public abstract class Module : IModule
    {
        protected Module()
        {
            Assembly = Assembly.GetCallingAssembly();
            Name ??= Assembly.GetName().Name;
            Author ??= "Unknown";
            Version ??= Assembly.GetName().Version;
        }
        
        private Assembly Assembly { get; }
        public virtual string Name { get; set; }
        public virtual string Author { get; set; }
        public virtual Version Version { get; set; }
        
        public virtual void OnEnabled()
        {
            Log.Raw($"The module {Name} [{Version}] by {Author} was enabled.", ConsoleColor.DarkMagenta);
        }

        public virtual void OnDisabled()
        {
            Log.Raw($"The module {Name} [{Version}] by {Author} was disabled.", ConsoleColor.DarkMagenta);
        }
        
        public virtual void OnRegisteringCommands()
        {
            Program.CommandHandler.RegisterCommands(Assembly);
        }
    }
}