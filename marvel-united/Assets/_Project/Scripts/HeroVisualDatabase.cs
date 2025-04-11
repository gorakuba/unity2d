using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HeroVisualDatabase", menuName = "MarvelUnited/Hero Visual Database")]
public class HeroVisualDatabase : ScriptableObject
{
    public List<HeroVisual> heroVisuals;

    public Sprite GetHeroSprite(string heroId)
    {
        HeroVisual visual = heroVisuals.Find(h => h.heroID == heroId);
        return visual != null ? visual.sprite : null;
    }
}

[System.Serializable]
public class HeroVisual
{
    public string heroID;
    public Sprite sprite;
}
