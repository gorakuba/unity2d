using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroActionHandler : MonoBehaviour
{
    public MissionManager missionManager;
    public HeroMovementManager movementManager;
    public SymbolPanelUI       symbolPanelUI;

    // Civillian tokens
    public List<Transform> missionTokenSlots;
    public GameObject      civilianTokenPrefab;

    // Thug tokens
    public List<Transform> thugTokenSlots;
    public GameObject      thugTokenPrefab;

    // Wild symbol panel
    public GameObject wildSymbolPanel;
    public Button     wildMoveButton;
    public Button     wildHeroicButton;
    public Button     wildAttackButton;

    // Internal state for wild handling
    private GameObject         pendingWildButton;
    private LocationController pendingWildLocation;

    public void HandleAction(string symbolId, GameObject symbolButton)
    {
        if (symbolButton == null) return;
        movementManager.CancelHeroMovement();

        var loc = movementManager.GetCurrentLocation();
        if (loc == null) return;

        switch (symbolId.ToLower())
        {
            case "move":
                wildSymbolPanel.SetActive(false);
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
                wildSymbolPanel.SetActive(false);
                if (!loc.HasThug()) return;
                DisableAllLocationButtons();
                loc.EnableAttackButton(() =>
                {
                    var thug = loc.RemoveFirstThug();
                    if (thug != null)
                    {
                        Vector3 ws = thug.transform.lossyScale;
                        foreach (var slot in thugTokenSlots)
                        {
                            if (slot != null && slot.childCount == 0)
                            {
                                thug.transform.SetParent(slot, false);
                                thug.transform.localPosition = Vector3.zero;
                                thug.transform.localRotation = Quaternion.identity;
                                Vector3 ps = slot.lossyScale;
                                thug.transform.localScale = new Vector3(ws.x / ps.x, ws.y / ps.y, ws.z / ps.z);
                                break;
                            }
                        }
                    }
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                    missionManager.CheckMissions();
                });
                break;

            case "heroic":
                wildSymbolPanel.SetActive(false);
                if (!loc.HasCivillian()) return;
                DisableAllLocationButtons();
                loc.EnableHeroicButton(() =>
                {
                    var civ = loc.RemoveFirstCivillian();
                    if (civ != null)
                    {
                        Vector3 ws = civ.transform.lossyScale;
                        foreach (var slot in missionTokenSlots)
                        {
                            if (slot != null && slot.childCount == 0)
                            {
                                civ.transform.SetParent(slot, false);
                                civ.transform.localPosition = Vector3.zero;
                                civ.transform.localRotation = Quaternion.identity;
                                Vector3 ps = slot.lossyScale;
                                civ.transform.localScale = new Vector3(ws.x / ps.x, ws.y / ps.y, ws.z / ps.z);
                                break;
                            }
                        }
                    }
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                    missionManager.CheckMissions();
                });
                break;

            case "wild":
                // zapamiętujemy dziki symbol i lokację
                pendingWildButton   = symbolButton;
                pendingWildLocation = loc;

                // wyłączamy wszystkie przyciski na bieżącej lokacji
                loc.DisableMoveButton();
                loc.DisableHeroicButton();
                loc.DisableAttackButton();

                // otwieramy panel Wild
                wildSymbolPanel.SetActive(true);

                // resetujemy interaktywność przycisków wewnątrz panelu Wild
                wildMoveButton.interactable   = true;
                wildHeroicButton.interactable = true;
                wildAttackButton.interactable = true;

                // podłączamy callbacki
                wildMoveButton.onClick.RemoveAllListeners();
                wildHeroicButton.onClick.RemoveAllListeners();
                wildAttackButton.onClick.RemoveAllListeners();

                wildMoveButton.onClick.AddListener(OnWildMove);
                wildHeroicButton.onClick.AddListener(OnWildHeroic);
                wildAttackButton.onClick.AddListener(OnWildAttack);
                break;
        }
    }

    private void OnWildMove()
    {
        wildSymbolPanel.SetActive(false);
        pendingWildLocation.DisableHeroicButton();
        pendingWildLocation.DisableAttackButton();
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
        // validation: tylko jeśli jest civillian
        if (!pendingWildLocation.HasCivillian())
        {
            Debug.LogWarning("🚫 Brak Civillian — Wild Heroic anulowany");
            //Destroy(pendingWildButton);
            //symbolPanelUI.ClearSelectedSymbol();
            return;
        }

        pendingWildLocation.DisableMoveButton();
        pendingWildLocation.DisableAttackButton();

        pendingWildLocation.EnableHeroicButton(() =>
        {
            var civ = pendingWildLocation.RemoveFirstCivillian();
            if (civ != null)
            {
                Vector3 ws = civ.transform.lossyScale;
                foreach (var slot in missionTokenSlots)
                {
                    if (slot != null && slot.childCount == 0)
                    {
                        civ.transform.SetParent(slot, false);
                        civ.transform.localPosition = Vector3.zero;
                        civ.transform.localRotation = Quaternion.identity;
                        Vector3 ps = slot.lossyScale;
                        civ.transform.localScale = new Vector3(ws.x / ps.x, ws.y / ps.y, ws.z / ps.z);
                        break;
                    }
                }
            }
            Destroy(pendingWildButton);
            symbolPanelUI.ClearSelectedSymbol();
            missionManager.CheckMissions();
        });
    }

    private void OnWildAttack()
    {
        wildSymbolPanel.SetActive(false);
        // validation: tylko jeśli jest thug
        if (!pendingWildLocation.HasThug())
        {
            Debug.LogWarning("🚫 Brak Thug — Wild Attack anulowany");
            //Destroy(pendingWildButton);
            //symbolPanelUI.ClearSelectedSymbol();
            return;
        }

        pendingWildLocation.DisableMoveButton();
        pendingWildLocation.DisableHeroicButton();

        pendingWildLocation.EnableAttackButton(() =>
        {
            var thug = pendingWildLocation.RemoveFirstThug();
            if (thug != null)
            {
                Vector3 ws = thug.transform.lossyScale;
                foreach (var slot in thugTokenSlots)
                {
                    if (slot != null && slot.childCount == 0)
                    {
                        thug.transform.SetParent(slot, false);
                        thug.transform.localPosition = Vector3.zero;
                        thug.transform.localRotation = Quaternion.identity;
                        Vector3 ps = slot.lossyScale;
                        thug.transform.localScale = new Vector3(ws.x / ps.x, ws.y / ps.y, ws.z / ps.z);
                        break;
                    }
                }
            }
            Destroy(pendingWildButton);
            symbolPanelUI.ClearSelectedSymbol();
            missionManager.CheckMissions();
        });
    }

    /// <summary>
    /// Wyłącza Move, Heroic, Attack na każdej lokacji w scenie.
    /// </summary>
private void DisableAllLocationButtons()
{
    var all = Object.FindObjectsByType<LocationController>(
        FindObjectsInactive.Include,
        FindObjectsSortMode.None
    );
    foreach (var lc in all)
    {
        lc.DisableMoveButton();
        lc.DisableHeroicButton();
        lc.DisableAttackButton();
    }
}

}
