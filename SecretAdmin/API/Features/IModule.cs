using System;

namespace SecretAdmin.API.Features
{
    public interface IModule<out TCfg> where TCfg : IModuleConfig
    {
        string Name { get; set; }
        string Author { get; set; }
        Version Version { get; set; }
        TCfg Config { get; }
        
        void OnEnabled();
        void OnDisabled();
        void OnRegisteringCommands();
    }
}