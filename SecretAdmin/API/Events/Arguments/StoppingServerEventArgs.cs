﻿using SecretAdmin.Features.Server;

namespace SecretAdmin.API.Events.Arguments;

public class StoppingServerEventArgs
{
    public bool IsAllowed { get; set; }
    public ScpServer Server { get; }

    public StoppingServerEventArgs(ScpServer server)
    {
        IsAllowed = true;
        Server = server;
    }
}