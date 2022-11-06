namespace SecretAdmin.Features.Server.Enums;

public static class EnumExtensions
{
    public static bool IsOnline(this ServerStatus status)
    {
        return status != ServerStatus.Exiting && status != ServerStatus.Restarting;
    }
}