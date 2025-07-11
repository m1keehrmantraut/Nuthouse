using UnityEngine;

public class QuickTrigger : MonoBehaviour
{
    [SerializeField] private float delay = 0f;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private bool markSceneCompleted = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !string.IsNullOrEmpty(sceneToLoad))
        {
            Invoke(nameof(DoSceneTransition), delay);
        }
    }

    private void DoSceneTransition()
    {
        if (markSceneCompleted)
        {
            SceneTransitionManager.Instance.CompleteSceneAndLoad(sceneToLoad);
        }
        else
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(sceneToLoad);
        }
    }
}