namespace Nuthouse.Input
{
    public readonly struct BufferedCommand
    {
        public readonly InputAction Action;
        public readonly float Time;
        public readonly float Ttl;

        public BufferedCommand(InputAction action, float time, float ttl)
        {
            Action = action;
            Time = time;
            Ttl = ttl;
        }

        public bool IsExpired(float now) => now - Time > Ttl;
    }
}
