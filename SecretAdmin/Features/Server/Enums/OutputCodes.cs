namespace SecretAdmin.Features.Server.Enums;

public enum OutputCodes : byte
{
    // 0x00 - 0x0F - Reserved for ConsoleColor enum.
    // 0x10 - 0x16 - OutputCodes - From northwood-studios/LocalAdmin-V2

    RoundRestart = 0x10,
    IdleEnter = 0x11,
    IdleExit = 0x12,
    ExitActionReset = 0x13,
    ExitActionShutdown = 0x14,
    ExitActionSilentShutdown = 0x15,
    ExitActionRestart = 0x16
}