using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.AutoPilot
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<object>> _handlers = new Dictionary<Type, List<object>>();

        public void Handle<T>(Action<T> handler)
        {
            List<object> list;
            if (!_handlers.TryGetValue(typeof(T), out list))
            {
                list = new List<object>();
                _handlers.Add(typeof(T), list);
            }
            list.Add(handler);
        }

        public T Publish<T>() where T : new()
        {
            return Publish(new T());
        }

        public T Publish<T>(T @event)
        {
            List<object> list;
            if (_handlers.TryGetValue(typeof(T), out list))
                foreach (var handler in list.Cast<Action<T>>())
                    handler(@event);
            return @event;
        }
    }
}