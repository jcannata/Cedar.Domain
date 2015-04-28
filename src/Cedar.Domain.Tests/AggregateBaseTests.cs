namespace Cedar.Domain
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class AggregateBaseTests
    {
        [Fact]
        public void Two_aggregates_with_same_id_should_be_equal()
        {
            object aggregate1 = new Aggregate("id");
            object aggregate2 = new Aggregate("id");

            aggregate1.Equals(aggregate2).Should().BeTrue();
        }

        [Fact]
        public void Get_hash_code_should_be_id()
        {
            var aggregate = new Aggregate("id");

            aggregate.GetHashCode().Should().Be("id".GetHashCode());
        }

        [Fact]
        public void Should_not_require_an_explicit_handler()
        {
            var aggregate = new Aggregate("id");

            aggregate.Command();

            aggregate.Version.Should().Be(1);
        }

        [Fact]
        public void Should_route_event()
        {
            object @event = null;
            var aggregate = new Aggregate("id", e => @event = e);
            var eventThatIsRouted = new EventThatIsRouted();

            using (var rehydrate = ((IAggregate)aggregate).BeginRehydrate())
            {
                rehydrate.ApplyEvent(eventThatIsRouted);
            }

            @event.Should().Be(eventThatIsRouted);
        }

        [Fact]
        public void After_rehydrating_original_version_should_be_set()
        {
            var aggregate = ((IAggregate)new Aggregate("id"));
            using (var rehydrate = aggregate.BeginRehydrate())
            {
                rehydrate.ApplyEvent(new EventThatHasAnApply());
            }

            aggregate.OriginalVersion.Should().Be(1);
        }

        [Fact]
        public void When_applying_command_then_version_should_be_incremented()
        {
            var aggregate = new Aggregate("id");
            aggregate.Command();

            ((IAggregate)aggregate).OriginalVersion.Should().Be(0);
            aggregate.Version.Should().Be(1);
        }

        [Fact]
        public void When_taking_uncommitted_events_then_original_version_should_equal_version()
        {
            var aggregate = new Aggregate("id");
            aggregate.Command();

            ((IAggregate)aggregate).TakeUncommittedEvents();
            ((IAggregate)aggregate).OriginalVersion.Should().Be(1);
        }

        [Fact]
        public void When_raising_null_event_then_should_have_no_uncommitted_events()
        {
            var aggregate = new Aggregate("id");
            aggregate.CommandThatDoesNothing();

            var uncommittedEvents = ((IAggregate)aggregate).TakeUncommittedEvents();

            uncommittedEvents.Count.Should().Be(0);
        }

        private class EventThatHasAnApply
        { }

        private class EventThatDoesNotHaveAnApply
        { }

        private class EventThatIsRouted
        { }

        private class Aggregate : AggregateBase
        {
            public Aggregate(string id, Action<EventThatIsRouted> routedEventCallbackForTesting = null)
                : base(id)
            {
                Register<EventThatIsRouted>(@event => routedEventCallbackForTesting(@event));
            }

            public void Command()
            {
                RaiseEvent(new EventThatDoesNotHaveAnApply());
            }

            public void CommandThatDoesNothing()
            {
                RaiseEvent(null);
            }

            private void Apply(EventThatHasAnApply @event) { }
        }
    }
}