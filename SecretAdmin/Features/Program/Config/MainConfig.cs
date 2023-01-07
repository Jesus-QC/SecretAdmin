namespace SecretAdmin.Features.Program.Config;

public class MainConfig
{
    public bool RestartOnCrash = true;
    public int ArchiveLogsDays = 1;
    public int DeleteLogsDays = 2;
    public bool SafeShutdown = true;
    public bool RestartWithLowMemory = true;
    public int MaxDefaultMemory = 2048;
    public bool EnableModules = false;
    public int TimeOffset = 0;
}