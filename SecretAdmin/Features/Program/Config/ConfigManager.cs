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

        public void SaveServerConfig(ServerConfig config)
        {
            File.WriteAllText(Path.Combine(Paths.ServerConfigsFolder, "default.yml"), _serializer.Serialize(config));
            File.WriteAllText(Path.Combine(Paths.ServerConfigsFolder, "7777.yml"), _serializer.Serialize(config));
        }
        
        public ServerConfig GetServerConfig(string name)
        {
            var def = Path.Combine(Paths.ServerConfigsFolder, "default.yml");
            
            if(!File.Exists(def))
                SaveServerConfig(new ServerConfig());

            if (name != null && File.Exists(Path.Combine(Paths.ServerConfigsFolder, name)))
                return _deserializer.Deserialize<ServerConfig>(File.ReadAllText(Path.Combine(Paths.ServerConfigsFolder, name)));

            return _deserializer.Deserialize<ServerConfig>(File.ReadAllText(def));
        }
    }
}