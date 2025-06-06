using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeroActionHandler : MonoBehaviour
{
    public MissionManager      missionManager;
    public HeroMovementManager movementManager;
    public SymbolPanelUI       symbolPanelUI;

    [Header("Token Slots")]
    public List<Transform> missionTokenSlots;
    public GameObject       civilianTokenPrefab;
    public List<Transform> thugTokenSlots;
    public GameObject       thugTokenPrefab;

    [Header("Wild Panel")]
    public GameObject wildSymbolPanel;
    public Button     wildMoveButton;
    public Button     wildHeroicButton;
    public Button     wildAttackButton;

    [Header("Global UI")]
    public Button punchUIButton;

    [Header("Threat Token Prefabs")]
    public GameObject heroicTokenPrefab;
    public GameObject attackTokenPrefab;
    public GameObject moveTokenPrefab;
    public GameObject wildTokenPrefab;

    private GameObject         pendingWildButton;
    private LocationController pendingWildLocation;

    private void ConsumePersistentIfNeeded(GameObject button)
    {
        if (button == null) return;
        var data = button.GetComponent<SymbolButtonData>();
        if (data != null && data.IsPersistent)
        {
            var hero = GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller;
            hero?.RemovePersistentSymbol(data.SymbolId);
            symbolPanelUI.UsePersistentSymbols(new List<string> { data.SymbolId });
        }
    }

    void Start()
    {
        if (punchUIButton != null)
            punchUIButton.gameObject.SetActive(false);
    }

    public void HandleAction(string symbolId, GameObject symbolButton)
    {
        if (symbolButton == null) return;

        // reset stanu
        movementManager.CancelHeroMovement();
        punchUIButton?.gameObject.SetActive(false);
        wildSymbolPanel.SetActive(false);

        var loc = movementManager.GetCurrentLocation();
        if (loc == null) return;

        // na start ukryj wszystkie specjalne przyciski
        loc.threatCardButton.gameObject.SetActive(false);
        loc.minionButton.gameObject.SetActive(false);

        // 1) Standardowy flow akcji
        switch (symbolId.ToLower())
        {
            case "move":
                loc.DisableAllActionButtons();
                movementManager.OnMoveCompleted = () =>
                {
                    ConsumePersistentIfNeeded(symbolButton);
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                    movementManager.OnMoveCompleted = null;
                    missionManager.CheckMissions();
                };
                movementManager.PrepareHeroMovement();
                break;

            case "attack":
                DoAttack(loc, symbolButton);
                break;

            case "heroic":
                DoHeroic(loc, symbolButton);
                break;

            case "wild":
                pendingWildButton   = symbolButton;
                pendingWildLocation = loc;
                loc.DisableAllActionButtons();

                wildSymbolPanel.SetActive(true);
                wildMoveButton.interactable   = true;
                wildHeroicButton.interactable = true;
                wildAttackButton.interactable = true;

                wildMoveButton.onClick.RemoveAllListeners();
                wildHeroicButton.onClick.RemoveAllListeners();
                wildAttackButton.onClick.RemoveAllListeners();

                wildMoveButton.onClick.AddListener(OnWildMove);
                wildHeroicButton.onClick.AddListener(OnWildHeroic);
                wildAttackButton.onClick.AddListener(OnWildAttack);
                return;
        }

        // 2) Dodatkowo: Enable ThreatButton i/lub MinionButton
        TryEnableThreatButton(symbolId, symbolButton, loc);
        TryEnableMinionButton(symbolId, symbolButton, loc);
    }

    private void TryEnableThreatButton(string symbolId, GameObject symbolButton, LocationController loc)
    {
        var threat = loc.threatInstance;
        if (threat == null) return;

        // matchKey: wild jako Joker
        string matchKey = symbolId.Equals("wild", StringComparison.OrdinalIgnoreCase)
            ? threat.data.required_symbols.Keys
                    .FirstOrDefault(k => threat.data.used_symbols.GetValueOrDefault(k,0)
                                        < threat.data.required_symbols[k])
            : threat.data.required_symbols.Keys
                    .FirstOrDefault(k => string.Equals(k, symbolId, StringComparison.OrdinalIgnoreCase));
        if (matchKey == null) return;

        int used = threat.data.used_symbols.GetValueOrDefault(matchKey, 0);
        int req  = threat.data.required_symbols[matchKey];
        if (used >= req) return;

        GameObject tokenPrefab = matchKey.ToLower() switch
        {
            "heroic" => heroicTokenPrefab,
            "attack" => attackTokenPrefab,
            "move"   => moveTokenPrefab,
            "wild"   => wildTokenPrefab,
            _        => null
        };
        if (tokenPrefab == null) return;

        loc.threatCardButton.gameObject.SetActive(true);
        loc.threatCardButton.onClick.RemoveAllListeners();
        loc.threatCardButton.onClick.AddListener(() =>
        {
            threat.TryPlaceSymbol(matchKey, tokenPrefab);
            loc.DisableAllActionButtons();
            symbolPanelUI.ClearSelectedSymbol();
            missionManager.CheckMissions();
            ConsumePersistentIfNeeded(symbolButton);
            bool wasPersistent = symbolButton.GetComponent<SymbolButtonData>()?.IsPersistent ?? false;
            var hero = GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller;
            if (matchKey.Equals("attack", System.StringComparison.OrdinalIgnoreCase))
                hero?.NotifyAttackUsed(false, wasPersistent);
            Destroy(symbolButton);
        });
    }

    private void TryEnableMinionButton(string symbolId, GameObject symbolButton, LocationController loc)
    {
        var threat = loc.threatInstance;
        if (threat == null || !threat.data.minion) return;

        // tylko dla ataku lub wild->attack
        bool isAttack = symbolId.Equals("attack", StringComparison.OrdinalIgnoreCase)
                     || symbolId.Equals("wild",   StringComparison.OrdinalIgnoreCase);
        if (!isAttack) return;

        loc.minionButton.gameObject.SetActive(true);
        loc.minionButton.onClick.RemoveAllListeners();
        loc.minionButton.onClick.AddListener(() =>
        {
            // Usuń token miniona z karty threata
            threat.TryRemoveMinionToken(); // implementuj w ThreatCardInstance
            loc.DisableAllActionButtons();
            symbolPanelUI.ClearSelectedSymbol();
            missionManager.CheckMissions();
            ConsumePersistentIfNeeded(symbolButton);
            bool wasPersistent = symbolButton.GetComponent<SymbolButtonData>()?.IsPersistent ?? false;
            var hero = GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller;
            hero?.NotifyAttackUsed(false, wasPersistent);
            Destroy(symbolButton);
        });
    }

    private void DoAttack(LocationController loc, GameObject symbolButton)
    {
        loc.DisableAllActionButtons();
        if (missionManager.CompletedMissionsCount >= 2 && loc.HasVillain())
        {
            punchUIButton.gameObject.SetActive(true);
            punchUIButton.onClick.RemoveAllListeners();
            punchUIButton.onClick.AddListener(() =>
            {
                HUDMessageManager.Instance?.Enqueue("Bohater zadaje obrazenia przeciwnikowi");
                VillainController.Instance.DealDamageToVillain(1);
                punchUIButton.gameObject.SetActive(false);
                loc.DisableAllActionButtons();
                ConsumePersistentIfNeeded(symbolButton);
                bool wasPersistent = symbolButton.GetComponent<SymbolButtonData>()?.IsPersistent ?? false;
                var hero = GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller;
                hero?.NotifyAttackUsed(false, wasPersistent);
                Destroy(symbolButton);
                symbolPanelUI.ClearSelectedSymbol();
                missionManager.CheckMissions();
            });
        }
        if (loc.HasThug())
            {
                loc.EnableAttackButton(() =>
                {
                    bool shouldRemoveThug = true;
                    bool defeatedThug = false;

                    // Sprawdź, czy Threat06 jest aktywny na tej lokacji
                    var threat = loc.threatInstance;
                    if (threat != null && threat.ability is RedskullThreat06 r06)
                    {
                        shouldRemoveThug = r06.RegisterAttackOnLocation(loc);
                    }

                    if (shouldRemoveThug)
                    {
                        var thug = loc.RemoveFirstThug();
                        if (thug != null)
                        {
                            defeatedThug = true;
                            HUDMessageManager.Instance?.Enqueue($"Usunieto zbira w {loc.name}");
                            if (missionManager.thugsCompleted) Destroy(thug);
                            else
                            {
                                Vector3 ws = thug.transform.lossyScale;
                                foreach (var slot in thugTokenSlots)
                                    if (slot.childCount == 0)
                                    {
                                        Vector3 ps = slot.lossyScale;
                                        thug.transform.SetParent(slot, false);
                                        thug.transform.localScale = new Vector3(ws.x / ps.x, ws.y / ps.y, ws.z / ps.z);
                                        thug.transform.localPosition = Vector3.zero;
                                        thug.transform.localRotation = Quaternion.identity;
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("[RedskullThreat06] Pierwszy atak – jeszcze nie usuwamy Thuga.");
                    }
                    ConsumePersistentIfNeeded(symbolButton);
                    bool wasPersistent = symbolButton.GetComponent<SymbolButtonData>()?.IsPersistent ?? false;
                    var hero = GameManager.Instance.CurrentPlayerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller;
                    hero?.NotifyAttackUsed(defeatedThug, wasPersistent);
                    Destroy(symbolButton);
                    loc.DisableAllActionButtons();
                    symbolPanelUI.ClearSelectedSymbol();
                    missionManager.CheckMissions();
                });
            }

    }

    private void DoHeroic(LocationController loc, GameObject symbolButton)
    {
        if (!loc.HasCivillian()) return;
        loc.DisableAllActionButtons();
        loc.EnableHeroicButton(() =>
        {
            var civ = loc.RemoveFirstCivillian();
            if (civ != null)
            {
                HUDMessageManager.Instance?.Enqueue($"Uratowano cywila w {loc.name}");
                if (missionManager.civiliansCompleted) Destroy(civ);
                else
                {
                    Vector3 ws = civ.transform.lossyScale;
                    foreach (var slot in missionTokenSlots)
                        if (slot.childCount == 0)
                        {
                            Vector3 ps = slot.lossyScale;
                            civ.transform.SetParent(slot, false);
                            civ.transform.localScale = new Vector3(ws.x / ps.x, ws.y / ps.y, ws.z / ps.z);
                            civ.transform.localPosition = Vector3.zero;
                            civ.transform.localRotation = Quaternion.identity;
                            break;
                        }
                }
            }
            ConsumePersistentIfNeeded(symbolButton);
            Destroy(symbolButton);
            loc.DisableAllActionButtons();
            symbolPanelUI.ClearSelectedSymbol();
            missionManager.CheckMissions();
        });
    }

    private void OnWildMove()
    {
        wildSymbolPanel.SetActive(false);
        pendingWildLocation.DisableAllActionButtons();
        movementManager.OnMoveCompleted = () =>
        {
            ConsumePersistentIfNeeded(pendingWildButton);
            Destroy(pendingWildButton);
            symbolPanelUI.ClearSelectedSymbol();
            movementManager.OnMoveCompleted = null;
            missionManager.CheckMissions();
            TryEnableThreatButton("move", pendingWildButton, pendingWildLocation);
            TryEnableMinionButton("move", pendingWildButton, pendingWildLocation);
        };
        movementManager.PrepareHeroMovement();
    }

    private void OnWildHeroic()
    {
        wildSymbolPanel.SetActive(false);
        DoHeroic(pendingWildLocation, pendingWildButton);
        TryEnableThreatButton("heroic", pendingWildButton, pendingWildLocation);
        TryEnableMinionButton("heroic", pendingWildButton, pendingWildLocation);
    }

    private void OnWildAttack()
    {
        wildSymbolPanel.SetActive(false);
        DoAttack(pendingWildLocation, pendingWildButton);
        TryEnableThreatButton("attack", pendingWildButton, pendingWildLocation);
        TryEnableMinionButton("attack", pendingWildButton, pendingWildLocation);
    }
}
