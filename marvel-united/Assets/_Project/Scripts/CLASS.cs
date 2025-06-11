// CLASS.cs
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct SymbolCount
{
    public string symbol;
    public int    count;
}

[Serializable]
public class ThreatCard
{
    public string id;
    public string name;
    public string effect;
    public string remove_condition;
    public bool minion;
    public string minion_health;
    public bool to_remove;

    // z JSON: listy symboli
    public List<SymbolCount> required_symbol_list;
    public List<SymbolCount> used_symbol_list;
    public List<AbilityData> abilities;

    // runtime: słowniki tworzone w BuildDictionaries()
    [NonSerialized] public Dictionary<string, int> required_symbols;
    [NonSerialized] public Dictionary<string, int> used_symbols;

    public Sprite sprite;

    /// <summary>
    /// Wywołaj zaraz po JsonUtility.FromJson, żeby wypełnić required_symbols i used_symbols
    /// </summary>
    public void BuildDictionaries()
    {
        required_symbols = new Dictionary<string, int>();
        if (required_symbol_list != null)
        {
            foreach (var sc in required_symbol_list)
                required_symbols[sc.symbol] = sc.count;
        }

        used_symbols = new Dictionary<string, int>();
        if (used_symbol_list != null)
        {
            foreach (var sc in used_symbol_list)
                used_symbols[sc.symbol] = sc.count;
        }
    }
}
[Serializable]
public class AbilityData
{
    public string trigger; // "OnTurnStart", "OnBAM", "OnStand", "WhenActive" itp.
    public string id;      // "Redskull_threat_01"
}

[Serializable]
public class VillainCard
{
    public string id;
    public int    move;
    public bool   BAM_effect;
    public bool   special;
    public string special_ability;
    public string special_name;
    public string special_description;
    public List<LocationSpawnSymbol> Location_left;
    public List<LocationSpawnSymbol> Location_middle;
    public List<LocationSpawnSymbol> Location_right;
    public bool HasSpawn =>
        (Location_left   != null && Location_left.Count   > 0) ||
        (Location_middle != null && Location_middle.Count > 0) ||
        (Location_right  != null && Location_right.Count  > 0);
}

[Serializable]
public class Hero
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ImagePath { get; private set; }
    public List<HeroCard> Cards { get; private set; }

    public Hero(string id, string name, string description, string imagePath, List<HeroCard> cards)
    {
        Id = id;
        Name = name;
        Description = description;
        ImagePath = imagePath;
        Cards = cards;
    }
}

[Serializable]
public class HeroCard
{
    public string Id { get; private set; }
    public string heroId;
    public bool Special { get; private set; }
    public string SpecialAbility { get; private set; }
    public string SpecialDescription { get; private set; }
    public string SpecialName { get; private set; }
    public List<string> Symbols { get; private set; }

    public HeroCard(string id, bool special, string specialAbility, string specialDescription, string specialName, List<string> symbols)
    {
        Id = id;
        Special = special;
        SpecialAbility = specialAbility;
        SpecialDescription = specialDescription;
        SpecialName = specialName;
        Symbols = symbols;
    }
}

[Serializable]
public class LocationData
{
    public string id;
    public string name;
    public string script;
    public string end_turn;
    public int slots;
    public List<string> starting_tokens;
    [NonSerialized]
    public Sprite sprite;
}

[Serializable]
public class LocationDataList
{
    public List<LocationData> locations;
}

[Serializable]
public class VillainDashboard
{
    public string villainName;
    public GameObject dashboardPrefab;
}

[Serializable]
public class LocationSpawnSymbol
{
    public string symbol;
    public int count;
}
public class SymbolButtonData : MonoBehaviour
{
    public string SymbolId;
    public bool   IsPersistent;
}
