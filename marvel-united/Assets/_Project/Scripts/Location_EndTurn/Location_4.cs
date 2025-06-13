using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location_4 : MonoBehaviour, ILocationEndTurnAbility
{
    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        if (hero == null)
            yield break;

        var cardMgr = GameManager.Instance?.cardManager;
        if (cardMgr == null)
            yield break;

        int playerIndex = hero == SetupManager.hero1Controller ? 1 : 2;
        List<HeroCard> hand = playerIndex == 1 ? cardMgr.playerOneHand : cardMgr.playerTwoHand;

        while (hand.Count < 3)
        {
            var card = cardMgr.DrawHeroCard(playerIndex);
            if (card == null)
                break;
            hand.Add(card);
        }

        if (TurnManager.Instance != null && GameManager.Instance.CurrentPlayerIndex == playerIndex)
        {
            TurnManager.Instance.heroHandUI.ShowHand(hero.HeroId, hand, TurnManager.Instance.HandlePlayerCardSelected);
        }
        yield return null;
    }
}