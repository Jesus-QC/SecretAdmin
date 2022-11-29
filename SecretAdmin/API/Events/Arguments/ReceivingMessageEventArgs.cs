using System;

namespace SecretAdmin.API.Events.Arguments;

public class ReceivingMessageEventArgs : EventArgs
{
    public bool IsAllowed { get; set; }
    public string Message { get; set; }

    public ReceivingMessageEventArgs(string message)
    {
        IsAllowed = true;
        Message = message;
    }
}