using System;

namespace AtoroWebPanel
{
    public class PowerManager {
        public async void RebootServerLinux() {
            BashHelper bashhelper = new BashHelper();
            await bashhelper.ExecuteCommand("sudo reboot");
        }
        
        public async void RebootServerWindwos() {
            BatchHelper bathelper = new BatchHelper();
            await bathelper.ExecuteCommand("shutdown /r /t 0");
        }

        public async void ShutdownServerLinux() {
            BashHelper bashhelper = new BashHelper();
            await bashhelper.ExecuteCommand("sudo poweroff");
        }

        public async void ShutdownServerWindwos() {
            BatchHelper bathelper = new BatchHelper();
            await bathelper.ExecuteCommand("shutdown /s /t 0");
        }
    }    
}