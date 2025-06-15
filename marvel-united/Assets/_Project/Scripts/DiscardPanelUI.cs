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
    private Action<int> onIndexSelected;
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
        onIndexSelected = null;
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
        onCardSelected = null;
        onIndexSelected = null;

        Debug.Log("[DiscardPanelUI] -> Panel zamknięty");
    }

    public void OpenWithIndex(List<HeroCard> cards, Action<int> callback, CardManager cm, string heroId)
    {
        this.heroId = heroId;
        cardManager = cm;
        onIndexSelected = callback;
        onCardSelected = null;

        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }
        
        panelRoot.SetActive(true);

        for (int i = 0; i < cards.Count; i++)
        {
            var btnGO = Instantiate(buttonPrefab, buttonParent);
            var btnImg = btnGO.GetComponent<Image>();
            var btn = btnGO.GetComponent<Button>();

            var sprite = cardManager.GetCardSprite(heroId, cards[i]);
            btnImg.sprite = sprite;
            
            int idx = i;
            btn.onClick.AddListener(() =>
            {
                onIndexSelected?.Invoke(idx);
                Close();
            });
        }
        
        panelRoot.SetActive(true);
        IsActive = true;
    }
}
