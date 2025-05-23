using UnityEngine;

public class RedskullThreat01Ability : MonoBehaviour, IThreatAbility
{
    public void RegisterTrigger(string trigger, ThreatCardInstance inst)
    {
        if (trigger == "OnTurnStart")
        {
             Debug.Log($"[RedskullThreat01] RegisterTrigger: subskrybuję OnTurnStart dla karty na {inst.assignedLocation?.name}");
            TurnManager.Instance.OnStartHeroTurn += hero => OnTurnStart(inst, hero);
        }
        // w przyszłości dorzuć inne triggery:
        // else if (trigger == "OnBAM")
        //     BAMController.Instance.OnBAM += () => OnBAM(inst);
    }

    public void OnTurnStart(ThreatCardInstance threat, HeroController hero)
    {
        string heroLoc = hero.CurrentLocation  != null ? hero.CurrentLocation.gameObject.name : "null";
        string threatLoc = threat.assignedLocation != null ? threat.assignedLocation.name   : "null";
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

}
