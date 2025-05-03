using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

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

    [Header("Sloty postaci")]
    public CharacterSlots characterSlots = new CharacterSlots();

    [Header("Opóźnienia (sekundy)")]
    [Tooltip("Czas między kolejnymi spawnami lokacji")]
    public float delayBetweenLocations = 0.05f;

    // dane z JSON
    private Dictionary<string, LocationData> locationDataDict = new Dictionary<string, LocationData>();

    // runtime lists
    private List<GameObject> spawnedLocations = new List<GameObject>();
    private List<Transform> spawnedLocationTransforms = new List<Transform>();

    /// <summary>
    /// Wywoływane, kiedy wszystkie 8 lokacji zostało już wstawionych,
    /// ale przed spawnem żetonów
    /// </summary>
    public event System.Action OnLocationsReady;

    /// <summary>
    /// Wywoływane, kiedy lokacje + żetony są już gotowe
    /// </summary>
    public event System.Action OnLocationsAndTokensReady;

    public IReadOnlyList<Transform> LocationRoots => spawnedLocationTransforms;

    public IReadOnlyList<Transform> VillainSlots =>
        spawnedLocationTransforms
            .Select(root => FindDeepChild(root, "Villain_Slot"))
            .Where(t => t != null)
            .ToArray();

    void Start()
    {
        LoadLocationsFromJson();
        StartCoroutine(SpawnLocationsWithDelay());
    }

    public void ResetSpawnedLocations()
    {
        foreach (var go in spawnedLocations)
            if (go != null) Destroy(go);

        spawnedLocations.Clear();
        spawnedLocationTransforms.Clear();

        characterSlots = new CharacterSlots();
    }

    private void LoadLocationsFromJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Locations.json");
        if (!File.Exists(path)) return;
        string json = File.ReadAllText(path);
        var dataList = JsonUtility.FromJson<LocationDataList>(json);
        foreach (var loc in dataList.locations)
            locationDataDict[loc.script] = loc;
    }

    public IEnumerator SpawnLocationsWithDelay()
    {
        spawnedLocations.Clear();
        spawnedLocationTransforms.Clear();

        var shuffled = new List<GameObject>(locationPrefabs);
        Shuffle(shuffled);

        for (int i = 0; i < locationSlots.Length; i++)
        {
            var slot = locationSlots[i];

            foreach (Transform c in slot)
                Destroy(c.gameObject);

            var prefab = shuffled[i];
            var newLocation = Instantiate(prefab, slot);
            newLocation.transform.localPosition = Vector3.zero;
            newLocation.transform.localRotation = Quaternion.identity;
            newLocation.transform.localScale = Vector3.one;

            if (locationDataDict.TryGetValue(prefab.name, out var data))
            {
                var holder = newLocation.GetComponent<LocationDataHolder>();
                if (holder != null) holder.data = data;
                yield return LoadLocationSpriteAsync(data);
            }

            spawnedLocations.Add(newLocation);
            spawnedLocationTransforms.Add(newLocation.transform);

            if (i == 0)
                characterSlots.villainSlot = FindDeepChild(newLocation.transform, "Villain_Slot");
            if (i == 3)
            {
                characterSlots.heroSlot1 = FindDeepChild(newLocation.transform, "Hero_Slot_1");
                characterSlots.heroSlot2 = FindDeepChild(newLocation.transform, "Hero_Slot_2");
            }

            if (delayBetweenLocations > 0f)
                yield return new WaitForSeconds(delayBetweenLocations);
        }

        // Lokacje gotowe
        OnLocationsReady?.Invoke();

        // Start spawn żetonów
        StartCoroutine(SpawnAllTokens());
    }

    private IEnumerator SpawnAllTokens()
    {
        foreach (var loc in spawnedLocations)
        {
            var id = loc.name.Replace("(Clone)", "");
            if (locationDataDict.TryGetValue(id, out var data))
                yield return StartCoroutine(SpawnTokens(loc, data));
        }

        // Żetony gotowe, lokacje pełne → wysyłamy sygnał
        StartCoroutine(DelayBeforeThreatCards());
    }

    private IEnumerator SpawnTokens(GameObject go, LocationData data)
    {
        for (int i = 0; i < data.starting_tokens.Count; i++)
        {
            var slot = FindDeepChild(go.transform, $"Slot_{i}");
            var prefab = data.starting_tokens[i] == "Civilian"
                         ? civilianTokenPrefab
                         : thugTokenPrefab;

            if (slot != null && prefab != null)
            {
                var tok = Instantiate(prefab, slot);
                tok.transform.localPosition = Vector3.zero;
                tok.transform.localRotation = Quaternion.identity;
            }
        }

        var threatSlot = FindDeepChild(go.transform, "ThreatSlot");
        if (threatSlot != null && threatTokenPrefab != null)
        {
            var t = Instantiate(threatTokenPrefab, threatSlot);
            t.transform.localPosition = Vector3.zero;
            t.transform.localRotation = Quaternion.identity;
        }

        yield return null;
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform c in parent)
        {
            if (c.name == name) return c;
            var r = FindDeepChild(c, name);
            if (r != null) return r;
        }
        return null;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    private IEnumerator LoadLocationSpriteAsync(LocationData data)
    {
        var index = data.id.Split('_')[1];
        var address = $"Location/Card_{index}";
        var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(address);
        yield return handle;

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            data.sprite = handle.Result;
            Debug.Log($"✅ Załadowano sprite dla {data.id}");
        }
        else
        {
            Debug.LogError($"❌ Błąd ładowania sprite {address}");
        }
    }
    private IEnumerator DelayBeforeThreatCards()
{
    yield return new WaitForSeconds(delayBetweenLocations*18); // ⬅️ tutaj ustawiasz ile sekund ma być opóźnienia

    OnLocationsAndTokensReady?.Invoke();
}
}
