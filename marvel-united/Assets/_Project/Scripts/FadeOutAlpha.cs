using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FadeOutUI : MonoBehaviour
{
    public float delay = 1f;
    public float duration = 2f;

    private RawImage rawImage;
    private TextMeshProUGUI tmpText;

    private Color rawStartColor;
    private Color textStartColor;

    void OnEnable()
    {
        // 🔥 Włącz wszystkie dzieci (w tym RawImage)
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        // Teraz komponenty będą widoczne
        rawImage = GetComponentInChildren<RawImage>(true); // true = przeszukaj też nieaktywne
        tmpText = GetComponentInChildren<TextMeshProUGUI>(true);

        // Ustaw alfa na 1
        if (rawImage != null)
        {
            rawStartColor = rawImage.color;
            rawImage.color = new Color(rawStartColor.r, rawStartColor.g, rawStartColor.b, 1f);
        }

        if (tmpText != null)
        {
            textStartColor = tmpText.color;
            tmpText.color = new Color(textStartColor.r, textStartColor.g, textStartColor.b, 1f);
        }

        // Start fade
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            if (rawImage != null)
                rawImage.color = new Color(rawStartColor.r, rawStartColor.g, rawStartColor.b, alpha);

            if (tmpText != null)
                tmpText.color = new Color(textStartColor.r, textStartColor.g, textStartColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ustaw końcowe alfa na 0
        if (rawImage != null)
            rawImage.color = new Color(rawStartColor.r, rawStartColor.g, rawStartColor.b, 0f);
        if (tmpText != null)
            tmpText.color = new Color(textStartColor.r, textStartColor.g, textStartColor.b, 0f);

        // Wyłącz cały obiekt
        gameObject.SetActive(false);
    }
}
