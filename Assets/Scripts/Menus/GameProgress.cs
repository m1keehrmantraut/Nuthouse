using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }
    
    public List<string> sceneOrder = new List<string>(); // Порядок сцен по очереди
    private const string CURRENT_SCENE_KEY = "CurrentScene";
    private const string COMPLETED_SCENES_KEY = "CompletedScenes";
    
    private string sceneToLoadFromMenu; // Сцена для загрузки из главного меню
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public string GetSceneToLoadFromMenu()
    {
        return sceneToLoadFromMenu;
    }
    
    public void SetSceneToLoadFromMenu(string sceneName)
    {
        sceneToLoadFromMenu = sceneName;
        PlayerPrefs.SetString(CURRENT_SCENE_KEY, sceneName);
        PlayerPrefs.Save();
    }
    
    public void LoadSceneFromMenu()
    {
        if (!string.IsNullOrEmpty(sceneToLoadFromMenu))
        {
            SceneManager.LoadScene(sceneToLoadFromMenu);
        }
        else
        {
            LoadScene(sceneOrder[0]);
        }
    }
    
    public void LoadScene(string sceneName)
    {
        SetSceneToLoadFromMenu(sceneName);
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadNextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = sceneOrder.IndexOf(currentScene);
        
        if (currentIndex >= 0 && currentIndex < sceneOrder.Count - 1)
        {
            string nextScene = sceneOrder[currentIndex + 1];
            MarkSceneAsCompleted(currentScene);
            LoadScene(nextScene);
        }
        else
        {
            Debug.Log("Это последняя сцена в списке");
            MarkSceneAsCompleted(currentScene);
            LoadMainMenu();
        }
    }
    
    public void MarkSceneAsCompleted(string sceneName)
    {
        string completedScenes = PlayerPrefs.GetString(COMPLETED_SCENES_KEY, "");
        
        if (!completedScenes.Contains(sceneName))
        {
            if (!string.IsNullOrEmpty(completedScenes))
            {
                completedScenes += "," + sceneName;
            }
            else
            {
                completedScenes = sceneName;
            }
            
            PlayerPrefs.SetString(COMPLETED_SCENES_KEY, completedScenes);
            PlayerPrefs.Save();
        }
    }
    
    public bool IsSceneCompleted(string sceneName)
    {
        string completedScenes = PlayerPrefs.GetString(COMPLETED_SCENES_KEY, "");
        return completedScenes.Contains(sceneName);
    }
    
    public List<string> GetCompletedScenes()
    {
        string completedScenes = PlayerPrefs.GetString(COMPLETED_SCENES_KEY, "");
        if (string.IsNullOrEmpty(completedScenes))
        {
            return new List<string>();
        }
        
        return new List<string>(completedScenes.Split(','));
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
    }
    
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(CURRENT_SCENE_KEY);
        PlayerPrefs.DeleteKey(COMPLETED_SCENES_KEY);
        sceneToLoadFromMenu = "";
        PlayerPrefs.Save();
    }
    
    public string GetSavedScene()
    {
        return PlayerPrefs.GetString(CURRENT_SCENE_KEY, "");
    }   
}