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
            Log.Raw("[lightpink1]Loading module dependencies...[/]", showTimeStamp: false);

            var startTime = DateTime.Now;
            var count = 0;
            
            foreach (var file in Directory.GetFiles(Paths.ModulesDependenciesFolder, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.UnsafeLoadFrom(file);
                    count++;
                    Log.Raw($"[lightcyan1]Dependency {assembly.GetName().Name} ({assembly.GetName().Version}) has been loaded![/]", showTimeStamp: false);
                }
                catch (Exception e)
                {
                    Log.Raw($"[deeppink2]Couldn't load the dependency in the path {file}[/]", showTimeStamp: false);
                    AnsiConsole.WriteException(e);
                }
            }

            Log.Raw(count > 0 ? $"[cornflowerblue]{count} Dependencies loaded in {(DateTime.Now - startTime).TotalMilliseconds}ms[/]" : "[cornflowerblue]No Dependencies found.[/]", showTimeStamp: false);
            Log.Raw("[lightpink1]Loading modules...[/]", showTimeStamp: false);
            
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
                    Log.Raw($"[deeppink2]Couldn't load the module in the path {file}[/]", showTimeStamp: false);
                    AnsiConsole.WriteException(e);
                }
            }
            
            Log.Raw(count > 0 ? $"[cornflowerblue]{count} Modules loaded in {(DateTime.Now - startTime).TotalMilliseconds}ms[/]" : "[cornflowerblue]No Modules found.[/]", showTimeStamp: false);
        }
    }
}