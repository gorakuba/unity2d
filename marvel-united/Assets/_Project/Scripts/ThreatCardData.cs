using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MarvelUnited/Threat Card Data", fileName = "NewThreatCardData")]
public class ThreatCardData : ScriptableObject
{
    [Header("Podstawowe")]
    public string id;
    public string cardName;
    [TextArea] public string effect;
    [TextArea] public string removeCondition;
    public bool toRemove;

    [Header("Wymagane symbole")]
    public List<SymbolCount> requiredSymbols;
}
