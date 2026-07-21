using UnityEngine;
using UnityEngine.SceneManagement;

public class QuickTrigger : MonoBehaviour
{
    [SerializeField] private float delay = 0f;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private bool markSceneCompleted = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void DoSceneTransition()
    {
        if (markSceneCompleted)
        {
            //GameProgress.Instance.CompleteSceneAndLoad(sceneToLoad);
        }
        else
        {
            //GameProgress.Instance.LoadSceneWithFade(sceneToLoad);
        }
    }
}