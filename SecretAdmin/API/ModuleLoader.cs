using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SecretAdmin.API.Extensions;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Program;
using SecretAdmin.Features.Program.Config;
using SecretAdmin.Features.Server.Commands;
using Spectre.Console;

namespace SecretAdmin.API;

public static class ModuleLoader
{
    public static void Load()
    {
        Log.SpectreRaw("[gray]MODULE LOADER:[/] Loading modules...");
        foreach (string modulePath in Directory.GetFiles(Paths.ModulesFolder, "*.dll"))
        {
            try
            {
                Assembly moduleAssembly = Assembly.LoadFrom(modulePath);

                foreach (Type type in moduleAssembly.GetTypes())
                {
                    if (type.GetInterfaces().Contains(typeof(IModule<>)))
                    {
                        if (Activator.CreateInstance(type) is not IModule<IModuleConfig> module)
                        {
                            Log.SpectreRaw($"[gray]MODULE LOADER:[/] [red]Couldn't activate an instance of the module: {moduleAssembly.GetName().FullName.EscapeMarkup()}[/]");
                            return;
                        }

                        module.TryLoadConfig();

                        module.OnEnabled();
                        
                        Log.SpectreRaw($"[gray]MODULE LOADER:[/] [green]The module {module.Name.EscapeMarkup()} {module.Version.EscapeMarkup()} by {module.Author.EscapeMarkup()} has been enabled![/]");
                    }

                    foreach (MethodInfo method in type.GetMethods())
                    {
                        IEnumerable<Attribute> attributes = method.GetCustomAttributes();

                        if (attributes.FirstOrDefault() is ConsoleCommandAttribute query)
                        {
                            Program.CommandHandler.Commands.Add(query.Name.ToLower(), method);

                            foreach (string alias in query.Aliases)
                            {
                                Program.CommandHandler.Commands.Add(alias.ToLower(), method);
                            }
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                Log.SpectreRaw($"[gray]MODULE LOADER:[/] [red]There was an issue loading the module in the path: {modulePath}[/]");
                AnsiConsole.WriteException(e);
                throw;
            }
        }
    }
}