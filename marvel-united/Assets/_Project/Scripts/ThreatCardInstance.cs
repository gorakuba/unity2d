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

    [Header("Token Prefabs")]
    public GameObject heroicTokenPrefab;
    public GameObject attackTokenPrefab;
    public GameObject moveTokenPrefab;
    public GameObject wildTokenPrefab;

    private void Awake()
    {
        if (data != null)
        {
            data.BuildDictionaries();
            string keys = data.required_symbols != null
                ? string.Join(",", data.required_symbols.Keys)
                : "(null)";
            Debug.Log($"[ThreatCardInstance.Awake] threat={data.id} dict keys={keys}");
        }
    }

    private void Start()
    {
        if (data != null)
            StartCoroutine(LoadThreatSpriteAsync(data));
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
            Debug.Log("✅ Załadowano sprite threat: " + address);
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

    /// <summary>
    /// Próbuje położyć token symbolu na tej karcie.
    /// </summary>
    /// <param name="symbolId">np. "Heroic","Attack","Move","Wild"</param>
    /// <param name="tokenPrefab">prefab tokena do wstawienia</param>
    public void TryPlaceSymbol(string symbolId, GameObject tokenPrefab)
    {
        Debug.Log($"[ThreatCardInstance] TryPlaceSymbol: '{symbolId}', prefab='{tokenPrefab?.name}'");
        if (data.required_symbols == null || !data.required_symbols.ContainsKey(symbolId))
        {
            Debug.LogWarning($"[ThreatCardInstance] '{symbolId}' not required by threat '{data.id}'");
            return;
        }
        if (tokenPrefab == null)
        {
            Debug.LogError($"[ThreatCardInstance] tokenPrefab is null for '{symbolId}'");
            return;
        }

        int used = data.used_symbols?.GetValueOrDefault(symbolId) ?? 0;
        int req  = data.required_symbols[symbolId];
        if (used >= req)
        {
            Debug.LogWarning($"[ThreatCardInstance] '{symbolId}' already filled {used}/{req}");
            return;
        }

        // slots named Slot_<symbol>_1..3
        for (int i = 1; i <= 3; i++)
        {
            string slotName = $"Slot_{symbolId.ToLower()}_{i}";
            Transform slot  = transform.Find(slotName);
            if (slot == null) continue;

            Debug.Log($"[ThreatCardInstance] Checking slot '{slotName}', childCount={slot.childCount}");
            if (slot.childCount == 0)
            {
                // Instantiate token prefab directly into slot
                GameObject tokenGO = Instantiate(tokenPrefab, slot);
                tokenGO.transform.localPosition = Vector3.zero;
                tokenGO.transform.localRotation = Quaternion.identity;
                tokenGO.transform.localScale = Vector3.one;

                // Update used count
                if (data.used_symbols == null)
                    data.used_symbols = new Dictionary<string,int>();
                data.used_symbols[symbolId] = used + 1;

                Debug.Log($"[ThreatCardInstance] Placed {symbolId} token in {slotName} ({used+1}/{req})");

                // Resolve if complete
                if (data.to_remove && IsFullyResolved())
                {
                    Debug.Log($"[ThreatCardInstance] Threat '{data.id}' fully resolved, resolving...");
                    ResolveThreat();
                }
                return;
            }
        }

        Debug.LogWarning($"[ThreatCardInstance] No free slot for '{symbolId}'");
    }

    private bool IsFullyResolved()
    {
        foreach (var kv in data.required_symbols)
            if ((data.used_symbols?.GetValueOrDefault(kv.Key) ?? 0) < kv.Value)
                return false;
        return true;
    }

    /// <summary>
    /// Przenosi token z ThreatSlot do pierwszego wolnego slotu misji i niszczy tę kartę.
    /// </summary>
    private void ResolveThreat()
    {
        Transform threatSlot = transform.Find("ThreatSlot");
        if (threatSlot != null && threatSlot.childCount > 0 && assignedLocation != null)
        {
            var missionToken = threatSlot.GetChild(0).gameObject;
            for (int i = 1;; i++)
            {
                Transform ms = assignedLocation.transform.Find($"Slot_{i}");
                if (ms != null && ms.childCount == 0)
                {
                    missionToken.transform.SetParent(ms, false);
                    missionToken.transform.localPosition = Vector3.zero;
                    missionToken.transform.localRotation = Quaternion.identity;
                    Debug.Log($"[ThreatCardInstance] Moved mission token to '{ms.name}'");
                    break;
                }
            }
        }
        Debug.Log($"[ThreatCardInstance] Destroying threat '{data.id}'");
        Destroy(gameObject);
    }
}
