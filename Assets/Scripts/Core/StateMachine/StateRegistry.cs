using System;
using System.Collections.Generic;

namespace Nuthouse.Core.StateMachine
{
    public sealed class StateRegistry
    {
        private readonly Dictionary<Type, IState> states = new();

        public void Register(IState state) => states[state.GetType()] = state;
        public T Get<T>() where T : IState => (T)states[typeof(T)];
    }
}
