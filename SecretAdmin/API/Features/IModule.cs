using System;

namespace SecretAdmin.API.Features
{
    public interface IModule
    {
        string Name { get; set; }
        string Author { get; set; }
        Version Version { get; set; }

        void OnEnabled();
    }
}