using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResetButtonHandler : MonoBehaviour
{
    public Button resetButton;
    public DisableButtonsTemporarily buttonDisabler;
    public CardManager cardManager;
    public LocationButtonAutoBinder locationButtonAutoBinder;
    public void OnResetButtonClicked()
    {
        // Wywołanie resetu gry
        GameManager.Instance.ResetGame();
        cardManager.RollAllCards();

        // Dezaktywuj przycisk
        resetButton.interactable = false;
        // Odpal opóźnione ponowne włączenie
        StartCoroutine(ReactivateButtonAfterDelay(4.5f));
        
            if (buttonDisabler != null)
        buttonDisabler.DisableForSeconds();
        StartCoroutine(RebindButtonsAfterDelay(1f));
        

    }

    IEnumerator ReactivateButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        resetButton.interactable = true;
    }
    private void Start()
{
    resetButton.interactable = false;

    StartCoroutine(EnableButtonAfterStartDelay(4.5f));
}

IEnumerator EnableButtonAfterStartDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    resetButton.interactable = true;
}
private IEnumerator RebindButtonsAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    if (locationButtonAutoBinder != null)
    {
        locationButtonAutoBinder.RebindButtons();
        Debug.Log("✅ Przyciski lokacji zostały ponownie przypisane po resecie");
    }
    else
    {
        Debug.LogError("❌ locationButtonAutoBinder nie został przypisany");
    }
}
private void Awake()
{
    if (locationButtonAutoBinder == null)
    {
        locationButtonAutoBinder = FindFirstObjectByType<LocationButtonAutoBinder>();
    }
}
}