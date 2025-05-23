using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class VillainsRoot
{
    public List<VillainData> villains;
}

[Serializable]
public class VillainData
{
    public string id;
    public string name;
    public string bam;

    [Serializable]
    public class HealthPerPlayers { public int _2; public int _3; public int _4; }
    public HealthPerPlayers health_per_players;

    public string bam_effect;
    public string villainous_plot;
    public bool additional_win_condition;
    public string additional_win_condition_script;
    public string overflow;
    public List<ThreatCard> threats;
    public List<VillainCard> cards;
    public string imagePath;
    public string backTexturePath;
}

public class VillainController : MonoBehaviour
{
    public static VillainController Instance { get; private set; }
    public event Action<Transform> OnVillainStop;
    public event Func<IEnumerator> OnBAMEffect;
    public int CurrentHealth { get; private set; }

    [Header("References")]
    public SpriteRenderer visualRenderer;
    public VillainVisualDatabase visualDatabase;

    [Header("Movement Settings")]
    [Tooltip("Time (in seconds) for one movement step")]
    public float stepDuration = 0.3f;

    [Header("Health Settings")]
    [Tooltip("Tekst JSON z danymi villains")]
    public TextAsset villainJson;

    private VillainsRoot loadedVillainData;
    private Transform[] _villainSlots;
    private int _currentIndex;

    private Dictionary<string, Func<IEnumerator>> bamEffects = new();
    private string currentBAMId;
    private IVillainSpecials specialHandler;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        LoadVillainJson();
        InitializeHealthForPlayers(2);  // test: 2 graczy
        Debug.Log($"[Villain] HP = {CurrentHealth}");

        var locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
        _villainSlots = locMan.VillainSlots.ToArray();

        bamEffects["red_skull"]  = BAM_RedSkull;
        bamEffects["taskmaster"] = () => { BAM_Taskmaster(); return null; };
        bamEffects["ultron"]     = () => { BAM_Ultron();      return null; };

        currentBAMId = GetBAMIdForCurrentVillain();
        switch (GameManager.Instance.selectedVillain)
        {
            case "red_skull":
                specialHandler = new RedSkullSpecials();
                break;
            // inne...
        }
    }

    private void LoadVillainJson()
    {
        if (villainJson == null)
        {
            Debug.LogError("VillainController: nie przypisano villainJson!");
            return;
        }
        loadedVillainData = JsonUtility.FromJson<VillainsRoot>(villainJson.text);
    }

    private void InitializeHealthForPlayers(int players)
    {
        if (loadedVillainData == null) return;
        var vid = GameManager.Instance.selectedVillain;
        var vd  = loadedVillainData.villains.FirstOrDefault(v => v.id == vid);
        if (vd == null) return;

        CurrentHealth = players switch
        {
            2 => vd.health_per_players._2,
            3 => vd.health_per_players._3,
            4 => vd.health_per_players._4,
            _ => vd.health_per_players._2
        };
    }

    private string GetBAMIdForCurrentVillain()
    {
        if (loadedVillainData == null) return "";
        var vid = GameManager.Instance.selectedVillain;
        var vd  = loadedVillainData.villains.Find(v => v.id == vid);
        return vd != null ? vd.bam : "";
    }

    public void Initialize(string villainID, int startIndex = 0)
    {
        var locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
        _villainSlots = locMan.VillainSlots.ToArray();

        var sprite = visualDatabase.GetVillainSprite(villainID);
        if (sprite != null) visualRenderer.sprite = sprite;

        _currentIndex = startIndex;
        transform.SetParent(_villainSlots[_currentIndex], false);
        transform.localPosition = Vector3.zero;
    }

