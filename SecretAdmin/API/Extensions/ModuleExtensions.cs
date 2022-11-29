using System.IO;
using SecretAdmin.Features.Program;

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