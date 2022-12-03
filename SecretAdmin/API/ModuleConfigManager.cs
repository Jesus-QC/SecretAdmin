using System;
using System.Collections.Generic;
using System.IO;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Program.Config;
using Spectre.Console;

namespace SecretAdmin.API;

public static class ModuleConfigManager
{
    public static readonly Dictionary<Type, string> ConfigPaths = new ();
    public static readonly Dictionary<Type, IModuleConfig> SavedConfigs = new();
    
    public static T GetConfig<T>() where T : IModuleConfig
    {
        Type type = typeof(T);
        return (T)SavedConfigs[type];
    }
    
    public static void SaveConfig<T>() where T : IModuleConfig
    {
        Type type = typeof(T);
        string path = ConfigPaths[type];
        File.WriteAllText(path, ConfigManager.Serializer.Serialize(GetConfig<T>()));
    }

    public static void RegisterConfig<T>(string path) where T : IModuleConfig
    {
        RegisterConfigFullPath<T>(Path.Combine(Paths.ModulesConfigFolder, path));
    }
    
    public static void RegisterConfigFullPath<T>(string path) where T : IModuleConfig
    {
        Type type = typeof(T);
        T config = Activator.CreateInstance<T>();
        
        File.WriteAllText(path, ConfigManager.Serializer.Serialize(config));

        ConfigPaths.Add(type, path);
        SavedConfigs.Add(type, config);
    }
}