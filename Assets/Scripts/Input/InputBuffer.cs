using System.Collections.Generic;
using UnityEngine;

namespace Nuthouse.Input
{
    public sealed class InputBuffer
    {
        private readonly InputConfig config;
        private readonly List<BufferedCommand> commands = new(8);

        public InputBuffer(InputConfig config) => this.config = config;

        public void Add(InputAction action)
        {
            commands.RemoveAll(c => c.Action == action);
            commands.Add(new BufferedCommand(action, Time.time, config.GetTtl(action)));
        }

        public bool Has(InputAction action)
            => commands.Exists(c => c.Action == action);

        public bool TryConsume(InputAction action)
        {
            int i = commands.FindIndex(c => c.Action == action);
            if (i < 0) return false;
            commands.RemoveAt(i);
            return true;
        }

        public void Tick()
        {
            float now = Time.time;
            commands.RemoveAll(c => c.IsExpired(now));
        }

        public void Clear() => commands.Clear();

        public IReadOnlyList<BufferedCommand> Snapshot => commands;
    }
}
