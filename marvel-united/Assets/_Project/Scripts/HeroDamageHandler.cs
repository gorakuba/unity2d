using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroDamageHandler : MonoBehaviour
{
    private CardManager cardManager;
    private GameManager gameManager;
    private bool isPlayerTwo;
    private string heroId;
    private DiscardPanelUI discardPanel;

    private HeroController heroController;
    private VillainController villainController;
    private List<HeroCard> currentHand;
    private List<HeroCard> currentDeck;

    // Zamiast przypisywać w Awake → dynamiczny dostęp
    private DiscardPanelUI DiscardPanel => DiscardPanelUI.Instance;

    public void Initialize(GameManager gm, CardManager cm, bool playerTwo, string heroId)
    {
        gameManager = gm;
        cardManager = cm;
        isPlayerTwo = playerTwo;
        this.heroId = heroId;
        discardPanel = DiscardPanelUI.Instance;
        heroController = GetComponent<HeroController>();
        if (villainController == null)
            villainController = SetupManager.villainController;
    }

   public IEnumerator TakeDamageCoroutine()
    {
                var hero = GetComponent<HeroController>();
        if (hero != null && hero.IsStunned)
            yield break;
        var heroCtrl = GetComponent<HeroController>();
        if (heroCtrl != null && heroCtrl.IsStunned)
        {
            Debug.Log($"[HeroDamageHandler] {heroId} is stunned - ignoring damage");
            yield break;
        }

        if (discardPanel == null)
        {
            discardPanel = DiscardPanelUI.Instance;
        }

        currentHand = isPlayerTwo ? cardManager.playerTwoHand : cardManager.playerOneHand;
        currentDeck = isPlayerTwo ? cardManager.playerTwoDeck : cardManager.playerOneDeck;

        if (currentHand == null || currentHand.Count == 0)
        {
            Debug.LogWarning("Gracz nie ma kart (jest wyeliminowany?)");
            yield break;
        }

        Debug.Log($"[HeroDamageHandler] -> Uruchamiam discard panel dla gracza {(isPlayerTwo ? 2 : 1)}");
        
        discardPanel.Open(currentHand, OnCardSelected, cardManager, heroId);
        while (discardPanel.IsActive)
        {
            yield return null;
        }

        BAMController.PlayerFinishedDamage();
        Debug.Log($"[HeroDamageHandler] -> Gracz {(isPlayerTwo ? 2 : 1)} wybrał kartę i BAM idzie dalej");
    }


    private void OnCardSelected(HeroCard selectedCard)
    {
        if (currentHand.Contains(selectedCard))
        {
            currentHand.Remove(selectedCard);
            currentDeck.Add(selectedCard);
            Debug.Log($"❤️ Gracz {(isPlayerTwo ? 2 : 1)} odrzucił kartę {selectedCard.Id} → na spód talii");

            if (currentHand.Count == 0)
            {
                if (heroController != null)
                    heroController.IsStunned = true;

                if (villainController == null)
                    villainController = SetupManager.villainController;

                if (villainController != null)
                    villainController.TriggerBAM();
            }

        }
    }
}
