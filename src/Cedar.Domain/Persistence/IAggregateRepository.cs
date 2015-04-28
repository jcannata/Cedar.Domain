namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Domain;

    public interface IAggregateRepository
    {
        /// <summary>
        /// Gets an Aggregate by it's ID.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="bucketId"></param>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TAggregate> GetById<TAggregate>(
            string bucketId,
            string id,
            int version,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate;

        /// <summary>
        /// Saves the specified aggregate.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="bucketId">The bucket identifier.</param>
        /// <param name="commitId">The commit identifier. NOTE: If you have a Command ID, you should use that here.</param>
        /// <param name="updateHeaders">An action to update the stream headers</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        Task Save(
            IAggregate aggregate,
            string bucketId,
            Guid commitId,
            Action<IDictionary<string, string>> updateHeaders,
            CancellationToken cancellationToken);
    }
}