using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
public class SymbolPanelUI : MonoBehaviour
{
    public HeroActionHandler actionHandler;
    [Header("Kontenery (ustaw w Inspectorze)")]
    public Transform currentContainer;
    public Transform previousContainer;
    public Transform persistentContainer;

    [Header("Prefab ikony symbolu (SymbolIcon)")]
    public GameObject symbolIconPrefab;

    [Header("Mapowanie ID → Sprite")]
    public List<SymbolEntry> symbolEntries;
    private Button lastClickedButton;
    private string lastClickedSymbolId;
    private bool symbolActionInProgress = false;
    
    private List<GameObject> activeSymbolButtons = new();


    Dictionary<string, Sprite> lookup;

    // Pulka trwałych tokenów: ID → ilość
    Dictionary<string,int> persistentPool = new();
    [Header("Panel wybranego symbolu")]
    [Header("Panel Crisis Tokens")]
    public CrisisTokenUI crisisTokenUI;
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
        currentSymbolsList = new List<string>(symbols);
        Clear(currentContainer);
        foreach (var id in symbols)
            InstantiateIcon(currentContainer, id);
        UpdateCrisisTokens();
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

    var go = Instantiate(symbolIconPrefab, parent);
    activeSymbolButtons.Add(go); // ← zapamiętaj button

    var img = go.GetComponent<Image>();
    img.sprite = sprite;

    var btn = go.GetComponent<Button>();
    if (btn != null)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            if (lastClickedButton != null && lastClickedButton != btn)
                lastClickedButton.interactable = true;

            lastClickedButton = btn;
            lastClickedSymbolId = id;

            currentlySelectedImage.sprite = sprite;
            currentlySelectedImage.gameObject.SetActive(true);

            onSymbolClicked?.Invoke(id);
            actionHandler?.HandleAction(id, go); // ← przekaż button
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
private void UpdateCrisisTokens()
{
    if (crisisTokenUI != null)
        crisisTokenUI.UpdateUI(CrisisTokenManager.Instance.GetTotalCrisisTokens());
} 

private List<string> currentSymbolsList = new();


public void ConsumeAndRemoveSymbol(string id)
{
    int index = currentSymbolsList.IndexOf(id);
    if (index != -1)
    {
        currentSymbolsList.RemoveAt(index);
        Clear(currentContainer);

        foreach (var symbol in currentSymbolsList)
            InstantiateIcon(currentContainer, symbol);

        if (lastClickedSymbolId == id)
        {
            lastClickedButton = null;
            lastClickedSymbolId = null;
        }

        ClearSelectedSymbol();
    }
}



public void ReactivateLastClickedSymbol()
{
    if (lastClickedButton != null)
    {
        lastClickedButton.interactable = true;
        lastClickedButton = null;
        lastClickedSymbolId = null;
    }
}
    public void FinishSymbolAction(string id)
{
    symbolActionInProgress = false;

    if (currentSymbolsList.Contains(id))
    {
        currentSymbolsList.Remove(id);
        RefreshSymbolUI();
    }

    lastClickedButton = null;
    lastClickedSymbolId = null;
    ClearSelectedSymbol();
}
private void RefreshSymbolUI()
{
    Clear(currentContainer);
    foreach (var id in currentSymbolsList)
        InstantiateIcon(currentContainer, id);
}



}
