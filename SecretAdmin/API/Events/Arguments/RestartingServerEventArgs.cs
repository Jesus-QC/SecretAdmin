using SecretAdmin.Features.Server;

namespace SecretAdmin.API.Events.Arguments;

public class RestartingServerEventArgs
{
    public bool IsAllowed { get; set; }
    public ScpServer Server { get; }

    public RestartingServerEventArgs(ScpServer server)
    {
        IsAllowed = true;
        Server = server;
    }
}