using UnityEngine;

public class HeroController : MonoBehaviour
{
    public SpriteRenderer visualRenderer;
    public HeroVisualDatabase visualDatabase;

    private HeroDamageHandler heroDamageHandler;
    private string heroId;

    public void Initialize(string heroID, GameManager gameManager, CardManager cardManager, bool isPlayerTwo)
    {
        heroId = heroID;

        Sprite heroSprite = visualDatabase.GetHeroSprite(heroID);
        if (heroSprite != null)
        {
            visualRenderer.sprite = heroSprite;
        }

        heroDamageHandler = GetComponent<HeroDamageHandler>();
        if (heroDamageHandler != null)
        {
            heroDamageHandler.Initialize(gameManager, cardManager, isPlayerTwo, heroId);
        }
    }

    public void TakeDamage()
    {
        if (heroDamageHandler != null)
        {
            Debug.Log($"🔥 {heroId} otrzymuje obrażenie!");
            heroDamageHandler.TakeDamageCoroutine();
        }
        else
        {
            Debug.LogWarning($"Brak przypisanego HeroDamageHandler dla {heroId}!");
        }
    }
}
