namespace Cedar.Domain.Testing
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public static partial class Scenario
    {
        public static Aggregate.IGiven<T> ForAggregate<T>(
            Func<string, T> factory = null,
            string aggregateId = null,
            [CallerMemberName] string scenarioName = null) where T : IAggregate
        {
            aggregateId = aggregateId ?? "testid";
            factory = factory ?? (id => (T)Activator.CreateInstance(typeof(T), id));

            return new Aggregate.ScenarioBuilder<T>(factory, aggregateId, scenarioName);
        }

        public static class Aggregate
        {
            public interface IGiven<T> : IWhen<T> where T : IAggregate
            {
                IWhen<T> Given(params object[] events);
            }

            public interface IWhen<T> : IThen where T : IAggregate
            {
                IThen When(Expression<Func<T, Task>> when);

                IThen When(Expression<Action<T>> when);

                IThen When(Expression<Func<T>> when);
            }

            public interface IThen
            {
                IThen Then(params object[] expectedEvents);

                IThen ThenNothingHappened();

                IThen ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null)
                    where TException : Exception;

                Task Run();
            }

            internal class ScenarioBuilder<T> : IGiven<T> where T : IAggregate
            {
                private Func<string, T> _factory;
                private readonly string _aggregateId;
                private readonly string _name;

                private readonly Action<T> _runGiven;
                private Func<T, Task> _runWhen;
                private Action<T> _runThen;
                private Action<IAggregate> _afterGiven;

                private object[] _given;
                private LambdaExpression _when;
                private object[] _expect;
                private object _results;

                public ScenarioBuilder(Func<string, T> factory, string aggregateId, string name)
                {
                    _factory = factory;
                    _aggregateId = aggregateId;
                    _name = name;
                    _afterGiven = aggregate => aggregate.TakeUncommittedEvents();
                    _runGiven = aggregate =>
                    {
                        using(var rehydrateAggregate = aggregate.BeginRehydrate())
                        {
                            foreach(var @event in _given ?? new object[0])
                            {
                                rehydrateAggregate.ApplyEvent(@event);
                            }
                        }

                        _afterGiven(aggregate);
                    };
                    _runWhen = _ =>
                    {
                        throw new Exception("When not set.");
                    };
                }

                public IWhen<T> Given(params object[] events)
                {
                    _given = events;
                    return this;
                }

                public IThen When(Expression<Func<T, Task>> when)
                {
                    _when = when;
                    _runWhen = aggregate => when.Compile()(aggregate);
                    return this;
                }

                public IThen When(Expression<Action<T>> when)
                {
                    _when = when;
                    _runWhen = aggregate =>
                    {
                        when.Compile()(aggregate);
                        return Task.FromResult(true);
                    };
                    return this;
                }

                public IThen When(Expression<Func<T>> when)
                {
                    _when = when;
                    _factory = _ => when.Compile()();
                    _runWhen = _ => Task.FromResult(true);
                    _afterGiven = _ => { };
                    return this;
                }

                public IThen Then(params object[] expectedEvents)
                {
                    GuardThenNotSet();

                    _expect = expectedEvents;

                    _runThen = aggregate =>
                    {
                        var uncommittedEvents = aggregate.TakeUncommittedEvents().Select(e => e.Event);

                        _results = uncommittedEvents;

                        if(!uncommittedEvents.SequenceEqual(expectedEvents, MessageEqualityComparer.Instance) || _results is Exception)
                        {
                            throw new Exception(
                                string.Format(
                                    "Expected events ({1}), got ({0}) instead.",
                                    _results.NicePrint()
                                        .Aggregate(new StringBuilder(), (builder, s) => builder.Append(s))
                                        .ToString(),
                                    _expect.NicePrint()
                                        .Aggregate(new StringBuilder(), (builder, s) => builder.Append(s))
                                        .ToString()));
                        }
                    };
                    return this;
                }

                public IThen ThenNothingHappened()
                {
                    return Then(Enumerable.Empty<object>());
                }

                public IThen ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null)
                    where TException : Exception
                {
                    GuardThenNotSet();

                    _expect = isMatch != null
                        ? new object[] { typeof(TException), isMatch }
                        : new object[] { typeof(TException) };

                    _runThen = _ =>
                    {
                        if(_results == null || !typeof(TException).IsAssignableFrom(_results.GetType()))
                        {
                            throw new Exception(
                                string.Format("Expected exception of type {0}, got {1} instead.", 
                                typeof(TException).FullName, 
                                !(_results is Exception) ? "(no exception was thrown)" : _results.GetType().FullName));
                        }
                    };

                    return this;
                }

                private void GuardThenNotSet()
                {
                    if(_runThen != null)
                        throw new InvalidOperationException("Then already set.");
                }

                public string Name
                {
                    get { return _name; }
                }

                public async Task Run()
                {
                    var aggregate = _factory(_aggregateId);

                    _runGiven(aggregate);

                    if(_runWhen == null)
                    {
                        throw new Exception("When not set.");
                    }

                    try
                    {
                        await _runWhen(aggregate);
                    }
                    catch(Exception ex)
                    {
                        _results = ex;
                    }

                    if(_runThen == null)
                    {
                        throw new Exception("Then not set.");
                    }

                    _runThen(aggregate);
                }
            }
        }
    }
}