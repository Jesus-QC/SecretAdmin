using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;

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

                if (message == "Command saping does not exist!")
                {
                    _crashHandler.OnReceivePing();
                    continue;
                }

                Log.HandleMessage(message, codeType);
            }
        }
        
        public void Dispose()
        {
            _client?.Close();
            _listener?.Stop();
            _crashHandler.Stop();
        }

        public void SendMessage(string message)
        {
            if (_stream == null)
            {
                Log.Alert($"The server hasn't been initialized yet");
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
                Log.Alert("ERROR " + e);
            }
        }
        
        public void HandleAction(byte action)
        {
            switch ((OutputCodes)action)
            {
                // This seems to show up at the waiting for players event
                case OutputCodes.RoundRestart:
                    Log.Raw("Waiting for players.", ConsoleColor.DarkCyan);
                    break;

                case OutputCodes.IdleEnter:
                    Log.Raw("Server entered idle mode.", ConsoleColor.DarkYellow);
                    break;

                case OutputCodes.IdleExit:
                    Log.Raw("Server exited idle mode.", ConsoleColor.DarkYellow);
                    break;
                
                case OutputCodes.ExitActionReset:
                    Log.Alert("ExitActionReset."); // DEBUG: Dont remove
                    _server.Status = ServerStatus.Online;
                    break;
                
                case OutputCodes.ExitActionShutdown:
                    Log.Alert("ExitActionShutdown."); // DEBUG: Dont remove
                    _server.Status = ServerStatus.Exiting;
                    break;
                
                case OutputCodes.ExitActionSilentShutdown:
                    Log.Alert("ExitActionSilentShutdown."); // DEBUG: Dont remove
                    _server.Status = ServerStatus.Exiting;
                    break;
                
                case OutputCodes.ExitActionRestart:
                    Log.Alert("ExitActionRestart"); // DEBUG: Dont remove
                    _server.Status = ServerStatus.Restarting;
                    break;

                case OutputCodes.RoundEnd:
                    Log.Alert("RoundEnd"); // DEBUG: Dont remove
                    break;

                default:
                    Log.Alert($"Received unknown output code ({action})  ({(OutputCodes)action}), is SecretAdmin up to date?");
                    break;
            }
        }
    }
}