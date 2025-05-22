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

    [Header("Wild")]
    public GameObject wildSymbolPanel;
    public Button     wildMoveButton;
    public Button     wildHeroicButton;
    public Button     wildAttackButton;

    [Header("Punch (global UI)")]
    public Button punchUIButton;

    [Header("Threat Token Prefabs")]
    public GameObject heroicTokenPrefab;
    public GameObject attackTokenPrefab;
    public GameObject moveTokenPrefab;
    public GameObject wildTokenPrefab;

    private GameObject         pendingWildButton;
    private LocationController pendingWildLocation;

    void Start()
    {
        if (punchUIButton != null)
            punchUIButton.gameObject.SetActive(false);
    }

    public void HandleAction(string symbolId, GameObject symbolButton)
    {
        if (symbolButton == null) return;

        // reset state
        movementManager.CancelHeroMovement();
        if (punchUIButton != null)
            punchUIButton.gameObject.SetActive(false);
        wildSymbolPanel.SetActive(false);

        var loc = movementManager.GetCurrentLocation();
        if (loc == null) return;

        // hide threat button initially
        loc.threatCardButton.gameObject.SetActive(false);

        // 1) existing Move/Attack/Heroic/Wild flow
        switch (symbolId.ToLower())
        {
            case "move":
                loc.DisableAllActionButtons();
                movementManager.OnMoveCompleted = () =>
                {
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                    movementManager.OnMoveCompleted = null;
                    missionManager.CheckMissions();
                    loc.threatCardButton.gameObject.SetActive(false);
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
                break;

            default:
                Debug.LogWarning($"Unknown symbolId '{symbolId}'");
                break;
        }

        // 2) additionally, if a ThreatCard here needs this symbol, enable ThreatButton
        var threat = loc.threatInstance;
        if (threat != null)
        {
            // select correct token prefab
            GameObject tokenPrefab = symbolId.ToLower() switch
            {
                "heroic" => heroicTokenPrefab,
                "attack" => attackTokenPrefab,
                "move"   => moveTokenPrefab,
                "wild"   => wildTokenPrefab,
                _         => null
            };

            // check requirement
            if (tokenPrefab != null
                && threat.data.required_symbols.TryGetValue(symbolId, out int req)
                && threat.data.used_symbols.GetValueOrDefault(symbolId, 0) < req)
            {
                loc.threatCardButton.gameObject.SetActive(true);
                loc.threatCardButton.onClick.RemoveAllListeners();
                loc.threatCardButton.onClick.AddListener(() =>
                {
                    threat.TryPlaceSymbol(symbolId, tokenPrefab);
                    // standard cleanup
                    loc.DisableAllActionButtons();
                    symbolPanelUI.ClearSelectedSymbol();
                    missionManager.CheckMissions();
                    Destroy(symbolButton);
                });
            }
        }
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
                VillainController.Instance.DealDamageToVillain(1);
                punchUIButton.gameObject.SetActive(false);
                loc.DisableAllActionButtons();
                Destroy(symbolButton);
                symbolPanelUI.ClearSelectedSymbol();
                missionManager.CheckMissions();
            });
        }

        if (loc.HasThug())
        {
            loc.EnableAttackButton(() =>
            {
                var thug = loc.RemoveFirstThug();
                if (thug != null)
                {
                    if (missionManager.thugsCompleted)
                        Destroy(thug);
                    else
                    {
                        Vector3 ws = thug.transform.lossyScale;
                        foreach (var slot in thugTokenSlots)
                        {
                            if (slot.childCount == 0)
                            {
                                Vector3 ps = slot.lossyScale;
                                thug.transform.SetParent(slot, false);
                                thug.transform.localPosition = Vector3.zero;
                                thug.transform.localRotation = Quaternion.identity;
                                thug.transform.localScale = new Vector3(
                                    ws.x/ps.x, ws.y/ps.y, ws.z/ps.z
                                );
                                break;
                            }
                        }
                    }
                }
                Destroy(symbolButton);
                symbolPanelUI.ClearSelectedSymbol();
                missionManager.CheckMissions();
                loc.threatCardButton.gameObject.SetActive(false);
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
                if (missionManager.civiliansCompleted)
                    Destroy(civ);
                else
                {
                    Vector3 ws = civ.transform.lossyScale;
                    foreach (var slot in missionTokenSlots)
                    {
                        if (slot.childCount == 0)
                        {
                            Vector3 ps = slot.lossyScale;
                            civ.transform.SetParent(slot, false);
                            civ.transform.localPosition = Vector3.zero;
                            civ.transform.localRotation = Quaternion.identity;
                            civ.transform.localScale = new Vector3(
                                ws.x/ps.x, ws.y/ps.y, ws.z/ps.z
                            );
                            break;
                        }
                    }
                }
            }
            Destroy(symbolButton);
            symbolPanelUI.ClearSelectedSymbol();
            missionManager.CheckMissions();
            loc.threatCardButton.gameObject.SetActive(false);
        });
    }

    private void OnWildMove()
    {
        wildSymbolPanel.SetActive(false);
        pendingWildLocation.DisableAllActionButtons();
        movementManager.OnMoveCompleted = () =>
        {
            Destroy(pendingWildButton);
            symbolPanelUI.ClearSelectedSymbol();
            movementManager.OnMoveCompleted = null;
            missionManager.CheckMissions();
            pendingWildLocation.threatCardButton.gameObject.SetActive(false);
        };
        movementManager.PrepareHeroMovement();
    }

    private void OnWildHeroic() => DoHeroic(pendingWildLocation, pendingWildButton);
    private void OnWildAttack() => DoAttack(pendingWildLocation, pendingWildButton);
}