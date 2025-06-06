using System.Collections;
using UnityEngine;

public class CaptainMarvelSpecials : IHeroSpecials
{
    public IEnumerator ExecuteSpecial(string specialId, HeroController hero, SymbolPanelUI panel)
    {
        switch (specialId)
        {
            case "captain_marvel_special_1":
                yield return SpecialOne(hero, panel);
                break;
            default:
                Debug.LogWarning($"[CaptainMarvelSpecials] Unknown special ID: {specialId}");
                break;
        }
    }

    private IEnumerator SpecialOne(HeroController hero, SymbolPanelUI panel)
    {
        var loc = hero.CurrentLocation;
        if (loc == null || loc.neighbors == null || loc.neighbors.Count == 0)
            yield break;

        var choicePanel = GameManager.Instance.heroSelectionPanel;
        if (choicePanel == null)
        {
            Debug.LogWarning("[CaptainMarvelSpecials] No HeroSelectionPanel available");
            yield break;
        }

        // assume each location has exactly two neighbors
        var first = loc.neighbors[0];
        var second = loc.neighbors.Count > 1 ? loc.neighbors[1] : null;
        if (second == null)
        {
            Debug.LogWarning("[CaptainMarvelSpecials] Expected two neighbor locations");
            yield break;
        }

        choicePanel.SetActive(true);
        var ctrl = choicePanel.GetComponent<HeroSelectionPanelController>();
        bool? chooseFirst = null;
        ctrl.Init(
            "Select location to attack",
            first.name,
            second.name,
            onHero1: () => { chooseFirst = true; choicePanel.SetActive(false); },
            onHero2: () => { chooseFirst = false; choicePanel.SetActive(false); }
        );
        yield return new WaitUntil(() => chooseFirst.HasValue);

        var targetLoc = chooseFirst.Value ? first : second;

        var handler = panel.actionHandler;
        if (handler == null || targetLoc == null)
            yield break;

        handler.StartSpecialAttack(targetLoc);
        yield return new WaitUntil(() =>
            !targetLoc.attackButton.gameObject.activeSelf &&
            !handler.punchUIButton.gameObject.activeSelf &&
            !targetLoc.minionButton.gameObject.activeSelf &&
            !targetLoc.threatCardButton.gameObject.activeSelf);

        handler.StartSpecialAttack(targetLoc);
        yield return null;
    }
}