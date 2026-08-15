using System.Collections.Generic;

namespace Nuthouse.Services
{
    public interface IProgressService
    {
        void ResetProgress();
        string GetSavedScene();
        bool IsSceneCompleted(string sceneName);
        IReadOnlyList<string> GetCompletedScenes();
    }
}
