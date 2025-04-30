using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroHandUI : MonoBehaviour
{
    [Header("Prefaby i kontener")]
    public GameObject cardButtonPrefab;
    public Transform handPanel;

    private List<GameObject> spawnedButtons = new();

    public void ShowHand(List<HeroCard> hand, Action<HeroCard> onCardSelected)
    {
        ClearHandUI();

        foreach (var card in hand)
        {
            GameObject button = Instantiate(cardButtonPrefab, handPanel);
            HeroCardButton handler = button.GetComponent<HeroCardButton>();
            handler.Setup(card, onCardSelected);
            spawnedButtons.Add(button);
        }
    }

    public void ClearHandUI()
    {
        foreach (var go in spawnedButtons)
            Destroy(go);
        spawnedButtons.Clear();
    }
}
