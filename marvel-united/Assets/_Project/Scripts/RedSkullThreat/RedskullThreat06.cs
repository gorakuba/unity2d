using System;
using UnityEngine;

public class RedskullThreat06 : MonoBehaviour, IThreatAbility
{
    private ThreatCardInstance _threat;
    private int _attackCountThisTurn = 0;

    /// <summary>
    /// Inicjalizacja – zachowuje referencję do ThreatCardInstance
    /// oraz subskrybuje event resetujący licznik na początku każdej tury bohatera.
    /// </summary>
    public void Init(ThreatCardInstance threatInstance, GameObject unused)
    {
        _threat = threatInstance;
        Debug.Log("[RedskullThreat06] Init wykonane");

        TurnManager.Instance.OnStartHeroTurn += OnHeroTurnStart;
    }

    /// <summary>
    /// Trigger "WhenActive" – nieużywane, bo efekt jest ciągle aktywny,
    /// gdy karta stoi na stole.
    /// </summary>
   public void RegisterTrigger(string trigger, ThreatCardInstance inst)
{
    if (trigger == "WhenActive")
    {
        Init(inst, null);  // używasz null jak w pozostałych
    }
}
    public void OnTurnStart(ThreatCardInstance threat, HeroController hero) { }

    /// <summary>
    /// Każdorazowo, gdy bohater wybiera symbol „Attack” na tej lokacji,
    /// wywołujemy tę metodę, żeby zarejestrować atak.
    /// Zwraca true tylko, gdy to jest drugi atak w tej turze na tej lokacji –
    /// wtedy Thug faktycznie można przenieść do slotu.
    /// </summary>
    public bool RegisterAttackOnLocation(LocationController loc)
    {
        if (_threat == null)
        {
            Debug.LogError("[RedskullThreat06] _threat == null – Init prawdopodobnie nie został jeszcze wywołany.");
            return false;
        }

        if (_threat.assignedLocation == null)
        {
            Debug.LogError("[RedskullThreat06] assignedLocation == null – karta nie została przypisana do lokacji.");
            return false;
        }

        if (loc.gameObject != _threat.assignedLocation)
            return false;

        _attackCountThisTurn++;
        Debug.Log($"[RedskullThreat06] Zarejestrowano atak #{_attackCountThisTurn} w tej turze na lokacji {_threat.assignedLocation.name}");
            if (_attackCountThisTurn >= 2)
        {
            _attackCountThisTurn = 0; // <--- TU RESET PO UDANYM ATAKU
            return true;
        }
        return (_attackCountThisTurn >= 2);
    }


    /// <summary>
    /// Reset licznika ataków na początku każdej tury bohatera.
    /// </summary>
    private void OnHeroTurnStart(HeroController hero)
    {
        _attackCountThisTurn = 0;
        Debug.Log("[RedskullThreat06] Reset licznika ataku na początku tury");
    }

    private void OnDestroy()
    {
        TurnManager.Instance.OnStartHeroTurn -= OnHeroTurnStart;
    }
    
}
