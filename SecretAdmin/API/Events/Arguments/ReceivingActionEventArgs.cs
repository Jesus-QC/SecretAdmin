using System;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.API.Events.Arguments;

public class ReceivingActionEventArgs : EventArgs
{
    public bool IsAllowed { get; set; }
    public OutputCodes ActionCode { get; set; }

    public ReceivingActionEventArgs(OutputCodes code)
    {
        IsAllowed = true;
        ActionCode = code;
    }
}