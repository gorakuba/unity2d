using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroHandUI : MonoBehaviour
{
    [Header("Prefaby i kontener")]
    public GameObject cardButtonPrefab;
    public Transform handPanel;

    private List<GameObject> spawnedButtons = new();

    /// <summary>
    /// heroId – identyfikator bohatera ("iron_man", "captain_america", itp.),
    /// hand – lista kart gracza,
    /// onCardSelected – callback dla wybranej karty.
    /// </summary>
    public void ShowHand(string heroId, List<HeroCard> hand, Action<HeroCard> onCardSelected)
    {
        ClearHandUI();

        foreach (var card in hand)
        {
            GameObject button = Instantiate(cardButtonPrefab, handPanel);
            var handler = button.GetComponent<HeroCardButton>();
            handler.Setup(heroId, card, onCardSelected);
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
