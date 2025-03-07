using UnityEngine;
using System.Collections;

public class ScaleUp : MonoBehaviour
{
    public float scaleIncrease = 1.3f; // Powiększenie o 30%
    public float scaleDuration = 5f; // Czas trwania powiększania
    public float startDelay = 0f; // Opóźnienie przed startem
    public AudioSource sfxAudio; // Źródło dźwięku
    public GameObject vfxPrefab; // Prefab efektu cząsteczek

    private Vector3 initialScale; // Początkowa skala

    private void Start()
    {
        initialScale = transform.localScale; // Zapamiętanie początkowej skali
        StartCoroutine(StartWithDelay());
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay); // Oczekiwanie przed startem
        StartCoroutine(ScaleObject());
    }

    private IEnumerator ScaleObject()
    {
        float elapsedTime = 0f;
        Vector3 targetScale = initialScale * scaleIncrease; // Docelowa skala

        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / scaleDuration);
            yield return null;
        }

        // Upewnienie się, że skala jest dokładnie ustawiona na końcu animacji
        transform.localScale = targetScale;

        // Odtworzenie dźwięku po zakończeniu skalowania
        if (sfxAudio != null)
        {
            sfxAudio.Play();
        }

        // Instancjonowanie efektu VFX po zakończeniu skalowania
        if (vfxPrefab != null)
        {
            GameObject vfxInstance = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfxInstance, 2f); // Usunięcie efektu po 2 sekundach
        }
    }
}
