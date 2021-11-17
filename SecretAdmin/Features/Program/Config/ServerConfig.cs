namespace SecretAdmin.Features.Program.Config
{
    public class ServerConfig
    {
        public uint Port { get; set; } = 7777;
        public int RoundsToRestart { get; set; } = -1;
    }
}