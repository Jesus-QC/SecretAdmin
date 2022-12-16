using System.Threading;
using System.Threading.Tasks;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.Features.Server.Modules;

public class SilentCrashHandler
{
    private readonly CancellationTokenSource _cancellationToken = new ();
    private bool _awaitingForFirstHeartbeat = true;
    private bool _receivedHeartbeat;

    private async void CheckHeartbeats()
    {
        byte secondsWithoutContact = 0;

        while (!_cancellationToken.IsCancellationRequested)
        {
            if (SecretAdmin.Program.Server.Status is ServerStatus.Offline)
                return;

            if (SecretAdmin.Program.Server.Status is ServerStatus.Idle)
            {
                await Task.Delay(5000);
                continue;
            }

            if (secondsWithoutContact >= 16)
            {
                Log.Alert("Not Receiving Heartbeats... Waiting 5 last seconds.");
                await Task.Delay(5000);
                
                if (!_receivedHeartbeat)
                {
                    Log.Alert("Server not sending heartbeats... Restarting server.");
                    SecretAdmin.Program.Server.Restart();
                    return;
                }

                Log.Alert("Server sent a heartbeat in the last attempt... Continuing procedure.");
            }

            secondsWithoutContact += 2;
            
            if (_receivedHeartbeat)
                secondsWithoutContact = 0;

            await Task.Delay(2000);
        }
    }

    public void Stop() => _cancellationToken.Cancel();

    public void OnReceivedHeartbeat()
    {
        if (_awaitingForFirstHeartbeat)
        {
            _awaitingForFirstHeartbeat = false;
            Task.Run(CheckHeartbeats, _cancellationToken.Token);
        }

        _receivedHeartbeat = true;
    }
}