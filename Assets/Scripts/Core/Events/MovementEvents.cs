using Nuthouse.Combat;

namespace Nuthouse.Core.Events
{
    public static class MovementEvents
    {
        public static event System.Action Jumped;
        public static event System.Action<float> Landed;
        public static event System.Action<bool> RunChanged;
        public static event System.Action<bool> CrouchChanged;

        public static void PublishJumped() => Jumped?.Invoke();
        public static void PublishLanded(float impact) => Landed?.Invoke(impact);
        public static void PublishRunChanged(bool running) => RunChanged?.Invoke(running);
        public static void PublishCrouchChanged(bool crouching) => CrouchChanged?.Invoke(crouching);
    }
}
