using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
public class SymbolPanelUI : MonoBehaviour
{
    [Header("Kontenery (ustaw w Inspectorze)")]
    public Transform currentContainer;
    public Transform previousContainer;
    public Transform persistentContainer;

    [Header("Prefab ikony symbolu (SymbolIcon)")]
    public GameObject symbolIconPrefab;

    [Header("Mapowanie ID → Sprite")]
    public List<SymbolEntry> symbolEntries;

    Dictionary<string, Sprite> lookup;

    // Pulka trwałych tokenów: ID → ilość
    Dictionary<string,int> persistentPool = new();
    [Header("Panel wybranego symbolu")]
public Image currentlySelectedImage;

    // jeżeli chcesz reagować w TurnManagerze
    public event Action<string> onSymbolClicked;
    void Awake()
    {
        // zbuduj słownik do szybkiego lookup
        lookup = new Dictionary<string, Sprite>();
        foreach(var e in symbolEntries)
            if (!lookup.ContainsKey(e.id))
                lookup[e.id] = e.sprite;
    }

    // 1) Bieżące symbole
    public void ShowCurrentSymbols(List<string> symbols)
    {
        Clear(currentContainer);
        foreach(var id in symbols)
            InstantiateIcon(currentContainer, id);
    }

    // 2) Poprzednie symbole drugiego gracza
    public void ShowPreviousSymbols(List<string> symbols)
    {
        Clear(previousContainer);
        foreach(var id in symbols)
            InstantiateIcon(previousContainer, id);
    }

    // 3) Dodaj symbole trwałe
    public void AddPersistentSymbols(List<string> symbols)
    {
        foreach(var id in symbols)
        {
            if (!persistentPool.ContainsKey(id)) persistentPool[id] = 0;
            persistentPool[id]++;
        }
        RefreshPersistent();
    }

    // 4) Jeśli zużywasz trwałe:
    public void UsePersistentSymbols(List<string> symbols)
    {
        foreach(var id in symbols)
        {
            if (!persistentPool.ContainsKey(id)) continue;
            persistentPool[id]--;
            if (persistentPool[id] <= 0) persistentPool.Remove(id);
        }
        RefreshPersistent();
    }

    void RefreshPersistent()
    {
        Clear(persistentContainer);
        foreach(var kv in persistentPool)
            for(int i=0; i<kv.Value; i++)
                InstantiateIcon(persistentContainer, kv.Key);
    }

void InstantiateIcon(Transform parent, string id)
{
    if (!lookup.TryGetValue(id, out var sprite))
    {
        Debug.LogWarning($"[SymbolPanelUI] Brak sprite dla ID `{id}`");
        return;
    }

    // 1) Instancja
    var go = Instantiate(symbolIconPrefab, parent);

    // 2) Podmień obrazek
    var img = go.GetComponent<Image>();
    img.sprite = sprite;

    // 3) Dodaj Button.onClick
    var btn = go.GetComponent<Button>();
    if (btn != null)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            // 3a) Ustaw sprite w panelu „CurrentlySelectedSymbol”
            currentlySelectedImage.sprite = sprite;
            currentlySelectedImage.gameObject.SetActive(true);
            // 3b) (Opcjonalnie) wyślij event dalej
            onSymbolClicked?.Invoke(id);
        });
    }
}


    void Clear(Transform t)
    {
        for(int i=t.childCount-1; i>=0; i--)
            Destroy(t.GetChild(i).gameObject);
    }

    [System.Serializable]
    public struct SymbolEntry
    {
        [Tooltip("dokładny klucz, np. \"Attack\", \"Move\", \"Wild\"")]
        public string id;
        [Tooltip("sprite ikony tego symbolu")]
        public Sprite sprite;
    }
    public void ClearSelectedSymbol()
{
    if (currentlySelectedImage != null)
    {
        currentlySelectedImage.sprite = null;
        currentlySelectedImage.gameObject.SetActive(false);
    }
}

}
