using UnityEngine;

public class HeroCrisisHandler : MonoBehaviour
{
    public int playerId = 0;  // <-- tutaj przypisujesz 1 lub 2 w Inspectorze

    public int crisisTokens = 0;

private void Start()
{
        // Automatyczne przypisywanie PlayerId
    var heroController = GetComponent<HeroController>();

    if (heroController != null)
    {
        string heroId = heroController.HeroId; // <- pobierz unikalny id bohatera

        if (GameManager.Instance.playerOneHero == heroId)
        {
            playerId = 1;
        }
        else if (GameManager.Instance.playerTwoHero == heroId)
        {
            playerId = 2;
        }
    }

    // Zawsze odśwież UI
    CrisisTokenManager.Instance?.UpdateUI();
}

    public void AddCrisisToken()
    {
        crisisTokens++;
        Debug.Log($"[CRISIS] Added Crisis Token. Total now: {crisisTokens}");

        CrisisTokenManager.Instance.UpdateUI();
    }

    public void RemoveCrisisToken()
    {
        if (crisisTokens > 0)
        {
            crisisTokens--;
            Debug.Log($"[CRISIS] Removed Crisis Token. Total now: {crisisTokens}");

            CrisisTokenManager.Instance.UpdateUI();
        }
    }

    public int GetCrisisTokenCount()
    {
        return crisisTokens;
    }
}
