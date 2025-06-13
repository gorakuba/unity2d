using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location_1 : MonoBehaviour, ILocationEndTurnAbility
{
    private DiscardPanelUI DiscardPanel => DiscardPanelUI.Instance;

    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        if (hero == null)
            yield break;

        var cardMgr = GameManager.Instance?.cardManager;
        if (cardMgr == null || DiscardPanel == null)
            yield break;

        // Determine which player's hand/deck to use
        bool isPlayerTwo = hero == SetupManager.hero2Controller;
        List<HeroCard> hand = isPlayerTwo ? cardMgr.playerTwoHand : cardMgr.playerOneHand;
        List<HeroCard> deck = isPlayerTwo ? cardMgr.playerTwoDeck : cardMgr.playerOneDeck;

        if (hand == null || hand.Count == 0)
            yield break;

        HeroCard chosen = null;
        DiscardPanel.Open(hand, card =>
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
                deck.Add(card); // bottom of deck
                chosen = card;
            }
        }, cardMgr, hero.HeroId);

        // Wait until player picks a card
        while (DiscardPanel.IsActive)
            yield return null;

        if (chosen == null)
            yield break;

        // Ask which hero loses a Crisis token
        var panel = GameManager.Instance.heroSelectionPanel;
        if (panel == null)
            yield break;

        var ctrl = panel.GetComponent<HeroSelectionPanelController>();
        string hero1Name = GameManager.Instance.GetHeroName(GameManager.Instance.playerOneHero);
        string hero2Name = GameManager.Instance.GetHeroName(GameManager.Instance.playerTwoHero);

        bool? removeFromFirst = null;
        panel.SetActive(true);
        ctrl.Init(
            "Remove Crisis token from which hero?",
            hero1Name,
            hero2Name,
            onHero1: () => { removeFromFirst = true; panel.SetActive(false); },
            onHero2: () => { removeFromFirst = false; panel.SetActive(false); }
        );

        yield return new WaitUntil(() => removeFromFirst.HasValue);

        HeroController target = removeFromFirst.Value ? SetupManager.hero1Controller : SetupManager.hero2Controller;
        if (target != null)
        {
            var crisis = target.GetComponent<HeroCrisisHandler>();
            crisis?.RemoveCrisisToken();
        }

        CrisisTokenManager.Instance?.UpdateUI();
    }
}