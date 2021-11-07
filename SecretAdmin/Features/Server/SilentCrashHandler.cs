using System.Threading.Tasks;
using SecretAdmin.Features.Server.Enums;
using Main = SecretAdmin.Program;

namespace SecretAdmin.Features.Server
{
    public class SilentCrashHandler
    {
        private SocketServer _server;
        private int _pingCount;
        private Task _pingTask;

        // TODO: this
        
        public SilentCrashHandler(SocketServer server) => _server = server;

        public void Start()
        {
            _pingTask = Task.Run(SendPing);
        }

        private async void SendPing()
        {
            await Task.Delay(15000);
            while (true)
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

        public void Stop()
        {
            _pingTask.Dispose();
        }
    }
}