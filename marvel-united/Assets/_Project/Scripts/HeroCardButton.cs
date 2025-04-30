using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HeroCardButton : MonoBehaviour
{
    private HeroCard cardData;
    private System.Action<HeroCard> callback;

    public Image image; // przypisz w prefabie (Image na tym samym GameObject)

    public void Setup(HeroCard card, System.Action<HeroCard> onClick)
    {
        cardData = card;
        callback = onClick;

        gameObject.SetActive(false);
        // Załaduj sprite przez CardManager
        string path = FindFirstObjectByType<CardManager>().GetSpritePathForCard(card);

        var handle = Addressables.LoadAssetAsync<Sprite>(path);
        handle.Completed += (AsyncOperationHandle<Sprite> h) =>
        {
            if (h.Status == AsyncOperationStatus.Succeeded)
            {
                image.sprite = h.Result;
                gameObject.SetActive(true);
            }
            else
                Debug.LogWarning($"❌ Sprite nie znaleziony: {path}");
        };

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => callback?.Invoke(cardData));
    }
    
}
