using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.Features.Program
{
    public static class InputManager
    {
        public static void Start()
        {
            while (true)
            {
                var input = System.Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;
                
                input = input.TrimStart();
                
                Log.DeletePrevConsoleLine();
                
                if(!SecretAdmin.Program.CommandHandler.SendCommand(input))
                    ManageInput(input);
            }
        }

        private static void ManageInput(string input)
        {
            if (SecretAdmin.Program.Server.Status == ServerStatus.Restarting)
            {
                Log.Alert("The server hasn't been initialized yet.");
                return;
            }

            Log.Input(input);
            SecretAdmin.Program.Server.Socket.SendMessage(input);
        }
    }
}