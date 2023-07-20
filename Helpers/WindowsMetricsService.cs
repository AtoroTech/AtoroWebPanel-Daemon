using System.Management;
#pragma warning disable 

namespace MythicalWebPanel
{
    public class WindowsMetricsService
    {
        public string GetOperatingSystem()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            ManagementObjectCollection objects = searcher.Get();
            foreach (ManagementObject obj in objects)
            {
                return obj["Caption"].ToString();
            }
            return "Unknown";
        }   

        public string GetCpuModel()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            ManagementObjectCollection objects = searcher.Get();
            foreach (ManagementObject obj in objects)
            {
                return obj["Name"].ToString();
            }
            return "Unknown";
        }

        public string GetTotalDiskSpace()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Size FROM Win32_LogicalDisk WHERE DriveType = 3");
            ManagementObjectCollection objects = searcher.Get();
            string totalDiskSpace = "0";
            foreach (ManagementObject obj in objects)
            {
                totalDiskSpace += Convert.ToUInt64(obj["Size"]);
            }
            return totalDiskSpace;
        }

        public string GetTotalRAM()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            ManagementObjectCollection objects = searcher.Get();
            foreach (ManagementObject obj in objects)
            {
                return obj["TotalVisibleMemorySize"].ToString();
            }
            return "0";
        }

        public string GetUptime()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            ManagementObjectCollection objects = searcher.Get();
            foreach (ManagementObject obj in objects)
            {
                DateTime lastBootUpTime = ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"].ToString());
                TimeSpan uptime = DateTime.Now - lastBootUpTime;
                return uptime.ToString();
            }
            return TimeSpan.Zero.ToString();
        }
    }
}
