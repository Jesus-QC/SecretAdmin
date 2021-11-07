using System;
using System.Threading.Tasks;
using SecretAdmin.Features.Server.Enums;
using Main = SecretAdmin.Program;

namespace SecretAdmin.Features.Server
{
    public class SilentCrashHandler : IDisposable
    {
        private SocketServer _server;
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
                if (_pingCount == 4)
                {
                    Main.Server.Status = ServerStatus.Restarting;
                    Main.Server.Restart();
                }
            }
        }

        public void OnReceivePing()
        {
            _pingCount = 0;
        }

        public void Dispose()
        {
            _killed = true;
        }
    }
}