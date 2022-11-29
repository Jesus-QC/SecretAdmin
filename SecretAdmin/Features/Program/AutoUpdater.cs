using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretAdmin.Features.Console;
using Spectre.Console;

namespace SecretAdmin.Features.Program;

public static class AutoUpdater
{
    public static async Task CheckForUpdates()
    {
        Log.SpectreRaw("[gray]UPDATER:[/] Setting up HttpClient...");
                
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", "SecretAdmin");
                
        Log.SpectreRaw("[gray]UPDATER:[/] Getting latest releases...");
                
        string content =  await client.GetStringAsync("https://api.github.com/repos/Jesus-QC/SecretAdmin/releases/latest");
                
        Log.SpectreRaw("[gray]UPDATER:[/] Deserializing the releases...");
                
        Api a = JsonConvert.DeserializeObject<Api>(content);
        
        Version v = Version.Parse(a?.tag_name ?? "0.0.0");
                
        Log.SpectreRaw($"[gray]UPDATER:[/] Detected latest release: [green]{v}[/]");
        
        if (SecretAdmin.Program.Version < v)
        {
            Log.SpectreRaw("[gray]UPDATER:[/] [red]SecretAdmin is outdated, updating the plugin.[/]");
            await client.DownloadUpdateAsync(a?.tag_name);
        }
        
        Thread.Sleep(100);
    }

    private static async Task DownloadUpdateAsync(this HttpClient client, string tag)
    {
        FileInfo path = new(Assembly.GetExecutingAssembly().Location);
        
        File.Move(path.FullName, path.FullName + ".old", true);

        try
        {
            Stream file =  await client.GetStreamAsync($"https://github.com/Jesus-QC/SecretAdmin/releases/download/{tag}/SecretAdmin");
            await file.CopyToAsync(path.Open(FileMode.CreateNew));
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            File.Move(path.FullName + ".old", path.FullName);
            
            Log.SpectreRaw("[gray]UPDATER:[/] [red]An error occured while updating SecretAdmin, procedure canceled.[/]");
        }

        Log.SpectreRaw("[gray]UPDATER:[/] [green]SecretAdmin has been successfully updated, restart to apply changes![/]\n");
    }
    
    private record Api(string tag_name);
}