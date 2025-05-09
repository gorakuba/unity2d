using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public enum GamePhase { VillainTurn, Player1Turn, Player2Turn }

    // ============================================
    //               --- CONFIG ---
    // ============================================

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
    public Image villainCardFlashImage;
    public float villainCardFlashTime = 2f;

    [Header("Spawn kart")]
    public GameObject villainCardPrefab;
    public GameObject heroCardPrefab;
    public Transform[] cardSpawnPoints;

    [Header("UI gracza")]
    public HeroHandUI heroHandUI;
    public Button confirmButton;
    public Button endTurnButton;
    public Button backgroundBlocker;
    public GameObject symbolPanel;
    public GameObject selectionPanel;
    public Image selectedCardImage;

    [Header("Czasy (sekundy)")]
    public float pauseBeforeCardSpawn = 1f;
    public float pauseAfterCardSpawn = 1.5f;
    public float pauseBetweenCardEffects = 1f;
    public float phaseTextDuration = 1.5f;

    [Header("Rodzic tekstu fazy")]
    public GameObject villainPhaseContainer;
    public GameObject playerPhaseContainer;

    // ============================================
    //               --- PRIVATE STATE ---
    // ============================================

    private CardManager _cardMgr;
    private VillainController _villainController;
    private int nextPlayer = 1;
    private bool _endTurnClicked;
    private int currentSpawnIndex;
    private Sprite _pendingSelectedSprite;
    private bool cardConfirmed = false;
    private int playerTurnsCounter = 0;
    private int lastPlayerBeforeVillainTurn = 2;
    private HeroCard _pendingSelectedCard;
    private List<string> _pendingSelectedSymbols;
    private List<string> _lastSymbols;
    private VillainCard _currentVillainCard;

    // ============================================
    //               --- INIT ---
    // ============================================

    void Awake()
    {
        _cardMgr = FindAnyObjectByType<CardManager>();
        _villainController = FindAnyObjectByType<VillainController>();

        endTurnButton.onClick.AddListener(() => _endTurnClicked = true);
        confirmButton.onClick.AddListener(OnConfirmPlay);
        backgroundBlocker.onClick.AddListener(OnBackgroundClicked);

        currentSpawnIndex = 0;
    }

    public void OnPlayButtonClicked()
    {
        StartCoroutine(TurnLoop());
    }

    // ============================================
    //               --- TURN LOOP ---
    // ============================================

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
                while (BAMController.BamInProgress)
                    yield return null;

                nextPlayer = lastPlayerBeforeVillainTurn == 1 ? 2 : 1;
                currentPhase = nextPlayer == 1 ? GamePhase.Player1Turn : GamePhase.Player2Turn;
                playerTurnsCounter = 0;
            }
            else
            {
                yield return StartCoroutine(PlayerTurnSequence(nextPlayer));
                playerTurnsCounter++;

                lastPlayerBeforeVillainTurn = nextPlayer;

                if (playerTurnsCounter >= 3)
                    currentPhase = GamePhase.VillainTurn;
                else
                {
                    nextPlayer = nextPlayer == 1 ? 2 : 1;
                    currentPhase = nextPlayer == 1 ? GamePhase.Player1Turn : GamePhase.Player2Turn;
                }
            }
        }
    }

    // ============================================
    //               --- VILLAIN TURN ---
    // ============================================

    private IEnumerator VillainTurnSequence()
    {
        villainTurnUI.SetActive(true);
        yield return StartCoroutine(ShowPhaseText("VILLAIN TURN", villainPhaseContainer, villainPhaseText));

        yield return new WaitForSeconds(pauseBeforeCardSpawn);
        yield return StartCoroutine(DrawAndShowVillainCard());

        if (_villainController == null)
            _villainController = FindAnyObjectByType<VillainController>();

        if (_currentVillainCard != null)
            yield return StartCoroutine(ExecuteVillainCard(_currentVillainCard));

        villainTurnUI.SetActive(false);
    }

    private IEnumerator DrawAndShowVillainCard()
    {
        var card = _cardMgr.GetNextVillainCard();
        _currentVillainCard = card;

        if (card != null)
        {
            villainCardFlashPanel.SetActive(true);
            villainCardFlashImage.sprite = _cardMgr.GetCardSprite(card);
            yield return new WaitForSeconds(villainCardFlashTime);
            villainCardFlashPanel.SetActive(false);

            SpawnCardAtNextSlot(villainCardPrefab, villainCardFlashImage.sprite);
        }

        yield return new WaitForSeconds(pauseAfterCardSpawn);
    }

    private IEnumerator ExecuteVillainCard(VillainCard card)
    {
        List<IEnumerator> villainActions = new List<IEnumerator>();

        if (card.move > 0)
            villainActions.Add(_villainController.MoveVillain(card.move));

        if (card.BAM_effect)
            villainActions.Add(_villainController.ExecuteAttack(card));

        if (card.HasSpawn)
            villainActions.Add(_villainController.ExecuteSpawn(card));

        if (card.special)
            villainActions.Add(_villainController.ExecuteAbility(card));

        foreach (var action in villainActions)
        {
            yield return StartCoroutine(action);
            yield return new WaitForSeconds(pauseBetweenCardEffects);
        }
    }

    private IEnumerator ShowPhaseText(string text, GameObject container, TextMeshProUGUI textElement)
    {
        container.SetActive(true);
        textElement.text = text;
        textElement.gameObject.SetActive(true);

        yield return new WaitForSeconds(phaseTextDuration);

        textElement.gameObject.SetActive(false);
        container.SetActive(false);
    }

    // ============================================
    //               --- PLAYER TURN ---
    // ============================================

    private IEnumerator PlayerTurnSequence(int playerIndex)
    {
        playerTurnUI.SetActive(true);

        var drawn = _cardMgr.DrawHeroCard(playerIndex);
        if (drawn != null)
        {
            if (playerIndex == 1)
                _cardMgr.playerOneHand.Add(drawn);
            else
                _cardMgr.playerTwoHand.Add(drawn);
        }

        string heroId = playerIndex == 1 ? GameManager.Instance.playerOneHero : GameManager.Instance.playerTwoHero;
        string heroName = GameManager.Instance.GetHeroName(heroId);
        yield return StartCoroutine(ShowPhaseText($"{heroName.ToUpper()} TURN", playerPhaseContainer, playerPhaseText));

        _endTurnClicked = false;
        symbolPanel.SetActive(false);
        cardConfirmed = false;
        endTurnButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        selectionPanel.SetActive(false);
        symbolPanelUI.ClearSelectedSymbol();

        heroHandUI.ShowHand(heroId, playerIndex == 1 ? _cardMgr.playerOneHand : _cardMgr.playerTwoHand, OnPlayerCardSelected);
        yield return new WaitUntil(() => _endTurnClicked);

        heroHandUI.ClearHandUI();
        symbolPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        playerTurnUI.SetActive(false);
    }

    // ============================================
    //               --- PLAYER ACTIONS ---
    // ============================================

    private void OnPlayerCardSelected(HeroCard card)
    {
        if (cardConfirmed)
            return;

        string heroId = nextPlayer == 1 ? GameManager.Instance.playerOneHero : GameManager.Instance.playerTwoHero;
        _pendingSelectedCard = card;
        _pendingSelectedSprite = _cardMgr.GetCardSprite(heroId, card);
        _pendingSelectedSymbols = card.Symbols;

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

        if (_pendingSelectedCard != null)
        {
            var hand = nextPlayer == 1 ? _cardMgr.playerOneHand : _cardMgr.playerTwoHand;
            hand.Remove(_pendingSelectedCard);
            _pendingSelectedCard = null;
        }

        heroHandUI.ClearHandUI();
        string heroId = nextPlayer == 1 ? GameManager.Instance.playerOneHero : GameManager.Instance.playerTwoHero;
        heroHandUI.ShowHand(heroId, nextPlayer == 1 ? _cardMgr.playerOneHand : _cardMgr.playerTwoHand, OnPlayerCardSelected);

        SpawnCardAtNextSlot(heroCardPrefab, _pendingSelectedSprite);

        selectionPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        symbolPanel.SetActive(true);

        if (_lastSymbols != null && _lastSymbols.Count > 0)
            symbolPanelUI.ShowPreviousSymbols(_lastSymbols);

        if (_pendingSelectedSymbols != null && _pendingSelectedSymbols.Count > 0)
            symbolPanelUI.ShowCurrentSymbols(_pendingSelectedSymbols);

        _lastSymbols = new List<string>(_pendingSelectedSymbols);
        endTurnButton.gameObject.SetActive(true);
    }

    private void OnBackgroundClicked()
    {
        if (cardConfirmed)
            return;

        selectionPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
    }

    // ============================================
    //               --- SPAWN + UTILS ---
    // ============================================

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
        tex.SetPixels(sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height));
        tex.Apply();
        return tex;
    }
}
