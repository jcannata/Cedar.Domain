namespace Cedar.Domain
{
    using System;

    /// <summary>
    ///     Represents an operation to rehydrate an Aggregate
    /// </summary>
    public interface IRehydrateAggregate : IDisposable
    {
        /// <summary>
        ///     Applies the event.
        /// </summary>
        /// <param name="event">
        ///     The event to be applied
        /// </param>
        void ApplyEvent(object @event);
    }
}