using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RedskullThreat05 : MonoBehaviour, IThreatAbility
{
    private ThreatCardInstance _threat;
    private LocationManager _locMan;

    public void Init(ThreatCardInstance threatInstance)
    {
        _threat = threatInstance;
        _locMan  = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
    }

    public void RegisterTrigger(string trigger, ThreatCardInstance inst)
    {
        if (trigger == "OnStand")
            VillainController.Instance.OnVillainStop += OnStand;
    }

    public void OnTurnStart(ThreatCardInstance threat, HeroController hero) { }

    private void OnStand(Transform slot)
    {
        // tylko jeśli zatrzymał się na karcie
        HUDMessageManager.Instance?.Enqueue("Karta Threat sie aktywuje");
        var slotRoot = slot.GetComponentInParent<LocationController>()?.gameObject;
        if (slotRoot != _threat.assignedLocation)
            return;

        // pobieramy listę rootów i znajdujemy indeks tej lokacji
        var roots = _locMan.LocationRoots.ToList();
        int idx = roots.FindIndex(r => r.gameObject == slotRoot);
        if (idx < 0) return;

        int count = roots.Count;
        // zbieramy trzy interesujące lokacje
        var relevantRoots = new[]
        {
            roots[(idx - 1 + count) % count],  // lewy
            roots[idx],                        // środkowy
            roots[(idx + 1) % count]          // prawy
        }.Select(t => t.gameObject).ToHashSet();

        // znajdujemy wszystkich bohaterów na tych lokacjach
        var heroesHere = UnityEngine.Object
            .FindObjectsByType<HeroController>(FindObjectsSortMode.None)
            .Where(h => h.CurrentLocation != null
                     && relevantRoots.Contains(h.CurrentLocation.gameObject))
            .ToList();
        var activeHeroes = heroesHere.Where(h => !h.IsStunned).ToList();
        // każdemu dajemy po jednym Threat Tokenie
        foreach (var hero in activeHeroes)
            CrisisTokenManager.Instance.GiveCrisisToken(hero);

        Debug.Log($"[RedskullThreat05] Dodano {activeHeroes.Count} Threat Tokenów dla {string.Join(", ", activeHeroes.Select(h => h.HeroId))}");

    }

    private void OnDestroy()
    {
        if (VillainController.Instance != null)
            VillainController.Instance.OnVillainStop -= OnStand;
    }
}
