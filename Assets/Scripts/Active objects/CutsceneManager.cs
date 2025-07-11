using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;

    public void Enable()
    {
        playableDirector.Play();
    }
}
