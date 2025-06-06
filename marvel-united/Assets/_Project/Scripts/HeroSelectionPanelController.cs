using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HeroSelectionPanelController : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    public Button hero1Button;
    public Button hero2Button;
    public TextMeshProUGUI hero1LabelText;
    public TextMeshProUGUI hero2LabelText;

    /// <summary>
    /// Initialize button labels and callbacks.
    /// </summary>
    public void Init(string header, string hero1Name, string hero2Name, Action onHero1, Action onHero2)
    {
        if (headerText != null)
            headerText.text = header;
        if (hero1LabelText != null)
            hero1LabelText.text = hero1Name;
        if (hero2LabelText != null)
            hero2LabelText.text = hero2Name;

        if (hero1Button != null)
        {
            hero1Button.onClick.RemoveAllListeners();
            hero1Button.onClick.AddListener(() => onHero1?.Invoke());
        }
        if (hero2Button != null)
        {
            hero2Button.onClick.RemoveAllListeners();
            hero2Button.onClick.AddListener(() => onHero2?.Invoke());
        }
    }
}