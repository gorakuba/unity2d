using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ThreatCardInstance : MonoBehaviour
{
    [Tooltip("Dane tego threata – JsonUtility wypelnia listy, BuildDictionaries() tworzy z nich slowniki")]
    public ThreatCard data;
    public GameObject assignedLocation;
    public int currentMinionHealth = 0;

    private void Awake()
    {
        if (data != null)
        {
            data.BuildDictionaries();
            string keys = data.required_symbols != null
                ? string.Join(",", data.required_symbols.Keys)
                : "(null)";
            Debug.Log("[ThreatCardInstance.Awake] threat=" + data.id + " dict keys=" + keys);
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
        return "Villain/" + villainId + "/Threats/Card_" + index;
    }

    public void TryPlaceSymbol(string symbolId, GameObject symbolPrefab)
    {
        if (data.required_symbols == null || !data.required_symbols.ContainsKey(symbolId))
            return;

        int current = data.used_symbols != null && data.used_symbols.ContainsKey(symbolId)
            ? data.used_symbols[symbolId]
            : 0;
        int required = data.required_symbols[symbolId];
        if (current >= required) return;

        for (int i = 0; i < 3; i++)
        {
            Transform slot = transform.Find("Slot_" + symbolId + "_" + i);
            if (slot != null && slot.childCount == 0)
            {
                GameObject icon = Instantiate(symbolPrefab, slot);
                icon.transform.localPosition = Vector3.zero;
                icon.transform.localRotation = Quaternion.identity;

                if (data.used_symbols == null)
                    data.used_symbols = new Dictionary<string,int>();
                if (!data.used_symbols.ContainsKey(symbolId))
                    data.used_symbols[symbolId] = 0;
                data.used_symbols[symbolId]++;

                Debug.Log("✅ Dodano symbol " + symbolId + " do Threat " + data.name);
                if (data.to_remove && IsFullyResolved())
                {
                    Destroy(gameObject);
                    Debug.Log("💥 Threat " + data.name + " usunięty po spełnieniu warunków!");
                }
                return;
            }
        }
    }

    private bool IsFullyResolved()
    {
        foreach (var pair in data.required_symbols)
        {
            if (!data.used_symbols.ContainsKey(pair.Key) ||
                data.used_symbols[pair.Key] < pair.Value)
            {
                return false;
            }
        }
        return true;
    }
}
