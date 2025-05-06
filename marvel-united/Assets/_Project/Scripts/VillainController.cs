using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class VillainController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer visualRenderer;
    public VillainVisualDatabase visualDatabase;

    [Header("Movement Settings")]
    [Tooltip("Time (in seconds) for one movement step")]  
    public float stepDuration = 0.3f;

    private Transform[] _villainSlots;
    private int _currentIndex;

    // -------------------------------
    // BAM SYSTEM
    // -------------------------------
    private Dictionary<string, Func<IEnumerator>> bamEffects = new Dictionary<string, Func<IEnumerator>>();

    private string currentBAMId;
    private IVillainSpecials specialHandler;

    // Villains data loaded from StreamingAssets
    private VillainsRoot loadedVillainData;

    void Awake()
    {
        // Wczytaj dane JSON z StreamingAssets
        LoadVillainJson();

        // Pobierz sloty z LocationManager
        var locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
        _villainSlots = locMan.VillainSlots.ToArray();

        // Rejestracja BAM efektów
        bamEffects.Add("red_skull", BAM_RedSkull);
        bamEffects.Add("taskmaster", () => { BAM_Taskmaster(); return null; }); // template na przyszłość
        bamEffects.Add("ultron", () => { BAM_Ultron(); return null; });         // template na przyszłość

        // Pobierz aktualny BAM ID
        currentBAMId = GetBAMIdForCurrentVillain();
        string villainId = GameManager.Instance.selectedVillain;
        switch (villainId)
    {
        case "red_skull":
            specialHandler = new RedSkullSpecials();
            break;
        // tu dodajesz kolejnych zbirów
    }
    }

    private void LoadVillainJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Villains.json");

        if (!File.Exists(path))
        {
            Debug.LogError("VillainController: Nie znaleziono Villains.json w StreamingAssets!");
            return;
        }

        string json = File.ReadAllText(path);
        loadedVillainData = JsonUtility.FromJson<VillainsRoot>(json);
    }

    private string GetBAMIdForCurrentVillain()
    {
        var villainId = GameManager.Instance.selectedVillain;
        var villain = loadedVillainData.villains.Find(v => v.id == villainId);

        if (villain != null)
            return villain.bam;

        Debug.LogError("VillainController: Nie znaleziono BAM ID dla Villain!");
        return "";
    }

    // ============================
    // 1️⃣ Initialization
    // ============================
    public void Initialize(string villainID, int startIndex = 0)
    {
        var locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
        _villainSlots = locMan.VillainSlots.ToArray();

        var sprite = visualDatabase.GetVillainSprite(villainID);
        if (sprite != null)
            visualRenderer.sprite = sprite;

        _currentIndex = startIndex;
        transform.SetParent(_villainSlots[_currentIndex], false);
        transform.localPosition = Vector3.zero;
    }

    // ============================
    // 2️⃣ Movement
    // ============================
    public IEnumerator MoveVillain(int steps)
    {
        int count = _villainSlots.Length;
        for (int i = 0; i < steps; i++)
        {
            _currentIndex = (_currentIndex + 1) % count;
            var target = _villainSlots[_currentIndex];
            yield return StartCoroutine(AnimateMoveTo(target.position));
            transform.SetParent(target, true);
        }
    }

    private IEnumerator AnimateMoveTo(Vector3 targetPos)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < stepDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, targetPos, t / stepDuration);
            yield return null;
        }
        transform.position = targetPos;
    }

    // ============================
    // 3️⃣ BAM Attack (Main BAM trigger)
    // ============================
    public IEnumerator ExecuteAttack(VillainCard card)
    {
        if (card.BAM_effect)
        {
            Debug.Log("💥 BAM effect!");

            // Wykonaj BAM efekt
            ExecuteBAM();

            yield return new WaitForSeconds(0.5f);
        }
    }

private void ExecuteBAM()
{
    if (string.IsNullOrEmpty(currentBAMId))
    {
        Debug.LogWarning("BAM ID nie ustawiony → brak efektu BAM");
        return;
    }

    if (bamEffects.TryGetValue(currentBAMId, out var func))
    {
        Debug.Log($"🚨 [BAM] {currentBAMId.ToUpper()} → Aktywacja efektu BAM");
        StartCoroutine(func.Invoke());
    }
    else
    {
        Debug.LogWarning($"Brak efektu BAM dla ID: {currentBAMId}");
    }
}

    // ✅ BAM EFFECTS ========================

    IEnumerator BAM_RedSkull()
    {
        Debug.Log("🔥 BAM Red Skull → Obrażenia + Fear +2");

        var villainLocationRoot = GetLocationRoot(transform);

        var hero1 = SetupManager.hero1Controller;
        var hero2 = SetupManager.hero2Controller;

        var hero1LocationRoot = hero1 != null ? GetLocationRoot(hero1.transform) : null;
        var hero2LocationRoot = hero2 != null ? GetLocationRoot(hero2.transform) : null;
        int playersToDamage = 0;

        if (hero1 != null && hero1LocationRoot == villainLocationRoot)
            playersToDamage++;

        if (hero2 != null && hero2LocationRoot == villainLocationRoot)
            playersToDamage++;

        // UWAGA: START BAM -> RAZ dla wszystkich graczy
        BAMController.StartBAM(playersToDamage);
        Debug.Log("BAM");
        if (hero1 != null && hero1LocationRoot == villainLocationRoot)
        {
            Debug.Log("BAM trafia gracza 1");
            yield return StartCoroutine(hero1.GetComponent<HeroDamageHandler>().TakeDamageCoroutine());
            
        }

        if (hero2 != null && hero2LocationRoot == villainLocationRoot)
        {
            Debug.Log("BAM trafia gracza 2");
            yield return StartCoroutine(hero2.GetComponent<HeroDamageHandler>().TakeDamageCoroutine());
            
        }
        
        DashboardLoader.Instance.MoveFearTrack(2);
    }


    // TEMPLATE BAMs (przykłady na przyszłość)

    private void BAM_Taskmaster()
    {
        Debug.Log("🎯 BAM Taskmaster → Porusz + Atakuj (przykład)");
        // TODO: Dodaj implementację BAM dla Taskmaster
    }

    private void BAM_Ultron()
    {
        Debug.Log("🤖 BAM Ultron → Spawn robotów (przykład)");
        // TODO: Dodaj implementację BAM dla Ultron
    }

    // ============================
    // 4️⃣ Spawn Tokens
    // ============================
