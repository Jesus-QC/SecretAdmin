using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        _listener.Stop();
        _client.Close();
    }

    private async void ListenRequests()
    {
        byte[] codeBuffer = new byte[1];
        byte[] lenghtBuffer = new byte[sizeof(int)];
            
        try
        {
            while (true)
            {
                int codeBytes = await _stream.ReadAsync(codeBuffer.AsMemory(0, 1), _cancellationTokenSource.Token);
                byte codeType = codeBuffer[0];
                int lengthBytes = await _stream.ReadAsync(lenghtBuffer.AsMemory(0, sizeof(int)), _cancellationTokenSource.Token);
                int length = (lenghtBuffer[0] << 24) | (lenghtBuffer[1] << 16) | (lenghtBuffer[2] << 8) | lenghtBuffer[3];
                byte[] messageBuffer = new byte[length];
                int messageBytesRead = await _stream.ReadAsync(messageBuffer.AsMemory(0, length), _cancellationTokenSource.Token);

                if (codeBytes <= 0 || lengthBytes != sizeof(int) || messageBytesRead <= 0)
                {
                    if (SecretAdmin.Program.Server.Status == ServerStatus.Online)
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

                string message = Encoding.UTF8.GetString(messageBuffer, 0, length);

                if (!HandleSecretAdminEvents(message)) 
                    continue;
                    
                SecretAdmin.Program.Server.AddLog(message, $"[{DateTime.Now:T}]");
                Log.HandleMessage(message, codeType);
            }
        }
        catch (Exception)
        {
            Log.Alert("Socket cancelled.");
        }
        finally
        {
            _cancellationTokenSource.Dispose();
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
            if(e is IOException)
                return;
            
            AnsiConsole.WriteException(e);
        }
    }
        
    private static void HandleAction(byte action)
    {
        switch ((OutputCodes)action)
        {
            case OutputCodes.RoundRestart:
                Log.SpectreRaw("Waiting for players.", "lightsteelblue1", true, "slateblue1");
                SecretAdmin.Program.Server.AddLog("Waiting for players.");
                break;

            case OutputCodes.IdleEnter:
                SecretAdmin.Program.Server.Status = ServerStatus.Idling;
                Log.SpectreRaw("Server entered idle mode.", "plum2", true, "slateblue1");
                SecretAdmin.Program.Server.AddLog("Server entered idle mode.");
                break;

            case OutputCodes.IdleExit:
                SecretAdmin.Program.Server.Status = ServerStatus.Online;
                Log.SpectreRaw("Server exited idle mode.", "plum2", true, "slateblue1");
                SecretAdmin.Program.Server.AddLog("Server exited idle mode.");
                break;
                
            case OutputCodes.ExitActionReset:
                Log.SpectreRaw("Server won't be restarted next round.", "plum2", true, "slateblue1");
                SecretAdmin.Program.Server.Status = ServerStatus.Online;
                break;
                
            case OutputCodes.ExitActionShutdown:
                Log.SpectreRaw("Server will be stopped next round.", "plum2", true, "slateblue1");
                SecretAdmin.Program.Server.Status = ServerStatus.Exiting;
                break;
                
            case OutputCodes.ExitActionSilentShutdown:
                Log.SpectreRaw("Server will be stopped silently next round.", "plum2", true, "slateblue1");
                SecretAdmin.Program.Server.Status = ServerStatus.Exiting;
                break;
                
            case OutputCodes.ExitActionRestart:
                Log.SpectreRaw("Server will be restarted next round.", "plum2", true, "slateblue1");
                SecretAdmin.Program.Server.Status = ServerStatus.RestartingNextRound;
                break;

            default:
                Log.Alert($"Received unknown output code ({(OutputCodes)action}), possible buffer spam.");
                break;
        }
    }
        
    private bool HandleSecretAdminEvents(string message)
    {
        // if (message == "Command secretadminping does not exist!")
        // {
        //     SecretAdmin.Program.Server.SilentCrashHandler.OnReceivePing();
        //     return false;
        // }
        
        // ..
        
        return true;
    }
}