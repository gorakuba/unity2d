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
    public GameObject civilianTokenPrefab;

    public void HandleAction(string symbolId, GameObject symbolButton)
    {
        Debug.Log($"▶ [HandleAction] Kliknięto symbol: {symbolId} | Button={(symbolButton != null ? symbolButton.name : "NULL")}");
        Debug.Log($"[HandleAction] symbolId={symbolId}, symbolButton={symbolButton}");

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

        // --- Obsługa Threat Card ---
        var threat = loc.threatInstance;

        if (threat == null)
        {
            Debug.Log("⚠️ Brak threatInstance w lokacji");
        }
        else if (threat.data == null)
        {
            Debug.Log("⚠️ Threat istnieje, ale brak danych (data == null)");
        }
        else
        {
            Debug.Log($"🧩 Threat {threat.data.id} przypisany do lokacji {loc.name}");

            if (threat.data.required_symbols.ContainsKey(symbolId))
            {
                Debug.Log($"✅ Threat wymaga symbolu: {symbolId}");

                GameObject symbolPrefab = symbolPanelUI.GetSymbolPrefab(symbolId);
                if (symbolPrefab == null)
                {
                    Debug.LogError($"❌ Prefab symbolu `{symbolId}` jest null! Nie przypisany w SymbolPanelUI.");
                    return;
                }

                if (symbolPrefab != null)
                {
                    Debug.Log("✅ Pobrano prefab symbolu, aktywuję przycisk");

                    loc.EnableThreatCardButton(symbolId, symbolPrefab, threat, symbolButton);
                    symbolPanelUI.ClearSelectedSymbol(); // tylko schowaj zaznaczenie

                    return;
                }
                else
                {
                    Debug.LogWarning($"❌ Brak przypisanego prefab dla symbolu: {symbolId}");
                }
            }
            else
            {
                Debug.Log($"ℹ️ Threat NIE wymaga symbolu: {symbolId}");
            }
        }

        // --- Standardowe symbole
        switch (symbolId.ToLower())
        {
            case "move":
                Debug.Log("🚶‍♂️ Aktywacja ruchu");
                movementManager.OnMoveCompleted = () =>
                {
                    Debug.Log("✅ Ruch zakończony, usuwam symbol");
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                    movementManager.OnMoveCompleted = null;
                };
                movementManager.PrepareHeroMovement();
                break;

            case "attack":
                Debug.Log("⚔️ Atak!");
                Destroy(symbolButton);
                symbolPanelUI.ClearSelectedSymbol();
                break;

             case "heroic":
                Debug.Log("⭐ Akcja heroiczna!");
                if (!loc.HasCivillian())
                {
                    Debug.LogWarning("🚫 Brak civillian na lokacji");
                    break;
                }

                loc.EnableHeroicButton(() =>
                {
                    // Pobierz i odczep token z lokacji
                    var civ = loc.RemoveFirstCivillian();
                    if (civ == null)
                    {
                        Debug.LogWarning("⚠️ Nie udało się znaleźć civillian do usunięcia");
                        return;
                    }

                    // Zapisz bieżącą światową skalę tokena
                    Vector3 worldScale = civ.transform.lossyScale;

                    // Wstaw token do pierwszego wolnego slotu i zachowaj rozmiar
                    bool placed = false;
                    foreach (var slot in missionTokenSlots)
                    {
                        if (slot == null) continue;
                        if (slot.childCount == 0)
                        {
                            civ.transform.SetParent(slot, false);
                            civ.transform.localPosition = Vector3.zero;
                            civ.transform.localRotation = Quaternion.identity;

                            // Oblicz i ustaw lokalną skalę, by zachować worldScale
                            Vector3 parentScale = slot.lossyScale;
                            civ.transform.localScale = new Vector3(
                                worldScale.x / parentScale.x,
                                worldScale.y / parentScale.y,
                                worldScale.z / parentScale.z
                            );
                            Debug.Log($"✅ Civillian dodany do {slot.name} z zachowaniem skali");
                            placed = true;
                            break;
                        }
                    }
                    if (!placed)
                        Debug.LogWarning("⚠️ Brak wolnych slotów do dodania civilian token");

                    // Usuń symbol i odznacz w UI
                    Destroy(symbolButton);
                    symbolPanelUI.ClearSelectedSymbol();
                });
                break;


            case "wild":
                Debug.Log("🎲 Dziki symbol — domyślna akcja");
                Destroy(symbolButton);
                symbolPanelUI.ClearSelectedSymbol();
                break;

            default:
                Debug.LogWarning($"❓ Nieznany symbol: {symbolId}");
                break;
        }
    }
}
