using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    private List<HeroCard> cards;

    public Deck(List<HeroCard> deck)
    {
        cards = new List<HeroCard>(deck);
        Shuffle();
    }

    private void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int rand = Random.Range(i, cards.Count);
            var temp = cards[i];
            cards[i] = cards[rand];
            cards[rand] = temp;
        }
    }

    public List<HeroCard> Draw(int count)
    {
        List<HeroCard> hand = cards.GetRange(0, count);
        cards.RemoveRange(0, count);
        return hand;
    }

    public int Count => cards.Count;
}
