using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ThreatChoicePanelController : MonoBehaviour
{
    public Button damageButton;
    public Button tokenButton;
    public TextMeshProUGUI headerText;   // przeciągnij w Inspectorze swój TextMeshProUGUI

    /// <summary>
    /// Inicjalizuje panel:
    /// - ustawia nagłówek na nazwę bohatera,
    /// - podłącza callbacki do przycisków.
    /// </summary>
    public void Init(string heroName, Action onDamage, Action onTokens)
    {
        // ustaw nagłówek
        headerText.text = $"{heroName.ToUpper()} CHOOSE!";

        // usuń stare listener’y
        damageButton.onClick.RemoveAllListeners();
        tokenButton.onClick.RemoveAllListeners();

        // podłącz nowe
        damageButton.onClick.AddListener(() => onDamage?.Invoke());
        tokenButton.onClick.AddListener(() => onTokens?.Invoke());
    }
}
