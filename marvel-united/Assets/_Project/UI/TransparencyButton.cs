using UnityEngine;
using UnityEngine.UI;

public class TransparencyButton : MonoBehaviour
{
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();

        // Sprawdź, czy tekstura jest czytelna
        if (image.sprite != null && image.sprite.texture.isReadable)
        {
            image.alphaHitTestMinimumThreshold = 0.01f;
        }
        else
        {
            Debug.LogWarning("Tekstura nie jest czytelna! Włącz Read/Write Enabled w ustawieniach tekstury.");
        }
    }
}
