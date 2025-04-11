using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class VillainBackTextureEntry
{
    public string villainId;
    public Texture2D backTexture;
}

[CreateAssetMenu(fileName = "ThreatCardTextureDatabase", menuName = "MarvelUnited/ThreatCardTextureDatabase")]
public class ThreatCardTextureDatabase : ScriptableObject
{
    [System.Serializable]
    public class ThreatTextureEntry
    {
        public string villainId;
        public string threatId;
        public Texture2D texture;
    }

    public List<ThreatTextureEntry> threatTextures;
    public List<VillainBackTextureEntry> villainBackTextures;

    public Texture2D GetTexture(string villainId, string threatId)
    {
        foreach (var entry in threatTextures)
        {
            if (entry.villainId == villainId && entry.threatId == threatId)
                return entry.texture;
        }
        return null;
    }

public Texture2D GetBackTexture(string villainId)
{
    var entry = villainBackTextures.FirstOrDefault(v => v.villainId == villainId);
    return entry != null ? entry.backTexture : null;
}

}
