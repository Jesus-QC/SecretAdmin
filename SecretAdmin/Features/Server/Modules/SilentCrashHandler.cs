using System.Threading.Tasks;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.Features.Server.Modules;

public class SilentCrashHandler
{
    private readonly SocketServer _server;
    public bool Killed;
    private int _pingCount;

    public SilentCrashHandler(SocketServer server) => _server = server;

    private async void SendPing()
    {
        await Task.Delay(15000);
        while (!Killed)
        {
            if (SecretAdmin.Program.Server.Status == ServerStatus.Online)
            {
                _server.SendMessage("secretadminping");
                _pingCount++;
            }
            await Task.Delay(10000);

            if (_pingCount != 3) 
                continue;
            
            Log.Alert("The server silently crashed, restarting....");
            SecretAdmin.Program.Server.Restart();
        }
    }

    public void OnReceivePing() => _pingCount = 0;
    
    public void Start() => Task.Run(SendPing);
}