using System;                       
using System.Collections.Generic;
using UnityEngine;

static class ThreatAbilityFactory
{

    static readonly Dictionary<string, Type> _map = new()
    {
        ["Redskull_threat_01"] = typeof(RedskullThreat01),
        ["Redskull_threat_02"] = typeof(RedskullThreat02),
        ["Redskull_threat_03"] = typeof(RedskullThreat03),
        ["Redskull_threat_04"] = typeof(RedskullThreat04),
        ["Redskull_threat_05"] = typeof(RedskullThreat05),
    };

    public static IThreatAbility Attach(string key, ThreatCardInstance inst)
    {
        if (_map.TryGetValue(key, out var type))
        {
            var comp = inst.gameObject.AddComponent(type) as IThreatAbility;
            if (comp is RedskullThreat02 rt2)
                rt2.Init(inst, GameManager.Instance.threatChoicePanel);
            else if (comp is RedskullThreat04 rt4)
                rt4.Init(inst, GameManager.Instance.threatChoicePanel);
            else if (comp is RedskullThreat03 rt3)
                rt3.Init(inst, null);
            else if (comp is RedskullThreat05 rt5)
                rt5.Init(inst);
            return comp;
        }
        return null;
    }
}
