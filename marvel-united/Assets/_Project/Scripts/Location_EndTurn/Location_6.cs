using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location_6 : MonoBehaviour, ILocationEndTurnAbility
{
    private DiscardPanelUI DiscardPanel => DiscardPanelUI.Instance;

    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        if (hero == null)
            yield break;

        var cardMgr = GameManager.Instance?.cardManager;
        if (cardMgr == null || DiscardPanel == null)
            yield break;

        bool isPlayerTwo = hero == SetupManager.hero2Controller;
        List<HeroCard> deck = isPlayerTwo ? cardMgr.playerTwoDeck : cardMgr.playerOneDeck;
        if (deck == null || deck.Count == 0)
            yield break;

        HeroCard chosen = null;
        // Show entire deck and let player pick a card
        DiscardPanel.Open(deck, card => { chosen = card; }, cardMgr, hero.HeroId);

        while (DiscardPanel.IsActive)
            yield return null;

        if (chosen == null)
            yield break;

        deck.Remove(chosen);
        Shuffle(deck);
        deck.Insert(0, chosen); // place chosen card on top
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}