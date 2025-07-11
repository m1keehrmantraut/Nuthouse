using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneData
{
    public string sceneName;
    public bool isCompleted;
}

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    [SerializeField] private List<SceneData> scenes;
    private const string SaveKey = "GameProgress";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else Destroy(gameObject);
    }

    public string GetNextUncompletedScene()
    {
        foreach (var scene in scenes)
        {
            if (!scene.isCompleted)
                return scene.sceneName;
        }

        return scenes.Count > 0 ? scenes[0].sceneName : null;
    }

    public void MarkSceneCompleted(string sceneName)
    {
        var scene = scenes.Find(s => s.sceneName == sceneName);
        if (scene != null)
        {
            scene.isCompleted = true;
            SaveProgress();
        }
    }

    private void SaveProgress()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(SaveKey), this);
        }
    }

    public void ResetProgress()
    {
        foreach (var s in scenes)
            s.isCompleted = false;

        SaveProgress();
    }
}