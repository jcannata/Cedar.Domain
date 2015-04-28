namespace Cedar.Domain
{
    using System.Collections.Generic;

    public interface IUncommittedEvents : IReadOnlyCollection<IUncommittedEvent>
    {
        int OriginalVersion { get; }
    }
}