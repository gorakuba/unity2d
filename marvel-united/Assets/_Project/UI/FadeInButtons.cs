using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInButtons : MonoBehaviour
{
    public float fadeDuration = 2f; // Czas trwania efektu pojawiania się
    public float startDelay = 0f; // Opóźnienie przed startem

    private Button[] buttons;
    private CanvasGroup canvasGroup;
    private float targetAlpha;

    private void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            // Jeśli CanvasGroup nie istnieje, dodajemy go dynamicznie
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Pobranie docelowej przezroczystości z CanvasGroup
        targetAlpha = canvasGroup.alpha;

        // Ustawienie początkowej przezroczystości na 0 (całkowicie niewidoczne)
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false; // Wyłączenie interakcji
        canvasGroup.blocksRaycasts = false; // Nie pozwala na klikanie w ukryte przyciski

        // Rozpoczęcie efektu po opóźnieniu
        StartCoroutine(StartWithDelay());
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay); // Oczekiwanie przed startem efektu
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        // Upewnienie się, że docelowa przezroczystość jest osiągnięta
        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = true; // Aktywacja interakcji
        canvasGroup.blocksRaycasts = true; // Pozwala na klikanie w przyciski
    }
}
