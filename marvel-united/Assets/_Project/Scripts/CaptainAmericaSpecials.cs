using System.Collections;
using UnityEngine;

public class CaptainAmericaSpecials : IHeroSpecials
{
    public IEnumerator ExecuteSpecial(string specialId, HeroController hero, SymbolPanelUI panel)
    {
        switch (specialId)
        {
            case "captain_america_special_1":
                yield return Leadership(hero);
                break;
            default:
                Debug.LogWarning($"[CaptainAmericaSpecials] Unknown special ID: {specialId}");
                break;
        }
    }

    private IEnumerator Leadership(HeroController hero)
    {
        var choicePanel = GameManager.Instance.heroSelectionPanel;
        if (choicePanel == null)
        {
            Debug.LogWarning("[CaptainAmericaSpecials] No HeroSelectionPanel available");
            yield break;
        }

        var ctrl = choicePanel.GetComponent<HeroSelectionPanelController>();
        string hero1Id = GameManager.Instance.playerOneHero;
        string hero2Id = GameManager.Instance.playerTwoHero;
        string hero1Name = GameManager.Instance.GetHeroName(hero1Id);
        string hero2Name = GameManager.Instance.GetHeroName(hero2Id);

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
            target.AddPersistentSymbol("Wild");
            // if chosen hero is currently active, refresh UI
            if (TurnManager.Instance != null && target == (GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller))
            {
                TurnManager.Instance.symbolPanelUI.AddPersistentSymbols(new System.Collections.Generic.List<string>{"Wild"});
            }
        }
    }
}