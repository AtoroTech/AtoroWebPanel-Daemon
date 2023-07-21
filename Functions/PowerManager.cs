using System;

namespace MythicalWebPanel
{
    public class PowerManager {
        public async void RebootServerLinux() {
            BashHelper bashhelper = new BashHelper();
            await bashhelper.ExecuteCommand("sudo reboot");
        }

        public async void ShutdownServerLinux() {
            BashHelper bashhelper = new BashHelper();
            await bashhelper.ExecuteCommand("sudo poweroff");
        }
    }    
}