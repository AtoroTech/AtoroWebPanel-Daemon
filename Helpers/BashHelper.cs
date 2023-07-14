using System.Diagnostics;

namespace MythicalWebPanel
{
    public class BashHelper
    {
        public async Task<string> ExecuteCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Logger logger = new Logger();
                logger.Log(LogType.Error, await process.StandardError.ReadToEndAsync());
            }

            return output;
        }
    }   
}