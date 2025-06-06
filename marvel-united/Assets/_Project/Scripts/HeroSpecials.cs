using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeroSpecials
{
    IEnumerator ExecuteSpecial(string specialId, HeroController hero, SymbolPanelUI panel);
}

public class BlackPantherSpecials : IHeroSpecials
{
    public IEnumerator ExecuteSpecial(string specialId, HeroController hero, SymbolPanelUI panel)
    {
        switch (specialId)
        {
            case "black_panther_special_1":
                yield return SpecialOne(panel);
                break;
            case "black_panther_special_2":
                yield return SpecialTwo(panel);
                break;
            case "black_panther_special_3":
                yield return SpecialThree(panel);
                break;
            default:
                Debug.LogWarning($"[BlackPantherSpecials] Unknown special ID: {specialId}");
                break;
        }
    }

    private IEnumerator SpecialOne(SymbolPanelUI panel)
    {
        panel.AddCurrentSymbols(new List<string> { "Heroic", "Attack" });
        yield return null;
    }

    private IEnumerator SpecialTwo(SymbolPanelUI panel)
    {
        panel.AddCurrentSymbols(new List<string> { "Move", "Attack" });
        yield return null;
    }

    private IEnumerator SpecialThree(SymbolPanelUI panel)
    {
        panel.AddCurrentSymbols(new List<string> { "Move", "Heroic" });
        yield return null;
    }
}
