using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Slider volumeSlider;

    private Resolution[] uniqueResolutions;
    private int savedResolutionIndex;

    void Awake()
    {
        InitResolutionList();

        savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", uniqueResolutions.Length - 1);
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        float volume = PlayerPrefs.GetFloat("Volume", 1f);

        resolutionDropdown.value = savedResolutionIndex;
        fullscreenToggle.isOn = isFullscreen;
        volumeSlider.value = volume;

        ApplySettings(savedResolutionIndex, isFullscreen);
        AudioListener.volume = volume;

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void InitResolutionList()
    {
        Resolution[] allRes = Screen.resolutions;
        float targetRatio = Mathf.Round((float)Screen.currentResolution.width / Screen.currentResolution.height * 100f) / 100f;

        var options = new List<string>();
        var filtered = new List<Resolution>();
        var seen = new HashSet<string>();

        foreach (var res in allRes)
        {
            float resRatio = Mathf.Round((float)res.width / res.height * 100f) / 100f;
            if (Mathf.Abs(resRatio - targetRatio) > 0.01f) continue;

            string key = $"{res.width} х {res.height}";
            if (seen.Contains(key)) continue;

            seen.Add(key);
            filtered.Add(res);
            options.Add(key);
        }

        uniqueResolutions = filtered.ToArray();
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    public void OnResolutionChanged(int index)
    {
        ApplySettings(index, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void OnFullscreenChanged(bool isFullscreen)
    {
        ApplySettings(resolutionDropdown.value, isFullscreen);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    void ApplySettings(int resIndex, bool fullscreen)
    {
        if (resIndex < 0 || resIndex >= uniqueResolutions.Length) return;
        var res = uniqueResolutions[resIndex];
        Screen.SetResolution(res.width, res.height, fullscreen);
    }
}