public IEnumerator ExecuteSpawn(VillainCard card)
    {
        if (!card.HasSpawn)
            yield break;

        Debug.Log("🎯 Spawn tokens START");

        var locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
        var locations = locMan.LocationRoots;

        int villainIndex = _currentIndex;
        int leftIndex = (villainIndex - 1 + locations.Count) % locations.Count;
        int middleIndex = villainIndex;
        int rightIndex = (villainIndex + 1) % locations.Count;

        yield return StartCoroutine(SpawnTokensForLocation(card.Location_left, locations[leftIndex], "LEFT"));
        yield return StartCoroutine(SpawnTokensForLocation(card.Location_middle, locations[middleIndex], "MIDDLE"));
        yield return StartCoroutine(SpawnTokensForLocation(card.Location_right, locations[rightIndex], "RIGHT"));
    }

private IEnumerator SpawnTokensForLocation(List<LocationSpawnSymbol> spawnGroups, Transform locationRoot, string locationName)
    {
        if (spawnGroups == null || spawnGroups.Count == 0)
            yield break;

        List<Transform> freeSlots = GetAllFreeTokenSlots(locationRoot);

        int totalToSpawn = 0;
        foreach (var group in spawnGroups)
            totalToSpawn += group.count;

        Debug.Log($"[DEBUG] {locationName} → Free slots available: {freeSlots.Count}, Tokens to spawn: {totalToSpawn}");

        foreach (var group in spawnGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                yield return StartCoroutine(SpawnToken(group.symbol, locationRoot, locationName, freeSlots));
            }
        }
    }

private IEnumerator SpawnToken(string type, Transform locationRoot, string locationName, List<Transform> freeSlots)
{
    var locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
    GameObject prefab = null;

    if (type == "Civillian")
        prefab = locMan.civilianTokenPrefab;
    else if (type == "Thug")
        prefab = locMan.thugTokenPrefab;

    if (prefab != null)
    {
        if (freeSlots.Count > 0)
        {
            var slot = freeSlots[0];
            freeSlots.RemoveAt(0);

            // ✅ INSTANCJA → dokładnie tak jak Prepare Game (ustawia automatycznie parent i lokalną skalę)
            GameObject token = Instantiate(prefab, slot.position, slot.rotation, slot);
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;

            // ✅ TokenDrop → jeżeli jest w prefabie to Start() zrobi swoje, jak nie ma → możesz dodać
            if (token.GetComponent<TokenDrop>() == null)
            {
                token.AddComponent<TokenDrop>();
            }

            Debug.Log($"[SPAWN OK] {locationName} → Spawned {type} in {locationRoot.name}");
        }
        else
        {
            Debug.LogWarning($"[SPAWN FAIL] {locationName} → No free slot for {type} in {locationRoot.name}");

            if (type == "Civillian")
            {
                Debug.LogWarning($"[OVERFLOW] Civillian → 1 Fear Track ");
                DashboardLoader.Instance.MoveFearTrack(1);
            }
            else if (type == "Thug")
            {
                Debug.LogWarning($"[OVERFLOW] Thug → 1 Fear Track.");
                DashboardLoader.Instance.MoveFearTrack(1);
            }
        }
    }
    else
    {
        Debug.LogError($"[SPAWN ERROR] Invalid token type: {type}");
    }

    yield return new WaitForSeconds(0.3f);
}


private List<Transform> GetAllFreeTokenSlots(Transform locationRoot)
    {
        List<Transform> freeSlots = new List<Transform>();

        for (int i = 0; i < 6; i++)
        {
            var slot = FindDeepChild(locationRoot, $"Slot_{i}");
            if (slot != null && slot.childCount == 0)
                freeSlots.Add(slot);
        }

        return freeSlots;
    }

private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            var result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }

        return null;
    }


    // ============================
    // 5️⃣ Special Ability
    // ============================
    public IEnumerator ExecuteAbility(VillainCard card)
    {
          if (!card.special)
        yield break;

    if (specialHandler == null)
        {
            Debug.LogWarning("Special handler nie ustawiony dla Zbira!");
            yield break;
        }

        Debug.Log($"[SPECIAL] Wywołano → {card.special_ability}");

        yield return specialHandler.ExecuteSpecial(card.special_ability);
            yield return new WaitForSeconds(0.5f);
        }
    
    private Transform GetLocationRoot(Transform t)
    {
        while (t.parent != null && !t.parent.name.StartsWith("Location_PLACE"))
            t = t.parent;
        return t;
    }
}
