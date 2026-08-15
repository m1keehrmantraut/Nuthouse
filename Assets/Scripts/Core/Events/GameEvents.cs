namespace Nuthouse.Core.Events
{
    public static class GameEvents
    {
        public static event System.Action PauseRequested;
        public static event System.Action<bool> PauseChanged;
        public static event System.Action CheckpointReached;

        public static void PublishPauseRequested() => PauseRequested?.Invoke();
        public static void PublishPauseChanged(bool paused) => PauseChanged?.Invoke(paused);
        public static void PublishCheckpointReached() => CheckpointReached?.Invoke();
    }
}
