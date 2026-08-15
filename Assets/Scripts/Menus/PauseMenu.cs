using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Nuthouse.Core.Events;

namespace Nuthouse.Menus
{
public class PauseMenu : MonoBehaviour
{
    [HideInInspector] public static bool GameIsPaused = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private AudioMixerGroup Mixer;

    private void OnEnable()
    {
        GameEvents.PauseRequested += OnPauseRequested;
        GameEvents.PauseChanged += OnPauseChanged;
    }

    private void OnDisable()
    {
        GameEvents.PauseRequested -= OnPauseRequested;
        GameEvents.PauseChanged -= OnPauseChanged;
    }

    private void OnPauseRequested()
    {
        if (GameIsPaused) Resume();
        else Pause();
    }

    private void OnPauseChanged(bool paused)
    {
        if (paused) Pause();
        else Resume();
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        GameEvents.PublishPauseChanged(false);
    }

    public void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0.0001f;
        GameIsPaused = true;
        GameEvents.PublishPauseChanged(true);
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
}