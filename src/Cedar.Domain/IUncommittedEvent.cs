namespace Cedar.Domain
{
    using System;

    public interface IUncommittedEvent
    {
        Guid EventId { get; }

        int Version { get; }

        object Event { get; }
    }
}