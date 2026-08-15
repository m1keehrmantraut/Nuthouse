using System;
using System.Collections.Generic;

namespace Nuthouse.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new();

        public static void Register<TService>(TService instance) where TService : class
        {
            var key = typeof(TService);
            if (services.ContainsKey(key))
                throw new InvalidOperationException($"ServiceLocator: {key.Name} уже зарегистрирован.");
            services[key] = instance;
        }

        public static TService Get<TService>() where TService : class
        {
            var key = typeof(TService);
            if (!services.TryGetValue(key, out var svc))
                throw new InvalidOperationException(
                    $"ServiceLocator: {key.Name} не зарегистрирован. Проверь GameBootstrap.");
            return (TService)svc;
        }

        public static bool TryGet<TService>(out TService service) where TService : class
        {
            if (services.TryGetValue(typeof(TService), out var svc))
            {
                service = (TService)svc;
                return true;
            }
            service = null;
            return false;
        }

        public static bool Has<TService>() where TService : class
            => services.ContainsKey(typeof(TService));

        public static void Unregister<TService>() where TService : class
            => services.Remove(typeof(TService));

        public static void Clear() => services.Clear();
    }
}
