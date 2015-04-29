namespace Cedar.Domain.Testing
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class AggregateTests
    {
        [Fact]
        public Task a_passing_aggregate_scenario_should()
        {
            return Scenario.ForAggregate(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened())
                .Run();
        }

        [Fact]
        public Task a_passing_aggregate_with_events_raised_in_the_constructor_should()
        {
            return Scenario.ForAggregate<ConstructorBehaviorAggregate>()
                .When(() => new ConstructorBehaviorAggregate(Guid.Empty))
                .Then(new SomethingHappened())
                .Run();
        }

        [Fact]
        public Task a_passing_aggregate_scenario_with_no_given_should()
        {
            return Scenario.ForAggregate(id => new Aggregate(id))
                .When(a => a.DoSomething())
                .Then(new SomethingHappened())
                .Run();
        }

        [Fact]
        public async Task an_aggregate_throwing_an_exception_should()
        {
            Exception caughtException = null;

            try
            {
                await Scenario.ForAggregate(id => new BuggyAggregate(id))
                    .When(a => a.DoSomething())
                    .Then(new SomethingHappened())
                    .Run();
            }
            catch(Exception ex)
            {
                caughtException = ex;
            }

            caughtException.Should().NotBeNull();
        }

        [Fact]
        public async Task an_aggregate_throwing_an_exception_in_its_constructor_should()
        {
            Exception caughtException = null;

            try
            {
                await Scenario.ForAggregate(id => new ReallyBuggyAggregate(id))
                    .When(a => a.DoSomething())
                    .Then(new SomethingHappened()).Run();
            }
            catch(Exception ex)
            {
                caughtException = ex;
            }

            caughtException.Should().NotBeNull();
        }

        [Fact]
        public Task an_aggregate_throwing_an_expected_exception_should()
        {
            return Scenario.ForAggregate(id => new BuggyAggregate(id))
                .When(a => a.DoSomething())
                .ThenShouldThrow<InvalidOperationException>()
                .Run();
        }

        [Fact]
        public async Task an_aggregate_throwing_an_un_expected_exception_should()
        {
            Exception caughtException = null;

            try
            {
                await Scenario.ForAggregate(id => new BuggyAggregate(id))
                .When(a => a.DoSomething())
                .ThenShouldThrow<ArgumentException>()
                .Run();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            caughtException.Should().NotBeNull();
        }

        private class SomethingHappened
        {
            public override string ToString()
            {
                return "Something happened.";
            }
        }

        private class Aggregate : AggregateBase
        {
            private int _something;

            public Aggregate(string id) : base(id)
            {}

            private void Apply(SomethingHappened e)
            {
                _something++;
            }

            public void DoSomething()
            {
                RaiseEvent(new SomethingHappened());
            }
        }

        private class BuggyAggregate : AggregateBase
        {
            public BuggyAggregate(string id) : base(id)
            {}

            public void DoSomething()
            {
                throw new InvalidOperationException();
            }
        }

        private class ReallyBuggyAggregate : AggregateBase
        {
            public ReallyBuggyAggregate(string id)
                : base(id)
            {
                throw new InvalidOperationException();
            }

            public void DoSomething()
            {}
        }

        private class ConstructorBehaviorAggregate : AggregateBase
        {
            public ConstructorBehaviorAggregate(Guid id)
                : base(id.ToString())
            {
                RaiseEvent(new SomethingHappened());
            }

            protected ConstructorBehaviorAggregate(string id) : base(id)
            {}

            private void Apply(SomethingHappened e)
            {}
        }
    }
}