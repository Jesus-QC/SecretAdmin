using System;

namespace SecretAdmin.Features.Server.Commands
{
    public class ConsoleCommandAttribute : Attribute
    {
        public string Name { get; }

        public ConsoleCommandAttribute(string name)
        {
            Name = name;
        }
    }
}