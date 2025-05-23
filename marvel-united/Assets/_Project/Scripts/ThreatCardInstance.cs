using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ThreatCardInstance : MonoBehaviour
{
    [Tooltip("Dane tego threata – JsonUtility wypelnia listy, BuildDictionaries() tworzy z nich słowniki")]
    public ThreatCard data;
    public GameObject assignedLocation;
    public int currentMinionHealth = 0;

    [Header("Mission Slots (opcjonalnie możesz przypisać ręcznie)")]
    public List<Transform> missionSlots;

    [Header("Mission Manager (jeśli puste, zostanie automatycznie znaleziony)")]
    public MissionManager missionManager;

    private void Awake()
    {
        // 1) Build dictionaries
        if (data != null)
        {
            data.BuildDictionaries();
            var keys = data.required_symbols != null
                ? string.Join(",", data.required_symbols.Keys)
                : "(null)";
            Debug.Log($"[ThreatCardInstance.Awake] threat={data.id} dict keys={keys}");
        }

        // 2) Auto‐assign MissionManager, jeśli nie podpięto w Inspektorze
        if (missionManager == null)
        {
            missionManager = UnityEngine.Object.FindFirstObjectByType<MissionManager>();
            if (missionManager == null)
                Debug.LogError("[ThreatCardInstance] Nie znaleziono MissionManager w scenie!");
            else
                Debug.Log("[ThreatCardInstance] Automatycznie przypisano MissionManager");
        }

        // 3) Auto‐fill missionSlots, jeśli lista jest pusta
        if (missionSlots == null || missionSlots.Count == 0)
        {
            missionSlots = new List<Transform>();
            var missionRoot = GameObject.Find("ThreatMission");
            if (missionRoot != null)
            {
                foreach (Transform child in missionRoot.transform)
                    if (child.name.StartsWith("Slot_"))
                        missionSlots.Add(child);
                Debug.Log($"[ThreatCardInstance.Awake] Auto‐assigned {missionSlots.Count} missionSlots");
            }
            else
            {
                Debug.LogWarning("[ThreatCardInstance] Nie znaleziono obiektu 'ThreatMission' w scenie!");
            }
        }
    }

    private void Start()
    {
        if (data != null)
            StartCoroutine(LoadThreatSpriteAsync(data));
        var slotHealth = transform.Find("Slot_Health");
        currentMinionHealth = slotHealth != null ? slotHealth.childCount : 0;
    }

    private IEnumerator LoadThreatSpriteAsync(ThreatCard card)
    {
        string villainId = GameManager.Instance.selectedVillain;
        string address   = GetThreatSpriteAddress(villainId, card.id);
        var handle       = Addressables.LoadAssetAsync<Sprite>(address);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            card.sprite = handle.Result;
        }
        else
        {
            Debug.LogError("❌ Nie udało się załadować sprite'a: " + address);
        }
    }

    private string GetThreatSpriteAddress(string villainId, string cardId)
    {
        string index = cardId.Split('_')[1];
        return $"Villain/{villainId}/Threats/Card_{index}";
    }
public void TryRemoveMinionToken()
    {
        Transform slotHealth = transform.Find("Slot_Health");
        if (slotHealth == null)
        {
            Debug.LogWarning($"[ThreatCardInstance] Brak Slot_Health na karcie {data.id}");
            return;
        }

        if (slotHealth.childCount > 0)
        {
            // Usuń ostatni token
            int last = slotHealth.childCount - 1;
            Destroy(slotHealth.GetChild(last).gameObject);

            // Przelicz currentMinionHealth
            currentMinionHealth = slotHealth.childCount - 1;
            Debug.Log($"[ThreatCardInstance] Usunięto token życia z {data.id}, pozostało {currentMinionHealth}");

            // Jeśli nie ma już żadnego, odpal resolve po 2s
            if (currentMinionHealth == 0)
                StartCoroutine(CheckMinionResolvedDelayed(2f));
        }
        else
        {
            Debug.LogWarning($"[ThreatCardInstance] Brak tokenów życia do usunięcia na {data.id}");
        }
    }

    private IEnumerator CheckMinionResolvedDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        // ResolveThreat już przeniesie token misji i zniszczy tę kartę
        ResolveThreat();
    }



    public void TryPlaceSymbol(string symbolId, GameObject tokenPrefab)
    {
        if (data.required_symbols == null || !data.required_symbols.ContainsKey(symbolId))
            return;
        if (tokenPrefab == null)
        {
            Debug.LogError($"[ThreatCardInstance] tokenPrefab null for '{symbolId}'");
            return;
        }

        int used = data.used_symbols?.GetValueOrDefault(symbolId) ?? 0;
        int req = data.required_symbols[symbolId];
        if (used >= req) return;

        for (int i = 1; i <= 3; i++)
        {
            string slotName = $"Slot_{symbolId.ToLower()}_{i}";
            Transform slot = transform.Find(slotName);
            if (slot != null && slot.childCount == 0)
            {
                var tokenGO = Instantiate(tokenPrefab, slot);
                tokenGO.transform.localPosition = Vector3.zero;
                tokenGO.transform.localRotation = Quaternion.identity;
                tokenGO.transform.localScale = Vector3.one;

                if (data.used_symbols == null)
                    data.used_symbols = new Dictionary<string, int>();
                data.used_symbols[symbolId] = used + 1;

                // sprawdź po 2s
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
        // przenieś token z lokacji
        var threatSlot = assignedLocation.transform.Find("ThreatSlot");
        if (threatSlot != null && threatSlot.childCount > 0)
        {
            var missionToken = threatSlot.GetChild(0).gameObject;
            foreach (var ms in missionSlots)
            {
                if (ms != null && ms.childCount == 0)
                {
                    missionToken.transform.SetParent(ms, false);
                    missionToken.transform.localPosition = Vector3.zero;
                    missionToken.transform.localRotation = Quaternion.identity;
                    break;
                }
            }
        }

        // sprawdź misje
        if (missionManager != null)
            missionManager.CheckMissions();

        // usuń kartę
        Destroy(gameObject);
    }
}
