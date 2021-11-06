namespace SecretAdmin.Features.Program.Config
{
    public class MainConfig
    {
        public bool AutoUpdater { get; set; } = true;
        public bool RestartOnCrash { get; set; } = true;
        public bool ManualStart { get; set; } = true;
        public bool SafeShutdown { get; set; } = true;
        public bool RestartWithLowMemory { get; set; } = true;
        public int MaxMemory { get; set; } = 2048;
    }
}