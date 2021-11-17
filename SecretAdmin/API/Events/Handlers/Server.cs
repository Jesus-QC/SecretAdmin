using SecretAdmin.API.Events.EventArgs;

namespace SecretAdmin.API.Events.Handlers
{
    public static class Server
    {
        public static event Utils.CustomEventHandler<ReceivedMessageEventArgs> ReceivedMessage;
        public static event Utils.CustomEventHandler<ReceivedActionEventArgs> ReceivedAction;
        public static event Utils.CustomEventHandler Restarted;
        public static event Utils.CustomEventHandler RestartedRound;
        public static event Utils.CustomEventHandler RestartingRound;

        public static void OnReceivedMessage(ReceivedMessageEventArgs ev) => ReceivedMessage?.Invoke(ev);
        public static void OnReceivedAction(ReceivedActionEventArgs ev) => ReceivedAction?.Invoke(ev);
        public static void OnRestarted() => Restarted?.Invoke();
        public static void OnRestartedRound() => RestartedRound?.Invoke();
        public static void OnRestartingRound() => RestartingRound?.Invoke();
    }
}