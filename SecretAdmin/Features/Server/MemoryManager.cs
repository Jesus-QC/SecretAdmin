using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SecretAdmin.Features.Console;
using SecretAdmin.Features.Server.Enums;
using static SecretAdmin.Program;

namespace SecretAdmin.Features.Server
{
    public class MemoryManager : IDisposable
    {
        private Process _process;
        private bool _killed;
        
        public MemoryManager(Process process) => _process = process;

        public void Start()
        {
            _killed = false;
            //Task.Run(CheckUse);
        }
        
        /*private async void CheckUse()
        {
            await Task.Delay(5000);
            while (!_killed)
            {
                if(SecretAdmin.Program.Server.Status != ServerStatus.Online)
                    return;

                var mem = GetMemory();
                
                if (mem > ConfigManager.SecretAdminConfig.MaxDefaultMemory)
                {
                    Log.Raw($"LOW MEMORY. USING {mem}MB / {ConfigManager.SecretAdminConfig.MaxDefaultMemory}");
                    await Task.Delay(2500);
                    if(ConfigManager.SecretAdminConfig.RestartWithLowMemory)
                        SecretAdmin.Program.Server.ForceRestart();
                }

                await Task.Delay(5000);
            }
        }*/
        
        public long GetMemory()
        {
            _process.Refresh();
            return _process.WorkingSet64 / 1048576;
        }

        public void Dispose() => _killed = true;
    }
}