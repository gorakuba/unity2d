using UnityEngine;
using System.Linq;
using System.Collections;
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; } 
    [Header("Rooty z prefabami misji")]
    public Transform thugsMissionRoot;        // GameObject “ThugsMission”
    public Transform threatMissionRoot;       // GameObject “ThreatMission”
    public Transform civiliansMissionRoot;    // GameObject “CivilliansMission”

    [Header("Ile slotów w każdej misji")]
    public int thugsSlotsCount      = 9;
    public int threatSlotsCount     = 4;
    public int civiliansSlotsCount  = 9;

    [HideInInspector] public bool thugsCompleted;
    [HideInInspector] public bool threatCompleted;
    [HideInInspector] public bool civiliansCompleted;

    public int CompletedMissionsCount { get; private set; }

    private int completed = 0;
    private bool reminderStarted = false;
    private Coroutine reminderRoutine;
    /// <summary>
    /// Wywołuj po każdej akcji gracza, żeby zaktualizować statusy misji.
    /// </summary>
    public void CheckMissions()
    {


        if (!thugsCompleted && CountFilledSlots(thugsMissionRoot) >= thugsSlotsCount)
        {
            thugsCompleted = true;
            completed++;
            Debug.Log("🎉 ThugsMission completed!");
            HUDMessageManager.Instance?.Enqueue("Misja Thugs zakonczona!");
        }
        if (!threatCompleted && CountFilledSlots(threatMissionRoot) >= threatSlotsCount)
        {
            threatCompleted = true;
            completed++;
            Debug.Log("🎉 ThreatMission completed!");
            HUDMessageManager.Instance?.Enqueue("Misja Threats zakonczona!");
        }
        if (!civiliansCompleted && CountFilledSlots(civiliansMissionRoot) >= civiliansSlotsCount)
        {
            civiliansCompleted = true;
            completed++;
            Debug.Log("🎉 CivilliansMission completed!");
            HUDMessageManager.Instance?.Enqueue("Misja Civilians zakonczona!");
        }

        CompletedMissionsCount = completed;
        Debug.Log($"🚩 Missions done: {CompletedMissionsCount}/3");
        
        if (CompletedMissionsCount >= 2 && !reminderStarted)
        {
            reminderStarted = true;
            reminderRoutine = StartCoroutine(AttackReminder());
        }
    }

    private IEnumerator AttackReminder()
    {
        while (true)
        {
            HUDMessageManager.Instance?.Enqueue("Można atakować przeciwnika!");
            yield return new WaitForSeconds(10f);
        }
    }

    private int CountFilledSlots(Transform parent)
    {
        if (parent == null) return 0;
        return parent.Cast<Transform>()
                     .Where(t => t.name.StartsWith("Slot_") && t.childCount > 0)
                     .Count();
    }
}
