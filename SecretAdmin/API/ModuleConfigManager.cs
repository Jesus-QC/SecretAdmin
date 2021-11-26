using System;
using System.IO;
using SecretAdmin.API.Features;
using SecretAdmin.Features.Program;

namespace SecretAdmin.API
{
    public static class ModuleConfigManager
    {
        public static string GetPath(this IModule<IModuleConfig> module) => Path.Combine(Paths.ModulesConfigFolder, module.Name);
        public static string GetPath(this IModule<IModuleConfig> module, uint port) => Path.Combine(module.GetPath(), port + ".yml");

        public static void LoadConfig(this IModule<IModuleConfig> module, uint port)
        {
            if (!File.Exists(module.GetPath(port)))
                module.SaveConfig(module.Config, port);
            
            var config = (IModuleConfig)Program.ConfigManager.Deserializer.Deserialize(File.ReadAllText(module.GetPath(port)), module.Config.GetType());
            module.Config.CopyProperties(config);
        }

        public static void SaveConfig(this IModule<IModuleConfig> module, IModuleConfig config, uint port)
        {
            module.CreateFolder();
            File.WriteAllText(module.GetPath(port), Program.ConfigManager.Serializer.Serialize(config));
        }

        private static void CreateFolder(this IModule<IModuleConfig> module) => Directory.CreateDirectory(module.GetPath());
        
        private static void CopyProperties(this object target, object source)
        {
            var type = target.GetType();

            if (type != source.GetType())
                return;

            foreach (var sourceProperty in type.GetProperties())
                type.GetProperty(sourceProperty.Name)?.SetValue(target, sourceProperty.GetValue(source, null), null);
        }
    }
}