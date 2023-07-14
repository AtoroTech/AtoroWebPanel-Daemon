using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Salaros.Configuration;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;


namespace MythicalWebPanel
{
    public class Program
    {
        public static string d_host = string.Empty;
        public static string d_port = string.Empty;
        public static string d_key = string.Empty;
        public static string d_os = string.Empty;
        public static string d_settings = string.Empty;        
        public static string os_name = string.Empty;
        public static string os_cpu = string.Empty;
        public static string os_disk = string.Empty;
        public static string os_ram = string.Empty;
        public static string os_uptime = string.Empty;
        public static string mcascii = @" 
          _              __          __  _     _____                 _ 
     /\  | |             \ \        / / | |   |  __ \               | |
    /  \ | |_ ___  _ __ __\ \  /\  / /__| |__ | |__) |_ _ _ __   ___| |
   / /\ \| __/ _ \| '__/ _ \ \/  \/ / _ \ '_ \|  ___/ _` | '_ \ / _ \ |
  / ____ \ || (_) | | | (_) \  /\  /  __/ |_) | |  | (_| | | | |  __/ |
 /_/    \_\__\___/|_|  \___/ \/  \/ \___|_.__/|_|   \__,_|_| |_|\___|_|
                                                                       
    
    ";
        public static string version = "1.0.0";
        public static Logger logger = new Logger();
        
        public static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine(mcascii);
            CheckOperatingSystem();
            if (args.Contains("-version"))
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) You are running version: '" + version + "'", DateTime.Now);
                Environment.Exit(0x0);
            }
            else if (args.Contains("-help"))
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) You are running version: '" + version + "'", DateTime.Now);
                Console.WriteLine("");
                Console.WriteLine("-help | Displays all the commands that you can execute");
                Console.WriteLine("-reset | It resets a full reset of the application");
                Console.WriteLine("-resetkey | It resets the secret key to a new one.");
                Console.WriteLine("");
                Environment.Exit(0x0);
            }
            else if (args.Contains("-reset"))
            {
                if (d_os == "win")
                {
                    d_settings = Directory.GetCurrentDirectory() + @"\config.ini";
                }
                else if (d_os == "linux")
                {
                    d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
                }
                else
                {
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Looks like we can't find your os info please use ubuntu or windwos", DateTime.Now);
                    Environment.Exit(0x0);
                }
                try
                {
                    var cfg = new ConfigParser(d_settings);
                    string skey = KeyChecker.GenerateStrongKey();
                    cfg.SetValue("Daemon", "key", skey);
                    cfg.SetValue("Daemon", "host", "127.0.0.1");
                    cfg.SetValue("Daemon", "port", "1953");
                    cfg.Save();
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) We updated your daemon settings", DateTime.Now);
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Your key is: '" + skey + "'", DateTime.Now);
                    Environment.Exit(0x0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Failed to generate a key: " + ex.Message, DateTime.Now);
                    Environment.Exit(0x0);
                }
            }
            else if (args.Contains("-resetkey"))
            {
                if (d_os == "win")
                {
                    d_settings = Directory.GetCurrentDirectory() + @"\config.ini";
                }
                else if (d_os == "linux")
                {
                    d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
                }
                else
                {
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Looks like we can't find your os info please use ubuntu or windwos", DateTime.Now);
                    Environment.Exit(0x0);
                }
                try
                {
                    var cfg = new ConfigParser(d_settings);
                    string skey = KeyChecker.GenerateStrongKey();
                    cfg.SetValue("Daemon", "key", skey);
                    cfg.Save();
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) We updated your daemon settings", DateTime.Now);
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Your key is: '" + skey + "'", DateTime.Now);
                    Environment.Exit(0x0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Failed to generate a key: " + ex.Message, DateTime.Now);
                    Environment.Exit(0x0);
                }
            }
            else if (args.Length > 0)
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) This is an invalid startup argument. Please use '-help' to get more information.", DateTime.Now);
                Environment.Exit(0x0);
            }
            logger.Log(LogType.Info, "Please wait while we start MythicalWebPanel-Daemon");
            LoadSettings();
            getOsInfo();
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    int port = int.Parse(d_port);
                    options.Listen(IPAddress.Parse(d_host), port);
                })
                .Configure(ConfigureApp)
                .Build();
            logger.Log(LogType.Info, "Daemon started on: " + d_host + ":" + d_port);
            logger.Log(LogType.Info, "Secret key: " + d_key);
            host.Run();
            
        }
        private static async void getOsInfo() {
            if (d_os == "win") {
                try {
                    WindowsMetricsService winMetricsService = new WindowsMetricsService();
                    string osName = winMetricsService.GetOperatingSystem();
                    os_name = osName.Replace("\n", "");
                    string osCpu = winMetricsService.GetCpuModel();
                    os_cpu = osCpu.Replace("\n", "");
                    string osDisk = winMetricsService.GetTotalDiskSpace();
                    os_disk = osDisk.Replace("\n", "");
                    string osRam = winMetricsService.GetTotalRAM();
                    os_ram = osRam.Replace("\n", "");
                    string osUptime = winMetricsService.GetUptime();
                    os_uptime = osUptime.Replace("\n", "");
                    logger.Log(LogType.Info, "Operating System: "+os_name);
                    logger.Log(LogType.Info, "CPU: "+os_cpu);
                    logger.Log(LogType.Info, "DISK: "+os_disk);
                    logger.Log(LogType.Info, "RAM: "+os_ram);
                    logger.Log(LogType.Info, "UPTIME: "+os_uptime);
                } catch (Exception ex) {
                    logger.Log(LogType.Error, "Faild to get the os info: '"+ex.Message+"'");

                }
            }
            else if (d_os == "linux") {
                try
                {
                    BashHelper bashHelper = new BashHelper();
                    LinuxMetricsService LinuxMetricsService = new LinuxMetricsService(bashHelper);
                    string osName = await LinuxMetricsService.GetOsName();
                    os_name = osName.Replace("\n", "");
                    string osCpu = await LinuxMetricsService.GetCpuModel();
                    os_cpu = osCpu.Replace("\n", "");
                    string osDisk = await LinuxMetricsService.GetTotalDisk();
                    string fos_disk = osDisk.Replace("\n", "");
                    os_disk = fos_disk + " KB";
                    string osRam = await LinuxMetricsService.GetTotalMemory();
                    string fos_ram = osRam.Replace("\n", "");
                    os_ram = fos_ram + " KB";
                    string osUptime = await LinuxMetricsService.GetUptime();
                    os_uptime = osUptime.Replace("\n", "");
                    logger.Log(LogType.Info, "Operating System: "+os_name);
                    logger.Log(LogType.Info, "CPU: "+os_cpu);
                    logger.Log(LogType.Info, "DISK: "+os_disk);
                    logger.Log(LogType.Info, "RAM: "+os_ram);
                    logger.Log(LogType.Info, "UPTIME: "+os_uptime);
                }
                catch (Exception ex)
                {
                    logger.Log(LogType.Error, "Faild to get the os info: '"+ex.Message+"'");
                }
            }
            else {
                os_name = "Unknown";
                os_cpu = "Unknown";
                os_disk = "Unknown";
                os_ram = "Unknown";
                os_uptime = "Unknown";
                logger.Log(LogType.Error, "Faild to get the os info'");
            }
        }
        private static void LoadSettings()
        {
            try
            {
                if (d_os == "win")
                {
                    d_settings = Directory.GetCurrentDirectory() + @"\config.ini";
                }
                else if (d_os == "linux")
                {
                    d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
                }
                else
                {
                    logger.Log(LogType.Error, "Looks like we can't find your os info please use ubuntu or windwos");
                    Environment.Exit(0x0);
                }
                var cfg = new ConfigParser(d_settings);
                if (!File.Exists(d_settings))
                {
                    cfg.SetValue("Daemon", "host", "127.0.0.1");
                    cfg.SetValue("Daemon", "port", "1953");
                    cfg.SetValue("Daemon", "key", "");
                    cfg.Save();
                    logger.Log(LogType.Warning, "Looks like this is your first time running our daemon. Please close the app, go into config.ini, and configure your app");
                    Environment.Exit(0x0);
                }
                d_host = cfg.GetValue("Daemon", "host");
                d_port = cfg.GetValue("Daemon", "port");
                d_key = cfg.GetValue("Daemon", "key");
                if (d_host == "")
                {
                    d_host = "127.0.0.1";
                }
                if (d_port == "")
                {
                    d_port = "1953";
                }
                if (d_key == "")
                {
                    logger.Log(LogType.Error, "Failed to start: 'Please use a strong key'");
                    Environment.Exit(0x0);
                }
                if (!KeyChecker.isStrongKey(d_key))
                {
                    logger.Log(LogType.Error, "Failed to start: 'Please use a strong key'");
                    Environment.Exit(0x0);
                }
                logger.Log(LogType.Info, "Loaded daemon config from 'config.ini'");
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Failed to load config: " + ex.Message);
                Environment.Exit(0x0);
            }
        }
        private static void ConfigureApp(IApplicationBuilder app)
        {
            app.Run(ProcessRequest);
        }
        private static async Task ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            if (IsAuthorized(request))
            {
                var absolutePath = request.Path.Value.TrimStart('/');
                switch (absolutePath)
                {
                    case "":
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
                            response.ContentLength = errorBuffer.Length;
                            await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                            break;
                        }
                    case "system/reboot":
                        {
                            PowerManager pw = new PowerManager();
                            if (d_os == "win") {
                                pw.RebootServerWindwos();
                            }
                            else if (d_os == "linux")
                            {
                                pw.RebootServerLinux();
                            }                            
                            var rebootResponse = new
                            {
                                message = "Server reboot initiated",
                                status = "Please wait..."
                            };
                            var rebootJson = Newtonsoft.Json.JsonConvert.SerializeObject(rebootResponse);
                            var rebootBuffer = Encoding.UTF8.GetBytes(rebootJson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = rebootBuffer.Length;
                            await response.Body.WriteAsync(rebootBuffer, 0, rebootBuffer.Length);
                            break;
                        }
                    case "system/shutdown":
                        {
                            PowerManager pw = new PowerManager();
                            if (d_os == "win") {
                                pw.ShutdownServerWindwos();
                            }
                            else if (d_os == "linux")
                            {
                                pw.ShutdownServerLinux();
                            }                            
                            var rebootResponse = new
                            {
                                message = "Server shutdown initiated",
                                status = "Please wait..."
                            };
                            var rebootJson = Newtonsoft.Json.JsonConvert.SerializeObject(rebootResponse);
                            var rebootBuffer = Encoding.UTF8.GetBytes(rebootJson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = rebootBuffer.Length;
                            await response.Body.WriteAsync(rebootBuffer, 0, rebootBuffer.Length);
                            break;
                        }
                    case "system/info":
                        {
                            var osInfo = new
                            {
                                os_name,
                                os_cpu,
                                os_disk,
                                os_ram,
                                os_uptime
                            };
                            var osInfoJson = Newtonsoft.Json.JsonConvert.SerializeObject(osInfo);
                            var osInfoBuffer = Encoding.UTF8.GetBytes(osInfoJson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = osInfoBuffer.Length;
                            await response.Body.WriteAsync(osInfoBuffer, 0, osInfoBuffer.Length);
                            break;
                        }
                    case "test":
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
                            response.ContentLength = pBuffer.Length;
                            await response.Body.WriteAsync(pBuffer, 0, pBuffer.Length);
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
                            response.ContentLength = errorBuffer.Length;
                            await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
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
                response.ContentLength = errorBuffer.Length;
                await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
            }
        }
        private static bool IsAuthorized(HttpRequest request)
        {
            string apiKey = request.Headers["Authorization"];
            bool authorized = (apiKey == d_key);
            return authorized;
        }

        private static void CheckOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                d_os = "win";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                d_os = "linux";
            }
            else
            {
                d_os = "unknown";
            }
        }
    }
}
