using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaspSpecials : IHeroSpecials
{
    public IEnumerator ExecuteSpecial(string specialId, HeroController hero, SymbolPanelUI panel)
    {
        switch (specialId)
        {
            case "wasp_special_1":
                yield return SpecialOne(hero, panel);
                break;
            case "wasp_special_2":
                yield return SpecialTwo(hero, panel);
                break;
            case "wasp_special_3":
                yield return SpecialThree(hero);
                break;
            default:
                Debug.LogWarning($"[WaspSpecials] Unknown special ID: {specialId}");
                break;
        }
    }

    private IEnumerator SpecialOne(HeroController hero, SymbolPanelUI panel)
    {
        var moveMgr = panel.actionHandler.movementManager;
        if (moveMgr == null) yield break;

        var otherHeroes = new List<HeroController>();
        if (SetupManager.hero1Controller != null && SetupManager.hero1Controller != hero &&
            SetupManager.hero1Controller.CurrentLocation == hero.CurrentLocation)
            otherHeroes.Add(SetupManager.hero1Controller);
        if (SetupManager.hero2Controller != null && SetupManager.hero2Controller != hero &&
            SetupManager.hero2Controller.CurrentLocation == hero.CurrentLocation)
            otherHeroes.Add(SetupManager.hero2Controller);

        var chosenHeroes = new List<HeroController>();
        foreach (var h in otherHeroes)
        {
            bool choice = false;
            yield return AskMoveWithWasp(h, val => choice = val);
            if (choice)
                chosenHeroes.Add(h);
        }

        bool moveDone = false;
        moveMgr.OnMoveCompleted = () => { moveDone = true; };
        moveMgr.PrepareHeroTeleport();
        yield return new WaitUntil(() => moveDone);

        var targetLoc = hero.CurrentLocation;
        foreach (var h in chosenHeroes)
            moveMgr.TeleportHero(h, targetLoc);

        yield return null;
    }

    private IEnumerator AskMoveWithWasp(HeroController otherHero, System.Action<bool> callback)
    {
        var panel = GameManager.Instance.heroSelectionPanel;
        if (panel == null)
        {
            Debug.LogWarning("[WaspSpecials] HeroSelectionPanel is missing");
            callback?.Invoke(false);
            yield break;
        }

        panel.SetActive(true);
        var ctrl = panel.GetComponent<HeroSelectionPanelController>();
        string name = GameManager.Instance.GetHeroName(otherHero.HeroId);
        string header = $"Move {name} with Wasp?";

        bool? result = null;
        ctrl.Init(
            header,
            "TAK",
            "NIE",
            onHero1: () => { result = true; panel.SetActive(false); },
            onHero2: () => { result = false; panel.SetActive(false); }
        );

        yield return new WaitUntil(() => result.HasValue);
        callback?.Invoke(result.Value);
    }

    private IEnumerator SpecialTwo(HeroController hero, SymbolPanelUI panel)
    {
        var moveMgr = panel.actionHandler.movementManager;
        if (moveMgr == null) yield break;

        bool moveDone = false;
        moveMgr.OnMoveCompleted = () => { moveDone = true; };
        moveMgr.PrepareHeroTeleport();
        yield return new WaitUntil(() => moveDone);

        var loc = hero.CurrentLocation;
        var handler = panel.actionHandler;
        if (loc == null || handler == null) yield break;

        handler.StartSpecialAttack(loc);

        yield return null;
    }

    private IEnumerator SpecialThree(HeroController hero)
    {
        hero.IsInvulnerable = true;
        void Reset(HeroController h)
        {
            if (h == hero)
            {
                hero.IsInvulnerable = false;
                TurnManager.Instance.OnStartHeroTurn -= Reset;
            }
        }
        TurnManager.Instance.OnStartHeroTurn += Reset;
        yield return null;
    }
}