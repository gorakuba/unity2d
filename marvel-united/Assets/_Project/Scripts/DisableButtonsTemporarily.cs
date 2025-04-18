using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DisableButtonsTemporarily : MonoBehaviour
{
    [Header("Przyciski do dezaktywacji")]
    public List<Button> buttonsToDisable;

    [Header("Czas dezaktywacji (sekundy)")]
    public float disableDuration = 5f;

    private void Start()
    {
        // Dezaktywacja na start
        DisableForSeconds();
    }

    public void DisableForSeconds()
    {
        StartCoroutine(DisableRoutine());
    }

    private IEnumerator DisableRoutine()
    {
        foreach (var button in buttonsToDisable)
        {
            if (button != null)
                button.interactable = false;
        }

        yield return new WaitForSeconds(disableDuration);

        foreach (var button in buttonsToDisable)
        {
            if (button != null)
                button.interactable = true;
        }
    }
}
