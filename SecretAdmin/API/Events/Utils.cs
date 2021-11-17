namespace SecretAdmin.API.Events
{
    public static class Utils
    {
        public delegate void CustomEventHandler<T>(T ev) where T : System.EventArgs;
        public delegate void CustomEventHandler();
    }
}