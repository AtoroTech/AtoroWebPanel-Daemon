using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Salaros.Configuration;
using System.Net;
using System.Text;
using MythicalWebPanel;


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
  __  __       _   _     _           ___          __  _     _____                 _ 
 |  \/  |     | | | |   (_)         | \ \        / / | |   |  __ \               | |
 | \  / |_   _| |_| |__  _  ___ __ _| |\ \  /\  / /__| |__ | |__) |_ _ _ __   ___| |
 | |\/| | | | | __| '_ \| |/ __/ _` | | \ \/  \/ / _ \ '_ \|  ___/ _` | '_ \ / _ \ |
 | |  | | |_| | |_| | | | | (_| (_| | |  \  /\  /  __/ |_) | |  | (_| | | | |  __/ |
 |_|  |_|\__, |\__|_| |_|_|\___\__,_|_|   \/  \/ \___|_.__/|_|   \__,_|_| |_|\___|_|
          __/ |                                                                     
         |___/                                                                      
    
    ";
        public static string version = "1.0.0";
        public static Logger logger = new Logger();

        public static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine(mcascii);
            if (!System.OperatingSystem.IsLinux())
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Sorry you have to be on debain / linux to use our daemon");
            }
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
                d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
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
                d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
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
        private static async void getOsInfo()
        {
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
                logger.Log(LogType.Info, "Operating System: " + os_name);
                logger.Log(LogType.Info, "CPU: " + os_cpu);
                logger.Log(LogType.Info, "DISK: " + os_disk);
                logger.Log(LogType.Info, "RAM: " + os_ram);
                logger.Log(LogType.Info, "UPTIME: " + os_uptime);
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Faild to get the os info: '" + ex.Message + "'");
            }
        }
        private static void LoadSettings()
        {
            try
            {
                d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
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
            var (isValidKey, keyMessage) = IsAuthorized(request);
            if (isValidKey)
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
                            pw.RebootServerLinux();
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
                    case "webspace/create":
                        {
                            string url = "webpanel.mythicalsystems.tech";
                            string name = "mythical_website";
                            bool sslEnabled = true;
                            string description = "Mythical WebPanel Website";
                            string user = "ftpuser";
                            string password = "ftppassword";
                            string phpversion = "8.1";

                            try
                            {
                                CreateWebSpace.CreateWebsite(url, phpversion, name, sslEnabled, description, user, password);
                                var successResponse = new
                                {
                                    message = "Website created successfully",
                                    status = "success"
                                };
                                var successJson = Newtonsoft.Json.JsonConvert.SerializeObject(successResponse);
                                var successBuffer = Encoding.UTF8.GetBytes(successJson);
                                response.StatusCode = (int)HttpStatusCode.OK;
                                response.ContentType = "application/json";
                                response.ContentLength = successBuffer.Length;
                                await response.Body.WriteAsync(successBuffer, 0, successBuffer.Length);
                            }
                            catch (Exception ex)
                            {
                                var failureResponse = new
                                {
                                    message = $"Failed to create website: {ex.Message}",
                                    status = "fail"
                                };
                                var failureJson = Newtonsoft.Json.JsonConvert.SerializeObject(failureResponse);
                                var failureBuffer = Encoding.UTF8.GetBytes(failureJson);
                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                response.ContentType = "application/json";
                                response.ContentLength = failureBuffer.Length;
                                await response.Body.WriteAsync(failureBuffer, 0, failureBuffer.Length);
                                logger.Log(LogType.Error,"Faild to create webspace: "+ex.Message);
                            }
                            break;
                        }

                    case "system/shutdown":
                        {
                            PowerManager pw = new PowerManager();
                            pw.ShutdownServerLinux();
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
                    message = keyMessage,
                    error = "Invalid API key."
                };
                var errorJson = Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse);
                var errorBuffer = Encoding.UTF8.GetBytes(errorJson);
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.ContentType = "application/json";
                response.ContentLength = errorBuffer.Length;
                await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
            }
        }
        private static (bool isValid, string message) IsAuthorized(HttpRequest request)
        {
            string apiKey = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return (false, "API key is empty.");
            }

            if (apiKey == d_key)
            {
                return (true, "Authorized.");
            }

            return (false, "API key is invalid.");
        }

    }

}
