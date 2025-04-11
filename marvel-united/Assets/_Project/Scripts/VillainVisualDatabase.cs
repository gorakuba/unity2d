using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "VillainVisualDatabase", menuName = "MarvelUnited/Villain Visual Database")]
public class VillainVisualDatabase : ScriptableObject
{
    public List<VillainVisual> villainVisuals;

    public Sprite GetVillainSprite(string villainId)
    {
        VillainVisual visual = villainVisuals.Find(v => v.villainID == villainId);
        return visual != null ? visual.sprite : null;
    }
}

[System.Serializable]
public class VillainVisual
{
    public string villainID;
    public Sprite sprite;
}
