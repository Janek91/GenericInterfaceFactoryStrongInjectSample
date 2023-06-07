using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using StrongInject;

namespace GenericInterfaceFactoryStrongInjectSample;

//[Register(typeof(LoggerService))]
public partial class Container //: IContainer<LoggerService> //cannot register it like that, because it creates circular dependency
{
    [Instance] public static ILoggerProvider[] LoggerProviders { get; set; } = { };

    [Factory(Scope.SingleInstance)]
    public static SerilogLoggerFactory GetSerilogLoggerFactory(LoggerProviderCollection providerCollection)
    {
        return new(null, true, providerCollection);
    }

    [FactoryOf(typeof(ILogger<>))]
    public static T CreateLogger<T>(T loggerRequestingInstance)
    {
        var loggerFactory = GetSerilogLoggerFactory(GetLoggerProviderCollection(LoggerProviders));
        var methodInfo = typeof(LoggerFactoryExtensions).GetMethods()
            .Single(x => x.Name == nameof(LoggerFactoryExtensions.CreateLogger) && x.IsGenericMethod);
        var senderType = typeof(T).GenericTypeArguments.Single();
        var genericMethod = methodInfo.MakeGenericMethod(senderType);
        return (T)genericMethod.Invoke(loggerFactory, new[] { loggerFactory });
    }

    [Factory(Scope.SingleInstance)]
    public static LoggerProviderCollection GetLoggerProviderCollection(ILoggerProvider[] loggerProviders)
    {
        var collection = new LoggerProviderCollection();
        foreach (var loggerProvider in loggerProviders)
            collection.AddProvider(loggerProvider);
        return collection;
    }
}