using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VillainsRoot
{
    public List<VillainData> villains;
}
    public class BAMController : MonoBehaviour
    {
        public static bool BamInProgress = false;
        private static int playersPendingDamage = 0;

        public static void StartBAM(int playersToDamage)
        {
            playersPendingDamage = playersToDamage;
            BamInProgress = playersPendingDamage > 0;

            Debug.Log($"[BAM] Rozpoczęto BAM → gracze do obrażenia: {playersPendingDamage}");
        }

        public static void PlayerFinishedDamage()
        {
            playersPendingDamage--;
            Debug.Log($"[BAM] Gracz skończył obrażenia. Pozostało: {playersPendingDamage}");

            if (playersPendingDamage <= 0)
            {
                BamInProgress = false;
                Debug.Log("[BAM] Wszyscy gracze skończyli obrażenia → BAM KONIEC.");
            }
        }
    }



[Serializable]
public class VillainData
{
    public string id;
    public string name;
    public string bam;
    [System.Serializable]
    public class HealthPerPlayers
    {
        public int _2;
        public int _3;
        public int _4;
    }

public HealthPerPlayers health_per_players;
    public string bam_effect;
    public string villainous_plot;
    public bool additional_win_condition;
    public string additional_win_condition_script;
    public string overflow;
    public List<ThreatCard> threats;
    public List<VillainCard> cards;
    public string imagePath;
    public string backTexturePath;
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
    public bool bam_effect;
    public string bam_ability;
    public bool on_stand_effect;
    public string on_stand_ability;
    public bool to_remove;
    public Dictionary<string, int> required_symbols;
    public Dictionary<string, int> used_symbols;
    public bool special;
    public string special_ability;
    public Sprite sprite;
}

[Serializable]
public class VillainCard
{
    public string id;
    public int move;
    public bool BAM_effect;
    public bool special;
    public string special_name;
    public string special_description;
    public List<LocationSpawnSymbol> Location_left;
    public List<LocationSpawnSymbol> Location_middle;
    public List<LocationSpawnSymbol> Location_right;
    public bool HasSpawn =>
        (Location_left != null && Location_left.Count > 0) ||
        (Location_middle != null && Location_middle.Count > 0) ||
        (Location_right != null && Location_right.Count > 0);
}
[System.Serializable]
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

[System.Serializable]
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
[System.Serializable]
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

[System.Serializable]
public class LocationDataList
{
    public List<LocationData> locations;
}

[System.Serializable]
public class VillainDashboard
{
    public string villainName;          // np. "red_skull"
    public GameObject dashboardPrefab;  // przypisany prefab w Inspectorze
}

[Serializable]
public class LocationSpawnSymbol
{
    public string symbol;
    public int count;
}