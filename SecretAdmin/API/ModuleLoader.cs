using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SecretAdmin.API.Attributes;
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
        Log.SpectreRaw("[gray]MODULE LOADER:[/] Loading dependencies...");
        foreach (string dependencyPath in Directory.GetFiles(Paths.DependenciesFolder, "*.dll"))
        {
            Log.SpectreRaw($"[gray]MODULE LOADER:[/] Loaded dependency [green]{Assembly.Load(File.ReadAllBytes(dependencyPath)).FullName.EscapeMarkup()}[/]");
        }
        
        Log.SpectreRaw("[gray]MODULE LOADER:[/] Loading modules...");
        foreach (string modulePath in Directory.GetFiles(Paths.ModulesFolder, "*.dll"))
        {
            try
            {
                Assembly moduleAssembly = Assembly.Load(File.ReadAllBytes(modulePath));

                foreach (Type type in moduleAssembly.GetTypes())
                {
                    if (type.GetCustomAttribute(typeof(SecretAdminModuleAttribute)) is not null)
                    {
                        try
                        {
                            IModule module = Activator.CreateInstance(type) as IModule;
                            module.OnEnabled();
                            Log.SpectreRaw($"[gray]MODULE LOADER:[/] [green]The module {module.Name.EscapeMarkup()} {module.Version.EscapeMarkup()} by {module.Author.EscapeMarkup()} has been enabled![/]");

                        }
                        catch (Exception e)
                        {
                            Log.SpectreRaw($"[gray]MODULE LOADER:[/] [red]Couldn't activate an instance of the module: {moduleAssembly.GetName().FullName.EscapeMarkup()}[/]");
                            AnsiConsole.WriteException(e);
                            return;
                        }
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