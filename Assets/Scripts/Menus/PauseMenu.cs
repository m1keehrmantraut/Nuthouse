using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [HideInInspector] public static bool GameIsPaused = false;
    [SerializeField] private MeleeCombat com;
    
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private AudioMixerGroup Mixer;
    
    public void Resume()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        com.GameIsPaused = false;
        pauseMenuUI.SetActive(false);
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        com.GameIsPaused = true;
        Time.timeScale = 0.0001f;
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeMusicVolume(float volume)
    {
        Mixer.audioMixer.SetFloat("MusicVolume", Mathf.Lerp(-80, 0, volume));
    }
    
    public void ChangeEffectsVolume(float volume)
    {
        Mixer.audioMixer.SetFloat("EffectsVolume", Mathf.Lerp(-80, 0, volume));
    }
}
