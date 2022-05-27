using Orleans;
using Orleans.Runtime;
using Orleans.Storage;
using Serilog;

namespace PalmTree.Abstractions;

public class StorageProvider<TState> : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    where TState : StateBase, new()
{
    private readonly IStorageContext _dbContext;
    private readonly ILogger _logger;
    private readonly string _storageName;

    public StorageProvider(IStorageContext dbContext, string storageName, ILogger logger)
    {
        _dbContext = dbContext;
        _storageName = storageName;
        _logger = logger;
    }

    public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        var persistedState = await _dbContext.ReadState<TState>(grainReference.GetPrimaryKey());
        grainState.State = persistedState;
        grainState.RecordExists = true;
        _logger.Debug("Read state from storage {GrainId}", grainReference.GetPrimaryKey());
    }

    public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        await _dbContext.WriteState(grainReference.GetPrimaryKey(), (TState)grainState.State);
        _logger.Debug("State for grain {GrainId} added to storage", grainReference.GetPrimaryKey());
    }

    public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        await _dbContext.ClearState<TState>(grainReference.GetPrimaryKey());
        _logger.Debug("State for grain {GrainId} cleared", grainReference.GetPrimaryKey());
    }

    public void Participate(ISiloLifecycle lifecycle)
    {
        lifecycle.Subscribe(
            OptionFormattingUtilities.Name<StorageProvider<TState>>(_storageName),
            ServiceLifecycleStage.ApplicationServices,
            cancellationToken => _dbContext.InitializeStorage(cancellationToken));

        _logger.Debug("Storage initialized");
    }
}