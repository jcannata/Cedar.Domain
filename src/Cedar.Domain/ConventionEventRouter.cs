namespace Cedar.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ConventionEventRouter : IEventRouter
    {
        private readonly IDictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();
        private readonly bool _throwOnApplyNotFound;
        private IAggregate _registered;

        public ConventionEventRouter(bool throwOnApplyNotFound = false)
        {
            _throwOnApplyNotFound = throwOnApplyNotFound;
        }

        public virtual void Register<T>(Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            Register(typeof(T), @event => handler((T)@event));
        }

        public void Register(IAggregate aggregate)
        {
            if (aggregate == null)
            {
                throw new ArgumentNullException("aggregate");
            }

            _registered = aggregate;

            var applyMethods = aggregate.GetType()
                .GetRuntimeMethods()
                .Where(m => !m.IsStatic
                    && m.Name == "Apply"
                    && m.GetParameters().Length == 1
                    && m.ReturnType == typeof(void))
                .Select(m => new { Method = m, MessageType = m.GetParameters().Single().ParameterType });

            foreach (var apply in applyMethods)
            {
                MethodInfo applyMethod = apply.Method;
                _handlers.Add(apply.MessageType, m => applyMethod.Invoke(aggregate, new[] { m }));
            }
        }

        public virtual void Dispatch(object eventMessage)
        {
            if (eventMessage == null)
            {
                throw new ArgumentNullException("eventMessage");
            }

            Action<object> handler;
            if (_handlers.TryGetValue(eventMessage.GetType(), out handler))
            {
                handler(eventMessage);
            }
            else if (_throwOnApplyNotFound)
            {
                _registered.ThrowHandlerNotFound(eventMessage);
            }
        }

        private void Register(Type messageType, Action<object> handler)
        {
            _handlers[messageType] = handler;
        }
    }
}