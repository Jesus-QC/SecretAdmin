using System;
using System.Threading.Tasks;
using SecretAdmin.Features.Console;
using Main = SecretAdmin.Program;

namespace SecretAdmin.Features.Server
{
    public class SilentCrashHandler : IDisposable
    {
        private readonly SocketServer _server;
        private bool _killed;
        private int _pingCount;

        public SilentCrashHandler(SocketServer server) => _server = server;

        public void Start()
        {
            _killed = false;
           Task.Run(SendPing);
        }

        private async void SendPing()
        {
            await Task.Delay(15000);
            while (!_killed)
            {
                _server.SendMessage("saping");
                _pingCount++;
                await Task.Delay(5000);
                if (_pingCount == 3)
                {
                    Log.Alert("The server silently crashed, restarting....");
                    Main.Server.ForceRestart();
                }
            }
        }

        public void OnReceivePing() => _pingCount = 0;

        public void Dispose() => _killed = true;
    }
}