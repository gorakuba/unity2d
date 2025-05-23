using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationController : MonoBehaviour
{
    public List<LocationController> neighbors;

    // — MOVE —
    public Button moveButton;
    private Action<LocationController> onMoveCallback;

    // — HEROIC / ATTACK / THREAT —
    public Button heroicButton;
    public Button attackButton;
    public Button threatCardButton;
    public Button minionButton;
    public ThreatCardInstance threatInstance;

    // — SLOTY —
    public Transform GetHeroSlot(int playerIndex)
        => transform.Find(playerIndex == 1 ? "Hero_Slot_1" : "Hero_Slot_2");

    // ================================
    // MOVE
    // ================================
    public void EnableMoveButton(Action<LocationController> callback)
    {
        DisableAllActionButtons();
        if (moveButton == null) return;
        moveButton.gameObject.SetActive(true);
        onMoveCallback = callback;
        moveButton.onClick.RemoveAllListeners();
        moveButton.onClick.AddListener(() => onMoveCallback?.Invoke(this));
    }

    public void DisableMoveButton()
    {
        if (moveButton == null) return;
        moveButton.onClick.RemoveAllListeners();
        moveButton.gameObject.SetActive(false);
    }

    // ================================
    // HEROIC
    // ================================
    public void EnableHeroicButton(Action onClick)
    {
        DisableAllActionButtons();
        if (heroicButton == null) return;
        heroicButton.gameObject.SetActive(true);
        heroicButton.onClick.RemoveAllListeners();
        heroicButton.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            heroicButton.gameObject.SetActive(false);
        });
    }

    public void DisableHeroicButton()
    {
        if (heroicButton == null) return;
        heroicButton.onClick.RemoveAllListeners();
        heroicButton.gameObject.SetActive(false);
    }

    // ================================
    // ATTACK (ściąganie Thugów)
    // ================================
    public void EnableAttackButton(Action onClick)
    {
        DisableAllActionButtons();
        if (attackButton == null)
        {
            Debug.LogError("attackButton nie przypisany!");
            return;
        }
        attackButton.gameObject.SetActive(true);
        attackButton.onClick.RemoveAllListeners();
        attackButton.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            attackButton.gameObject.SetActive(false);
        });
    }

    public void DisableAttackButton()
    {
        if (attackButton == null) return;
        attackButton.onClick.RemoveAllListeners();
        attackButton.gameObject.SetActive(false);
    }

    // ================================
    // THREAT CARD
    // ================================
    public void AssignThreatCard(ThreatCardInstance card)
    {
        threatInstance = card;
        card.assignedLocation = gameObject;
    }

    public void EnableThreatCardButton(string symbolId, GameObject symbolPrefab,
                                       ThreatCardInstance threat, GameObject symbolButton)
    {
        DisableAllActionButtons();
        if (threatCardButton == null) return;
        threatCardButton.gameObject.SetActive(true);
        threatCardButton.onClick.RemoveAllListeners();
        threatCardButton.onClick.AddListener(() =>
        {
            threat.TryPlaceSymbol(symbolId, symbolPrefab);
            threatCardButton.gameObject.SetActive(false);
            Destroy(symbolButton);
        });
    }

    // ================================
    // TOKENS: Civilians & Thugs
    // ================================
    public bool HasCivillian()
    {
        for (int i = 0; i < 5; i++)
        {
            var s = transform.Find($"Slot_{i}");
            if (s != null && s.childCount > 0 && s.GetChild(0).name.Contains("Civillian"))
                return true;
        }
        return false;
    }

    public GameObject RemoveFirstCivillian()
    {
        for (int i = 0; i < 5; i++)
        {
            var s = transform.Find($"Slot_{i}");
            if (s != null && s.childCount > 0)
            {
                var t = s.GetChild(0).gameObject;
                if (t.name.Contains("Civillian"))
                {
                    t.transform.SetParent(null);
                    return t;
                }
            }
        }
        return null;
    }

    public bool HasThug()
    {
        for (int i = 0; i < 5; i++)
        {
            var s = transform.Find($"Slot_{i}");
            if (s != null && s.childCount > 0 && s.GetChild(0).name.Contains("Thug"))
                return true;
        }
        return false;
    }

    public GameObject RemoveFirstThug()
    {
        for (int i = 0; i < 5; i++)
        {
            var s = transform.Find($"Slot_{i}");
            if (s != null && s.childCount > 0)
            {
                var t = s.GetChild(0).gameObject;
                if (t.name.Contains("Thug"))
                {
                    t.transform.SetParent(null);
                    return t;
                }
            }
        }
        return null;
    }

    // ================================
    // VILLAIN DETECTION
    // ================================
    public bool HasVillain()
    {
        var slot = transform.Find("Villain_Slot");
        return slot != null && slot.childCount > 0;
    }

    // ================================
    // UTILITY
    // ================================
    /// <summary>
    /// Wyłącza WSZYSTKIE przyciski akcji na tej lokacji.
    /// </summary>
    public void DisableAllActionButtons()
    {
        moveButton?.gameObject.SetActive(false);
        heroicButton?.gameObject.SetActive(false);
        attackButton?.gameObject.SetActive(false);
        threatCardButton?.gameObject.SetActive(false);
        minionButton?.gameObject.SetActive(false);
    }
}
