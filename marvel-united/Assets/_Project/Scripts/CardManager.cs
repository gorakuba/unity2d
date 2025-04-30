using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CardManager : MonoBehaviour
{
    public List<HeroCard> playerOneHand { get; private set; }
    public List<HeroCard> playerTwoHand { get; private set; }
    public VillainCard firstVillainCard { get; private set; }

    public HeroCardDisplay displayPlayer1;
    public HeroCardDisplay displayPlayer2;
    public VillainCardDisplay villainDisplay;

    private List<VillainCard> villainDeck = new();
    private int villainCardIndex = 1;

    private void Start()
    {
        RollAllCards();
        displayPlayer1.ShowCards();
        displayPlayer2.ShowCards();
        villainDisplay.ShowFirstCard();
    }

    public void RollAllCards()
    {
        Debug.Log("🔁 Losuję wszystkie karty...");

        HeroCardLoader heroLoader = new HeroCardLoader();

        // GRACZ 1
        var deck1 = heroLoader.LoadHeroDeck(GameManager.Instance.playerOneHero);
        playerOneHand = new Deck(deck1).Draw(3);

        // GRACZ 2
        if (!string.IsNullOrEmpty(GameManager.Instance.playerTwoHero))
        {
            var deck2 = heroLoader.LoadHeroDeck(GameManager.Instance.playerTwoHero);
            playerTwoHand = new Deck(deck2).Draw(3);
        }

        // VILLAIN
        var villainLoader = new VillainLoader();
        var villain = villainLoader.LoadVillainData(GameManager.Instance.selectedVillain);
        if (villain != null && villain.cards != null && villain.cards.Count > 0)
        {
            var cards = new List<VillainCard>(villain.cards);
            Shuffle(cards);
            villainDeck = cards;
            firstVillainCard = villainDeck[0];
            villainCardIndex = 1;
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public VillainCard GetNextVillainCard()
    {
        if (villainCardIndex >= villainDeck.Count)
        {
            Debug.LogWarning("🛑 Brak więcej kart Zbira!");
            return null;
        }

        return villainDeck[villainCardIndex++];
    }

public string GetSpritePathForCard(HeroCard card)
{
    return $"{card.heroId}/{card.Id}";
}
    public Sprite GetCardSprite(HeroCard card)
    {
        string spritePath = GetSpritePathForCard(card);
        var handle = Addressables.LoadAssetAsync<Sprite>(spritePath);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;

        Debug.LogWarning($"⚠️ Nie znaleziono sprite'a dla {card.Id} (ścieżka: {spritePath})");
        return null;
    }
    public Sprite GetCardSprite(VillainCard card)
{
    string villainId = GameManager.Instance.selectedVillain;
    string spritePath = $"{villainId}/{card.id}";

    var handle = Addressables.LoadAssetAsync<Sprite>(spritePath);
    handle.WaitForCompletion();

    if (handle.Status == AsyncOperationStatus.Succeeded)
        return handle.Result;

    Debug.LogWarning($"❌ Sprite Zbira nie znaleziony: {spritePath}");
    return null;
}

} 
