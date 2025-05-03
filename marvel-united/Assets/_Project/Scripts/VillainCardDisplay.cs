using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class VillainCardDisplay : MonoBehaviour
{
    public CardManager cardManager;
    public GameManager gameManager;
    public Image villainCardImage;

    private bool initialized = false; // <- Flaga

    public async void ShowFirstCard()
    {
        if (!initialized) return; // <- Sprawdzenie, czy gotowe (dzięki Initialize)

        var card = cardManager.firstVillainCard;
        var villainId = gameManager.selectedVillain;
        if (card == null) return;

        string address = $"{villainId}/{card.id}";
        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            villainCardImage.sprite = handle.Result;
    }

    public void Initialize(GameManager gameManager, CardManager cardManager)
    {
        this.gameManager = gameManager;
        this.cardManager = cardManager;

        initialized = true;  // <- oznacz jako zainicjalizowane!

        ShowFirstCard(); // <- możesz od razu odpalić bezpiecznie
    }
}
