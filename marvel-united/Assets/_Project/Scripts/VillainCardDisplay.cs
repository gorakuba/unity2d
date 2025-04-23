using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class VillainCardDisplay : MonoBehaviour
{
    public CardManager cardManager;
    public GameManager gameManager;
    public Image villainCardImage;

    public async void ShowFirstCard()
    {
        var card = cardManager.firstVillainCard;
        var villainId = gameManager.selectedVillain;
        if (card == null) return;

        string address = $"{villainId}/{card.id}";
        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            villainCardImage.sprite = handle.Result;
    }
}
