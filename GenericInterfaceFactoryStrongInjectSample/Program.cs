using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using StrongInject;

namespace GenericInterfaceFactoryStrongInjectSample;

public class Program
{
    public static string DefaultConsoleOutputTemplate { get; set; } =
        "[{Timestamp:HH:mm:ss}|{Level:u3}] <s:{SourceContext}>{NewLine}   {Message:lj}  {Exception}{NewLine}";

    private Container _container;

    private static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: DefaultConsoleOutputTemplate)
            .CreateLogger();

        var program = new Program();
        program.Run();
    }

    private void Run()
    {
        var logger = Container.CreateLogger<ILogger<Program>>(null);
        logger.LogInformation("Hello, World!");
        //that works, but I want to resolve services dependent on ILogger<>,
        //but I cannot register them even in container, because it creates circular dependency according to StrongInject
        //So this is impossible:
        _container = new();
        try
        {
            var service = ((IContainer<LoggerService>)_container).Resolve<LoggerService>().Value;
            service.Log("Hello again!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }

        //Or I don't always know the generic parameter type, and I want to resolve loggers directly from container
        //something like that:
        try
        {
            logger = GetLogger<Program>();
            logger.LogInformation("Hello from the other side!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private ILogger<T> GetLogger<T>()
    {
        //I know that Container doesn't inherit from that interface, but how should it be done correctly?
        var owned = (_container as IContainer<ILogger<T>>).Resolve();
        return owned.Value;
    }
}