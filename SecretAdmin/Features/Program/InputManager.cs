using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Commands;

namespace SecretAdmin.Features.Program
{
    public static class InputManager
    {
        private static CommandHandler _handler;
        
        public static void Start()
        {
            _handler = new CommandHandler();
            
            while (true)
            {
                var input = System.Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                input = input.TrimStart();
                
                Log.DeletePrevConsoleLine();
                
                if(!_handler.SendCommand(input))
                    ManageInput(input);
            }
        }

        private static void ManageInput(string input)
        {
            Log.Input(input);
            SecretAdmin.Program.Server.Socket.SendMessage(input);
        }
    }
}