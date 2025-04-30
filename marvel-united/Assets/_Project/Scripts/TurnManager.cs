using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public enum GamePhase { VillainTurn, Player1Turn, Player2Turn }

    [Header("Panele fazy")]
    public GameObject playerPhasePanel;
    public GameObject villainPhasePanel;
    [Header("UI Villain Flash")]
    public GameObject villainCardFlashPanel;
    public Image villainCardFlashImage;
    public float villainCardFlashTime = 2f;

    [Header("UI")]
    public GameObject preparingUI;
    public GameObject villainTurnUI;
    public GameObject playerTurnUI;
    public TMPro.TextMeshProUGUI villainPhaseText;
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

    [Header("UI Gracza")]
    public HeroHandUI heroHandUI;
    public Button confirmButton;
    public Button endTurnButton;
    private HeroCard selectedCard;
    private int nextPlayer = 1; // 1 = G1, 2 = G2
    private bool hasCardBeenPlayedThisTurn = false;


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
        playerTurnUI.SetActive(false);
        villainTurnUI.SetActive(true);
        StartCoroutine(VillainTurnSequence());
    }

    private IEnumerator VillainTurnSequence()
    {
        currentPhase = GamePhase.VillainTurn;

        yield return ShowPhasePanel(villainPhasePanel);

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
            Sprite sprite = cardManager.GetCardSprite(card); // Requires overloaded method for VillainCard

            AutoHideImage flash = villainCardFlashPanel.GetComponent<AutoHideImage>();
            if (flash != null)
            {
                flash.ShowForDuration(sprite);
                yield return new WaitForSeconds(flash.displayTime);
            }

            SpawnCardAndReturnObject(villainCardPrefab, sprite);
        }

        yield return new WaitForSeconds(pauseAfterCardSpawn);

        if (card != null)
            yield return ExecuteVillainCardEffects(card);

        StartCoroutine(PlayerTurnSequence(nextPlayer));
    }

    private IEnumerator PlayerTurnSequence(int playerIndex)
    {
        hasCardBeenPlayedThisTurn = false;
        villainTurnUI.SetActive(false);
        playerTurnUI.SetActive(true);
        confirmButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        confirmButton.interactable = false;
        endTurnButton.interactable = false;
        selectedCard = null;

        currentPhase = (playerIndex == 1) ? GamePhase.Player1Turn : GamePhase.Player2Turn;

        string heroId = (playerIndex == 1) ? GameManager.Instance.playerOneHero : GameManager.Instance.playerTwoHero;
        HeroCardLoader loader = new HeroCardLoader();
        Hero hero = loader.LoadHeroById(heroId);
        playerPhaseText.text = $"{hero?.Name ?? heroId} Phase";
        List<HeroCard> currentHand = (playerIndex == 1) ? cardManager.playerOneHand : cardManager.playerTwoHand;
        List<HeroCard> fullDeck = loader.LoadHeroDeck(heroId);
        int drawIndex = (playerIndex == 1) ? player1CardDrawIndex : player2CardDrawIndex;
        List<GameObject> afterPanelObjects = new()
{
    heroHandUI.gameObject,
};

// ukryj je najpierw (na wszelki wypadek)
foreach (var go in afterPanelObjects)
    go.SetActive(false);

// wyświetl panel fazy, a potem te elementy
yield return ShowPhasePanel(playerPhasePanel, 1.5f, afterPanelObjects);
confirmButton.gameObject.SetActive(false);
endTurnButton.gameObject.SetActive(false);

if (drawIndex < fullDeck.Count)
{
    currentHand.Add(fullDeck[drawIndex]);

    if (playerIndex == 1) player1CardDrawIndex++;
    else player2CardDrawIndex++;
}

        
        heroHandUI.ShowHand(currentHand, OnPlayerCardSelected);

        confirmButton.interactable = false;
        endTurnButton.interactable = false;
        selectedCard = null;
    }

    private void OnPlayerCardSelected(HeroCard card)
    {
        if (hasCardBeenPlayedThisTurn) return;
        selectedCard = card;
        confirmButton.gameObject.SetActive(true);
        confirmButton.interactable = true;
    }

public void OnConfirmCardClick()
{
    if (selectedCard == null) return;

    // usuń z ręki gracza
    List<HeroCard> currentHand = (currentPhase == GamePhase.Player1Turn) ? 
        cardManager.playerOneHand : 
        cardManager.playerTwoHand;

    if (currentHand.Contains(selectedCard))
    {
        currentHand.Remove(selectedCard);
    }

    // zaktualizuj wyświetlaną rękę
    heroHandUI.ShowHand(currentHand, OnPlayerCardSelected);

    // zagraj kartę
    Sprite sprite = cardManager.GetCardSprite(selectedCard);
    SpawnCardAndReturnObject(heroCardPrefab, sprite);

    // dezaktywuj wybór
    confirmButton.gameObject.SetActive(false);
    endTurnButton.gameObject.SetActive(true);
    endTurnButton.interactable = true;
    hasCardBeenPlayedThisTurn = true;
}


public void OnEndTurnButtonClicked()
{
    playerTurnUI.SetActive(false);
    villainTurnUI.SetActive(false);

    playerTurnCounter++;

    if (currentPhase == GamePhase.Player1Turn || currentPhase == GamePhase.Player2Turn)
    {
        // co 3 tury graczy: tura Zbira
        nextPlayer = (nextPlayer == 1) ? 2 : 1;
        if (playerTurnCounter % 3 == 0)
        {
            StartVillainTurn();
        }
        else
        {
            // naprzemiennie G1 <-> G2
            StartCoroutine(PlayerTurnSequence(nextPlayer));
        }
    }
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
private IEnumerator ShowPhasePanel(GameObject panel, float duration = 1.5f, List<GameObject> objectsToEnableAfter = null)
{
    panel.SetActive(true);
    yield return new WaitForSeconds(duration);
    panel.SetActive(false);

    if (objectsToEnableAfter != null)
    {
        foreach (var go in objectsToEnableAfter)
            go.SetActive(true);
    }
}




} 
