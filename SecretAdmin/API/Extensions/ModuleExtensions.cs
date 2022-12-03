using System;
using System.IO;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Program.Config;
using Spectre.Console;

namespace SecretAdmin.API.Extensions;

public static class ModuleExtensions
{
    public static string GetConfigPath(this IModule module)
    {
        string cleanName = module.Name;
        
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
            cleanName = cleanName.Replace(invalidChar, '_');

        return Path.Combine(Paths.ModulesConfigFolder, cleanName + ".yml");
    }
}