using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ThreatChoicePanelController : MonoBehaviour
{
    public Button damageButton;
    public Button tokenButton;
    public TextMeshProUGUI damageLabelText;  // przypnij w Inspectorze
    public TextMeshProUGUI tokenLabelText;   // przypnij w Inspectorze
    public TextMeshProUGUI headerText;

    /// <summary>
    /// heroName   – tekst w nagłówku
    /// dmgLabel   – podpis na pierwszym przycisku
    /// tokenLabel – podpis na drugim przycisku
    /// </summary>
    public void Init(string heroName, string dmgLabel, string tokenLabel, Action onDamage, Action onTokens)
    {
        headerText.text         = $"{heroName.ToUpper()} CHOOSE!";
        damageLabelText.text    = dmgLabel;
        tokenLabelText.text     = tokenLabel;

        damageButton.onClick.RemoveAllListeners();
        tokenButton.onClick.RemoveAllListeners();

        damageButton.onClick.AddListener(() => onDamage?.Invoke());
        tokenButton.onClick.AddListener(() => onTokens?.Invoke());
    }
}
