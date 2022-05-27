namespace PalmTree.Abstractions;

public abstract class JournalStateBase<TEvent> : StateBase
    where TEvent : EventBase
{
    public abstract void Apply(TEvent @event);
}