public IEnumerator MoveVillain(int steps)
{
    int count = _villainSlots.Length;

    if (steps <= 0)
    {
        // Villain nie rusza się, ale wciąż „staje” na obecnym slocie
        OnVillainStop?.Invoke(_villainSlots[_currentIndex]);
        yield break;
    }

    for (int i = 0; i < steps; i++)
    {
        _currentIndex = (_currentIndex + 1) % count;
        var target = _villainSlots[_currentIndex];
        yield return StartCoroutine(AnimateMoveTo(target.position));
        transform.SetParent(target, true);
    }

    // po zakończeniu ruchu – emitujemy event
    OnVillainStop?.Invoke(_villainSlots[_currentIndex]);
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

    public IEnumerator ExecuteAttack(VillainCard card)
    {
        if (card.BAM_effect)
        {
            Debug.Log("💥 BAM effect!");
            if (OnBAMEffect != null)
                foreach (Func<IEnumerator> handler in OnBAMEffect.GetInvocationList())
                yield return StartCoroutine(handler());
            
            ExecuteBAM();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ExecuteBAM()
    {
        if (string.IsNullOrEmpty(currentBAMId)) return;
        if (bamEffects.TryGetValue(currentBAMId, out var func))
            StartCoroutine(func.Invoke());
    }

    private IEnumerator BAM_RedSkull()
    {
        Debug.Log("🔥 BAM Red Skull");
        var root = GetLocationRoot(transform);
        var h1   = SetupManager.hero1Controller;
        var h2   = SetupManager.hero2Controller;
        int dmg  = 0;
        if (h1 != null && GetLocationRoot(h1.transform) == root) dmg++;
        if (h2 != null && GetLocationRoot(h2.transform) == root) dmg++;
        BAMController.StartBAM(dmg);
        if (dmg > 0)
        {
            if (h1 != null && GetLocationRoot(h1.transform) == root)
                yield return StartCoroutine(h1.GetComponent<HeroDamageHandler>().TakeDamageCoroutine());
            if (h2 != null && GetLocationRoot(h2.transform) == root)
                yield return StartCoroutine(h2.GetComponent<HeroDamageHandler>().TakeDamageCoroutine());
        }
        DashboardLoader.Instance.MoveFearTrack(2);
    }

    private void BAM_Taskmaster() { Debug.Log("🎯 BAM Taskmaster (przykład)"); }
    private void BAM_Ultron()     { Debug.Log("🤖 BAM Ultron (przykład)"); }

    public IEnumerator ExecuteSpawn(VillainCard card)
    {
        if (!card.HasSpawn) yield break;
        var locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
        var locs   = locMan.LocationRoots;
        int i = _currentIndex;

        yield return StartCoroutine(SpawnTokens(card.Location_left,   locs[(i-1+locs.Count)%locs.Count], "LEFT"));
        yield return StartCoroutine(SpawnTokens(card.Location_middle, locs[i],                       "MIDDLE"));
        yield return StartCoroutine(SpawnTokens(card.Location_right,  locs[(i+1)%locs.Count],     "RIGHT"));
    }

    private IEnumerator SpawnTokens(List<LocationSpawnSymbol> groups, Transform root, string name)
    {
        if (groups == null || groups.Count == 0) yield break;
        var free = GetAllFreeSlots(root);
        foreach (var g in groups)
            for (int k = 0; k < g.count; k++)
                yield return StartCoroutine(SpawnSingle(g.symbol, root, name, free));
    }

    private IEnumerator SpawnSingle(string type, Transform root, string name, List<Transform> free)
    {
        var locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
        GameObject prefab = type == "Civillian" ? locMan.civilianTokenPrefab
                         : type == "Thug"      ? locMan.thugTokenPrefab
                         : null;
        if (prefab != null && free.Count > 0)
        {
            var slot = free[0]; free.RemoveAt(0);
            var tok  = Instantiate(prefab, slot.position, slot.rotation, slot);
            tok.transform.localPosition  = Vector3.zero;
            tok.transform.localRotation  = Quaternion.identity;
            if (tok.GetComponent<TokenDrop>() == null) tok.AddComponent<TokenDrop>();
            Debug.Log($"[SPAWN] {name} → {type}");
        }
        else
        {
            Debug.LogWarning($"[{name}] brak slotu dla {type}");
            DashboardLoader.Instance.MoveFearTrack(1);
        }
        yield return new WaitForSeconds(0.3f);
    }

    private List<Transform> GetAllFreeSlots(Transform root)
    {
        var list = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            var s = FindDeepChild(root, $"Slot_{i}");
            if (s != null && s.childCount == 0) list.Add(s);
        }
        return list;
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform c in parent)
        {
            if (c.name == name) return c;
            var r = FindDeepChild(c, name);
            if (r != null) return r;
        }
        return null;
    }

    public IEnumerator ExecuteAbility(VillainCard card)
    {
        if (!card.special) yield break;
        if (specialHandler == null) yield break;
        yield return specialHandler.ExecuteSpecial(card.special_ability);
        yield return new WaitForSeconds(0.5f);
    }

    private Transform GetLocationRoot(Transform t)
    {
        while (t.parent != null && !t.parent.name.StartsWith("Location_PLACE"))
            t = t.parent;
        return t;
    }

    /// <summary>
    /// Zadaj obrażenia Zbirovi.
    /// </summary>
    public void DealDamageToVillain(int amount)
    {
        CurrentHealth -= amount;
        Debug.Log($"Villain otrzymuje {amount} dmg → pozostało {CurrentHealth}");
        for (int i = 0; i < amount; i++)
    {
        DashboardLoader.Instance.RemoveFirstHealthToken();
    }
        if (CurrentHealth <= 0)
            Debug.Log("💀 Villain pokonany!");
        
    }
}
