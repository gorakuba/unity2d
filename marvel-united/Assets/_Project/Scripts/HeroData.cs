using System.Collections.Generic;
using UnityEngine;

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
