using SecretAdmin.API.Events;
using SecretAdmin.API.Events.Arguments;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.Features.Program;

public static class InputManager
{
    public static void Start()
    {
        while (true)
        {
            string input = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;
            
            input = input.TrimStart();

            ReceivingInputEventArgs args = new (input);
            Handler.OnReceivingInput(args);
            
            if (!args.IsAllowed)
                continue;

            input = args.Input;
                
            if (!SecretAdmin.Program.CommandHandler.SendCommand(input))
                ManageInput(input);
        }
    }

    private static void ManageInput(string input)
    {
        if (SecretAdmin.Program.Server.Status == ServerStatus.Offline)
        {
            Log.Alert("The server hasn't been initialized yet.");
            return;
        }
        
        Log.Input(input);
        
        SecretAdmin.Program.Server?.SocketServer?.SendMessage(input);
    }
}