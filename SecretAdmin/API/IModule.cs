namespace SecretAdmin.API;

public interface IModule
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    
    public IModuleConfig Config { get; set; }

    public void OnEnabled();
}