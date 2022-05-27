using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Serilog;

namespace PalmTree.Abstractions;

public abstract class PersistentJournalGrain<TState, TEvent, TStorageContext> : JournaledGrain<TState, TEvent>,
    ICustomStorageInterface<TState, TEvent>
    where TState : JournalStateBase<TEvent>, new()
    where TEvent : EventBase
    where TStorageContext : IStorageContext
{
    private TStorageContext _storageContext;
    private ILogger _logger;

    private TStorageContext Storage => _storageContext ??= ServiceProvider.GetRequiredService<TStorageContext>();

    private ILogger Logger => _logger ??= ServiceProvider.GetRequiredService<ILogger>()
        .ForContext<PersistentJournalGrain<TState, TEvent, TStorageContext>>();
    
    public async Task<KeyValuePair<int, TState>> ReadStateFromStorage()
    {
        
        try
        {
            var state = await Storage.ReadVersionedState<TState, TEvent>(this.GetPrimaryKey());
            return state;
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "Error reading state");
            throw;
        }
    }

    public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<TEvent> updates, int expectedversion)
    {
        try
        {
            var sequence = await Storage.AttachJournalEvents(this.GetPrimaryKey(), updates);
            return sequence == expectedversion;
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "Error applying events");
            throw;
        }
    }

    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync();
        Trace.CorrelationManager.ActivityId = this.GetPrimaryKey();
    }
}