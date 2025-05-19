using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationController : MonoBehaviour
{
    public List<LocationController> neighbors;
    public Button moveButton;
    public GameObject moveParticles;

    public Button heroicButton;
    public Button threatCardButton;
    public Button attackButton;

    // Referencja do aktualnej karty Threat
    public ThreatCardInstance threatInstance;

    private Action<LocationController> onMoveCallback;

    public void EnableMoveButton(Action<LocationController> callback)
    {
        if (moveButton == null) return;
        moveButton.gameObject.SetActive(true);
        if (moveParticles != null) moveParticles.SetActive(true);
        onMoveCallback = callback;
        moveButton.onClick.RemoveAllListeners();
        moveButton.onClick.AddListener(() => onMoveCallback?.Invoke(this));
    }

    public void DisableMoveButton()
    {
        if (moveButton == null) return;
        moveButton.onClick.RemoveAllListeners();
        moveButton.gameObject.SetActive(false);
        if (moveParticles != null) moveParticles.SetActive(false);
    }

    public Transform GetHeroSlot(int playerIndex)
    {
        return transform.Find(playerIndex == 1 ? "Hero_Slot_1" : "Hero_Slot_2");
    }

    // --- Heroic ---

    public void EnableHeroicButton(Action onClick)
    {
        if (heroicButton == null) return;
        heroicButton.gameObject.SetActive(true);
        heroicButton.onClick.RemoveAllListeners();
        heroicButton.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            heroicButton.gameObject.SetActive(false);
        });
    }

    public bool HasCivillian()
    {
        for (int i = 0; i < 5; i++)
        {
            var slot = transform.Find($"Slot_{i}");
            if (slot != null && slot.childCount > 0 && slot.GetChild(0).name.Contains("Civillian"))
                return true;
        }
        return false;
    }

    public GameObject RemoveFirstCivillian()
    {
        for (int i = 0; i < 5; i++)
        {
            var slot = transform.Find($"Slot_{i}");
            if (slot != null && slot.childCount > 0)
            {
                var token = slot.GetChild(0).gameObject;
                if (token.name.Contains("Civillian"))
                {
                    token.transform.SetParent(null);
                    return token;
                }
            }
        }
        return null;
    }

    // --- Attack ---

    public void EnableAttackButton(Action onClick)
    {
        if (attackButton == null)
        {
            Debug.LogError("❌ attackButton nie przypisany w LocationController!");
            return;
        }

        attackButton.gameObject.SetActive(true);
        attackButton.onClick.RemoveAllListeners();
        attackButton.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            attackButton.gameObject.SetActive(false);
        });
        Debug.Log("✅ AttackButton aktywowany");
    }

    public bool HasThug()
    {
        for (int i = 0; i < 5; i++)
        {
            var slot = transform.Find($"Slot_{i}");
            if (slot != null && slot.childCount > 0 && slot.GetChild(0).name.Contains("Thug"))
                return true;
        }
        return false;
    }

    public GameObject RemoveFirstThug()
    {
        for (int i = 0; i < 5; i++)
        {
            var slot = transform.Find($"Slot_{i}");
            if (slot != null && slot.childCount > 0)
            {
                var token = slot.GetChild(0).gameObject;
                if (token.name.Contains("Thug"))
                {
                    token.transform.SetParent(null);
                    return token;
                }
            }
        }
        return null;
    }

    // --- Threat Card ---

    public void AssignThreatCard(ThreatCardInstance card)
    {
        threatInstance = card;
        card.assignedLocation = gameObject;
    }

    public void EnableThreatCardButton(string symbolId, GameObject symbolPrefab, ThreatCardInstance threat, GameObject symbolButton)
    {
        if (threatCardButton == null)
        {
            Debug.LogError("❌ ThreatCardButton nie przypisany w inspektorze!");
            return;
        }

        threatCardButton.gameObject.SetActive(true);
        threatCardButton.onClick.RemoveAllListeners();
        threatCardButton.onClick.AddListener(() =>
        {
            threat.TryPlaceSymbol(symbolId, symbolPrefab);
            threatCardButton.gameObject.SetActive(false);
            Destroy(symbolButton);
            Debug.Log($"🟡 Kliknięto ThreatCardButton → dodano {symbolId}");
        });

        Debug.Log("✅ ThreatCardButton aktywowany");
    }
}
