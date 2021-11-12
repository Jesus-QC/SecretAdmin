namespace SecretAdmin.Features.Program.Config
{
    public class MainConfig
    {
        public bool AutoUpdater { get; set; } = true;
        public bool RestartOnCrash { get; set; } = true;
        public int ArchiveLogsDays { get; set; } = 1;
        public bool ManualStart { get; set; } = false;
        public bool SafeShutdown { get; set; } = true;
        public bool RestartWithLowMemory { get; set; } = true;
        public int MaxDefaultMemory { get; set; } = 2048;
    }
}