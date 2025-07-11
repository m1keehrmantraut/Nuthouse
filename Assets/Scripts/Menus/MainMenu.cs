using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        string nextScene = GameProgress.Instance.GetNextUncompletedScene();

        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(nextScene);
        }
        else
        {
            Debug.Log("Нет доступных сцен.");
        }
    }

    public void ResetProgress()
    {
        GameProgress.Instance.ResetProgress();
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}