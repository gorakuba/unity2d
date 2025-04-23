using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DashboardLoader : MonoBehaviour
{
    [System.Serializable]
    public class VillainDashboardEntry
    {
        public string villainName; // np. "red_skull"
        public GameObject dashboardPrefab;
    }

    public List<VillainDashboardEntry> dashboardPrefabs; // przypisujesz w Inspectorze
    public Transform dashboardParent;                    // gdzie wstawić dashboard
    public GameObject healthTokenPrefab;
    public int playerCount = 2;
    public GameObject fearTrackCubePrefab;

    private void Start()
    {
        LoadDashboard();
    }

    private void LoadDashboard()
    {
        string selectedVillain = GameManager.Instance.selectedVillain.ToLower();
        GameObject prefabToUse = null;

        foreach (var entry in dashboardPrefabs)
        {
            if (entry.villainName.ToLower() == selectedVillain)
            {
                prefabToUse = entry.dashboardPrefab;
                break;
            }
        }


        GameObject dashboard = Instantiate(prefabToUse, dashboardParent);

        Transform fearSlot = dashboard.transform.Find("FearTrack_Slot0");

if (fearSlot != null)
{
    GameObject fearCube = Instantiate(fearTrackCubePrefab, fearSlot);
    fearCube.transform.localPosition = Vector3.zero;
    fearCube.transform.localRotation = Quaternion.identity;
    fearCube.transform.localScale = fearTrackCubePrefab.transform.localScale;

    Debug.Log("Zainstancjonowano FearTrackCube.");
}
else
{
    Debug.Log("Brak FearTrack_Slot0 – przeciwnik nie używa Fear Tracka.");
}
        // Wczytanie JSON i ustawienie tokenów
        string path = Path.Combine(Application.streamingAssetsPath, "Villains.json");
        string json = File.ReadAllText(path);
        VillainsRoot root = JsonUtility.FromJson<VillainsRoot>(json);
        VillainData villain = root.villains.Find(v => v.id == selectedVillain);

        if (villain == null)
        {
            Debug.LogError($"Brak danych dla villain: {selectedVillain}");
            return;
        }

        int health = playerCount switch
{
    2 => villain.health_per_players._2,
    3 => villain.health_per_players._3,
    4 => villain.health_per_players._4,
    _ => 0
};
        Transform slotHealth = dashboard.transform.Find("Slot_Health");

        if (slotHealth == null)
        {
            Debug.LogError("Brak obiektu Slot_Health!");
            return;
        }

        float spacing = 0.00424f;
Vector3 originalScale = healthTokenPrefab.transform.localScale;

for (int i = 0; i < health; i++)
{
    GameObject token = Instantiate(healthTokenPrefab, slotHealth);
    token.transform.localPosition = new Vector3(0, 0, i*spacing);
    token.transform.localRotation = Quaternion.identity;
    token.transform.localScale = originalScale; // 👈 używa dokładnej skali z prefaba
}


    }
}
