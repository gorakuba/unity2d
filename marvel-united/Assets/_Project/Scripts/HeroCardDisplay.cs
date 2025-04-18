using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class HeroCardDisplay : MonoBehaviour
{
    public CardManager cardManager;
    public GameManager gameManager;
    public Image card1Image, card2Image, card3Image;
    public bool isPlayerTwo = false;

    public async void ShowCards()
    {
        var heroId = isPlayerTwo ? gameManager.playerTwoHero : gameManager.playerOneHero;
        var hand = isPlayerTwo ? cardManager.playerTwoHand : cardManager.playerOneHand;

        if (hand == null || hand.Count < 3) return;

        await LoadSprite(card1Image, heroId, hand[0].Id);
        await LoadSprite(card2Image, heroId, hand[1].Id);
        await LoadSprite(card3Image, heroId, hand[2].Id);
    }

    private async System.Threading.Tasks.Task LoadSprite(Image img, string heroId, string cardId)
{
    string address = $"{heroId}/{cardId}";
    Debug.Log($"🧭 Ładowanie sprite: {address}");

    var handle = Addressables.LoadAssetAsync<Sprite>(address);
    await handle.Task;

    if (handle.Status == AsyncOperationStatus.Succeeded)
    {
        img.sprite = handle.Result;
        Debug.Log("✅ Sprite załadowany: " + address);
    }
    else
    {
        Debug.LogError("❌ Nie udało się załadować sprite: " + address);
    }
}

}
