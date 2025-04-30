using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public enum GamePhase { VillainTurn, Player1Turn, Player2Turn }
[Header("UI Villain Flash")]
public GameObject villainCardFlashPanel;
public Image villainCardFlashImage;
public float villainCardFlashTime = 2f;
    [Header("UI")]
    public GameObject preparingUI;
    public GameObject villainTurnUI;
    public GameObject playerTurnUI;
    public TMPro.TextMeshProUGUI playerPhaseText;

    [Header("Prefaby kart")]
    public GameObject villainCardPrefab;
    public GameObject heroCardPrefab;

    [Header("Miejsca na karty")]
    public Transform[] cardSpawnPoints;

    [Header("Inne komponenty")]
    public VillainCardDisplay villainCardDisplay;
    public ThreatCardTextureDatabase threatCardTextureDatabase;

    [Header("Czas trwania fazy Zbira")]
    public float pauseBeforeCardSpawn = 1f;
    public float pauseAfterCardSpawn = 1.5f;
    public float pauseBetweenCardEffects = 1f;

    private CardManager cardManager;

    private int currentCardIndex = 0;
    private int playerTurnCounter = 0;
    private GamePhase currentPhase;

    private int player1CardDrawIndex = 3;
    private int player2CardDrawIndex = 3;
    private bool isFirstVillainTurn = true;
    private void Awake()
    {
        cardManager = FindFirstObjectByType<CardManager>();
    }

    public void OnPlayButtonClicked()
    {
        preparingUI.SetActive(false);
        StartVillainTurn();
    }

    public void StartVillainTurn()
    {
        StartCoroutine(VillainTurnSequence());
    }

    private IEnumerator VillainTurnSequence()
    {
        currentPhase = GamePhase.VillainTurn;

        villainTurnUI.SetActive(true);
        
        FadeOutUI fade = villainTurnUI.GetComponent<FadeOutUI>();
        float fadeDuration = fade != null ? fade.delay + fade.duration : 1f;
        yield return new WaitForSeconds(fadeDuration);

        yield return new WaitForSeconds(pauseBeforeCardSpawn);

        VillainCard card;

if (isFirstVillainTurn)
{
    card = cardManager.firstVillainCard;
    isFirstVillainTurn = false;
}
else
{
    card = cardManager.GetNextVillainCard();
}
        if (card != null)
        {
            string villainId = GameManager.Instance.selectedVillain;
            string spritePath = $"{villainId}/{card.id}";

            var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(spritePath);
            yield return handle;

if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
{
    Sprite sprite = handle.Result;

    AutoHideImage flash = villainCardFlashPanel.GetComponent<AutoHideImage>();
    if (flash != null)
    {
        flash.ShowForDuration(sprite);
        yield return new WaitForSeconds(flash.displayTime); // poczekaj na wyświetlenie
    }

    SpawnCardAndReturnObject(villainCardPrefab, sprite);
}
        }

        yield return new WaitForSeconds(pauseAfterCardSpawn);

        if (card != null)
            yield return ExecuteVillainCardEffects(card);

        villainTurnUI.SetActive(false);
        StartCoroutine(PlayerTurnSequence(1));
    }

    private IEnumerator ExecuteVillainCardEffects(VillainCard card)
    {
        if (card.move > 0)
        {
            yield return ExecuteMove(card.move);
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }

        if (card.BAM_effect)
        {
            yield return ExecuteBAM();
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }

        if (card.special)
        {
            yield return ExecuteSpecial(card.special_name, card.special_description);
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }

        if (!string.IsNullOrEmpty(card.Location_left) || !string.IsNullOrEmpty(card.Location_middle) || !string.IsNullOrEmpty(card.Location_right))
        {
            yield return ExecuteTokenPlacement(card);
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }
    }

    private IEnumerator ExecuteMove(int steps)
    {
        Debug.Log($"🔁 Zbir porusza się o {steps} pola.");
        yield return null;
    }

    private IEnumerator ExecuteBAM()
    {
        Debug.Log("💥 Efekt BAM!");
        yield return null;
    }

    private IEnumerator ExecuteSpecial(string name, string description)
    {
        Debug.Log($"🌟 Special: {name} — {description}");
        yield return null;
    }

    private IEnumerator ExecuteTokenPlacement(VillainCard card)
    {
        Debug.Log("🎯 Umieszczanie żetonów w lokacjach:");
        if (!string.IsNullOrEmpty(card.Location_left)) Debug.Log($"- LEFT: {card.Location_left}");
        if (!string.IsNullOrEmpty(card.Location_middle)) Debug.Log($"- MIDDLE: {card.Location_middle}");
        if (!string.IsNullOrEmpty(card.Location_right)) Debug.Log($"- RIGHT: {card.Location_right}");
        yield return null;
    }

    public void StartPlayerTurn(int playerIndex)
    {
        currentPhase = (playerIndex == 1) ? GamePhase.Player1Turn : GamePhase.Player2Turn;

        var hand = (playerIndex == 1) ? cardManager.playerOneHand : cardManager.playerTwoHand;
        int drawIndex = (playerIndex == 1) ? player1CardDrawIndex : player2CardDrawIndex;

        HeroCardLoader loader = new HeroCardLoader();
        var fullDeck = loader.LoadHeroDeck((playerIndex == 1) ? GameManager.Instance.playerOneHero : GameManager.Instance.playerTwoHero);

        if (drawIndex < fullDeck.Count)
        {
            hand.Add(fullDeck[drawIndex]);

            if (playerIndex == 1) player1CardDrawIndex++;
            else player2CardDrawIndex++;
        }

        Debug.Log($"🎮 Start tury Gracza {playerIndex}. Karty na ręce: {hand.Count}");
    }

    public void OnPlayerCardPlayed(Sprite cardSprite)
    {
        SpawnCardAndReturnObject(heroCardPrefab, cardSprite);
    }

    public void OnEndTurnButtonClicked()
    {
        if (currentPhase == GamePhase.Player1Turn)
        {
            playerTurnCounter++;
            if (playerTurnCounter % 3 == 0)
                StartVillainTurn();
            else
                StartPlayerTurn(2);
        }
        else if (currentPhase == GamePhase.Player2Turn)
        {
            playerTurnCounter++;
            if (playerTurnCounter % 3 == 0)
                StartVillainTurn();
            else
                StartPlayerTurn(1);
        }
    }

    private IEnumerator NextTurnAfterDelay(GamePhase nextPhase)
    {
        yield return new WaitForSeconds(1.5f);
        if (nextPhase == GamePhase.Player1Turn)
            StartPlayerTurn(1);
    }

    private Texture2D ConvertSpriteToTexture(Sprite sprite)
    {
        if (sprite == null) return null;

        Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private GameObject SpawnCardAndReturnObject(GameObject prefab, Sprite sprite)
    {
        if (currentCardIndex >= cardSpawnPoints.Length)
        {
            Debug.LogWarning("🔴 Brak wolnych miejsc na karty!");
            return null;
        }

        var spawnPoint = cardSpawnPoints[currentCardIndex];
        GameObject card = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        card.transform.SetParent(spawnPoint, true);

        var display = card.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.frontTexture = ConvertSpriteToTexture(sprite);
            string villainId = GameManager.Instance.selectedVillain;
            display.backTexture = threatCardTextureDatabase.GetBackTexture(villainId);
            display.ApplyTextures();
        }

        currentCardIndex++;
        return card;
    }

    private IEnumerator PlayerTurnSequence(int playerIndex)
    {
        currentPhase = (playerIndex == 1) ? GamePhase.Player1Turn : GamePhase.Player2Turn;

        string heroId = (playerIndex == 1) ? GameManager.Instance.playerOneHero : GameManager.Instance.playerTwoHero;
        HeroCardLoader loader = new HeroCardLoader();
        Hero hero = loader.LoadHeroById(heroId);
        playerPhaseText.text = $"{hero?.Name ?? heroId} Phase";

        playerTurnUI.SetActive(true);
        FadeOutUI fade = playerTurnUI.GetComponent<FadeOutUI>();
        float fadeDuration = fade != null ? fade.delay + fade.duration : 1f;
        yield return new WaitForSeconds(fadeDuration);

        playerTurnUI.SetActive(false);
        StartPlayerTurn(playerIndex);
    }
}
