using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location_8 : MonoBehaviour, ILocationEndTurnAbility
{
    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        var locMan = GameManager.Instance?.locationManager;
        var handler = TurnManager.Instance?.symbolPanelUI?.actionHandler;
        var missionMgr = handler?.missionManager;

        if (locMan == null || handler == null || missionMgr == null)
            yield break;

        var locations = new List<LocationController>();
        foreach (var root in locMan.LocationRoots)
        {
            var ctrl = root.GetComponent<LocationController>();
            if (ctrl != null && ctrl.HasThug())
                locations.Add(ctrl);
        }

        if (locations.Count == 0)
            yield break;

        LocationController chosen = null;
        foreach (var loc in locations)
        {
            bool use = false;
            yield return AskYesNo($"Remove Thug from {loc.name}?", val => use = val);
            if (use)
            {
                chosen = loc;
                break;
            }
        }

        if (chosen == null)
            yield break;

        var thug = chosen.RemoveFirstThug();
        if (thug == null)
            yield break;

        HUDMessageManager.Instance?.Enqueue($"Usunieto zbira w {chosen.name}");
        if (missionMgr.thugsCompleted)
        {
            Object.Destroy(thug);
        }
        else
        {
            Vector3 ws = thug.transform.lossyScale;
            foreach (var slot in handler.thugTokenSlots)
                if (slot.childCount == 0)
                {
                    Vector3 ps = slot.lossyScale;
                    thug.transform.SetParent(slot, false);
                    thug.transform.localScale = new Vector3(ws.x / ps.x, ws.y / ps.y, ws.z / ps.z);
                    thug.transform.localPosition = Vector3.zero;
                    thug.transform.localRotation = Quaternion.identity;
                    break;
                }
        }

        missionMgr.CheckMissions();
        yield return null;
    }

    private IEnumerator AskYesNo(string text, System.Action<bool> callback)
    {
        var panel = GameManager.Instance.heroSelectionPanel;
        if (panel == null)
        {
            callback?.Invoke(false);
            yield break;
        }

        panel.SetActive(true);
        var ctrl = panel.GetComponent<HeroSelectionPanelController>();
        bool? result = null;
        ctrl.Init(text, "TAK", "NIE",
            onHero1: () => { result = true; panel.SetActive(false); },
            onHero2: () => { result = false; panel.SetActive(false); });

        yield return new WaitUntil(() => result.HasValue);
        callback?.Invoke(result.Value);
    }
}