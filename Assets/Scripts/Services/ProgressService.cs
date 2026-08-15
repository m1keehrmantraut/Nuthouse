using System.Collections.Generic;
using UnityEngine;

namespace Nuthouse.Services
{
    public sealed class ProgressService : IProgressService
    {
        private const string CURRENT_SCENE_KEY = "CurrentScene";
        private const string COMPLETED_SCENES_KEY = "CompletedScenes";

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(CURRENT_SCENE_KEY);
            PlayerPrefs.DeleteKey(COMPLETED_SCENES_KEY);
            PlayerPrefs.Save();
        }

        public string GetSavedScene()
        {
            return PlayerPrefs.GetString(CURRENT_SCENE_KEY, "");
        }

        public bool IsSceneCompleted(string sceneName)
        {
            string completedScenes = PlayerPrefs.GetString(COMPLETED_SCENES_KEY, "");
            return completedScenes.Contains(sceneName);
        }

        public IReadOnlyList<string> GetCompletedScenes()
        {
            string completedScenes = PlayerPrefs.GetString(COMPLETED_SCENES_KEY, "");
            if (string.IsNullOrEmpty(completedScenes))
                return new List<string>();

            return new List<string>(completedScenes.Split(','));
        }
    }
}
