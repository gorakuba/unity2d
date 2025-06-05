using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    // Ręce graczy
    public List<HeroCard> playerOneHand { get; private set; }
    public List<HeroCard> playerTwoHand { get; private set; }

    // Pierwsza karta Zbira
    public VillainCard firstVillainCard { get; private set; }

    // Prefaby / wyświetlacze
    public HeroCardDisplay displayPlayer1;
    public HeroCardDisplay displayPlayer2;
    public VillainCardDisplay villainDisplay;

    // Talia Zbira
    private List<VillainCard> villainDeck = new();
    private int villainCardIndex = 0;

    // Pełne, przetasowane talie bohaterów
    private List<HeroCard> heroDeck1;
    private List<HeroCard> heroDeck2;

    // Dostęp do aktualnych talii bohaterów (potrzebne np. do wrzucania kart na spód)
    public List<HeroCard> playerOneDeck => heroDeck1;
    public List<HeroCard> playerTwoDeck => heroDeck2;


    private void Start()
    {
        RollAllCards();

        // Pokaż początkowe ręce i pierwszą kartę Zbira
        displayPlayer1.ShowCards();
        displayPlayer2.ShowCards();
        villainDisplay.ShowFirstCard();
    }

    public void RollAllCards()
    {
        Debug.Log("🔁 Losuję wszystkie karty...");

        var loader = new HeroCardLoader();

        // 1) Wczytaj pełne talie z JSON
        var full1 = loader.LoadHeroDeck(GameManager.Instance.playerOneHero);
        var full2 = loader.LoadHeroDeck(GameManager.Instance.playerTwoHero);

        // 2) Przetasuj je raz na starcie
        Shuffle(full1);
        Shuffle(full2);

        // 3) Zachowaj stan talii
        heroDeck1 = new List<HeroCard>(full1);
        heroDeck2 = new List<HeroCard>(full2);

        // 4) Rozdaj początkowo po 3 karty każdemu
        playerOneHand = heroDeck1.GetRange(0, 3);
        heroDeck1.RemoveRange(0, playerOneHand.Count);

        playerTwoHand = heroDeck2.GetRange(0, 3);
        heroDeck2.RemoveRange(0, playerTwoHand.Count);

        // Teraz talia Zbira (bez zmian)
        var villainLoader = new VillainLoader();
        var villain = villainLoader.LoadVillainData(GameManager.Instance.selectedVillain);
        if (villain != null && villain.cards != null && villain.cards.Count > 0)
        {
            var cards = new List<VillainCard>(villain.cards);
            Shuffle(cards);
            villainDeck = cards;
            firstVillainCard = villainDeck[0];
            villainCardIndex = 0;
        }
    }

    // Uniwersalny tasownik
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    /// <summary>
    /// Dociąga kolejną kartę z talii bohatera (playerIndex 1 lub 2) — bez powtórek.
    /// </summary>
    public HeroCard DrawHeroCard(int playerIndex)
    {
        var deck = (playerIndex == 1) ? heroDeck1 : heroDeck2;
        if (deck == null || deck.Count == 0)
        {
            Debug.LogWarning($"🛑 Brak kart do dobrania dla gracza {playerIndex}");
            GameManager.Instance?.TriggerDefeat();
            return null;
        }
        var card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    /// <summary>
    /// Pobiera następną (już przetasowaną) kartę Zbira.
    /// </summary>
    public VillainCard GetNextVillainCard()
    {
        if (villainCardIndex >= villainDeck.Count)
        {
            Debug.LogWarning("🛑 Brak więcej kart Zbira!");
            return null;
        }
        return villainDeck[villainCardIndex++];
    }

    /// <summary>
    /// Zwraca ścieżkę Addressables w formacie "deckId/cardId"
    /// dla kart bohatera.
    /// </summary>
    public string GetSpritePathForCard(string deckId, HeroCard card)
    {
        return $"{deckId}/{card.Id}";
    }

    /// <summary>
    /// Ładuje sprite bohatera z Addressables, synchronously.
    /// </summary>
    public Sprite GetCardSprite(string deckId, HeroCard card)
    {
        string path = GetSpritePathForCard(deckId, card);
        var handle = Addressables.LoadAssetAsync<Sprite>(path);
        handle.WaitForCompletion();
        return handle.Status == AsyncOperationStatus.Succeeded
            ? handle.Result
            : null;
    }

    /// <summary>
    /// Ładuje sprite karty Zbira z Addressables, synchronously.
    /// </summary>
    public Sprite GetCardSprite(VillainCard card)
    {
        string deckId = GameManager.Instance.selectedVillain;
        string path = $"{deckId}/{card.id}";
        var handle = Addressables.LoadAssetAsync<Sprite>(path);
        handle.WaitForCompletion();
        return handle.Status == AsyncOperationStatus.Succeeded
            ? handle.Result
            : null;
    }
}
