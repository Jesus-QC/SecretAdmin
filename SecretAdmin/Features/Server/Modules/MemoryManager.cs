using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;

namespace SecretAdmin.Features.Server.Modules;

public class MemoryManager
{
    private readonly CancellationTokenSource _cancellationToken = new ();
    private readonly Process _process;

    public MemoryManager(Process process) => _process = process;

    public long GetMemory()
    {
        _process.Refresh();
        return _process.WorkingSet64 / 1048576;
    }
    
    private async void CheckUse()
    {
        int warns = 0;
        int maxMem = SecretAdmin.Program.ConfigManager.SecretAdminConfig.MaxDefaultMemory;
        
        while (!_cancellationToken.IsCancellationRequested)
        {
            if (SecretAdmin.Program.Server.Status is ServerStatus.Offline)
                return;

            long memory = GetMemory();
            
            if (memory > maxMem)
            {
                Log.SpectreRaw($"LOW MEMORY. USING {memory}MB / {maxMem}MB", "gold1");
                warns++;

                if (warns > 2 && SecretAdmin.Program.ConfigManager.SecretAdminConfig.RestartWithLowMemory)
                {
                    SecretAdmin.Program.Server.Restart();
                }
            }

            await Task.Delay(5000);
        }
    }
    
    public void Start() => Task.Run(CheckUse, _cancellationToken.Token);

    public void Stop() => _cancellationToken.Cancel();
}