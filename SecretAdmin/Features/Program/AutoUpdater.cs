using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretAdmin.Features.Console;
using Spectre.Console;

namespace SecretAdmin.Features.Program;

public static class AutoUpdater
{
    public static void CheckForUpdates()
    {
        Log.SpectreRaw("[gray]UPDATER:[/] Setting up HttpClient...");
                
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", "SecretAdmin");
                
        Log.SpectreRaw("[gray]UPDATER:[/] Getting latest releases...");
                
        string content =  client.GetStringAsync("https://api.github.com/repos/Jesus-QC/SecretAdmin/releases/latest").GetAwaiter().GetResult();
                
        Log.SpectreRaw("[gray]UPDATER:[/] Deserializing the releases...");
                
        Api a = JsonConvert.DeserializeObject<Api>(content);
        
        Version v = Version.Parse(a?.tag_name ?? "0.0.0");
                
        Log.SpectreRaw($"[gray]UPDATER:[/] Detected latest release: [green]{v}[/]");
        
        if (SecretAdmin.Program.Version < v)
        {
            Log.SpectreRaw("[gray]UPDATER:[/] [red]SecretAdmin is outdated, updating the plugin.[/]");
            Update(a?.tag_name);
        }
                
        Log.SpectreRaw("[gray]UPDATER:[/] [green]SecretAdmin is updated![/]\n");
        
        Thread.Sleep(100);
    }

    private static void Update(string tag)
    {
        
    }
    
    private record Api(string tag_name);
}