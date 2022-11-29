using System;
using System.IO;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Program.Config;
using Spectre.Console;

namespace SecretAdmin.API.Extensions;

public static class ModuleExtensions
{
    public static string GetConfigPath<T>(this IModule<T> module) where T : IModuleConfig
    {
        string cleanName = module.Name;
        
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
            cleanName = cleanName.Replace(invalidChar, '_');

        return Path.Combine(Paths.ModulesConfigFolder, cleanName + ".yml");
    }

    public static void TryLoadConfig<T>(this IModule<T> module) where T : IModuleConfig
    {
        try
        {
            string configPath = module.GetConfigPath();
        
            if (File.Exists(configPath))
                module!.Config = ConfigManager.Deserializer.Deserialize<T>(File.ReadAllText(configPath));
        
            File.WriteAllText(configPath!, ConfigManager.Serializer.Serialize(module.Config));
        }
        catch (Exception e)
        {
            Log.Alert("There was an issue when loading the module configs.");
            AnsiConsole.WriteException(e);
        }
    }
}