using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

[System.Serializable]
public class CharacterSlots
{
    public Transform heroSlot1;
    public Transform heroSlot2;
    public Transform villainSlot;
}

public class LocationManager : MonoBehaviour
{
    [Header("Sloty Lokacji (dzieci Location_Slot_X/location)")]
    public Transform[] locationSlots;

    [Header("Prefaby Lokacji (dokładnie 8)")]
    public List<GameObject> locationPrefabs;

    [Header("Prefaby Żetonów")]
    public GameObject civilianTokenPrefab;
    public GameObject thugTokenPrefab;
    public GameObject threatTokenPrefab;
    public CharacterSlots characterSlots = new CharacterSlots();

    [Header("Opóźnienia")]
    public float delayBetweenLocations = 0.2f;
    public float delayBeforeTokens = 0.2f;

    private Dictionary<string, LocationData> locationDataDict = new Dictionary<string, LocationData>();
    private List<GameObject> spawnedLocations = new List<GameObject>();
    public List<Transform> spawnedLocationTransforms = new List<Transform>();   
    public System.Action OnLocationsReady;
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
            return;
        }

        string json = File.ReadAllText(path);
        LocationDataList dataList = JsonUtility.FromJson<LocationDataList>(json);

        foreach (var loc in dataList.locations)
        {
            locationDataDict[loc.script] = loc;
        }
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

            // Jeśli to lokacja 1 (index 0) → ZBIR
            if (i == 0)
            {
                characterSlots.villainSlot = FindDeepChild(newLocation.transform, "Villain_Slot");
            }
            // Jeśli to lokacja 4 (index 3) → BOHATEROWIE
            if (i == 3)
            {
                characterSlots.heroSlot1 = FindDeepChild(newLocation.transform, "Hero_Slot_1");
                characterSlots.heroSlot2 = FindDeepChild(newLocation.transform, "Hero_Slot_2");
            }

            yield return new WaitForSeconds(delayBetweenLocations);
        }

        // Poczekaj i potem spawnuj żetony
        yield return new WaitForSeconds(delayBeforeTokens);
        OnLocationsReady?.Invoke();
        StartCoroutine(SpawnAllTokens());
    }

    IEnumerator SpawnAllTokens()
    {
        foreach (GameObject locationGO in spawnedLocations)
        {
            string scriptId = locationGO.name.Replace("(Clone)", "");

            if (locationDataDict.TryGetValue(scriptId, out LocationData data))
            {
                yield return StartCoroutine(SpawnTokens(locationGO, data));
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
                continue;
            }

            GameObject tokenPrefab = tokenType == "Civilian" ? civilianTokenPrefab : thugTokenPrefab;
            if (tokenPrefab == null)
            {
                continue;
            }

            GameObject token = Instantiate(tokenPrefab, tokenSlot);
            
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;
        }
        // Sprawdź i dodaj threatToken, jeśli slot istnieje
    
    
    
    Transform threatSlot = FindDeepChild(locationGO.transform, "ThreatSlot");
    if (threatSlot != null && threatTokenPrefab != null)
    {
        GameObject threatToken = Instantiate(threatTokenPrefab, threatSlot);
        threatToken.transform.localPosition = Vector3.zero;
        threatToken.transform.localRotation = Quaternion.identity;
    }
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
    public CharacterSlots GetCharacterSlots()
{
    return characterSlots;
}
}
