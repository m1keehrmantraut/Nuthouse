namespace Nuthouse.Core
{
    public abstract class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
    {
        protected override bool PersistAcrossScenes => true;
    }
}
