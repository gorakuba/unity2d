using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location_3 : MonoBehaviour, ILocationEndTurnAbility
{
    private Transform FindFirstFreeSlot(LocationController loc)
    {
        for (int i = 0; i < 6; i++)
        {
            var slot = loc.transform.Find($"Slot_{i}");
            if (slot != null && slot.childCount == 0)
                return slot;
        }
        return null;
    }

    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        if (hero == null)
            yield break;

        var currentLoc = hero.CurrentLocation;
        if (currentLoc == null)
            yield break;

        int moved = 0;
        var panel = GameManager.Instance.heroSelectionPanel;
        var ctrl = panel != null ? panel.GetComponent<HeroSelectionPanelController>() : null;
        var neighbors = currentLoc.neighbors;

        while (moved < 2 && (currentLoc.HasCivillian() || currentLoc.HasThug()))
        {
            bool moveCivilian = currentLoc.HasCivillian();
            if (currentLoc.HasCivillian() && currentLoc.HasThug() && ctrl != null)
            {
                bool? chooseCiv = null;
                panel.SetActive(true);
                ctrl.Init("Move which token?", "Civilian", "Thug",
                    onHero1: () => { chooseCiv = true; panel.SetActive(false); },
                    onHero2: () => { chooseCiv = false; panel.SetActive(false); });
                yield return new WaitUntil(() => chooseCiv.HasValue);
                moveCivilian = chooseCiv.Value;
            }
            GameObject token = moveCivilian ? currentLoc.RemoveFirstCivillian() : currentLoc.RemoveFirstThug();
            if (token == null)
                break;

            LocationController target = neighbors != null && neighbors.Count > 1 ? neighbors[0] : null;
            if (neighbors != null && neighbors.Count > 1 && ctrl != null)
            {
                bool? chooseFirst = null;
                panel.SetActive(true);
                ctrl.Init("Move token to which location?", neighbors[0].name, neighbors[1].name,
                    onHero1: () => { chooseFirst = true; panel.SetActive(false); },
                    onHero2: () => { chooseFirst = false; panel.SetActive(false); });
                yield return new WaitUntil(() => chooseFirst.HasValue);
                target = chooseFirst.Value ? neighbors[0] : neighbors[1];
            }
            if (target == null)
            {
                Destroy(token);
                break;
            }
            var slot = FindFirstFreeSlot(target);
            if (slot == null)
            {
                Destroy(token);
            }
            else
            {
                token.transform.SetParent(slot);
                token.transform.localPosition = Vector3.zero;
                token.transform.localRotation = Quaternion.identity;
                if (token.GetComponent<TokenDrop>() == null)
                    token.AddComponent<TokenDrop>();
            }
            moved++;
            yield return null;
        }
    }
}