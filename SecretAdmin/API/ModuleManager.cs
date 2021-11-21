using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SecretAdmin.API.Features;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using Spectre.Console;
using Module = SecretAdmin.API.Features.Module;

namespace SecretAdmin.API
{
    public static class ModuleManager
    {
        public static List<IModule> Modules = new ();

        public static void LoadAll()
        {
            Log.WriteLine();
            Log.SpectreRaw("Loading module dependencies...", "lightpink1");

            var startTime = DateTime.Now;
            var count = 0;
            
            foreach (var file in Directory.GetFiles(Paths.ModulesDependenciesFolder, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.UnsafeLoadFrom(file);
                    count++;
                    Log.SpectreRaw($"Dependency {assembly.GetName().Name} ({assembly.GetName().Version}) has been loaded!", "lightcyan1");
                }
                catch (Exception e)
                {
                    Log.SpectreRaw($"Couldn't load the dependency in the path {file}", "deeppink2");
                    AnsiConsole.WriteException(e);
                }
            }

            Log.SpectreRaw(count > 0 ? $"{count} Dependencies loaded in {(DateTime.Now - startTime).TotalMilliseconds}ms" : "No Dependencies found.", "cornflowerblue");
            Log.SpectreRaw("Loading modules...", "lightpink1");
            
            startTime = DateTime.Now;
            count = 0;
            
            foreach (var file in Directory.GetFiles(Paths.ModulesFolder, "*.dll"))
            {
                var assembly = Assembly.Load(File.ReadAllBytes(file));

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if(type.IsAbstract || type.IsInterface || type.BaseType != typeof(Module))
                            continue;

                        var constructor = type.GetConstructor(Type.EmptyTypes);
                        if (constructor == null)
                            continue;

                        var module = constructor.Invoke(null) as IModule;
                        module?.OnEnabled();
                        module?.OnRegisteringCommands();

                        Modules.Add(module);
                        count++;
                    }
                }
                catch (Exception e)
                {
                    Log.SpectreRaw($"Couldn't load the module in the path {file}", "deeppink2");
                    AnsiConsole.WriteException(e);
                }
            }
            
            Log.SpectreRaw(count > 0 ? $"{count} Modules loaded in {(DateTime.Now - startTime).TotalMilliseconds}ms" : "No Modules found.", "cornflowerblue");
        }
    }
}