using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nuthouse.Services
{
    public sealed class SceneService : ISceneService
    {
        public List<string> sceneOrder = new();

        public string SceneToLoadFromMenu { get; private set; }

        public void SetSceneToLoadFromMenu(string sceneName)
        {
            SceneToLoadFromMenu = sceneName;
            PlayerPrefs.SetString("CurrentScene", sceneName);
            PlayerPrefs.Save();
        }

        public void LoadSceneFromMenu()
        {
            if (!string.IsNullOrEmpty(SceneToLoadFromMenu))
                LoadScene(SceneToLoadFromMenu);
            else if (sceneOrder.Count > 0)
                LoadScene(sceneOrder[0]);
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

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void MarkSceneAsCompleted(string sceneName)
        {
            const string key = "CompletedScenes";
            string completedScenes = PlayerPrefs.GetString(key, "");

            if (!completedScenes.Contains(sceneName))
            {
                completedScenes = string.IsNullOrEmpty(completedScenes)
                    ? sceneName
                    : completedScenes + "," + sceneName;

                PlayerPrefs.SetString(key, completedScenes);
                PlayerPrefs.Save();
            }
        }
    }
}
