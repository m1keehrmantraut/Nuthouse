using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CustomResolutionDropdown : MonoBehaviour
{
    [SerializeField] private Button selectedButton;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    private Resolution[] filteredResolutions;
    private List<Button> buttons = new List<Button>();
    private Button currentSelectedButton;
    private Color selectedColor = new Color(1f, 1f, 1f);
    private Color unselectedColor = new Color(0.3f, 0.3f, 0.3f);

    private void Awake()
    {
        selectedButton.onClick.AddListener(ToggleOptions);

        float aspect = Mathf.Round((float)Screen.width / Screen.height * 100) / 100f;
        Resolution[] all = Screen.resolutions;
        filteredResolutions = all.Where(r => Mathf.Round((float)r.width / r.height * 100) / 100f == aspect)
                                 .GroupBy(r => new { r.width, r.height })
                                 .Select(g => g.First())
                                 .OrderByDescending(r => r.width * r.height)
                                 .ToArray();

        foreach (var res in filteredResolutions)
        {
            GameObject buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"{res.width} x {res.height}";

            Button btn = buttonObj.GetComponent<Button>();
            buttons.Add(btn);

            Resolution localRes = res;
            btn.onClick.AddListener(() => SetResolution(localRes, btn));
        }

        int index = PlayerPrefs.GetInt("ResolutionIndex", -1);
        if (index >= 0 && index < filteredResolutions.Length)
        {
            SetResolution(filteredResolutions[index], buttons[index]);
        }
        else
        {
            Resolution current = Screen.currentResolution;
            int closestIndex = System.Array.FindIndex(filteredResolutions, r => r.width == current.width && r.height == current.height);
            if (closestIndex == -1) closestIndex = 0;
            SetResolution(filteredResolutions[closestIndex], buttons[closestIndex]);
        }
    }

    private void ToggleOptions()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    private void SetResolution(Resolution resolution, Button btn)
    {
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        selectedButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{resolution.width} х {resolution.height}";

        if (currentSelectedButton != null)
            currentSelectedButton.GetComponent<Image>().color = unselectedColor;

        currentSelectedButton = btn;
        currentSelectedButton.GetComponent<Image>().color = selectedColor;

        int index = System.Array.IndexOf(filteredResolutions, resolution);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
        optionsPanel.SetActive(false);
    }
}
