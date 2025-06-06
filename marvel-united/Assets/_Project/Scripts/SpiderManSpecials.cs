using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderManSpecials : IHeroSpecials
{
    private int special1Tokens = 0;

    public IEnumerator ExecuteSpecial(string specialId, HeroController hero, SymbolPanelUI panel)
    {
        switch (specialId)
        {
            case "spider-man_special_1":
                yield return SpecialOne(hero, panel);
                break;
            case "spider-man_special_2":
                yield return SpecialTwo(hero, panel);
                break;
            case "spider-man_special_3":
                yield return SpecialThree(hero, panel);
                break;
            default:
                Debug.LogWarning($"[SpiderManSpecials] Unknown special ID: {specialId}");
                break;
        }
    }

    private IEnumerator SpecialOne(HeroController hero, SymbolPanelUI panel)
    {
        for (int i = 0; i < 3; i++)
            hero.AddPersistentSymbol("Attack");
        special1Tokens += 3;

        if (TurnManager.Instance != null && hero == (GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller))
        {
            panel.AddPersistentSymbols(new List<string> { "Attack", "Attack", "Attack" });
        }
        yield return null;
    }

    private IEnumerator SpecialTwo(HeroController hero, SymbolPanelUI panel)
    {
        var loc = hero.CurrentLocation;
        if (loc == null) yield break;
        var handler = panel.actionHandler;
        if (handler == null) yield break;

        int rescued = 0;
        while (rescued < 3 && loc.HasCivillian())
        {
            var civ = loc.RemoveFirstCivillian();
            if (civ == null) break;
            HUDMessageManager.Instance?.Enqueue($"Uratowano cywila w {loc.name}");
            if (handler.missionManager.civiliansCompleted) Object.Destroy(civ);
            else
            {
                Vector3 ws = civ.transform.lossyScale;
                foreach (var slot in handler.missionTokenSlots)
                    if (slot.childCount == 0)
                    {
                        Vector3 ps = slot.lossyScale;
                        civ.transform.SetParent(slot, false);
                        civ.transform.localScale = new Vector3(ws.x / ps.x, ws.y / ps.y, ws.z / ps.z);
                        civ.transform.localPosition = Vector3.zero;
                        civ.transform.localRotation = Quaternion.identity;
                        break;
                    }
            }
            rescued++;
        }

        for (int i = 0; i < rescued; i++)
            hero.AddPersistentSymbol("Attack");

        if (rescued > 0)
        {
            if (TurnManager.Instance != null && hero == (GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller))
            {
                var list = new List<string>();
                for (int i = 0; i < rescued; i++) list.Add("Attack");
                panel.AddPersistentSymbols(list);
            }
            handler.missionManager.CheckMissions();
        }
        yield return null;
    }

    private IEnumerator SpecialThree(HeroController hero, SymbolPanelUI panel)
    {
        hero.AddPersistentSymbol("Move");
        hero.AddPersistentSymbol("Move");
        if (TurnManager.Instance != null && hero == (GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller))
        {
            panel.AddPersistentSymbols(new List<string> { "Move", "Move" });
        }
        yield return null;
    }

    public void RegisterAttackUsage(HeroController hero, bool defeatedThug, bool wasPersistent)
    {
        if (!wasPersistent || special1Tokens <= 0) return;
        special1Tokens--;
        if (defeatedThug)
        {
            hero.AddPersistentSymbol("Heroic");
            if (TurnManager.Instance != null && hero == (GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller))
            {
                TurnManager.Instance.symbolPanelUI.AddPersistentSymbols(new List<string> { "Heroic" });
            }
        }
    }
}