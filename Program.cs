using Salaros.Configuration;
using System.Net;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class Program
{
    private static string d_settings = Directory.GetCurrentDirectory() + @"\config.ini";
    public static string d_host = string.Empty;
    public static string d_protocol = string.Empty;
    public static string d_port = string.Empty;
    public static string d_key = string.Empty;
    public static string d_ssl = string.Empty;

    public static void Main(string[] args)
    {
        LoadSettings();        
        var serverThread = new Thread(StartServer);
        serverThread.Start();
        Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Daemon started", DateTime.Now);
        Console.ReadKey();
        StopServer();
    } 

    private static void LoadSettings()
    {
        try
        {
            var cfg = new ConfigParser(d_settings);
            if (!File.Exists(d_settings))
            {
                cfg.SetValue("Daemon", "host", "127.0.0.1");
                cfg.SetValue("Daemon", "useSSL", "false");
                cfg.SetValue("Daemon", "port", "3000");
                cfg.SetValue("Daemon", "key", "");
                cfg.Save(d_settings);
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Looks like this is your first time running our daemon please close the app go into config.ini and config your app",DateTime.Now);
                Environment.Exit(0x0);                
            }
            d_host = cfg.GetValue("Daemon", "host");
            d_port = cfg.GetValue("Daemon", "port");
            d_key = cfg.GetValue("Daemon", "key");
            d_ssl = cfg.GetValue("Daemon", "useSSL");
            if (d_ssl == "true")
            {
                d_protocol = "https://";
            }
            else
            {
                d_protocol = "http://";
            }
            if (d_host == "")
            {
                d_host = "127.0.0.1";
            }
            if (d_port == "")
            {
                d_port = "3000";
            }
            if (d_key == "")
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Faild to start: 'Please use a strong key'", DateTime.Now);
                Environment.Exit(0x0);
            }
            Console.WriteLine("[{0:HH:mm:ss}] (CONFIG) Loaded daemon config from 'config.ini'", DateTime.Now);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[{0:HH:mm:ss}] (CONFIG) Faild to load config: " + ex.Message, DateTime.Now);
            Environment.Exit(0x0);
        }
    }

    private static void StartServer()
    {
        using (var listener = new HttpListener())
        {
            Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Started webserver on: " + d_protocol + d_host + ":" + d_port, DateTime.Now);
            listener.Prefixes.Add(d_protocol + d_host + ":" + d_port + "/");
            listener.Start();

            while (listener.IsListening)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                if (IsAuthorized(request))
                {
                    switch (request.Url.AbsolutePath)
                    {
                        case "/":
                            {
                                var errorResponse = new
                                {
                                    message = "Bad Request",
                                    error = "Please provide a valid API endpoint."
                                };
                                var errorJson = Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse);
                                var errorBuffer = Encoding.UTF8.GetBytes(errorJson);

                                response.StatusCode = (int)HttpStatusCode.BadRequest;
                                response.ContentType = "application/json";
                                response.ContentEncoding = Encoding.UTF8;
                                response.ContentLength64 = errorBuffer.Length;

                                using (var responseStream = response.OutputStream)
                                {
                                    responseStream.Write(errorBuffer, 0, errorBuffer.Length);
                                }

                                break;
                            }

                        case "/test":
                            {
                                var presponse = new
                                {
                                    message = "Example Request",
                                    error = "This is an example request"
                                };
                                var pjson = Newtonsoft.Json.JsonConvert.SerializeObject(presponse);
                                var pBuffer = Encoding.UTF8.GetBytes(pjson);

                                response.StatusCode = (int)HttpStatusCode.OK;
                                response.ContentType = "application/json";
                                response.ContentEncoding = Encoding.UTF8;
                                response.ContentLength64 = pBuffer.Length;

                                using (var responseStream = response.OutputStream)
                                {
                                    responseStream.Write(pBuffer, 0, pBuffer.Length);
                                }

                                break;
                            }

                        case "/system/info":
                            {
                                var osInfo = GetOperatingSystemInfo();
                                var osInfoJson = Newtonsoft.Json.JsonConvert.SerializeObject(osInfo);
                                var buffer = Encoding.UTF8.GetBytes(osInfoJson);

                                response.StatusCode = (int)HttpStatusCode.OK;
                                response.ContentType = "application/json";
                                response.ContentEncoding = Encoding.UTF8;
                                response.ContentLength64 = buffer.Length;

                                using (var responseStream = response.OutputStream)
                                {
                                    responseStream.Write(buffer, 0, buffer.Length);
                                }

                                break;
                            }


                        default:
                            {
                                var errorResponse = new
                                {
                                    message = "Page not found",
                                    error = "The requested page does not exist."
                                };
                                var errorJson = Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse);
                                var errorBuffer = Encoding.UTF8.GetBytes(errorJson);

                                response.StatusCode = (int)HttpStatusCode.NotFound;
                                response.ContentType = "application/json";
                                response.ContentEncoding = Encoding.UTF8;
                                response.ContentLength64 = errorBuffer.Length;

                                using (var responseStream = response.OutputStream)
                                {
                                    responseStream.Write(errorBuffer, 0, errorBuffer.Length);
                                }

                                break;
                            }
                    }
                }
                else
                {
                    var errorResponse = new
                    {
                        message = "Unauthorized",
                        error = "API key not provided or invalid."
                    };
                    var errorJson = Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse);
                    var errorBuffer = Encoding.UTF8.GetBytes(errorJson);

                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.ContentType = "application/json";
                    response.ContentEncoding = Encoding.UTF8;
                    response.ContentLength64 = errorBuffer.Length;

                    using (var responseStream = response.OutputStream)
                    {
                        responseStream.Write(errorBuffer, 0, errorBuffer.Length);
                    }
                }
            }
        }
    }

    private static dynamic GetOperatingSystemInfo()
    {
        var osInfo = new SystemInfo();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var osQuery = new ObjectQuery("SELECT Caption, Version, BuildNumber FROM Win32_OperatingSystem");
            var osSearcher = new ManagementObjectSearcher(osQuery);
            var osCollection = osSearcher.Get();

            foreach (var managementObject in osCollection)
            {
                osInfo.OperatingSystem = managementObject["Caption"].ToString();
                osInfo.Version = managementObject["Version"].ToString();
                osInfo.BuildNumber = managementObject["BuildNumber"].ToString();
            }

            var ramQuery = new ObjectQuery("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            var ramSearcher = new ManagementObjectSearcher(ramQuery);
            var ramCollection = ramSearcher.Get();

            foreach (var managementObject in ramCollection)
            {
                var totalMemorySize = Convert.ToInt64(managementObject["TotalVisibleMemorySize"]);
                osInfo.TotalRAM = totalMemorySize / 1024; 
            }

            var diskQuery = new ObjectQuery("SELECT Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType = 3");
            var diskSearcher = new ManagementObjectSearcher(diskQuery);
            var diskCollection = diskSearcher.Get();

            long totalDiskSpace = 0;
            long totalFreeSpace = 0;

            foreach (var managementObject in diskCollection)
            {
                totalDiskSpace += Convert.ToInt64(managementObject["Size"]);
                totalFreeSpace += Convert.ToInt64(managementObject["FreeSpace"]);
            }

            osInfo.TotalDiskSpace = totalDiskSpace / (1024 * 1024 * 1024);
            osInfo.FreeDiskSpace = totalFreeSpace / (1024 * 1024 * 1024);

            var cpuQuery = new ObjectQuery("SELECT Name FROM Win32_Processor");
            var cpuSearcher = new ManagementObjectSearcher(cpuQuery);
            var cpuCollection = cpuSearcher.Get();

            foreach (var managementObject in cpuCollection)
            {
                osInfo.CPU = managementObject["Name"].ToString();
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            osInfo.OperatingSystem = RuntimeInformation.OSDescription;
            var memInfo = new MemoryInfo();
            memInfo.ParseFile("/proc/meminfo");
            osInfo.TotalRAM = memInfo.MemTotal / 1024;
            var driveInfo = new DriveInfo("/");
            osInfo.TotalDiskSpace = driveInfo.TotalSize / (1024 * 1024 * 1024);
            osInfo.FreeDiskSpace = driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024);
            osInfo.CPU = GetLinuxCPUInfo();
        }

        return osInfo;
    }

    private static string GetLinuxCPUInfo()
    {
        try
        {
            string cpuInfo = File.ReadAllText("/proc/cpuinfo");
            var match = Regex.Match(cpuInfo, @"model name\s+:\s+(.+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to retrieve CPU information: {ex.Message}");
        }

        return string.Empty;
    }

    private class SystemInfo
    {
        public string OperatingSystem { get; set; }
        public string Version { get; set; }
        public string BuildNumber { get; set; }
        public long TotalRAM { get; set; }
        public long TotalDiskSpace { get; set; }
        public long FreeDiskSpace { get; set; }
        public string CPU { get; set; }
    }

    private class MemoryInfo
    {
        public long MemTotal { get; set; }

        public void ParseFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal:"))
                {
                    var match = Regex.Match(line, @"\d+");
                    if (match.Success)
                    {
                        MemTotal = long.Parse(match.Value);
                    }
                    break;
                }
            }
        }
    }


    private static bool IsAuthorized(HttpListenerRequest request)
    {
        string apiKey = request.Headers["api_key"];
        bool authorized = (apiKey == d_key);

        return authorized;
    }

    private static void StopServer()
    {
        Environment.Exit(0x0);
    }

}
