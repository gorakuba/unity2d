using System.Collections;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public SpriteRenderer visualRenderer;
    public HeroVisualDatabase visualDatabase;


    private HeroDamageHandler heroDamageHandler;
    private string heroId;
    public string HeroId => heroId;
    private IHeroSpecials specialHandler;

    public bool IsStunned { get; set; }
    /// <summary>
    /// Ustawiane przez HeroMovementManager po zakończeniu ruchu.
    /// </summary>
    public LocationController CurrentLocation { get; set; }
    private void Awake()
    {
        // Spróbuj znaleźć LocationController w górę hierarchii
        var loc = GetComponentInParent<LocationController>();
        if (loc != null)
        {
            CurrentLocation = loc;
            Debug.Log($"[HeroController] {HeroId} zainicjowany w lokacji {loc.name}");
        }
        else
        {
            Debug.LogWarning($"[HeroController] Nie udało się wykryć startowej lokacji dla {HeroId}");
        }
    }
    public void Initialize(string heroID, GameManager gameManager, CardManager cardManager, bool isPlayerTwo)
    {
        heroId = heroID;

        Sprite heroSprite = visualDatabase.GetHeroSprite(heroID);
        if (heroSprite != null)
            visualRenderer.sprite = heroSprite;

        heroDamageHandler = GetComponent<HeroDamageHandler>();
        if (heroDamageHandler != null)
            heroDamageHandler.Initialize(gameManager, cardManager, isPlayerTwo, heroId);

        // assign special handler depending on hero
        switch (heroId)
        {
            case "black_panther":
                specialHandler = new BlackPantherSpecials();
                break;
                // add more heroes here
        }
    }


    public void TakeDamage()
    {
        if (IsStunned)
            return;
        if (heroDamageHandler != null)
            heroDamageHandler.TakeDamageCoroutine();
        else
            Debug.LogWarning($"Brak HeroDamageHandler dla {heroId}!");
    }

    public void Stun()
    {
        IsStunned = true;
        Debug.Log($"[HeroController] {heroId} zostal oszolomiony");
    }

    public void RecoverStun()
    {
        IsStunned = false;
        Debug.Log($"[HeroController] {heroId} odzyskal przytomnosc");
    }
    
    public IEnumerator ExecuteSpecialAbility(string abilityId, SymbolPanelUI panel)
    {
        if (specialHandler != null && !string.IsNullOrEmpty(abilityId))
            yield return specialHandler.ExecuteSpecial(abilityId, this, panel);
    }
}
