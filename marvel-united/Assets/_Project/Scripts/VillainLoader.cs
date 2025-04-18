using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class VillainLoader
{
    [System.Serializable]
    public class VillainRoot
    {
        public List<VillainData> villains;
    }

    public VillainData LoadVillainData(string villainId)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Villains.json");

        if (!File.Exists(path))
        {
            Debug.LogError("Nie znaleziono pliku Villains.json");
            return null;
        }

        string json = File.ReadAllText(path);
        VillainRoot root = JsonUtility.FromJson<VillainRoot>(json);

        foreach (var villain in root.villains)
        {
            if (villain.id == villainId)
            {
                return villain;
            }
        }

        Debug.LogWarning($"Nie znaleziono złoczyńcy o ID: {villainId}");
        return null;
    }
}
