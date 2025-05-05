using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class DiscardPanelUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject buttonPrefab;
    public Transform buttonParent;
    public GameObject panelRoot;

    public static DiscardPanelUI Instance;

    private Action<HeroCard> onCardSelected;
    private CardManager cardManager;
    private string heroId;

    public bool IsActive { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
        panelRoot.SetActive(false);
    }

    public void Open(List<HeroCard> handCards, Action<HeroCard> callback, CardManager cm, string heroId)
    {
        Debug.Log("DiscardPanelUI → Open called"); // <<< Dodaj to
        this.heroId = heroId;
        cardManager = cm;
        onCardSelected = callback;
        foreach (Transform child in buttonParent)
            Destroy(child.gameObject);
        panelRoot.SetActive(true);
        foreach (var card in handCards)
        {
            var btnGO = Instantiate(buttonPrefab, buttonParent);
            var btnImg = btnGO.GetComponent<Image>();
            var btn = btnGO.GetComponent<Button>();

            var sprite = cardManager.GetCardSprite(heroId, card);
            btnImg.sprite = sprite;

            btn.onClick.AddListener(() =>
            {
                onCardSelected?.Invoke(card);
                Close();
            });
        }
        Debug.Log("AKTYWACJA PANELU -> " + panelRoot);
        panelRoot.SetActive(true);
        IsActive = true;

        Debug.Log("[DiscardPanelUI] -> Panel otwarty");
    }

    public void Close()
    {
        panelRoot.SetActive(false);
        IsActive = false;

        Debug.Log("[DiscardPanelUI] -> Panel zamknięty");
    }
}
