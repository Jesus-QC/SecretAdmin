using System;

namespace SecretAdmin.Features.Server.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string Name { get; }

        public ConsoleCommandAttribute(string name)
        {
            Name = name;
        }
    }
}