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

    // Tu przechowujemy referencję do aktualnej karty threat
    public ThreatCardInstance threatInstance;

    private System.Action<LocationController> onMoveCallback;

    public void EnableMoveButton(System.Action<LocationController> callback)
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

    public void EnableHeroicButton(System.Action onClick)
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
        return CountCivillians() > 0;
    }

    public GameObject RemoveFirstCivillian()
    {
        for (int i = 0; i <= 4; i++)
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

    public int CountCivillians()
    {
        int count = 0;
        for (int i = 0; i <= 4; i++)
        {
            var slot = transform.Find($"Slot_{i}");
            if (slot != null && slot.childCount > 0 && slot.GetChild(0).name.Contains("Civillian"))
                count++;
        }
        return count;
    }

    public Transform GetFirstCivillianSlot()
    {
        for (int i = 0; i <= 4; i++)
        {
            var slot = transform.Find($"Slot_{i}");
            if (slot != null && slot.childCount > 0 && slot.GetChild(0).name.Contains("Civillian"))
                return slot;
        }
        return null;
    }

    /// <summary>
    /// Wywoływane z ThreatCardSpawner, przypisuje instancję threat do tej lokacji
    /// </summary>
    public void AssignThreatCard(ThreatCardInstance card)
    {
        threatInstance = card;
        card.assignedLocation = this.gameObject;
    }

    public void EnableThreatCardButton(string symbolId,
                                       GameObject symbolPrefab,
                                       ThreatCardInstance threat,
                                       GameObject symbolButton)
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
