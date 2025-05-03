using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public enum GamePhase { VillainTurn, Player1Turn, Player2Turn }

    [Header("Symbol Panel UI")]
    public SymbolPanelUI symbolPanelUI;

    [Header("Panele faz")]
    public GameObject villainTurnUI;
    public GameObject playerTurnUI;

    [Header("Teksty faz (TMP + FadeOutAlpha)")]
    public TextMeshProUGUI villainPhaseText;
    public TextMeshProUGUI playerPhaseText;

    [Header("Flash karty zbira")]
    public GameObject villainCardFlashPanel;
    public Image     villainCardFlashImage;
    public float     villainCardFlashTime = 2f;

    [Header("Spawn kart")]
    public GameObject villainCardPrefab;
    public GameObject heroCardPrefab;
    public Transform[] cardSpawnPoints;

    [Header("UI gracza")]
    public HeroHandUI heroHandUI;
    public Button     confirmButton;
    public Button     endTurnButton;
    public Button     backgroundBlocker;
    public GameObject symbolPanel;
    public GameObject selectionPanel;
    public Image      selectedCardImage;

    [Header("Czasy (sekundy)")]
    public float pauseBeforeCardSpawn    = 1f;
    public float pauseAfterCardSpawn     = 1.5f;
    public float pauseBetweenCardEffects = 1f;
    public float phaseTextDuration       = 1.5f;
    [Header("Rodzic tekstu fazy")]
    public GameObject villainPhaseContainer;  // przeciągnij tu GameObject "VillainTurn/Text"
    public GameObject playerPhaseContainer;   // przeciągnij tu GameObject "PlayerTurn/Text"
    private CardManager       _cardMgr;
    private VillainController _villainController;
    private int               nextPlayer = 1;
    private bool              _endTurnClicked;
    private int               currentSpawnIndex;
    private Sprite            _pendingSelectedSprite;
    private bool cardConfirmed = false; // Flaga wskazująca czy karta została potwierdzona
    private int playerTurnsCounter = 0; // ile tur graczy minęło
    private int lastPlayerBeforeVillainTurn = 2;
    private HeroCard _pendingSelectedCard;
    private List<string> _pendingSelectedSymbols;
    private List<string> _lastSymbols;

    void Awake()
    {
        _cardMgr           = FindAnyObjectByType<CardManager>();
        _villainController = FindAnyObjectByType<VillainController>();

        endTurnButton.onClick.AddListener(() => _endTurnClicked = true);
        confirmButton.onClick.AddListener(OnConfirmPlay);
        backgroundBlocker.onClick.AddListener(OnBackgroundClicked);

        // Reset indeksu tylko raz przy starcie
        currentSpawnIndex = 0;
    }

    public void OnPlayButtonClicked()
    {
        StartCoroutine(TurnLoop());
    }

    private IEnumerator VillainTurnSequence()
    {
        villainTurnUI.SetActive(true);

        villainPhaseContainer.SetActive(true);
        villainPhaseText.gameObject.SetActive(true);
        yield return new WaitForSeconds(phaseTextDuration);

        villainPhaseText.gameObject.SetActive(false);
        villainPhaseContainer.SetActive(false);

        // Flash tekstu fazy
        villainPhaseText.text = "VILLAIN TURN";
        villainPhaseText.gameObject.SetActive(true);
        yield return new WaitForSeconds(phaseTextDuration);
        villainPhaseText.gameObject.SetActive(false);

        yield return new WaitForSeconds(pauseBeforeCardSpawn);

        // Dobór i flash karty
        var card = _cardMgr.GetNextVillainCard();
        if (card != null)
        {
            villainCardFlashPanel.SetActive(true);
            villainCardFlashImage.sprite = _cardMgr.GetCardSprite(card);
            yield return new WaitForSeconds(villainCardFlashTime);
            villainCardFlashPanel.SetActive(false);

            SpawnCardAtNextSlot(villainCardPrefab, villainCardFlashImage.sprite);
        }

        yield return new WaitForSeconds(pauseAfterCardSpawn);

        // Efekty z pauzami
        if (_villainController == null)
            _villainController = FindAnyObjectByType<VillainController>();

        if (card != null)
        {
            if (card.move > 0)
            {
                yield return StartCoroutine(_villainController.MoveVillain(card.move));
                yield return new WaitForSeconds(pauseBetweenCardEffects);
            }
            if (card.BAM_effect)
            {
                yield return StartCoroutine(_villainController.ExecuteAttack(card));
                yield return new WaitForSeconds(pauseBetweenCardEffects);
            }
            if (card.HasSpawn)
            {
                yield return StartCoroutine(_villainController.ExecuteSpawn(card));
                yield return new WaitForSeconds(pauseBetweenCardEffects);
            }
            if (card.special)
            {
                yield return StartCoroutine(_villainController.ExecuteAbility(card));
                yield return new WaitForSeconds(pauseBetweenCardEffects);
            }
        }

        villainTurnUI.SetActive(false);
    }

    private IEnumerator PlayerTurnSequence(int playerIndex)
    {
        playerTurnUI.SetActive(true);

            // --- 1) DOBIERANIE KARTY NA POCZĄTKU ---
    var drawn = _cardMgr.DrawHeroCard(playerIndex);
    if (drawn != null)
    {
        // dodaj do listy rąk
        if (playerIndex == 1) 
            _cardMgr.playerOneHand.Add(drawn);
        else 
            _cardMgr.playerTwoHand.Add(drawn);
    }


        // Flash tekstu fazy z ID bohatera
        string heroId = playerIndex == 1
            ? GameManager.Instance.playerOneHero
            : GameManager.Instance.playerTwoHero;
        string heroName = GameManager.Instance.GetHeroName(heroId);
        playerPhaseText.text = $"{heroName.ToUpper()} TURN";
        playerPhaseContainer.SetActive(true);
        playerPhaseText.gameObject.SetActive(true);
        yield return new WaitForSeconds(phaseTextDuration);
        playerPhaseText.gameObject.SetActive(false);
        playerPhaseContainer.SetActive(false);

        // Przygotuj UI gracza
        _endTurnClicked = false;
        symbolPanel.SetActive(false);
        cardConfirmed = false;
        endTurnButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        selectionPanel.SetActive(false);
        symbolPanelUI.ClearSelectedSymbol();

        // Pokaż rękę i czekaj wybór
        heroHandUI.ShowHand(
            heroId,
            playerIndex == 1 ? _cardMgr.playerOneHand : _cardMgr.playerTwoHand,
            OnPlayerCardSelected
        );
        yield return new WaitUntil(() => _endTurnClicked);

        // Sprzątanie UI gracza
        heroHandUI.ClearHandUI();
        symbolPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        playerTurnUI.SetActive(false);
    }

    private void OnPlayerCardSelected(HeroCard card)
    {
        if (cardConfirmed)
            return; // Jeśli karta już została potwierdzona, ignoruj kolejne wybory

        // Zapisz sprite na później
        string heroId = nextPlayer == 1
            ? GameManager.Instance.playerOneHero
            : GameManager.Instance.playerTwoHero;
                    _pendingSelectedCard  = card;
        _pendingSelectedSprite = _cardMgr.GetCardSprite(heroId, card);

        // 2) ZAPISZ TUTAJ liste symboli:
        _pendingSelectedSymbols = card.Symbols;  // uwaga: to jest List<string>

        selectedCardImage.sprite = _pendingSelectedSprite;
        selectedCardImage.preserveAspect = true;

        selectionPanel.SetActive(true);
        confirmButton.gameObject.SetActive(true);

        symbolPanel.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
    }

