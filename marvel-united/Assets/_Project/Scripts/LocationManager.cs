using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class LocationManager : MonoBehaviour
{
    [Header("Sloty Lokacji (dzieci Location_Slot_X/location)")]
    public Transform[] locationSlots;

    [Header("Prefaby Lokacji (dokładnie 8)")]
    public List<GameObject> locationPrefabs;

    [Header("Prefaby Żetonów")]
    public GameObject civilianTokenPrefab;
    public GameObject thugTokenPrefab;

    [Header("Opóźnienia")]
    public float delayBetweenLocations = 0.5f;
    public float delayBeforeTokens = 3.5f;

    private Dictionary<string, LocationData> locationDataDict = new Dictionary<string, LocationData>();
    private List<GameObject> spawnedLocations = new List<GameObject>();
    public List<Transform> spawnedLocationTransforms = new List<Transform>();

    void Start()
    {
        LoadLocationsFromJson();
        StartCoroutine(SpawnLocationsWithDelay());
    }
    public List<Transform> GetSpawnedLocationRoots()
    {
    return spawnedLocations.Select(loc => loc.transform).ToList();
    }
    void LoadLocationsFromJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Locations.json");
        if (!File.Exists(path))
        {
            Debug.LogError("❌ Nie znaleziono Locations.json w StreamingAssets!");
            return;
        }

        string json = File.ReadAllText(path);
        LocationDataList dataList = JsonUtility.FromJson<LocationDataList>(json);

        foreach (var loc in dataList.locations)
        {
            locationDataDict[loc.script] = loc;
        }

        Debug.Log($"✅ Załadowano {locationDataDict.Count} lokacji z JSON-a.");
    }

    IEnumerator SpawnLocationsWithDelay()
    {
        List<GameObject> shuffled = new List<GameObject>(locationPrefabs);
        Shuffle(shuffled);

        for (int i = 0; i < locationSlots.Length; i++)
        {
            Transform slot = locationSlots[i];

            // Usuń placeholdery
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }

            GameObject selectedPrefab = shuffled[i];
            GameObject newLocation = Instantiate(selectedPrefab, slot);
            newLocation.transform.localPosition = Vector3.zero;
            newLocation.transform.localRotation = Quaternion.identity;
            newLocation.transform.localScale = Vector3.one;

            spawnedLocations.Add(newLocation);
            spawnedLocationTransforms.Add(newLocation.transform);

            yield return new WaitForSeconds(delayBetweenLocations);
        }

        // Poczekaj i potem spawnuj żetony
        yield return new WaitForSeconds(delayBeforeTokens);
        StartCoroutine(SpawnAllTokens());
    }

    IEnumerator SpawnAllTokens()
    {
        foreach (GameObject locationGO in spawnedLocations)
        {
            string scriptId = locationGO.name.Replace("(Clone)", "");

            if (locationDataDict.TryGetValue(scriptId, out LocationData data))
            {
                Debug.Log($"🟡 Dodaję tokeny do {scriptId}");
                yield return StartCoroutine(SpawnTokens(locationGO, data));
            }
            else
            {
                Debug.LogWarning($"⚠️ Nie znaleziono danych JSON dla {scriptId}");
            }
        }
    }

    IEnumerator SpawnTokens(GameObject locationGO, LocationData data)
    {
        for (int i = 0; i < data.starting_tokens.Count; i++)
        {
            string tokenType = data.starting_tokens[i];

            Transform tokenSlot = FindDeepChild(locationGO.transform, $"Slot_{i}");
            if (tokenSlot == null)
            {
                Debug.LogWarning($"⚠️ Brakuje Slot_{i} w {locationGO.name}");
                continue;
            }

            GameObject tokenPrefab = tokenType == "Civilian" ? civilianTokenPrefab : thugTokenPrefab;
            if (tokenPrefab == null)
            {
                Debug.LogError($"❌ Brakuje przypisanego prefabry dla: {tokenType}");
                continue;
            }

            GameObject token = Instantiate(tokenPrefab, tokenSlot);
            
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;

            Debug.Log($"✅ TOKEN: {token.name} dodany do {tokenSlot.name}");
        }
        // Sprawdź i dodaj threatToken, jeśli slot istnieje

        yield return null;
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
