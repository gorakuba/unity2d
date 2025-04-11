using UnityEngine;
using System.Collections;

public class AppearAnimation : MonoBehaviour
{
    [Header("Czas trwania animacji")]
    public float duration = 0.4f;

    [Header("Docelowa skala obiektu")]
    public Vector3 targetScale = Vector3.one;

    private void OnEnable()
    {
        // Ustaw skalę początkową na 0 (niewidoczny)
        transform.localScale = Vector3.zero;

        // Rozpocznij animację pojawiania się
        StartCoroutine(AnimateScale());
    }

    private IEnumerator AnimateScale()
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Płynne przejście z użyciem SmoothStep
            transform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // Upewnij się, że końcowa skala została ustawiona dokładnie
        transform.localScale = endScale;
    }
}
