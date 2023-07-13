using McControllerX;

namespace McControllerX
{
    public class LinuxMetricsService
{
    private readonly BashHelper BashHelper;

    public LinuxMetricsService(BashHelper bashHelper)
    {
        BashHelper = bashHelper;
    }

    public async Task<string> GetCpuModel()
    {
        return await BashHelper.ExecuteCommand("lscpu | grep 'Model name' | awk -F: '{print $2}' | sed 's/^ *//'");
    }

    public async Task<long> GetTotalMemory()
    {
        return long.Parse(
            await BashHelper
                .ExecuteCommand("grep 'MemTotal:' /proc/meminfo | awk '{print $2}'")
        );
    }

    
    public async Task<long> GetTotalDisk()
    {
        return long.Parse(
            await BashHelper
                .ExecuteCommand("df -B 1 --total | tail -1 | awk '{print $2}'")
        );
    }
    public async Task<string> GetUptime()
    {
        return 
            await BashHelper
                .ExecuteCommand("uptime -p | sed 's/^up //'");
    }

    public async Task<string> GetOsName()
    {
        return await BashHelper
            .ExecuteCommand("lsb_release -s -d");
    }
}
}

