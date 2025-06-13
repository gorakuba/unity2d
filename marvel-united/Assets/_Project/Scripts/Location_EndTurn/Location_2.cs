using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location_2 : MonoBehaviour, ILocationEndTurnAbility
{
    private DiscardPanelUI DiscardPanel => DiscardPanelUI.Instance;
    private StorylinePanelUI StorylinePanel => StorylinePanelUI.Instance;

    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        if (hero == null)
            yield break;

        var cardMgr = GameManager.Instance?.cardManager;
        var turnMgr = TurnManager.Instance;
        if (cardMgr == null || turnMgr == null || StorylinePanel == null || DiscardPanel == null)
            yield break;

        bool isPlayerTwo = hero == SetupManager.hero2Controller;
        List<HeroCard> hand = isPlayerTwo ? cardMgr.playerTwoHand : cardMgr.playerOneHand;
        List<HeroCard> storyline = isPlayerTwo ? turnMgr.PlayerTwoStoryline : turnMgr.PlayerOneStoryline;

        if (hand == null || hand.Count == 0 || storyline == null || storyline.Count == 0)
            yield break;

        int storyIndex = -1;
        StorylinePanel.Open(storyline, idx => storyIndex = idx, cardMgr, hero.HeroId);
        while (StorylinePanel.IsActive)
            yield return null;

        if (storyIndex < 0)
            yield break;

        HeroCard storylineCard = storyline[storyIndex];

        HeroCard handCard = null;
        DiscardPanel.Open(hand, c => { if (hand.Contains(c)) handCard = c; }, cardMgr, hero.HeroId);
        while (DiscardPanel.IsActive)
            yield return null;

        if (handCard == null)
            yield break;

        hand.Remove(handCard);
        hand.Add(storylineCard);
        storyline[storyIndex] = handCard;

        turnMgr.UpdateStorylineCard(isPlayerTwo, storyIndex, handCard, hero.HeroId);
    }
}