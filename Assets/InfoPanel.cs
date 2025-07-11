using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private Image infoImage;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private GameObject panel;

    public void DisplayImage(Sprite image)
    {
        if (image != null)
        {
            infoImage.sprite = image;
        }
    }

    public void DisplayText(string text)
    {
        if (text != null)
        {
            infoText.text = text;
        }
    }

    public void PanelMode(bool mode)
    {
        panel.SetActive(mode);
        Time.timeScale = mode ? 0f : 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PanelMode(false);
        }
    }
}
