using UnityEngine;
using UnityEngine.UI;

public class CrisisTokenUI : MonoBehaviour
{
    public Transform crisisTokenContainer;
    public Sprite crisisTokenSprite;

    // Wyświetla aktualną liczbę tokenów (dostarczoną z managera)
    public void UpdateUI(int totalTokens)
    {
        // Usuń obecne tokeny
        foreach (Transform child in crisisTokenContainer)
        {
            Destroy(child.gameObject);
        }

        // Dodaj nowe tokeny
        for (int i = 0; i < totalTokens; i++)
        {
            GameObject token = new GameObject("CrisisToken", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            token.transform.SetParent(crisisTokenContainer);
            token.transform.localScale = Vector3.one;

            Image img = token.GetComponent<Image>();
            img.sprite = crisisTokenSprite;
        }
    }
}
