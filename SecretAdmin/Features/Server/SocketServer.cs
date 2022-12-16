using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecretAdmin.API.Events;
using SecretAdmin.API.Events.Arguments;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;
using Spectre.Console;

namespace SecretAdmin.Features.Server;

public class SocketServer
{
    public readonly int Port;

    private readonly TcpListener _listener;
    private TcpClient _client;
    private NetworkStream _stream;
    
    private readonly CancellationTokenSource _cancellationTokenSource = new ();
    
    public SocketServer()
    {
        _listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
        _listener.Start();

        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

        _listener.BeginAcceptTcpClient(asyncResult =>
        {
            _client = _listener.EndAcceptTcpClient(asyncResult);
            _stream = _client.GetStream();

            Task.Run(ListenRequests);
        }, _listener);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _client.Close();
    }

    private async void ListenRequests()
    {
        byte[] codeBuffer = new byte[1];
        byte[] lenghtBuffer = new byte[sizeof(int)]; // use size of an int since the lenght is sent as an int
            
        try
        {
            // To work with this part of SecretAdmin please take a look at how the game manages output
            // Check the namespace ServerOutput
            
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                // First byte is the output code
                int codeBytes = await _stream.ReadAsync(codeBuffer.AsMemory(0, 1), _cancellationTokenSource.Token);
                byte codeType = codeBuffer[0];
                
                // We skip non-coloured messages and only handle the event so weird things don't happen.
                if (codeType > 0xF)
                {
                    HandleAction(codeType);
                    continue;
                }
                
                // 4 bytes for the lenght
                int lengthBytes = await _stream.ReadAsync(lenghtBuffer.AsMemory(0, 4), _cancellationTokenSource.Token);
                int length = (lenghtBuffer[0] << 24) | (lenghtBuffer[1] << 16) | (lenghtBuffer[2] << 8) | lenghtBuffer[3];

                // We get the amount of bytes that lenght tell us
                byte[] messageBuffer = new byte[length];
                int messageBytesRead = await _stream.ReadAsync(messageBuffer.AsMemory(0, length), _cancellationTokenSource.Token);

                // Null message is 99% a disconnection.
                if (codeBytes <= 0 || lengthBytes != sizeof(int) || messageBytesRead <= 0)
                {
                    if (SecretAdmin.Program.Server.Status != ServerStatus.Offline)
                        Log.Alert("Socket disconnected.");
                    
                    break;
                }

                string message = Encoding.UTF8.GetString(messageBuffer, 0, length);

                ReceivingMessageEventArgs args = new (message, codeType);
                Handler.OnReceivingMessage(args);
                
                if (!args.IsAllowed) 
                    continue;

                message = args.Message;
                    
                SecretAdmin.Program.Server.AddLog(message, $"[{DateTime.Now:T}]");
                Log.HandleMessage(message, args.Color);
            }
        }
        catch (Exception)
        {
            Log.Alert("Socket cancelled.");
        }
        finally
        {
            _cancellationTokenSource.Cancel();
        }
    }
    
    public void SendMessage(string message)
    {
        if (_stream == null)
        {
            Log.Alert("The server hasn't been initialized yet");
            return;
        }

        byte[] messageBuffer = new byte[Encoding.UTF8.GetMaxByteCount(message.Length) + sizeof(int)];
        int actualMessageLength = Encoding.UTF8.GetBytes(message, 0, message.Length, messageBuffer, sizeof(int));
            
        Array.Copy(BitConverter.GetBytes(actualMessageLength), messageBuffer, sizeof(int));

        try
        {
            if (_stream.CanWrite)
                _stream.Write(messageBuffer, 0, actualMessageLength + sizeof(int));
        }
        catch (Exception e)
        {
            if (e is IOException)
                return;
            
            AnsiConsole.WriteException(e);
        }
    }
        
    private static void HandleAction(byte action)
    {
        ReceivingActionEventArgs args = new ((OutputCodes)action);
        Handler.OnReceivingAction(args);
        
        if (!args.IsAllowed)
            return;
        
        switch (args.ActionCode)
        {
            case OutputCodes.RoundRestart:
                SecretAdmin.Program.Server.AddLog("Waiting for players.");
                break;

            case OutputCodes.IdleEnter:
                SecretAdmin.Program.Server.Status = ServerStatus.Idle;
                SecretAdmin.Program.Server.AddLog("Server entered idle mode.");
                break;

            case OutputCodes.IdleExit:
                SecretAdmin.Program.Server.Status = ServerStatus.Online;
                SecretAdmin.Program.Server.AddLog("Server exited idle mode.");
                break;
                
            case OutputCodes.ExitActionReset:
                SecretAdmin.Program.Server.Status = ServerStatus.Online;
                break;
                
            case OutputCodes.ExitActionShutdown:
                SecretAdmin.Program.Server.Status = ServerStatus.ExitingNextRound;
                break;
                
            case OutputCodes.ExitActionSilentShutdown:
                SecretAdmin.Program.Server.Status = ServerStatus.ExitingNextRound;
                break;
                
            case OutputCodes.ExitActionRestart:
                SecretAdmin.Program.Server.Status = ServerStatus.RestartingNextRound;
                break;

            case OutputCodes.Heartbeat:
                SecretAdmin.Program.Server.SilentCrashHandler?.OnReceivedHeartbeat();
                break;
            
            default:
                Log.Alert($"Received unknown output code ({action}), possible buffer spam.");
                break;
        }
    }
}