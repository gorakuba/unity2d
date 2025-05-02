using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public enum GamePhase { VillainTurn, Player1Turn, Player2Turn }
    [Header("Symbol Panel")]
    public GameObject symbolPanel; 
    public SymbolPanelUI symbolPanelUI;

    // przechowujemy symbole poprzedniej karty drugiego gracza
    private List<string> lastPlayedSymbolsP1 = new();
    private List<string> lastPlayedSymbolsP2 = new();

    [Header("Panele fazy")]
    public GameObject playerPhasePanel;
    public GameObject villainPhasePanel;

    [Header("UI Villain Flash")]
    public GameObject villainCardFlashPanel;
    public Image villainCardFlashImage;
    public float villainCardFlashTime = 2f;

    [Header("UI główne")]
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
    public HeroHandUI heroHandUI;

    [Header("UI Gracza")]
    public Button confirmButton;
    public Button endTurnButton;

    [Header("Selection UI")]
    public GameObject selectionPanel;
    public Image selectedImage;
    public Button backgroundBlocker;

    [Header("Czasy faz")]
    public float pauseBeforeCardSpawn = 1f;
    public float pauseAfterCardSpawn = 1.5f;
    public float pauseBetweenCardEffects = 1f;

    private CardManager cardManager;
    private int currentCardIndex = 0;
    private int playerTurnCounter = 0;
    private GamePhase currentPhase;
    private bool isFirstVillainTurn = true;
    private int nextPlayer = 1; // 1 = Gracz1, 2 = Gracz2
    private HeroCard selectedCard;
    private bool hasCardBeenPlayedThisTurn = false;

    private void Awake()
    {
        cardManager = FindFirstObjectByType<CardManager>();
        backgroundBlocker.onClick.AddListener(OnBackgroundClicked);
        symbolPanelUI.onSymbolClicked += OnSymbolUsed;
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

        // Pokaż panel fazy Zbira
        yield return ShowPhasePanel(villainPhasePanel);

        yield return new WaitForSeconds(pauseBeforeCardSpawn);

        VillainCard card = isFirstVillainTurn
            ? cardManager.firstVillainCard
            : cardManager.GetNextVillainCard();
        isFirstVillainTurn = false;

        if (card != null)
        {
            Sprite sprite = cardManager.GetCardSprite(card);
            var flash = villainCardFlashPanel.GetComponent<AutoHideImage>();
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
        // --- RESET WSZYSTKIEGO ---
        selectionPanel.SetActive(false);
        backgroundBlocker.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        confirmButton.interactable = false;
        endTurnButton.interactable = false;
        hasCardBeenPlayedThisTurn = false;
        selectedCard = null;
        villainTurnUI.SetActive(false);
        playerTurnUI.SetActive(true);

        // **WYŁĄCZ HAND UI zanim pojawi się panel fazy**
        heroHandUI.gameObject.SetActive(false);

        // ustaw aktualną fazę
        currentPhase = (playerIndex == 1) ? GamePhase.Player1Turn : GamePhase.Player2Turn;

        // przygotuj tekst fazy
        string heroId = (playerIndex == 1)
            ? GameManager.Instance.playerOneHero
            : GameManager.Instance.playerTwoHero;
        HeroCardLoader loader = new HeroCardLoader();
        Hero hero = loader.LoadHeroById(heroId);
        playerPhaseText.text = $"{hero?.Name ?? heroId} Phase";

        // Pokaż panel fazy i rękę
        List<GameObject> afterPanel = new() { heroHandUI.gameObject };
        yield return ShowPhasePanel(playerPhasePanel, 1.5f, afterPanel);

        // Dobierz nową kartę bez powtórzeń
        List<HeroCard> hand = (playerIndex == 1)
            ? cardManager.playerOneHand
            : cardManager.playerTwoHand;
        HeroCard newCard = cardManager.DrawHeroCard(playerIndex);
        if (newCard != null)
            hand.Add(newCard);

         heroHandUI.ShowHand(heroId, hand, OnPlayerCardSelected);
    }

private void OnPlayerCardSelected(HeroCard card)
{
    if (hasCardBeenPlayedThisTurn) 
        return;

    selectedCard = card;

    // Wyciągamy heroId tylko raz
    string heroId = (currentPhase == GamePhase.Player1Turn)
        ? GameManager.Instance.playerOneHero
        : GameManager.Instance.playerTwoHero;

    // Wczytujemy sprite
    selectedImage.sprite = cardManager.GetCardSprite(heroId, card);

    // Pokaż panel selekcji + blocker
    selectionPanel.SetActive(true);
    backgroundBlocker.gameObject.SetActive(true);

    // Odblokuj Confirm
    confirmButton.gameObject.SetActive(true);
    confirmButton.interactable = true;
}

public void OnConfirmCardClick()
{
    if (selectedCard == null) 
        return;

    hasCardBeenPlayedThisTurn = true;
    // 0) upewnij się, że panel jest aktywny
    symbolPanel.SetActive(true);

    // 1) Wybierz odpowiednią rękę i heroId
    List<HeroCard> hand = (currentPhase == GamePhase.Player1Turn)
        ? cardManager.playerOneHand
        : cardManager.playerTwoHand;

    string heroId = (currentPhase == GamePhase.Player1Turn)
        ? GameManager.Instance.playerOneHero
        : GameManager.Instance.playerTwoHero;

    // 2) Usuń kartę z ręki i odśwież UI
    hand.Remove(selectedCard);
    heroHandUI.ShowHand(heroId, hand, OnPlayerCardSelected);

    // 3) Zagraj kartę na stole
    Sprite sprite = cardManager.GetCardSprite(heroId, selectedCard);
    SpawnCardAndReturnObject(heroCardPrefab, sprite);

    // ────────────────
    // 4) SYMBOL PANEL
    // 4a) Current symbols
    var currentSymbols = selectedCard.Symbols;
    symbolPanelUI.ShowCurrentSymbols(currentSymbols);

    // 4b) Previous symbols (ostatnie zagrane przez drugiego gracza)
    var previous = (currentPhase == GamePhase.Player1Turn)
        ? lastPlayedSymbolsP2
        : lastPlayedSymbolsP1;
    symbolPanelUI.ShowPreviousSymbols(previous);

    symbolPanel.SetActive(true);

    // // 4c) Persistent symbols (ze specjalnych zdolności)
    // if (selectedCard.Special)
    // {
    //     // zakładam, że masz List<string> SpecialSymbols w HeroCard
    //     symbolPanelUI.AddPersistentSymbols(selectedCard.SpecialSymbols);
    // }

    // 4d) Zapamiętaj co teraz zagrał TEN gracz
    if (currentPhase == GamePhase.Player1Turn)
        lastPlayedSymbolsP1 = new List<string>(currentSymbols);
    else
        lastPlayedSymbolsP2 = new List<string>(currentSymbols);
    // ────────────────

    // 5) Schowaj selekcję, pokaż EndTurn
    selectionPanel.SetActive(false);
    backgroundBlocker.gameObject.SetActive(false);
    confirmButton.gameObject.SetActive(false);

    endTurnButton.gameObject.SetActive(true);
    endTurnButton.interactable = true;

    selectedCard = null;
}



    public void OnEndTurnButtonClicked()
    {
        symbolPanel.SetActive(false);
        playerTurnUI.SetActive(false);
        villainTurnUI.SetActive(false);
        symbolPanelUI.currentlySelectedImage.sprite = null;

        playerTurnCounter++;
        nextPlayer = (nextPlayer == 1) ? 2 : 1;

        if (playerTurnCounter % 3 == 0)
            StartVillainTurn();
        else
            StartCoroutine(PlayerTurnSequence(nextPlayer));
    }

    private IEnumerator ExecuteVillainCardEffects(VillainCard card)
    {
        if (card.move > 0)
        {
            Debug.Log($"🔁 Zbir porusza się o {card.move} pola.");
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }
        if (card.BAM_effect)
        {
            Debug.Log("💥 Efekt BAM!");
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }
        if (card.special)
        {
            Debug.Log($"🌟 Special: {card.special_name}");
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }
        if (!string.IsNullOrEmpty(card.Location_left) ||
            !string.IsNullOrEmpty(card.Location_middle) ||
            !string.IsNullOrEmpty(card.Location_right))
        {
            Debug.Log("🎯 Umieszczanie żetonów…");
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }
    }

    private GameObject SpawnCardAndReturnObject(GameObject prefab, Sprite sprite)
    {
        if (currentCardIndex >= cardSpawnPoints.Length)
        {
            Debug.LogWarning("🔴 Brak miejsc na karty!");
            return null;
        }

        Transform spawn = cardSpawnPoints[currentCardIndex++];
        GameObject go = Instantiate(prefab, spawn.position, spawn.rotation);
        go.transform.SetParent(spawn, true);

        var display = go.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.frontTexture = ConvertSpriteToTexture(sprite);
            display.backTexture = threatCardTextureDatabase.GetBackTexture(
                GameManager.Instance.selectedVillain);
            display.ApplyTextures();
        }

        return go;
    }

    private IEnumerator ShowPhasePanel(
        GameObject panel,
        float duration = 1.5f,
        List<GameObject> toEnable = null)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(duration);
        panel.SetActive(false);

        if (toEnable != null)
            toEnable.ForEach(go => go.SetActive(true));
    }

    private void OnBackgroundClicked()
    {
        selectionPanel.SetActive(false);
        backgroundBlocker.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        selectedCard = null;
    }

    private Texture2D ConvertSpriteToTexture(Sprite sprite)
    {
        var tex = new Texture2D(
            (int)sprite.rect.width,
            (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
    private void OnSymbolUsed(string symbolId)
{
    Debug.Log("Użyto symbolu: " + symbolId);
    // tu dorzuć logikę ruchu / ataku / czegokolwiek
}
}
