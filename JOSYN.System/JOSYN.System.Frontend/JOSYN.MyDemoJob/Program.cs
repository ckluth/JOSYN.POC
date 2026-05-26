using System.Text;

namespace MyDemoJob;

internal class Program
{
    private static async Task<int> Main(string[] args) { return await JOSYN.System.Frontend.JobHost.Core.Run(args); }
}

