using UnityEngine;
using System;
public class RedskullThreat01 : MonoBehaviour, IThreatAbility
{
    private Action<HeroController> _startTurnHandler;
    public void RegisterTrigger(string trigger, ThreatCardInstance inst)
    {
        if (trigger == "OnTurnStart")
        {
            Debug.Log($"[RedskullThreat01] RegisterTrigger: subskrybuję OnTurnStart dla karty na {inst.assignedLocation?.name}");
            _startTurnHandler = hero => OnTurnStart(inst, hero);
            TurnManager.Instance.OnStartHeroTurn += _startTurnHandler;
        }
        // w przyszłości dorzuć inne triggery:
        // else if (trigger == "OnBAM")
        //     BAMController.Instance.OnBAM += () => OnBAM(inst);
    }

    public void OnTurnStart(ThreatCardInstance threat, HeroController hero)
    {
        if (hero.IsStunned) return;
        string heroLoc = hero.CurrentLocation != null ? hero.CurrentLocation.gameObject.name : "null";
        string threatLoc = threat.assignedLocation != null ? threat.assignedLocation.name : "null";
        Debug.Log($"[RedskullThreat01] OnTurnStart dla {hero.HeroId}: CurrentLocation={heroLoc}, ThreatLocation={threatLoc}");

        if (hero.CurrentLocation != null
        && hero.CurrentLocation.gameObject == threat.assignedLocation)
        {
            Debug.Log($"[RedskullThreat01] WARUNEK SPEŁNIONY — daję CrisisToken");
            CrisisTokenManager.Instance.GiveCrisisToken(hero);
        }
        else
        {
            Debug.Log($"[RedskullThreat01] WARUNEK NIEZWYPLNIONY");
        }
    }
    private void OnDestroy()
    {
        if (TurnManager.Instance != null && _startTurnHandler != null)
            TurnManager.Instance.OnStartHeroTurn -= _startTurnHandler;
    }


}
