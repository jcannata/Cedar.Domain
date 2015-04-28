namespace Cedar.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Cedar.Domain.Internal;
    using EnsureThat;

    public abstract class AggregateBase : IAggregate, IEquatable<IAggregate>
    {
        private readonly string _id;
        private readonly IEventRouter _registeredRoutes;
        private readonly List<IUncommittedEvent> _uncommittedEvents = new List<IUncommittedEvent>();
        private int _originalVersion;

        protected AggregateBase(string id)
            : this(id, new ConventionEventRouter())
        { }

        protected AggregateBase(string id, IEventRouter eventRouter)
        {
            Ensure.That(id, "id").IsNotNullOrWhiteSpace();
            Ensure.That(eventRouter, "eventRouter").IsNotNull();

            _id = id;
            _registeredRoutes = eventRouter;
            _registeredRoutes.Register(this);
        }

        public string Id
        {
            get { return _id; }
        }

        public int Version { get; private set; }

        IRehydrateAggregate IAggregate.BeginRehydrate()
        {
            return new RehydrateAggregate(this);
        }

        int IAggregate.OriginalVersion
        {
            get { return _originalVersion; }
        }

        IUncommittedEvents IAggregate.TakeUncommittedEvents()
        {
            var uncommittedEvents = new UncommittedEvents(
                _originalVersion,
                new ReadOnlyCollection<IUncommittedEvent>(_uncommittedEvents.ToArray()));
            _uncommittedEvents.Clear();
            _originalVersion = Version;
            return uncommittedEvents;
        }

        public bool Equals(IAggregate other)
        {
            return null != other && other.Id == Id;
        }

        private void ApplyEvent(object @event)
        {
            _registeredRoutes.Dispatch(@event);
            Version++;
        }

        protected void Register<T>(Action<T> route)
        {
            _registeredRoutes.Register(route);
        }

        protected void RaiseEvent(object @event)
        {
            if (@event == null)
            {
                return;
            }
            ApplyEvent(@event);
            var eventId = DeterministicEventIdGenerator.Generate(@event, _id, Version);
            _uncommittedEvents.Add(new UncommittedEvent(eventId, Version, @event));
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IAggregate);
        }

        private class RehydrateAggregate : IRehydrateAggregate
        {
            private readonly AggregateBase _aggregateBase;

            public RehydrateAggregate(AggregateBase aggregateBase)
            {
                _aggregateBase = aggregateBase;
            }

            public void Dispose()
            {
                _aggregateBase._originalVersion = _aggregateBase.Version;
            }

            public void ApplyEvent(object @event)
            {
                _aggregateBase.ApplyEvent(@event);
            }
        }

        private class UncommittedEvents : ReadOnlyCollection<IUncommittedEvent>, IUncommittedEvents
        {
            private readonly int _originalVersion;


            public UncommittedEvents(int originalVersion, IList<IUncommittedEvent> events)
                : base(events)
            {
                _originalVersion = originalVersion;
            }

            public int OriginalVersion
            {
                get { return _originalVersion; }
            }
        }

        private class UncommittedEvent : IUncommittedEvent
        {
            private readonly object _event;
            private readonly Guid _eventId;
            private readonly int _version;

            public UncommittedEvent(Guid eventId, int version, object @event)
            {
                _eventId = eventId;
                _version = version;
                _event = @event;
            }

            public Guid EventId
            {
                get { return _eventId; }
            }

            public int Version
            {
                get { return _version; }
            }

            public object Event
            {
                get { return _event; }
            }
        }
    }
}