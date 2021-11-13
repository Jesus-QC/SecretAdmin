namespace SecretAdmin.API.Events.EventArgs
{
    public class ReceivedMessageEventArgs : System.EventArgs
    {
        public ReceivedMessageEventArgs(string message, byte code)
        {
            Message = message;
            Code = code;
            IsAllowed = !string.IsNullOrWhiteSpace(message);
        }
        
        public string Message { get; set; }
        public byte Code { get; set; }
        public bool IsAllowed { get; set; }
    }
}