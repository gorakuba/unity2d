using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location_6 : MonoBehaviour, ILocationEndTurnAbility
{
    private StorylinePanelUI StorylinePanel => StorylinePanelUI.Instance;

    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        if (hero == null)
            yield break;

        var cardMgr = GameManager.Instance?.cardManager;
        if (cardMgr == null || StorylinePanel == null)
            yield break;

        bool isPlayerTwo = hero == SetupManager.hero2Controller;
        List<HeroCard> deck = isPlayerTwo ? cardMgr.playerTwoDeck : cardMgr.playerOneDeck;
        if (deck == null || deck.Count == 0)
            yield break;

        int chosenIndex = -1;

        StorylinePanel.Open(deck, idx => { chosenIndex = idx; }, cardMgr);

        while (StorylinePanel.IsActive)
            yield return null;

        if (chosenIndex < 0 || chosenIndex >= deck.Count)
            yield break;

        HeroCard chosen = deck[chosenIndex];
        deck.RemoveAt(chosenIndex);
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