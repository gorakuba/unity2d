using UnityEngine;
using System.Linq;

public class MissionManager : MonoBehaviour
{
    [Header("Rooty z prefabami misji")]
    public Transform thugsMissionRoot;        // np. GameObject ThugsMission
    public Transform threatMissionRoot;       // np. GameObject ThreatMission
    public Transform civiliansMissionRoot;    // np. GameObject CivilliansMission

    [Header("Ile slotów w każdej misji")]
    public int thugsSlotsCount = 9;
    public int threatSlotsCount = 4;
    public int civiliansSlotsCount = 9;

    [HideInInspector] public bool thugsCompleted;
    [HideInInspector] public bool threatCompleted;
    [HideInInspector] public bool civiliansCompleted;

    public int CompletedCount { get; private set; }

    /// <summary>
    /// Wywołuj po każdej akcji gracza, żeby zaktualizować statusy misji.
    /// </summary>
    public void CheckMissions()
    {
        // 1) ThugsMission
        if (!thugsCompleted && CountFilledSlots(thugsMissionRoot) >= thugsSlotsCount)
        {
            thugsCompleted = true;
            CompletedCount++;
            Debug.Log("🎉 ThugsMission completed!");
        }

        // 2) ThreatMission
        if (!threatCompleted && CountFilledSlots(threatMissionRoot) >= threatSlotsCount)
        {
            threatCompleted = true;
            CompletedCount++;
            Debug.Log("🎉 ThreatMission completed!");
        }

        // 3) CivilliansMission
        if (!civiliansCompleted && CountFilledSlots(civiliansMissionRoot) >= civiliansSlotsCount)
        {
            civiliansCompleted = true;
            CompletedCount++;
            Debug.Log("🎉 CivilliansMission completed!");
        }

        // (opcjonalnie) feedback ile zostało zrobionych:
        Debug.Log($"🚩 Missions done: {CompletedCount}/3");
    }

    /// <summary>
    /// Zlicza pod-transformy o nazwie „Slot_*” w parent, które mają childCount>0.
    /// </summary>
    private int CountFilledSlots(Transform parent)
    {
        if (parent == null) return 0;
        return parent.Cast<Transform>()
                     .Where(t => t.name.StartsWith("Slot_") && t.childCount > 0)
                     .Count();
    }
}
