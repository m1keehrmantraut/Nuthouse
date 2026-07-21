using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    
    public void PlayGame()
    {
        SceneManager.LoadScene("HomeLocation");
    }
    private void OnContinueClicked()
    {
        GameProgress.Instance.LoadSceneFromMenu();
    }
    
    private void OnNewGameClicked()
    {
        GameProgress.Instance.ResetProgress();
        GameProgress.Instance.LoadScene(GameProgress.Instance.sceneOrder[0]);
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