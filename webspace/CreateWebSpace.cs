using System.Diagnostics;


namespace MythicalWebPanel
{
    public static class CreateWebSpace
    {
        public static void CreateWebsite(string url, string phpversion, string name, bool sslEnabled, string description, string user, string password)
        {
            string websiteFolderPath = $"/var/www/{url}";

            CreateFolder(websiteFolderPath);
            CreateFolder(Path.Combine(websiteFolderPath, "logs")); 
            CreateSftpUser(user, password, websiteFolderPath);
            string nginxConfig = GenerateNginxConfig(url, sslEnabled, phpversion);
            SaveNginxConfig(nginxConfig, url);
            if (sslEnabled)
            {
                GenerateSSLCertificate(url);
            }
        }

        private static void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }

        private static void CreateSftpUser(string user, string password, string websiteFolderPath)
        {
            var processStartInfo = new ProcessStartInfo("useradd", $"-m -d {websiteFolderPath} -s /bin/bash -p $(echo {password} | openssl passwd -1 -stdin) {user}");
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            var process = Process.Start(processStartInfo);
            #pragma warning disable
            process.WaitForExit();
            #pragma warning restore
            if (process.ExitCode != 0)
            {
                throw new Exception($"Failed to create SFTP user. Error: {process.StandardError.ReadToEnd()}");
            }
        }

        private static string GenerateNginxConfig(string url, bool sslEnabled, string phpversion)
        {
            string sslConfig = sslEnabled ? GetSSLEnabledConfig(url) : GetSSLDisabledConfig(url);
            string nginxConfig = $@"
server {{
    listen 80;
    server_name {url};
    return 301 https://$server_name$request_uri;
}}

server {{
    listen 443 ssl http2;
    server_name {url};

    root /var/www/{url};
    index index.php index.html;

    access_log /var/www/{url}/logs/app-access.log;
    error_log  /var/www/{url}/logs/app-error.log error;

    # allow larger file uploads and longer script runtimes
    client_max_body_size 100m;
    client_body_timeout 120s;

    sendfile off;

    # SSL Configuration - Replace the example <domain> with your domain
    {sslConfig}

    location / {{
        try_files $uri $uri/ /index.php?$query_string;
    }}

    location ~ \.php$ {{
        fastcgi_split_path_info ^(.+\.php)(/.+)$;
        fastcgi_pass unix:/run/php/php{phpversion}-fpm.sock; // Use the appropriate PHP version here
        fastcgi_index index.php;
        include fastcgi_params;
        fastcgi_param PHP_VALUE ""upload_max_filesize = 100M \n post_max_size=100M"";
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        fastcgi_param HTTP_PROXY """";
        fastcgi_intercept_errors off;
        fastcgi_buffer_size 16k;
        fastcgi_buffers 4 16k;
        fastcgi_connect_timeout 300;
        fastcgi_send_timeout 300;
        fastcgi_read_timeout 300;
        include /etc/nginx/fastcgi_params;
    }}

    location ~ /\.ht {{
        deny all;
    }}
}}";

            return nginxConfig;
        }

        private static string GetSSLEnabledConfig(string url)
        {
            return $@"
    ssl_certificate /etc/letsencrypt/live/{url}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/{url}/privkey.pem;
    ssl_session_cache shared:SSL:10m;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ""ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384"";
    ssl_prefer_server_ciphers on;";
        }

        private static string GetSSLDisabledConfig(string url)
        {
            return "";
        }

        private static void SaveNginxConfig(string nginxConfig, string url)
        {
            string nginxConfigFilePath = $"/etc/nginx/sites-available/{url}";
            File.WriteAllText(nginxConfigFilePath, nginxConfig);
        }

        private static void GenerateSSLCertificate(string url)
        {
            var processStartInfo = new ProcessStartInfo("certbot", $"certonly --webroot -d {url} -w /var/www/{url}");
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            var process = Process.Start(processStartInfo);
            #pragma warning disable
            process.WaitForExit();
            #pragma warning restore
            if (process.ExitCode != 0)
            {
                throw new Exception("Failed to generate SSL certificate.");
            }
        }
    }
}
