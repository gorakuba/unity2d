using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class HeroCardLoader
{
    [System.Serializable]
    public class HeroRoot
    {
        public List<HeroJson> heroes;
    }

    [System.Serializable]
    public class HeroJson
    {
        public string id;
        public string name;
        public string description;
        public string imagepath;
        public List<HeroCardJson> cards;
    }

    [System.Serializable]
    public class HeroCardJson
    {
        public string id;
        public bool special;
        public string special_ability;
        public string special_description;
        public string special_name;
        public List<string> symbols;
    }

    public List<HeroCard> LoadHeroDeck(string heroId)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Heroes.json");
        string json = File.ReadAllText(path);

        HeroRoot root = JsonUtility.FromJson<HeroRoot>(json);

        foreach (var hero in root.heroes)
        {
            if (hero.id == heroId)
            {
                List<HeroCard> cards = new List<HeroCard>();
                foreach (var c in hero.cards)
                {
                    HeroCard card = new HeroCard(c.id, c.special, c.special_ability, c.special_description, c.special_name, c.symbols);
card.heroId = hero.id;
cards.Add(card);
                }
                return cards;
            }
        }

        Debug.LogError($"Nie znaleziono bohatera {heroId}");
        return null;
    }
public Hero LoadHeroById(string heroId)
{
    string path = Path.Combine(Application.streamingAssetsPath, "Heroes.json");
    string json = File.ReadAllText(path);

    HeroRoot root = JsonUtility.FromJson<HeroRoot>(json);

    foreach (var hero in root.heroes)
    {
        if (hero.id == heroId)
        {
            List<HeroCard> cards = new List<HeroCard>();
            foreach (var c in hero.cards)
            {
                HeroCard card = new HeroCard(c.id, c.special, c.special_ability, c.special_description, c.special_name, c.symbols);
card.heroId = hero.id;
cards.Add(card);
            }

            return new Hero(hero.id, hero.name, hero.description, hero.imagepath, cards);
        }
    }

    Debug.LogError($"Nie znaleziono bohatera {heroId}");
    return null;
}


}
