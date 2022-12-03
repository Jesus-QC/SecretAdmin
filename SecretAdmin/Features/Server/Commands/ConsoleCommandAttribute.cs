using System;

namespace SecretAdmin.Features.Server.Commands;

[AttributeUsage(AttributeTargets.Method)]
public class ConsoleCommandAttribute : Attribute
{
    public string Name { get; }
    public string[] Aliases { get; }
    
    public ConsoleCommandAttribute(string name, string[] aliases = null)
    {
        Name = name;
        Aliases = aliases ?? Array.Empty<string>();
    }
}