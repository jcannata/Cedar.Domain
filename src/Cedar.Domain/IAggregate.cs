namespace Cedar.Domain
{
    public interface IAggregate
    {
        /// <summary>
        ///     Gets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        ///     Gets the curent version of the Aggregate
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        int Version { get; }

        /// <summary>
        ///     Gets the original version of the Aggrgate before any events have been raised.
        /// </summary>
        /// <value>
        ///     The original version of the Aggregate.
        /// </value>
        int OriginalVersion { get; }

        IRehydrateAggregate BeginRehydrate();

        /// <summary>
        ///     Takes the uncommitted events. This will reset the Aggregate and it's original version number.
        /// </summary>
        /// <returns>
        ///     A readonly collection of uncommitted events;
        /// </returns>
        IUncommittedEvents TakeUncommittedEvents();
    }
}