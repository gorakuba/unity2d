using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StorylinePanelUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject buttonPrefab;
    public Transform buttonParent;
    public GameObject panelRoot;
    public Button exitButton;

    public static StorylinePanelUI Instance;

    private Action<int> onCardSelected;
    private CardManager cardManager;

    public bool IsActive { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
        if (panelRoot != null)
            panelRoot.SetActive(false);
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(Close);
            exitButton.gameObject.SetActive(false);
        }
    }

    public void Open(List<HeroCard> cards, Action<int> callback, CardManager cm)
    {
        cardManager = cm;
        onCardSelected = callback;

        foreach (Transform child in buttonParent)
            Destroy(child.gameObject);

        for (int i = 0; i < cards.Count; i++)
        {
            var btnGO = Instantiate(buttonPrefab, buttonParent);
            var btnImg = btnGO.GetComponent<Image>();
            var btn = btnGO.GetComponent<Button>();

            var sprite = cardManager.GetCardSprite(cards[i].heroId, cards[i]);
            btnImg.sprite = sprite;

            int idx = i;
            btn.onClick.AddListener(() =>
            {
                onCardSelected?.Invoke(idx);
                Close();
            });
        }

        panelRoot.SetActive(true);
        if (exitButton != null)
            exitButton.gameObject.SetActive(true);
        IsActive = true;
    }

    public void Close()
    {
        panelRoot.SetActive(false);
        if (exitButton != null)
            exitButton.gameObject.SetActive(false);
        IsActive = false;
    }
}