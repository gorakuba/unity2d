using UnityEngine;
using System;

public class RedskullThreat03 : MonoBehaviour, IThreatAbility
{
    private ThreatCardInstance _threat;

    public void Init(ThreatCardInstance threatInstance, GameObject unused)
    {
        _threat = threatInstance;
    }

    public void RegisterTrigger(string trigger, ThreatCardInstance inst)
    {
        if (trigger == "OnStand")
            VillainController.Instance.OnVillainStop += OnStand;
    }

    public void OnTurnStart(ThreatCardInstance threat, HeroController hero) { }

    private void OnStand(Transform slot)
    {
        HUDMessageManager.Instance?.Enqueue("Karta Threat sie aktywuje");
        // 1) root lokacji (przechwycony assignedLocation)
        var locRoot = _threat.assignedLocation.transform;

    Debug.Log($"[RedskullThreat03] OnStand → slot={slot.name}, thrLoc={locRoot.name}");

    // 2) Sprawdź, czy villain stoi na tej samej lokacji
    //    slot może być Villain_Slot, więc szukamy lokacji po parentach
    var slotRoot = slot.GetComponentInParent<LocationController>()?.transform;
    if (slotRoot != locRoot)
        return;

    // 3) Iteruj po wszystkich kontenerach tokenów: Slot_0, Slot_1, ..., Slot_5
    int removed = 0;
    foreach (Transform child in locRoot)
    {
        if (!child.name.StartsWith("Slot_")) continue;

        // każdy wpis w "Slot_?" może trzymać wiele tokenów
        for (int i = child.childCount - 1; i >= 0; i--)
        {
            var tok = child.GetChild(i);
            if (tok.GetComponent<TokenDrop>() != null)
            {
                Destroy(tok.gameObject);
                removed++;
            }
        }
    }

    Debug.Log($"[RedskullThreat03] Usunięto {removed} tokenów z {locRoot.name}");

    // 4) Przesuń FearTrack o liczbę usuniętych
    DashboardLoader.Instance.MoveFearTrack(removed);
}

    private void OnDestroy()
    {
        if (VillainController.Instance != null)
            VillainController.Instance.OnVillainStop -= OnStand;
    }
}