private void OnConfirmPlay()
{
    cardConfirmed = true;

    // 1) usuń kartę z ręki i odśwież UI ręki
    if (_pendingSelectedCard != null)
    {
        var hand = nextPlayer == 1 
            ? _cardMgr.playerOneHand 
            : _cardMgr.playerTwoHand;
        hand.Remove(_pendingSelectedCard);
        _pendingSelectedCard = null;
    }
    heroHandUI.ClearHandUI();
    string heroId = nextPlayer == 1 
        ? GameManager.Instance.playerOneHero 
        : GameManager.Instance.playerTwoHero;
    heroHandUI.ShowHand(
        heroId,
        nextPlayer == 1 
            ? _cardMgr.playerOneHand 
            : _cardMgr.playerTwoHand,
        OnPlayerCardSelected
    );

    // 2) spawn bohatera
    SpawnCardAtNextSlot(heroCardPrefab, _pendingSelectedSprite);

    // 3) zamknij selekcję
    selectionPanel.SetActive(false);
    confirmButton.gameObject.SetActive(false);

    // 4) otwórz panel symboli
    symbolPanel.SetActive(true);

    // 5) najpierw pokaż to, co było poprzednio
    if (_lastSymbols != null && _lastSymbols.Count > 0)
    {
        symbolPanelUI.ShowPreviousSymbols(_lastSymbols);
    }

    // 6) teraz pokaż bieżące symbole
    if (_pendingSelectedSymbols != null && _pendingSelectedSymbols.Count > 0)
    {
        symbolPanelUI.ShowCurrentSymbols(_pendingSelectedSymbols);
    }
    else
    {
        Debug.LogWarning("Brak symboli do pokazania!");
    }

    // 7) zapisz bieżące jako przyszłe „poprzednie”
    _lastSymbols = new List<string>(_pendingSelectedSymbols);

    // 8) włącz przycisk końca tury
    endTurnButton.gameObject.SetActive(true);
}

    private void OnBackgroundClicked()
    {
        if (cardConfirmed)
            return; // Jeśli karta już została potwierdzona, nie pozwalaj już nic zmieniać

        selectionPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
    }

    private void SpawnCardAtNextSlot(GameObject prefab, Sprite sprite)
    {
        if (cardSpawnPoints == null || cardSpawnPoints.Length == 0)
        {
            Debug.LogError("SpawnCardAtNextSlot: brak punktów spawnów!");
            return;
        }

        if (currentSpawnIndex >= cardSpawnPoints.Length)
            currentSpawnIndex = 0;

        Transform spawn = cardSpawnPoints[currentSpawnIndex++];
        GameObject go = Instantiate(prefab, spawn.position + new Vector3(0, 0.0003f, 0), spawn.rotation, spawn);
        var disp = go.GetComponent<CardDisplay>();
        if (disp != null)
        {
            disp.frontTexture = ConvertSpriteToTexture(sprite);
            disp.ApplyTextures();
        }
    }

    private Texture2D ConvertSpriteToTexture(Sprite sprite)
    {
        if (sprite == null) return null;
        var tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        tex.SetPixels(sprite.texture.GetPixels(
            (int)sprite.rect.x, (int)sprite.rect.y,
            (int)sprite.rect.width, (int)sprite.rect.height));
        tex.Apply();
        return tex;
    }
    private IEnumerator TurnLoop()
{
    GamePhase currentPhase = GamePhase.VillainTurn;
    nextPlayer = 1; 
    playerTurnsCounter = 0;

    while (true)
    {
        if (currentPhase == GamePhase.VillainTurn)
        {
            yield return StartCoroutine(VillainTurnSequence());
            
            // Ustaw kolejnego gracza po villainie poprawnie
            nextPlayer = lastPlayerBeforeVillainTurn == 1 ? 2 : 1;

            currentPhase = nextPlayer == 1 ? GamePhase.Player1Turn : GamePhase.Player2Turn;
            playerTurnsCounter = 0;
        }
        else
        {
            yield return StartCoroutine(PlayerTurnSequence(nextPlayer));
            playerTurnsCounter++;

            // Zapisz ostatniego gracza przed Villainem
            lastPlayerBeforeVillainTurn = nextPlayer;

            if (playerTurnsCounter >= 3)
            {
                currentPhase = GamePhase.VillainTurn;
            }
            else
            {
                nextPlayer = nextPlayer == 1 ? 2 : 1;
                currentPhase = nextPlayer == 1 ? GamePhase.Player1Turn : GamePhase.Player2Turn;
            }
        }
    }
}

    
}
