using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    public Image targetImage; // Obiekt UI Image do zmiany
    public string[] imageNames; // Nazwy plików obrazów (bez rozszerzenia)

    private string imagePath = "Art/"; // Ścieżka w Resources

    void Start()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    public void ChangeImage(int index)
    {
        if (index < 0 || index >= imageNames.Length)
        {
            Debug.LogWarning("Nieprawidłowy indeks obrazka!");
            return;
        }

        // Wczytaj obrazek z Resources
        Sprite newSprite = Resources.Load<Sprite>(imagePath + imageNames[index]);

        if (newSprite != null)
        {
            targetImage.sprite = newSprite;
        }
        else
        {
            Debug.LogError("Nie znaleziono obrazka: " + imagePath + imageNames[index]);
        }
    }
}
