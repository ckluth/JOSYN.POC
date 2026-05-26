using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using JOSYN.System.Shared.Contract;
using JOSYN.System.Shared.Log;
using System.Diagnostics;
using System.Text;

namespace JOSYN.System.Backend.JAPServer;

internal static class Host
{
    internal static async Task<int> Run(string[] args)
    {
        
#if DEBUG
        Console.InputEncoding = new UTF8Encoding();
        Console.OutputEncoding = new UTF8Encoding();
        LocalLog.EnableConsoleOutput = true;
#endif
        try
        {
            Console.WriteLine("ARGS: " + string.Join(" | ", args));
            var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
            if (sessionKey == Guid.Empty)
            {
                LocalLog.Error("Keine IPC-Session-UID angegeben.");
                return 1;
            }

            return await RunServer(sessionKey);
        }
        catch (Exception ex)
        {
            LocalLog.Error("Unbehandelte Exception im Host.", exceptionDetails: ex.ToString());
            Console.WriteLine(ex);
            return 1;
        }
        finally
        {
#if DEBUG
            Console.Write("\n[PRESS ANY KEY TO EXIT...]");
            Console.ReadKey(true);
#endif
        }
    }

    // -------------------------------------------------------------------------
    // Server lifecycle
    // -------------------------------------------------------------------------

    private static async Task<int> RunServer(Guid sessionKey)
    {
        Console.WriteLine("Starting Server...");
        var sw = Stopwatch.StartNew();

        var serverStartArguments = new ServerStartArguments
        {
            ConnectionTimeout    = TimeSpan.FromDays(1),
            HandleStringRequest  = HandleRequest,
            SessionKey           = sessionKey,
            HandleErrorNotification = HandleHandlerError,
            IsCancellationRequested = WasEscapePressed,
        };

        var res = await PipesServer.RunAsync(serverStartArguments, true, () =>
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nResestablishing Connection\n");
            Console.ResetColor();
        });

        Console.WriteLine($"Finished after {sw.Elapsed}");
        if (!res.Succeeded)
        {
            LocalLog.Error(res);
            return 1;
        }

        LocalLog.Info("Server terminiert.");
        return 0;
    }

    private static Task<bool> WasEscapePressed()
    {
        if (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            return Task.FromResult(false);
        Console.WriteLine("ESC gedrückt. Abbruch...");
        return Task.FromResult(true);
    }

    // -------------------------------------------------------------------------
    // Dispatch
    // -------------------------------------------------------------------------

    private static readonly JAPServer _japServer = new();

    private static readonly IJipDispatcher _jipDispatcher =
        new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(_japServer);

    private static async Task<string> HandleRequest(string requestStr)
    {
        Console.WriteLine($"SRV|RECEIVED> {requestStr}");
        var responseStr = await _jipDispatcher.Dispatch(requestStr);
        Console.WriteLine($"SRV|SENDING>  {responseStr}");
        return responseStr;
    }

    private static async Task HandleHandlerError(string request, Exception ex)
    {
        LocalLog.Error($"Fehler beim Verarbeiten der Anfrage: {request}", exceptionDetails: ex.ToString());
        await Task.CompletedTask;
    }
}
