namespace Nuthouse.Services
{
    public interface ISceneService
    {
        string SceneToLoadFromMenu { get; }
        void SetSceneToLoadFromMenu(string sceneName);
        void LoadSceneFromMenu();
        void LoadScene(string sceneName);
        void LoadNextScene();
        void LoadMainMenu();
    }
}
