// HeroController.cs
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public SpriteRenderer visualRenderer;
    public HeroVisualDatabase visualDatabase;

    public void Initialize(string heroID)
    {
        Sprite heroSprite = visualDatabase.GetHeroSprite(heroID);
        if (heroSprite != null)
        {
            visualRenderer.sprite = heroSprite;
        }
    }
}
