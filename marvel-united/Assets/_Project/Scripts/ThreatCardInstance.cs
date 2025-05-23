using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;  // <-- potrzebne dla Enumerable.Empty<>
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
public class ThreatCardInstance : MonoBehaviour
{
    [Tooltip("Dane tego threata – JsonUtility wypełnia listy, BuildDictionaries() tworzy słowniki")]
    public ThreatCard data;
    public GameObject assignedLocation;
    public int currentMinionHealth = 0;

    [Header("Mission Slots (opcjonalnie ręcznie)")]
    public List<Transform> missionSlots;

    [Header("Mission Manager (autopodpinany jeśli null)")]
    public MissionManager missionManager;

private void Awake()
{
    // ---------------------------------------
    // 1) Autopodpinanie MissionManagera
    // ---------------------------------------
    if (missionManager == null)
    {
        missionManager = Object.FindFirstObjectByType<MissionManager>();
        if (missionManager == null)
            Debug.LogError("[ThreatCardInstance] Nie znaleziono MissionManager w scenie!");
        else
            Debug.Log($"[ThreatCardInstance] Automatycznie przypięto MissionManager ({missionManager.name})");
    }

    // ---------------------------------------
    // 2) Autopodpinanie missionSlots
    // ---------------------------------------
    if (missionSlots == null || missionSlots.Count == 0)
    {
        missionSlots = new List<Transform>();
        var root = GameObject.Find("ThreatMission");
        if (root != null)
        {
            foreach (Transform c in root.transform)
                if (c.name.StartsWith("Slot_"))
                    missionSlots.Add(c);
            Debug.Log($"[ThreatCardInstance.Awake] Autopodpinano {missionSlots.Count} missionSlots");
        }
        else
        {
            Debug.LogWarning("[ThreatCardInstance] Nie znaleziono obiektu 'ThreatMission' w hierarchii!");
        }
    }

    // ---------------------------------------
    // 3) Inicjalizacja currentMinionHealth
    // ---------------------------------------
    var slotHealth = transform.Find("Slot_Health");
    currentMinionHealth = slotHealth != null ? slotHealth.childCount : 0;
}

private void Start()
{
    // ---------------------------------------------------
    // 4) Jeżeli mamy dane z JSON-a, to je zbuduj i podłącz
    // ---------------------------------------------------
    if (data != null)
    {
        // budujemy słowniki symboli
        data.BuildDictionaries();

        // logowanie dla debugowania
        var keys = data.required_symbols != null
            ? string.Join(",", data.required_symbols.Keys)
            : "(null)";
        Debug.Log($"[ThreatCardInstance.Start] threat={data.id} dict keys={keys}");

        // podpinamy wszystkie ability z JSON-owego pola abilities
        foreach (var ab in data.abilities ?? Enumerable.Empty<AbilityData>())
        {
            var comp = ThreatAbilityFactory.Attach(ab.id, this);
            if (comp != null)
            {
                comp.RegisterTrigger(ab.trigger, this);
                Debug.Log($"[ThreatCardInstance.Start] podpiąłem ability {ab.id} pod trigger {ab.trigger}");
            }
            else
            {
                Debug.LogWarning($"[ThreatCardInstance.Start] nie znaleziono ability {ab.id}");
            }
        }
    }
    else
    {
        Debug.LogWarning("[ThreatCardInstance.Start] Brak danych (data == null), nie podpinam ability");
    }

    // jeżeli sprite jeszcze nie załadowany - odpalenie w tle
    if (data != null)
        StartCoroutine(LoadThreatSpriteAsync(data));
}


    private IEnumerator LoadThreatSpriteAsync(ThreatCard card)
    {
        string villainId = GameManager.Instance.selectedVillain;
        string address = $"Villain/{villainId}/Threats/Card_{card.id.Split('_')[1]}";
        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            card.sprite = handle.Result;
        else
            Debug.LogError("Nie udało się załadować sprite'a: " + address);
    }

    public void TryRemoveMinionToken()
    {
        var slotHealth = transform.Find("Slot_Health");
        if (slotHealth == null)
        {
            Debug.LogWarning($"[ThreatCardInstance] Brak Slot_Health na karcie {data?.id}");
            return;
        }
        if (slotHealth.childCount > 0)
        {
            int last = slotHealth.childCount - 1;
            Destroy(slotHealth.GetChild(last).gameObject);

            currentMinionHealth = slotHealth.childCount-1;
            Debug.Log($"[ThreatCardInstance] Usunięto token zdrowia z {data.id}, pozostało {currentMinionHealth}");

            if (currentMinionHealth == 0)
                StartCoroutine(CheckMinionResolvedDelayed(1f));
        }
        else
        {
            Debug.LogWarning($"[ThreatCardInstance] Brak tokenów zdrowia do usunięcia na {data.id}");
        }
    }

    private IEnumerator CheckMinionResolvedDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResolveThreat();
    }

    public void TryPlaceSymbol(string symbolId, GameObject tokenPrefab)
    {
        if (data.required_symbols == null || !data.required_symbols.ContainsKey(symbolId))
            return;
        if (tokenPrefab == null)
        {
            Debug.LogError($"[ThreatCardInstance] tokenPrefab null dla '{symbolId}'");
            return;
        }
        int used = data.used_symbols?.GetValueOrDefault(symbolId) ?? 0;
        int req = data.required_symbols[symbolId];
        if (used >= req) return;

        for (int i = 1; i <= 3; i++)
        {
            var slot = transform.Find($"Slot_{symbolId.ToLower()}_{i}");
            if (slot != null && slot.childCount == 0)
            {
                var go = Instantiate(tokenPrefab, slot);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                if (data.used_symbols == null)
                    data.used_symbols = new Dictionary<string,int>();
                data.used_symbols[symbolId] = used + 1;

                StartCoroutine(CheckResolvedDelayed(2f));
                return;
            }
        }
    }

    private IEnumerator CheckResolvedDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (IsFullyResolved())
            ResolveThreat();
    }

    private bool IsFullyResolved()
    {
        foreach (var kv in data.required_symbols)
            if ((data.used_symbols?.GetValueOrDefault(kv.Key) ?? 0) < kv.Value)
                return false;
        return true;
    }

    private void ResolveThreat()
    {
        var threatSlot = assignedLocation.transform.Find("ThreatSlot");
        if (threatSlot != null && threatSlot.childCount > 0)
        {
            var missionToken = threatSlot.GetChild(0).gameObject;
            foreach (var ms in missionSlots)
                if (ms.childCount == 0)
                {
                    missionToken.transform.SetParent(ms, false);
                    missionToken.transform.localPosition = Vector3.zero;
                    missionToken.transform.localRotation = Quaternion.identity;
                    break;
                }
        }

        missionManager?.CheckMissions();
        Destroy(gameObject);
    }
}
