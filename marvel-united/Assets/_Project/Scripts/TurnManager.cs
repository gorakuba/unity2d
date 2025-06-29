using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public event Action<HeroController> OnStartHeroTurn;
    public enum GamePhase { VillainTurn, Player1Turn, Player2Turn }

    // ============================================
    //               --- CONFIG ---
    // ============================================
    public MissionManager missionManager;

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
    public Button specialAbilityButton;
    public Button backgroundBlocker;
    public GameObject symbolPanel;
    public GameObject selectionPanel;
    public Image selectedCardImage;

    [Header("Czasy (sekundy)")]
    public float pauseBeforeCardSpawn = 1f;
    public float pauseAfterCardSpawn = 1.5f;
    public float pauseBetweenCardEffects = 2f;
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
    private string pendingSpecialAbilityId;
    private int currentSpawnIndex;
    private Sprite _pendingSelectedSprite;
    private bool cardConfirmed = false;
    private int playerTurnsCounter = 0;
    private int lastPlayerBeforeVillainTurn = 2;
    private HeroCard _pendingSelectedCard;
    private List<string> _pendingSelectedSymbols;
    private List<string> _lastSymbols;
    private VillainCard _currentVillainCard;
    private Sprite lastCardPlayer1Sprite;
    private Sprite lastCardPlayer2Sprite;
    private bool missionBonusScheduled = false;
    private bool pendingBonusP1 = false;
    private bool pendingBonusP2 = false;

    public List<HeroCard> PlayerOneStoryline { get; private set; } = new();
    public List<GameObject> PlayerOneStorylineObjects { get; private set; } = new();
    public List<HeroCard> PlayerTwoStoryline { get; private set; } = new();
    public List<GameObject> PlayerTwoStorylineObjects { get; private set; } = new();

    // ============================================
    //               --- INIT ---
    // ============================================

    void Awake()
    {
        Instance = this;
        _cardMgr = FindAnyObjectByType<CardManager>();
        _villainController = FindAnyObjectByType<VillainController>();

                endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmPlay);
        backgroundBlocker.onClick.AddListener(OnBackgroundClicked);
        if (specialAbilityButton != null)
        {
            specialAbilityButton.gameObject.SetActive(false);
        }

        currentSpawnIndex = 0;
    }

    private void OnEndTurnButtonClicked()
    {
        _endTurnClicked = true;
        DisableLocationButtons();
        if (specialAbilityButton != null)
        {
            specialAbilityButton.gameObject.SetActive(false);
            pendingSpecialAbilityId = null;
        }
    }

    private void DisableLocationButtons()
    {
        var locMan = GameManager.Instance?.locationManager;
        if (locMan != null)
        {
            foreach (var t in locMan.LocationRoots)
            {
                var ctrl = t.GetComponent<LocationController>();
                if (ctrl != null)
                    ctrl.DisableAllActionButtons();
            }
        }

        var moveMgr = FindAnyObjectByType<HeroMovementManager>();
        moveMgr?.CancelHeroMovement();
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
        GameManager.Instance.CurrentPlayerIndex = nextPlayer;
        playerTurnsCounter = 0;

        while (true)
        {

            if (!missionBonusScheduled && missionManager.CompletedMissionsCount == 3)
            {
                missionBonusScheduled = true;
                pendingBonusP1 = pendingBonusP2 = true;
            }
            if (currentPhase == GamePhase.VillainTurn)
            {
                yield return StartCoroutine(VillainTurnSequence());
                while (BAMController.BamInProgress)
                    yield return null;

                nextPlayer = lastPlayerBeforeVillainTurn == 1 ? 2 : 1;
                currentPhase = nextPlayer == 1 ? GamePhase.Player1Turn : GamePhase.Player2Turn;
                GameManager.Instance.CurrentPlayerIndex = nextPlayer;
                playerTurnsCounter = 0;
            }
            else
            {
                yield return StartCoroutine(PlayerTurnSequence(nextPlayer));
                playerTurnsCounter++;

                lastPlayerBeforeVillainTurn = nextPlayer;
                int maxPlayerTurns = missionManager.CompletedMissionsCount > 0 ? 2 : 3;

                if (playerTurnsCounter >= maxPlayerTurns)
                    currentPhase = GamePhase.VillainTurn;
                else
                {
                    nextPlayer = nextPlayer == 1 ? 2 : 1;
                    currentPhase = nextPlayer == 1 ? GamePhase.Player1Turn : GamePhase.Player2Turn;
                    GameManager.Instance.CurrentPlayerIndex = nextPlayer;
                }
            }
        }
    }

    // ============================================
    //               --- VILLAIN TURN ---
    // ============================================

    private IEnumerator VillainTurnSequence()
    {
        var lastPanel = LastCardsPanelUI.Instance;
        if (lastPanel != null)
            lastPanel.Hide();
        if (CameraManager.Instance != null)
            CameraManager.Instance.FocusVillain();
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
            if (HUDMessageManager.Instance != null)
                yield return HUDMessageManager.Instance.ShowAndWait("Przeciwnik zagrywa karte");
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
        if (CameraManager.Instance != null)
            CameraManager.Instance.FocusHero(playerIndex);
        var heroController = playerIndex == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller;
        if (heroController != null && heroController.IsStunned)
        {
            heroController.IsStunned = false;
            for (int i = 0; i < 3; i++)
            {
                var extra = _cardMgr.DrawHeroCard(playerIndex);
                if (extra != null)
                {
                    if (playerIndex == 1)
                        _cardMgr.playerOneHand.Add(extra);
                    else
                        _cardMgr.playerTwoHand.Add(extra);
                }
            }
        }

        var drawn = _cardMgr.DrawHeroCard(playerIndex);
        if (drawn != null)
        {
            if (playerIndex == 1)
                _cardMgr.playerOneHand.Add(drawn);
            else
                _cardMgr.playerTwoHand.Add(drawn);
        }

        if (missionBonusScheduled)
        {
            if ((playerIndex == 1 && pendingBonusP1) ||
                (playerIndex == 2 && pendingBonusP2))
            {
                var bonus = _cardMgr.DrawHeroCard(playerIndex);
                if (bonus != null)
                {
                    if (playerIndex == 1) _cardMgr.playerOneHand.Add(bonus);
                    else _cardMgr.playerTwoHand.Add(bonus);
                }

                if (playerIndex == 1) pendingBonusP1 = false;
                else pendingBonusP2 = false;
            }
        }

        string heroId = playerIndex == 1 ? GameManager.Instance.playerOneHero : GameManager.Instance.playerTwoHero;
        string heroName = GameManager.Instance.GetHeroName(heroId);
        var heroGO = GameManager.Instance.FindObjectInScene(heroId);
        var hero = FindHeroById(heroId);
        if (hero == null)
        {
            Debug.LogError($"TurnManager: nie znalazłem HeroController o HeroId='{heroId}'");
        }
        else
        {
            OnStartHeroTurn?.Invoke(hero);
            // Refresh persistent symbol display for the active hero
            symbolPanelUI.SetPersistentSymbols(new List<string>(hero.PersistentSymbols));
            if (hero.IsStunned)
            {
                for (int i = 0; i < 3; i++)
                {
                    var extra = _cardMgr.DrawHeroCard(playerIndex);
                    if (extra != null)
                    {
                        if (playerIndex == 1) _cardMgr.playerOneHand.Add(extra);
                        else _cardMgr.playerTwoHand.Add(extra);
                    }
                }
                hero.RecoverStun();
            }
        }

        var lastPanel = LastCardsPanelUI.Instance;
        if (lastPanel != null)
            lastPanel.Hide();

        yield return StartCoroutine(ShowPhaseText($"{heroName.ToUpper()} TURN", playerPhaseContainer, playerPhaseText));

        if (lastPanel != null)
        {
            Sprite current = playerIndex == 1 ? lastCardPlayer2Sprite : lastCardPlayer1Sprite;
            Sprite previous = playerIndex == 1 ? lastCardPlayer1Sprite : lastCardPlayer2Sprite;
            if (current != null)
                lastPanel.Show(current, previous);
        }

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
        
        if (hero != null)
            yield return StartCoroutine(HandleLocationEndTurnAbility(hero));
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
        string abilityId = null;
        bool isSpecial = false;

        HeroCard playedCard = _pendingSelectedCard;
        if (playedCard != null)
        {
            abilityId = playedCard.SpecialAbility;
            isSpecial = playedCard.Special;
            var hand = nextPlayer == 1 ? _cardMgr.playerOneHand : _cardMgr.playerTwoHand;
            hand.Remove(playedCard);
            _pendingSelectedCard = null;
        }

        heroHandUI.ClearHandUI();
        string heroId = nextPlayer == 1 ? GameManager.Instance.playerOneHero : GameManager.Instance.playerTwoHero;
        heroHandUI.ShowHand(heroId, nextPlayer == 1 ? _cardMgr.playerOneHand : _cardMgr.playerTwoHand, OnPlayerCardSelected);

        GameObject spawned = SpawnCardAtNextSlot(heroCardPrefab, _pendingSelectedSprite);
        if (spawned != null && playedCard != null)
        {
            var inst = spawned.GetComponent<HeroCardInstance>() ?? spawned.AddComponent<HeroCardInstance>();
            inst.card = playedCard;
            inst.heroId = heroId;
            if (nextPlayer == 1)
            {
                PlayerOneStoryline.Add(playedCard);
                PlayerOneStorylineObjects.Add(spawned);
            }
            else
            {
                PlayerTwoStoryline.Add(playedCard);
                PlayerTwoStorylineObjects.Add(spawned);
            }
        }
        var panel = LastCardsPanelUI.Instance;
        if (panel != null)
        {
            Sprite previous = nextPlayer == 1 ? lastCardPlayer2Sprite : lastCardPlayer1Sprite;
            panel.Show(_pendingSelectedSprite, previous);
        }

        if (nextPlayer == 1)
            lastCardPlayer1Sprite = _pendingSelectedSprite;
        else
            lastCardPlayer2Sprite = _pendingSelectedSprite;
        selectionPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        symbolPanel.SetActive(true);
        
        if (_lastSymbols != null && _lastSymbols.Count > 0)
            symbolPanelUI.ShowPreviousSymbols(_lastSymbols);

        if (_pendingSelectedSymbols != null && _pendingSelectedSymbols.Count > 0)
            symbolPanelUI.ShowCurrentSymbols(_pendingSelectedSymbols);
        if (isSpecial)
        {
            var heroCtrl = nextPlayer == 1 ? SetupManager.hero1Controller : SetupManager.hero2Controller;
            if (heroCtrl != null)
                            {
                if (specialAbilityButton != null)
                {
                    pendingSpecialAbilityId = abilityId;
                    specialAbilityButton.gameObject.SetActive(true);
                    specialAbilityButton.onClick.RemoveAllListeners();
                    specialAbilityButton.onClick.AddListener(() =>
                    {
                        specialAbilityButton.gameObject.SetActive(false);
                        StartCoroutine(heroCtrl.ExecuteSpecialAbility(pendingSpecialAbilityId, symbolPanelUI));
                        pendingSpecialAbilityId = null;
                    });
                }
                else
                {
                    StartCoroutine(heroCtrl.ExecuteSpecialAbility(abilityId, symbolPanelUI));
                }
            }
        }

        

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
        /// <summary>
    /// Public wrapper used by external systems to display the hand and allow
    /// card selection. Internally forwards to <see cref="OnPlayerCardSelected"/>.
    /// </summary>
    public void HandlePlayerCardSelected(HeroCard card)
    {
        OnPlayerCardSelected(card);
    }
    // ============================================
    //               --- SPAWN + UTILS ---
    // ============================================

    private GameObject SpawnCardAtNextSlot(GameObject prefab, Sprite sprite)
    {
        if (cardSpawnPoints == null || cardSpawnPoints.Length == 0)
        {
            Debug.LogError("SpawnCardAtNextSlot: brak punktów spawnów!");
            return null;
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
        return go;
    }

    private Texture2D ConvertSpriteToTexture(Sprite sprite)
    {
        if (sprite == null) return null;
        var tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        tex.SetPixels(sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height));
        tex.Apply();
        return tex;
    }

        public void UpdateStorylineCard(bool isPlayerTwo, int index, HeroCard newCard, string heroId)
    {
        var objs = isPlayerTwo ? PlayerTwoStorylineObjects : PlayerOneStorylineObjects;
        var cards = isPlayerTwo ? PlayerTwoStoryline : PlayerOneStoryline;
        if (index < 0 || index >= objs.Count || index >= cards.Count)
            return;

        cards[index] = newCard;
        var go = objs[index];
        var disp = go.GetComponent<CardDisplay>();
        if (disp != null)
        {
            var sprite = _cardMgr.GetCardSprite(heroId, newCard);
            disp.frontTexture = ConvertSpriteToTexture(sprite);
            disp.ApplyTextures();
        }

        var inst = go.GetComponent<HeroCardInstance>();
        if (inst != null)
        {
            inst.card = newCard;
            inst.heroId = heroId;
        }
    }

    
    private IEnumerator HandleLocationEndTurnAbility(HeroController hero)
    {
        var loc = hero?.CurrentLocation;
        if (loc == null) yield break;

        var simple = loc.GetComponent<Location>();
        if (simple != null && simple.IsBlockedByThreat()) yield break;

        var data = loc.GetComponent<LocationDataHolder>()?.data;
        var ability = loc.GetComponent<ILocationEndTurnAbility>();

        if (ability == null || data == null || string.IsNullOrEmpty(data.end_turn))
            yield break;

        bool use = false;
        yield return StartCoroutine(AskYesNo(data.end_turn, val => use = val));
        if (use)
            yield return ability.ExecuteEndTurn(hero);
    }

    private IEnumerator AskYesNo(string text, System.Action<bool> callback)
    {
        var panel = GameManager.Instance.heroSelectionPanel;
        if (panel == null)
        {
            callback?.Invoke(false);
            yield break;
        }

        panel.SetActive(true);
        var ctrl = panel.GetComponent<HeroSelectionPanelController>();
        bool? result = null;
        ctrl.Init(text, "YES", "NO",
            onHero1: () => { result = true; panel.SetActive(false); },
            onHero2: () => { result = false; panel.SetActive(false); });

        yield return new WaitUntil(() => result.HasValue);
        callback?.Invoke(result.Value);
    }
    private HeroController FindHeroById(string heroId)
    {
        var heroes = UnityEngine.Object.FindObjectsByType<HeroController>(FindObjectsSortMode.None);
        foreach (var hero in heroes)
        {
            if (hero.HeroId == heroId)
                return hero;
        }
        return null;
    }
}
