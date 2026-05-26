namespace JOSYN.System.Backend.JAPServer;

internal class Program
{
    private static async Task<int> Main(string[] args) => await Host.Run(args);
}
