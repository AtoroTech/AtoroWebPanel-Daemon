using System.Management;

namespace McControllerX
{
    public class WindowsMetricsService
    {
        #pragma warning disable 
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

        public ulong GetTotalDiskSpace()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Size FROM Win32_LogicalDisk WHERE DriveType = 3");
            ManagementObjectCollection objects = searcher.Get();
            ulong totalDiskSpace = 0;
            foreach (ManagementObject obj in objects)
            {
                totalDiskSpace += Convert.ToUInt64(obj["Size"]);
            }
            return totalDiskSpace;
        }

        public ulong GetTotalRAM()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            ManagementObjectCollection objects = searcher.Get();
            foreach (ManagementObject obj in objects)
            {
                return Convert.ToUInt64(obj["TotalVisibleMemorySize"]);
            }
            return 0;
        }
        public TimeSpan GetUptime()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            ManagementObjectCollection objects = searcher.Get();
            foreach (ManagementObject obj in objects)
            {
                DateTime lastBootUpTime = ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"].ToString());
                return DateTime.Now - lastBootUpTime;
            }
            return TimeSpan.Zero;
        }
        #pragma warning restore
    }
}