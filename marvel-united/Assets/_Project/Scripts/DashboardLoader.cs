using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DashboardLoader : MonoBehaviour
{
    [System.Serializable]
    public class VillainDashboardEntry
    {
        public string villainName;
        public GameObject dashboardPrefab;
    }

    [Header("Dashboard Settings")]
    public List<VillainDashboardEntry> dashboardPrefabs;
    public Transform dashboardParent;
    public GameObject healthTokenPrefab;
    public GameObject fearTrackCubePrefab;
    public int playerCount = 2;

    public static DashboardLoader Instance;

    private GameObject currentDashboard;
    private GameObject fearTrackCubeInstance;
    private int fearTrackIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

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

        if (prefabToUse == null)
        {
            Debug.LogError($"Brak dashboardPrefab dla villain: {selectedVillain}");
            return;
        }

        currentDashboard = Instantiate(prefabToUse, dashboardParent);

        // --------- Fear Track ----------
        Transform fearSlot = currentDashboard.transform.Find("FearTrack_Slot0");

        if (fearSlot != null)
        {
            fearTrackCubeInstance = Instantiate(fearTrackCubePrefab, fearSlot);
            fearTrackCubeInstance.transform.localPosition = Vector3.zero;
            fearTrackCubeInstance.transform.localRotation = Quaternion.identity;
            fearTrackCubeInstance.transform.localScale = fearTrackCubePrefab.transform.localScale;

            fearTrackIndex = 0;

            Debug.Log("✅ Zainstancjonowano FearTrackCube.");
        }
        else
        {
            Debug.Log("❗ Brak FearTrack_Slot0 – ten villain może nie używać Fear Tracka.");
        }

        // --------- Health Tokens ----------
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

        Transform slotHealth = currentDashboard.transform.Find("Slot_Health");

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
            token.transform.localPosition = new Vector3(0, 0, i * spacing);
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Przesuwa Fear Track o podaną liczbę punktów.
    /// </summary>
    public void MoveFearTrack(int amount)
    {
        if (fearTrackCubeInstance == null)
        {
            Debug.LogWarning("Brak instancji FearTrackCube!");
            return;
        }

        if (HUDMessageManager.Instance != null)
            HUDMessageManager.Instance.Enqueue($"Fear Track przesuwa sie o {amount}");
        fearTrackIndex += amount;

        if (fearTrackIndex > 20)
            fearTrackIndex = 20;

        string slotName = $"FearTrack_Slot{fearTrackIndex}";
        Transform targetSlot = currentDashboard.transform.Find(slotName);

        if (targetSlot != null)
        {
            fearTrackCubeInstance.transform.SetParent(targetSlot, false);
            fearTrackCubeInstance.transform.localPosition = Vector3.zero;
            fearTrackCubeInstance.transform.localRotation = Quaternion.identity;

            Debug.Log($"➡️ Przesunięto FearTrackCube na slot {fearTrackIndex}");
        }
        else
        {
            Debug.LogWarning($"Brak slota {slotName} dla FearTrack");
        }
    }
    /// <summary>
/// Niszczy pierwszy (najstarszy) token życia w slocie "Slot_Health".
/// </summary>
public void RemoveFirstHealthToken()
{
    // Szukamy kontenera ze wszystkimi tokenami życia
    Transform slotHealth = currentDashboard.transform.Find("Slot_Health");
    if (slotHealth != null && slotHealth.childCount > 0)
    {
            // Destroy pierwszego dziecka (indeks 0)
        int lastIndex = slotHealth.childCount - 1;
        Destroy(slotHealth.GetChild(lastIndex).gameObject);
    }
}
}
