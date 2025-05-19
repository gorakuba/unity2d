using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroActionHandler : MonoBehaviour
{
    public HeroMovementManager movementManager;
    public SymbolPanelUI symbolPanelUI;

    public List<Transform> missionTokenSlots;
    public GameObject      civilianTokenPrefab;

    public List<Transform> thugTokenSlots;
    public GameObject      thugTokenPrefab;

    public void HandleAction(string symbolId, GameObject symbolButton)
    {
        Debug.Log($"▶ [HandleAction] Kliknięto symbol: {symbolId}");

        if (symbolButton == null)
        {
            Debug.LogError("❌ symbolButton jest null w HandleAction!");
            return;
        }

        movementManager.CancelHeroMovement();
        var loc = movementManager.GetCurrentLocation();
        if (loc == null)
        {
            Debug.LogError("❌ Nie znaleziono lokacji gracza");
            return;
        }

        // ===== Threat Card =====
        var threat = loc.threatInstance;
        if (threat != null && threat.data != null && threat.data.required_symbols.ContainsKey(symbolId))
        {
            var prefab = symbolPanelUI.GetSymbolPrefab(symbolId);
            loc.EnableThreatCardButton(symbolId, prefab, threat, symbolButton);
            symbolPanelUI.ClearSelectedSymbol();
            return;
        }

        // ===== Standard Symbols =====
        switch (symbolId.ToLower())
        {
            case "move":
                movementManager.OnMoveCompleted = () =>
                {
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                    movementManager.OnMoveCompleted = null;
                };
                movementManager.PrepareHeroMovement();
                break;

            case "attack":
                Debug.Log("⚔️ Atak!");
                // nie ma Thug → symbol nie przepada, tylko ostrzeżenie
                if (!loc.HasThug())
                {
                    Debug.LogWarning("🚫 Brak Thug na lokacji — symbol ataku pozostaje");
                    break;
                }
                // włączamy przycisk potwierdzenia ataku
                loc.EnableAttackButton(() =>
                {
                    // 1) ściągamy Thuga
                    var thug = loc.RemoveFirstThug();
                    if (thug == null)
                    {
                        Debug.LogWarning("⚠️ Nie znaleziono Thug do zniszczenia");
                        return;
                    }
                    // 2) przenosimy Thuga do pierwszego wolnego slotu
                    foreach (var slot in thugTokenSlots)
                    {
                        if (slot != null && slot.childCount == 0)
                        {
                            var scale = thug.transform.lossyScale;
                            thug.transform.SetParent(slot, false);
                            thug.transform.localPosition = Vector3.zero;
                            thug.transform.localRotation = Quaternion.identity;
                            // zachowaj skalę
                            var pScale = slot.lossyScale;
                            thug.transform.localScale = new Vector3(
                                scale.x / pScale.x,
                                scale.y / pScale.y,
                                scale.z / pScale.z
                            );
                            break;
                        }
                    }
                    // 3) zużywamy symbol i odznaczamy
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                });
                break;

            case "heroic":
                Debug.Log("⭐ Akcja heroiczna!");
                if (!loc.HasCivillian())
                {
                    Debug.LogWarning("🚫 Brak Civillian na lokacji — symbol heroic pozostaje");
                    break;
                }
                loc.EnableHeroicButton(() =>
                {
                    var civ = loc.RemoveFirstCivillian();
                    if (civ == null) return;

                    var scale = civ.transform.lossyScale;
                    foreach (var slot in missionTokenSlots)
                    {
                        if (slot != null && slot.childCount == 0)
                        {
                            civ.transform.SetParent(slot, false);
                            civ.transform.localPosition = Vector3.zero;
                            civ.transform.localRotation = Quaternion.identity;
                            var pScale = slot.lossyScale;
                            civ.transform.localScale = new Vector3(
                                scale.x / pScale.x,
                                scale.y / pScale.y,
                                scale.z / pScale.z
                            );
                            break;
                        }
                    }
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                });
                break;

            case "wild":
                Destroy(symbolButton);
                symbolPanelUI.ClearSelectedSymbol();
                break;

            default:
                Debug.LogWarning($"❓ Nieznany symbol: {symbolId}");
                break;
        }
    }
}
