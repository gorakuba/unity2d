using System.Collections.Generic;
using UnityEngine;

static class ThreatAbilityFactory
{
    static readonly Dictionary<string, System.Type> _map = new()
    {
        ["Redskull_threat_01"] = typeof(RedskullThreat01Ability),
        // kolejne mapowania trigger→klasa:
        // ["Redskull_threat_02"] = typeof(RedskullThreat02Ability),
    };

    public static IThreatAbility Attach(string key, ThreatCardInstance inst)
    {
        if (_map.TryGetValue(key, out var type))
            return inst.gameObject.AddComponent(type) as IThreatAbility;
        return null;
    }
}
