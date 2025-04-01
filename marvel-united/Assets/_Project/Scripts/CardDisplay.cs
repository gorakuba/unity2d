using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    public Texture frontTexture;
    public Texture backTexture;

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        var mats = renderer.materials;

        if (mats.Length >= 2)
        {
            if (frontTexture != null)
                mats[0].mainTexture = frontTexture;

            if (backTexture != null)
                mats[1].mainTexture = backTexture;

            renderer.materials = mats; // zaktualizuj materiały
        }
    }
}
