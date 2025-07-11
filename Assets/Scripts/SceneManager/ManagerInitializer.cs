using UnityEngine;

public class ManagersInitializer : MonoBehaviour
{
    private static bool isInitialized = false;

    private void Awake()
    {
        if (isInitialized) return;

        if (GameProgress.Instance == null)
        {
            GameObject gp = Instantiate(Resources.Load<GameObject>("GameProgressManager"));
            DontDestroyOnLoad(gp);
        }

        if (SceneTransitionManager.Instance == null)
        {
            GameObject stm = Instantiate(Resources.Load<GameObject>("SceneTransitionManager"));
            DontDestroyOnLoad(stm);
        }

        isInitialized = true;
    }
}