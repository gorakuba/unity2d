using System;                       
using System.Collections.Generic;
using UnityEngine;

static class ThreatAbilityFactory
{

    static readonly Dictionary<string, Type> _map = new()
    {
        ["Redskull_threat_01"] = typeof(RedskullThreat01),
        ["Redskull_threat_02"] = typeof(RedskullThreat02),
    };

    public static IThreatAbility Attach(string key, ThreatCardInstance inst)
    {
        if (_map.TryGetValue(key, out var type))
        {
            var comp = inst.gameObject.AddComponent(type) as IThreatAbility;
            if (comp is RedskullThreat02 rt2)
                rt2.Init(inst, GameManager.Instance.threatChoicePanel);
            return comp;
        }
        return null;
    }
}
