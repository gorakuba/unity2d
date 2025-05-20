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
    public Button punchUIButton; // przypnij swój PunchButton z UI

    private GameObject pendingWildButton;
    private LocationController pendingWildLocation;

    void Start()
    {
        if (punchUIButton != null)
            punchUIButton.gameObject.SetActive(false);
    }

    public void HandleAction(string symbolId, GameObject symbolButton)
    {
        if (symbolButton == null) return;
        
        if (punchUIButton != null)
            punchUIButton.gameObject.SetActive(false);

        movementManager.CancelHeroMovement();
        var loc = movementManager.GetCurrentLocation();
        if (loc == null) return;

        wildSymbolPanel.SetActive(false);

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
                DisableAllLocationButtons();
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
        }
    }

    private void DoAttack(LocationController loc, GameObject symbolButton)
    {
        DisableAllLocationButtons();

        // Punch?
        if (missionManager.CompletedMissionsCount >= 2 && loc.HasVillain())
        {
            punchUIButton.gameObject.SetActive(true);
            punchUIButton.onClick.RemoveAllListeners();
            punchUIButton.onClick.AddListener(() =>
            {
                VillainController.Instance.DealDamageToVillain(1);
                punchUIButton.gameObject.SetActive(false);
                DisableAllLocationButtons();
                Destroy(symbolButton);
                symbolPanelUI.ClearSelectedSymbol();
                missionManager.CheckMissions();
            });
        }

        // Thug attack
        if (loc.HasThug())
        {
            loc.EnableAttackButton(() =>
            {
                punchUIButton.gameObject.SetActive(false);
                var thug = loc.RemoveFirstThug();
                if (thug != null)
                {
                    // jeśli misja thugów ukończona → niszcz
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
            });
        }
    }

    private void DoHeroic(LocationController loc, GameObject symbolButton)
    {
        if (!loc.HasCivillian()) return;
        DisableAllLocationButtons();
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
        });
    }

    // --- Wild callbacks delegate to the same routines above ---

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
        };
        movementManager.PrepareHeroMovement();
    }

    private void OnWildHeroic()
    {
        wildSymbolPanel.SetActive(false);
        DoHeroic(pendingWildLocation, pendingWildButton);
    }

    private void OnWildAttack()
    {
        wildSymbolPanel.SetActive(false);
        DoAttack(pendingWildLocation, pendingWildButton);
    }

    private void DisableAllLocationButtons()
    {
        var all = Object.FindObjectsByType<LocationController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var lc in all)
            lc.DisableAllActionButtons();
    }
}
