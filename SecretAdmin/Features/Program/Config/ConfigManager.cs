using System.IO;
using YamlDotNet.Serialization;

namespace SecretAdmin.Features.Program.Config
{
    public class ConfigManager
    {
        public MainConfig SecretAdminConfig { get; private set; } = new ();
        private readonly Serializer _serializer = new ();
        private readonly Deserializer _deserializer = new ();

        public void LoadConfig()
        {
            if(!File.Exists(Paths.ProgramConfig))
                SaveConfig(new MainConfig());
            
            SecretAdminConfig = _deserializer.Deserialize<MainConfig>(File.ReadAllText(Paths.ProgramConfig));
        }

        public void SaveConfig(MainConfig config)
        {
            File.WriteAllText(Paths.ProgramConfig, _serializer.Serialize(config));
            LoadConfig();
        }
    }
}