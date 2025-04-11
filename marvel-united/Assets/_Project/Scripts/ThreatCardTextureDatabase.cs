using System.Collections.Generic;
using UnityEngine;

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

    public Texture2D GetTexture(string villainId, string threatId)
    {
        foreach (var entry in threatTextures)
        {
            if (entry.villainId == villainId && entry.threatId == threatId)
                return entry.texture;
        }
        return null;
    }
}
