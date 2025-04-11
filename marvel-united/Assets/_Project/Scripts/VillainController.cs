using UnityEngine;

public class VillainController : MonoBehaviour
{
    public SpriteRenderer visualRenderer;
    public VillainVisualDatabase visualDatabase;

public void Initialize(string villainID)
{
    Sprite villainSprite = visualDatabase.GetVillainSprite(villainID);
    if (villainSprite != null)
    {
        visualRenderer.sprite = villainSprite;
    }
}
}