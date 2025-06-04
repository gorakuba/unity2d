using UnityEngine;

public interface IThreatAbility
{
    /// <summary>
    /// Zarejestruj tę zdolność pod danym triggerem
    /// (np. "OnTurnStart", "OnBAM", "OnStand", "WhenActive" itd.).
    /// </summary>
    void RegisterTrigger(string trigger, ThreatCardInstance inst);

    /// <summary>
    /// Wywoływane na początku tury bohatera.
    /// </summary>
    void OnTurnStart(ThreatCardInstance threatInstance, HeroController hero);

    // W przyszłości możesz je dopisać tu:
    // void OnBAM(ThreatCardInstance threatInstance);
    // void OnStand(ThreatCardInstance threatInstance, LocationController loc);
    // void OnWhenActive(ThreatCardInstance threatInstance);
}
