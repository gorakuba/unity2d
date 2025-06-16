using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BAMController : MonoBehaviour
{
    public static bool BamInProgress = false;
    private static int playersPendingDamage = 0;
    private static Queue<(int count, Func<IEnumerator> routine)> pendingBams = new();
    private static Queue<IEnumerator> pendingBamEffects = new();
    private static bool bamRoutineActive = false;

    private static IEnumerator RunBamRoutine(IEnumerator routine)
    {
        bamRoutineActive = true;
        if (routine != null)
        {
            if (SetupManager.villainController == null)
            {
                Debug.LogError("BAMController: villain controller missing; cannot start routine.");
            }
            else
            {
                yield return SetupManager.villainController.StartCoroutine(routine);
            }
        }
        bamRoutineActive = false;
        TryStartNextBamEffect();
    }

    private static void TryStartNextBamEffect()
    {
        if (!BamInProgress && !bamRoutineActive && pendingBamEffects.Count > 0)
        {
            var nextRoutine = pendingBamEffects.Dequeue();
            if (SetupManager.villainController == null)
            {
                Debug.LogError("BAMController: villain controller missing; cannot start next BAM effect.");
            }
            else
            {
                SetupManager.villainController.StartCoroutine(RunBamRoutine(nextRoutine));
            }
        }
    }

    public static void QueueBamRoutine(IEnumerator routine)
    {
        if (BamInProgress || bamRoutineActive)
        {
            pendingBamEffects.Enqueue(routine);
        }
        else
        {
            if (SetupManager.villainController == null)
            {
                Debug.LogError("BAMController: villain controller missing; cannot queue routine.");
            }
            else
            {
                SetupManager.villainController.StartCoroutine(RunBamRoutine(routine));
            }
        }
    }
        public static IEnumerator QueueBamRoutineAndWait(IEnumerator routine)
    {
        QueueBamRoutine(routine);
        while (BamInProgress || bamRoutineActive)
            yield return null;
    }


        public static bool StartBAM(int playersToDamage, Func<IEnumerator> damageRoutine = null)
    {
        if (SetupManager.villainController == null)
        {
            Debug.LogError("BAMController: villain controller missing; cannot start BAM.");
            return false;
        }
        if (!BamInProgress)
        {
            playersPendingDamage = playersToDamage;
            BamInProgress = playersPendingDamage > 0;
            Debug.Log($"[BAM] Rozpoczęto BAM → gracze do obrażenia: {playersPendingDamage}");
            // let the caller start the damage routine when this method
            // returns true
            return true;
        }
        else
        {
            pendingBams.Enqueue((playersToDamage, damageRoutine));
            Debug.Log($"[BAM] Dodano kolejny BAM do kolejki → gracze do obrażenia: {playersToDamage}");
            return false;
        }
    }

    public static void PlayerFinishedDamage()
    {
        playersPendingDamage--;
        Debug.Log($"[BAM] Gracz skończył obrażenia. Pozostało: {playersPendingDamage}");
        if (playersPendingDamage <= 0)
        {
            while (playersPendingDamage <= 0 && pendingBams.Count > 0)
            {
                var next = pendingBams.Dequeue();
                playersPendingDamage = next.count;
                BamInProgress = playersPendingDamage > 0;
                Debug.Log($"[BAM] Rozpoczyna się kolejny BAM → gracze do obrażenia: {playersPendingDamage}");
                if (next.routine != null)
                {
                    if (SetupManager.villainController == null)
                    {
                        Debug.LogError("BAMController: villain controller missing; cannot run pending BAM routine.");
                    }
                    else
                    {
                        SetupManager.villainController.StartCoroutine(next.routine());
                    }
                }
            }
            if (playersPendingDamage <= 0 && pendingBams.Count == 0)
            {
                BamInProgress = false;
                Debug.Log("[BAM] Wszyscy gracze skończyli obrażenia → BAM KONIEC.");
                TryStartNextBamEffect();
            }
        }
    }
}