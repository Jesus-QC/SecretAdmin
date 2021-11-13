using System;
using SecretAdmin.API.Events.EventArgs;

namespace SecretAdmin.API.Events.Handlers
{
    public static class Server
    {
        public static event Utils.CustomEventHandler<ReceivedMessageEventArgs> ReceivedMessage;
        public static event Utils.CustomEventHandler<ReceivedActionEventArgs> ReceivedAction;

        public static void OnReceivedMessage(ReceivedMessageEventArgs ev) => ReceivedMessage?.Invoke(ev);
        public static void OnReceivedAction(ReceivedActionEventArgs ev) => ReceivedAction?.Invoke(ev);
    }
}