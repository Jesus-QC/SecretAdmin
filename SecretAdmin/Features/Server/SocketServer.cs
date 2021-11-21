using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using SecretAdmin.API.Events.EventArgs;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;
using Spectre.Console;
using SEvents = SecretAdmin.API.Events.Handlers.Server;

namespace SecretAdmin.Features.Server
{
    public class SocketServer : IDisposable
    {
        private ScpServer _server;
        public readonly ushort Port;
        private static readonly UTF8Encoding Encoding = new (false, true);
        
        private readonly TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;
        private SilentCrashHandler _crashHandler;

        public SocketServer(ScpServer server)
        {
            _server = server;
            _listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
            _listener.Start();

            Port = (ushort)((IPEndPoint)_listener.LocalEndpoint).Port;

            _listener.BeginAcceptTcpClient(asyncResult =>
            {
                _client = _listener.EndAcceptTcpClient(asyncResult);
                _stream = _client.GetStream();
                Task.Run(ListenRequests);
            }, _listener);

            _crashHandler = new SilentCrashHandler(this);
            _crashHandler.Start();
        }

        public async void ListenRequests()
        {
            var codeBuffer = new byte[1];
            var lenghtBuffer = new byte[sizeof(int)];
            
            while (true)
            {
                var codeBytes = await _stream.ReadAsync(codeBuffer, 0, 1);
                var codeType = codeBuffer[0];
                var lengthBytes = await _stream.ReadAsync(lenghtBuffer, 0, sizeof(int));
                var length = (lenghtBuffer[0] << 24) | (lenghtBuffer[1] << 16) | (lenghtBuffer[2] << 8) | lenghtBuffer[3];
                
                var messageBuffer = new byte[length];
                var messageBytesRead = await _stream.ReadAsync(messageBuffer, 0, length);

                if (codeBytes <= 0 || lengthBytes != sizeof(int) || messageBytesRead <= 0)
                {
                    if(_server.Status == ServerStatus.Online)
                        Log.Alert("Socket disconnected.");
                    break;
                }

                if (codeType >= 16)
                {
                    HandleAction(codeType);
                    continue;
                }
                
                if (length <= 0)
                    return;
                
                var message = Encoding.GetString(messageBuffer, 0, length);
                if (HandleSecretAdminEvents(message))
                {
                    _server.AddLog(message, $"[{DateTime.Now:T}]");
                    Log.HandleMessage(message, codeType);
                }
            }
        }
        
        public void Dispose()
        {
            _crashHandler?.Dispose();
            _client?.Close();
            _listener?.Stop();
        }

        public void SendMessage(string message)
        {
            if (_stream == null)
            {
                Log.Alert("The server hasn't been initialized yet");
                return;
            }

            var messageBuffer = new byte[Encoding.GetMaxByteCount(message.Length) + sizeof(int)];
            var actualMessageLength = Encoding.GetBytes(message, 0, message.Length, messageBuffer, sizeof(int));
            
            Array.Copy(BitConverter.GetBytes(actualMessageLength), messageBuffer, sizeof(int));

            try
            {
                _stream.Write(messageBuffer, 0, actualMessageLength + sizeof(int));
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }
        
        public void HandleAction(byte action)
        {
            var ev = new ReceivedActionEventArgs(action);
            SEvents.OnReceivedAction(ev);
            
            if(!ev.IsEnabled)
                return;
            
            switch (ev.OutputCode)
            {
                case OutputCodes.RoundRestart:
                    SEvents.OnRestartedRound();
                    Log.SpectreRaw("Waiting for players.", "lightsteelblue1", true, "slateblue1");
                    _server.AddLog("Waiting for players.");
                    break;

                case OutputCodes.IdleEnter:
                    _server.Status = ServerStatus.Idling;
                    Log.SpectreRaw("Server entered idle mode.", "plum2", true, "slateblue1");
                    _server.AddLog("Server entered idle mode.");
                    break;

                case OutputCodes.IdleExit:
                    _server.Status = ServerStatus.Online;
                    Log.SpectreRaw("Server exited idle mode.", "plum2", true, "slateblue1");
                    _server.AddLog("Server exited idle mode.");
                    break;
                
                case OutputCodes.ExitActionReset:
                    _server.Status = ServerStatus.Online;
                    break;
                
                case OutputCodes.ExitActionShutdown:
                    _server.Status = ServerStatus.Exiting;
                    break;
                
                case OutputCodes.ExitActionSilentShutdown:
                    _server.Status = ServerStatus.Exiting;
                    break;
                
                case OutputCodes.ExitActionRestart:
                    _server.Status = ServerStatus.Restarting;
                    break;

                default:
                    Log.Alert($"Received unknown output code ({(OutputCodes)action}), is SecretAdmin up to date?");
                    break;
            }
        }
        
        private bool HandleSecretAdminEvents(string message)
        {
            if (message == "Command saping does not exist!")
            {
                _crashHandler.OnReceivePing();
                return false;
            }

            if (message.StartsWith("Round finished!") || message.StartsWith("Round restart forced."))
            {
                SEvents.OnRestartingRound();
                _server.Rounds++;
                
                if (_server.Config.RoundsToRestart >= _server.Rounds && _server.Status == ServerStatus.Online)
                    _server.ForceRestart();
                return true;
            }

            return true;
        }
    }
}