using System;
using SecretAdmin.Features.Console;

namespace SecretAdmin.Features.Program
{
    public class InputManager
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
            Log.Input(input);
            SecretAdmin.Program.Server.Socket.SendMessage(input);
        }
    }
}