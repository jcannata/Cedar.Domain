namespace Cedar.Domain.Persistence
{
    using System;

    public delegate IAggregate CreateAggregate(Type type, string id);
}