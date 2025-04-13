using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResetButtonHandler : MonoBehaviour
{
    public Button resetButton;

    public void OnResetButtonClicked()
    {
        // Wywołanie resetu gry
        GameManager.Instance.ResetGame();

        // Dezaktywuj przycisk
        resetButton.interactable = false;

        // Odpal opóźnione ponowne włączenie
        StartCoroutine(ReactivateButtonAfterDelay(7f));
    }

    IEnumerator ReactivateButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        resetButton.interactable = true;
    }
    private void Start()
{
    resetButton.interactable = false;

    StartCoroutine(EnableButtonAfterStartDelay(7f));
}

IEnumerator EnableButtonAfterStartDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    resetButton.interactable = true;
}
}
