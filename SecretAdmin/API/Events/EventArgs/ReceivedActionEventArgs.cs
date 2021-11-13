using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.API.Events.EventArgs
{
    public class ReceivedActionEventArgs : System.EventArgs
    {
        public ReceivedActionEventArgs(byte actionCode)
        {
            OutputCode = (OutputCodes)actionCode;
            IsEnabled = true;
        }
        
        public OutputCodes OutputCode { get; set; }
        public bool IsEnabled { get; set; }
    }
}