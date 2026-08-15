using UnityEngine;

namespace Nuthouse.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual bool PersistAcrossScenes => false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = (T)this;
            if (PersistAcrossScenes) DontDestroyOnLoad(gameObject);
            OnAwake();
        }

        protected virtual void OnAwake() { }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
