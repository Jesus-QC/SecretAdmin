using System;

namespace SecretAdmin.API.Events.Arguments;

public class ReceivingMessageEventArgs : EventArgs
{
    public bool IsAllowed { get; set; }
    public string Message { get; set; }
    public ConsoleColor Color { get; set; }

    public ReceivingMessageEventArgs(string message, byte color)
    {
        IsAllowed = true;
        Message = message;
        Color = (ConsoleColor)color;
    }
}