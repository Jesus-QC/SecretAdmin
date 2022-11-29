namespace SecretAdmin.API.Events.Arguments;

public class ReceivingInputEventArgs
{
    public bool IsAllowed { get; set; }
    public string Input { get; set; }

    public ReceivingInputEventArgs(string input)
    {
        IsAllowed = true;
        Input = input;
    }
}