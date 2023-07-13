using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Salaros.Configuration;
using System.Net;
using System.Text;

public class Program
{
    private static string d_settings = Directory.GetCurrentDirectory() + @"\config.ini";
    public static string d_host = string.Empty;
    public static string d_port = string.Empty;
    public static string d_key = string.Empty;
    
    public static void Main(string[] args)
    {
        LoadSettings();

       var host = new WebHostBuilder()
        .UseKestrel(options =>
        {
            int port = int.Parse(d_port);
            options.Listen(IPAddress.Parse(d_host), port);
        })
        .Configure(ConfigureApp)
        .Build();
        Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Daemon started", DateTime.Now);
        host.Run();
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
                cfg.Save();
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Looks like this is your first time running our daemon. Please close the app, go into config.ini, and configure your app", DateTime.Now);
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
                d_port = "3000";
            }
            if (d_key == "")
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Failed to start: 'Please use a strong key'", DateTime.Now);
                Environment.Exit(0x0);
            }
            Console.WriteLine("[{0:HH:mm:ss}] (CONFIG) Loaded daemon config from 'config.ini'", DateTime.Now);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[{0:HH:mm:ss}] (CONFIG) Failed to load config: " + ex.Message, DateTime.Now);
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
            var absolutePath = request.Path.Value.TrimStart('/'); // Remove the leading slash

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
        string apiKey = request.Headers["api_key"];
        bool authorized = (apiKey == d_key);

        return authorized;
    }
}
