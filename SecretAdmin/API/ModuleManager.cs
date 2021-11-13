using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using SecretAdmin.API.Features;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using Module = SecretAdmin.API.Features.Module;

namespace SecretAdmin.API
{
    public static class ModuleManager
    {
        public static List<IModule> Modules = new ();

        public static void LoadAll()
        {
            Log.WriteLine();
            Log.Raw("Loading modules dependencies!", ConsoleColor.DarkCyan);

            var startTime = DateTime.Now;
            
            foreach (var file in Directory.GetFiles(Paths.ModulesDependenciesFolder, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.UnsafeLoadFrom(file);
                    Log.Raw($"Dependency {assembly.GetName().Name} ({assembly.GetName().Version}) has been loaded!");
                }
                catch (Exception e)
                {
                    Log.Raw($"Couldn't load the dependency in the path {file}\n{e}", ConsoleColor.Red);
                    throw;
                }
            }
            
            Log.Raw($"Dependencies loaded in {(DateTime.Now - startTime).TotalMilliseconds}ms", ConsoleColor.Cyan);
            Log.Raw("Loading modules!", ConsoleColor.DarkCyan);
            
            startTime = DateTime.Now;

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
                    }
                }
                catch (Exception e)
                {
                    Log.Raw(e, ConsoleColor.Red);
                }
            }
            
            Log.Raw($"Modules loaded in {(DateTime.Now - startTime).TotalMilliseconds}ms",  ConsoleColor.Cyan);
        }
    }
}