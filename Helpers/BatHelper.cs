using System.Diagnostics;

namespace McControllerX
{
    public class BatchHelper
    {
        public async Task<string> ExecuteCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c \"{command.Replace("\"", "\"\"")}\"";
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
