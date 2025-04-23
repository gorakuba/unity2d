using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VillainDashboardImagesetter : MonoBehaviour
{
    [System.Serializable]
    public class VillainImageEntry
    {
        public string villainId; // np. "red_skull"
        public Sprite cardSprite;
    }

    public List<VillainImageEntry> villainImages; // przypisujesz przez Inspector
    public Image cardImageTarget; // obiekt typu UI Image (czyli Card_1)

    private void Start()
    {
        string selectedVillain = GameManager.Instance.selectedVillain.ToLower();

        foreach (var entry in villainImages)
        {
            if (entry.villainId.ToLower() == selectedVillain)
            {
                cardImageTarget.sprite = entry.cardSprite;
                Debug.Log($"Podmieniono sprite karty na {entry.cardSprite.name} dla {selectedVillain}");
                return;
            }
        }

        Debug.LogWarning($"Brak sprite’a dla {selectedVillain}");
    }
}
