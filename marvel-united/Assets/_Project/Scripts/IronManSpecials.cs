using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class IronManSpecials : IHeroSpecials
{
    public IEnumerator ExecuteSpecial(string specialId, HeroController hero, SymbolPanelUI panel)
    {
        switch (specialId)
        {
            case "iron_man_special_1":
                yield return DistributeTokens(hero, "Attack", 2);
                break;
            case "iron_man_special_2":
                yield return DrawUpToThree(hero);
                break;
            case "iron_man_special_3":
                yield return DistributeTokens(hero, "Move", 2);
                break;
            default:
                Debug.LogWarning($"[IronManSpecials] Unknown special ID: {specialId}");
                break;
        }
    }

    private IEnumerator DistributeTokens(HeroController hero, string tokenId, int count)
    {
        var choicePanel = GameManager.Instance.heroSelectionPanel;
        if (choicePanel == null)
        {
            Debug.LogWarning("[IronManSpecials] No HeroSelectionPanel available");
            yield break;
        }

        var ctrl = choicePanel.GetComponent<HeroSelectionPanelController>();
        string hero1Id = GameManager.Instance.playerOneHero;
        string hero2Id = GameManager.Instance.playerTwoHero;
        string hero1Name = GameManager.Instance.GetHeroName(hero1Id);
        string hero2Name = GameManager.Instance.GetHeroName(hero2Id);

        for (int i = 0; i < count; i++)
        {
            bool? giveToFirst = null;
            choicePanel.SetActive(true);
            ctrl.Init(
                GameManager.Instance.GetHeroName(hero.HeroId),
                hero1Name,
                hero2Name,
                onHero1: () => { giveToFirst = true; choicePanel.SetActive(false); },
                onHero2: () => { giveToFirst = false; choicePanel.SetActive(false); }
            );

            yield return new WaitUntil(() => giveToFirst.HasValue);

            HeroController target = giveToFirst.Value ? SetupManager.hero1Controller : SetupManager.hero2Controller;
            if (target != null)
            {
                target.AddPersistentSymbol(tokenId);
                // refresh UI if the chosen hero is currently active
                if (TurnManager.Instance != null && target == (GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller))
                {
                    TurnManager.Instance.symbolPanelUI.AddPersistentSymbols(new List<string> { tokenId });
                }
            }
        }
    }

    private IEnumerator DrawUpToThree(HeroController hero)
    {
        var cardMgr = GameManager.Instance.cardManager;
        if (cardMgr == null)
            yield break;

        int playerIndex = hero == SetupManager.hero1Controller ? 1 : 2;
        var hand = playerIndex == 1 ? cardMgr.playerOneHand : cardMgr.playerTwoHand;

        while (hand.Count < 3)
        {
            var card = cardMgr.DrawHeroCard(playerIndex);
            if (card == null) break;
            hand.Add(card);
        }

        // refresh UI if it's this hero's turn
        if (TurnManager.Instance != null && GameManager.Instance.CurrentPlayerIndex == playerIndex)
        {
            TurnManager.Instance.heroHandUI.ShowHand(hero.HeroId, hand, TurnManager.Instance.HandlePlayerCardSelected);
        }
        yield return null;
    }
}