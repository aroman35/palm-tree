using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans.Runtime;
using Orleans.Storage;
using Serilog;

namespace PalmTree.Abstractions;

public static class ServiceCollectionExtensions
{
    private static string s_storatge = "-storage";

    public static IServiceCollection AddStorage<TState, TDbContext>(this IServiceCollection services)
        where TState : StateBase, new()
        where TDbContext : class, IStorageContext
    {
        var stateName = StateName<TState>();

        services.TryAddSingleton<TDbContext>();
        services.AddSingletonNamedService<IGrainStorage>(
            stateName + s_storatge,
            (provider, name) => new StorageProvider<TState>(
                ServiceProviderServiceExtensions.GetRequiredService<TDbContext>(provider),
                name,
                ServiceProviderServiceExtensions.GetRequiredService<ILogger>(provider)
            ));

        return services;
    }

    public static IServiceCollection AddStorage<TState>(this IServiceCollection services)
        where TState : StateBase, new()
    {
        var stateName = StateName<TState>();
        
        services.AddSingletonNamedService<IGrainStorage>(
            stateName + s_storatge,
            (provider, name) => new StorageProvider<TState>(
                provider.GetRequiredService<IStorageContext>(),
                name,
                provider.GetRequiredService<ILogger>()
            ));

        return services;
    }

    private static string StateName<TState>() where TState : StateBase
    {
        var stateName = typeof(TState).GetCustomAttribute<StorageNameAttribute>()?.Name
                        ?? throw new InvalidOperationException(
                            $"Storage for type {typeof(TState)} has no name defined");

        return stateName;
    }
}