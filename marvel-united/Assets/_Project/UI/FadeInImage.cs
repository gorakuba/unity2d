using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInImage : MonoBehaviour
{
    private Image image; // Komponent Image
    public float fadeDuration = 2f; // Czas trwania efektu pojawiania się
    public float startDelay = 0f; // Opóźnienie przed startem
    public float targetAlpha = 1f; // Docelowa przezroczystość (0 = niewidoczny, 1 = pełna widoczność)

    private void Start()
    {
        image = GetComponent<Image>(); // Pobranie komponentu Image

        if (image != null)
        {
            // Ustawienie początkowej przezroczystości na 0 (niewidoczność)
            Color startColor = image.color;
            startColor.a = 0f; 
            image.color = startColor;

            // Rozpoczęcie efektu po opóźnieniu
            StartCoroutine(StartWithDelay());
        }
        else
        {
            Debug.LogError("Brak komponentu Image!");
        }
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay); // Oczekiwanie przed startem efektu
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color startColor = image.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, targetAlpha, elapsedTime / fadeDuration);
            image.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Upewnienie się, że alpha osiągnęła docelową wartość
        image.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }
}
