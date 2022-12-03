using System.IO;
using SecretAdmin.Features.Console;
using YamlDotNet.Serialization;

namespace SecretAdmin.Features.Program.Config;

public class ConfigManager
{
    public MainConfig SecretAdminConfig = new ();
    public static readonly Serializer Serializer = new ();
    public static readonly Deserializer Deserializer = new ();

    public void LoadConfig()
    {
        if (!File.Exists(Paths.ProgramConfig))
        {
            while (true)
            {
                ProgramIntroduction.ShowIntroduction();
            
                if (Log.GetConfirm("Do you want to configure again SecretAdmin?", false))
                    continue;
            
                break;
            }
        }
        
        SecretAdminConfig = Deserializer.Deserialize<MainConfig>(File.ReadAllText(Paths.ProgramConfig));
    }

    public void SaveConfig(MainConfig config)
    {
        File.WriteAllText(Paths.ProgramConfig, Serializer.Serialize(config));
        LoadConfig();
    }
}