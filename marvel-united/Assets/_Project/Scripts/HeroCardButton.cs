using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HeroCardButton : MonoBehaviour
{
    private HeroCard cardData;
    private System.Action<HeroCard> callback;

    public Image image; // podłącz w prefabie

    /// <summary>
    /// heroId: identyfikator bohatera, np. "iron_man"
    /// card: obiekt HeroCard
    /// onClick: callback, gdy gracz kliknie w tę kartę
    /// </summary>
    public void Setup(string heroId, HeroCard card, System.Action<HeroCard> onClick)
    {
        cardData = card;
        callback = onClick;

        // Ukryj do czasu załadowania tekstury
        gameObject.SetActive(false);

        // Pobierz ścieżkę i ładuj sprite
        var cardManager = FindFirstObjectByType<CardManager>();
        string path = cardManager.GetSpritePathForCard(heroId, cardData);
        var handle = Addressables.LoadAssetAsync<Sprite>(path);

        handle.Completed += (AsyncOperationHandle<Sprite> h) =>
        {
            if (h.Status == AsyncOperationStatus.Succeeded)
            {
                image.sprite = h.Result;
                gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"❌ Nie znaleziono sprite'a pod kluczem `{path}`");
            }
        };

        // Podłącz callback przycisku
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => callback?.Invoke(cardData));
    }
}
