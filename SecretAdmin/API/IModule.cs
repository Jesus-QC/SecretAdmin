namespace SecretAdmin.API;

public interface IModule<T> where T : IModuleConfig
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    
    public T Config { get; set; }

    public void OnEnabled();
}