using System;
using SecretAdmin.Features.Server;

namespace SecretAdmin.API.Events.Arguments;

public class StartingServerEventArgs : EventArgs
{
    public bool IsAllowed { get; set; }
    public ScpServer Server { get; }

    public StartingServerEventArgs(ScpServer server)
    {
        IsAllowed = true;
        Server = server;
    }
}