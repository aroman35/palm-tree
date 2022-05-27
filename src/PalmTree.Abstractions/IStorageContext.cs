namespace PalmTree.Abstractions;

public interface IStorageContext
{
    Task WriteState<TState>(Guid id, TState state) where TState : StateBase, new();

    Task ClearState<TState>(Guid id) where TState : StateBase, new();

    Task<TState> ReadState<TState>(Guid id) where TState : StateBase, new();

    Task<KeyValuePair<int, TState>> ReadVersionedState<TState, TEvent>(Guid id)
        where TState : JournalStateBase<TEvent>, new()
        where TEvent : EventBase;

    Task<int> AttachJournalEvents<TEvent>(Guid id, IReadOnlyList<TEvent> events)
        where TEvent : EventBase;

    Task InitializeStorage(CancellationToken cancellationToken);
}