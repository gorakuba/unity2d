using System.Collections;
using UnityEngine;

public class Location_5 : MonoBehaviour, ILocationEndTurnAbility
{
    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        var loc = GetComponent<LocationController>();
        if (loc == null || !loc.HasCivillian())
            yield break;

        var handler = TurnManager.Instance?.symbolPanelUI?.actionHandler;
        var missionMgr = handler?.missionManager;
        if (handler == null || missionMgr == null)
            yield break;

        var civ = loc.RemoveFirstCivillian();
        if (civ == null)
            yield break;

        HUDMessageManager.Instance?.Enqueue($"Uratowano cywila w {loc.name}");
        if (missionMgr.civiliansCompleted)
        {
            Object.Destroy(civ);
        }
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

        missionMgr.CheckMissions();
        yield return null;
    }
}