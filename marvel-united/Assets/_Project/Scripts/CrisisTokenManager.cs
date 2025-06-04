using UnityEngine;

public class CrisisTokenManager : MonoBehaviour
{
    public static CrisisTokenManager Instance;
    public CrisisTokenUI crisisTokenUI;

    void Awake()
    {
        Instance = this;
    }

    public int GetTotalCrisisTokens()
    {
        int total = 0;
        var hero1 = SetupManager.hero1Controller;
        var hero2 = SetupManager.hero2Controller;

        if (hero1 != null)
        {
            var crisisHandler = hero1.GetComponent<HeroCrisisHandler>();
            if (crisisHandler != null)
                total += crisisHandler.GetCrisisTokenCount();
        }

        if (hero2 != null)
        {
            var crisisHandler = hero2.GetComponent<HeroCrisisHandler>();
            if (crisisHandler != null)
                total += crisisHandler.GetCrisisTokenCount();
        }

        return total;
    }

     public void UpdateUI()
    {
        if (crisisTokenUI != null)
            crisisTokenUI.UpdateUI(GetTotalCrisisTokens());
    }
  /// <summary>
    /// Daje bohaterowi 1 CrisisToken i odświeża UI.
    /// </summary>
    public void GiveCrisisToken(HeroController hero)
    {
        if (hero == null) return;
        if (hero.IsStunned) return;
        var handler = hero.GetComponent<HeroCrisisHandler>();
        if (handler == null)
        {
            Debug.LogError($"[CrisisTokenManager] {hero.name} nie ma HeroCrisisHandler!");
            return;
        }
        handler.AddCrisisToken();
        UpdateUI();
        Debug.Log($"[CrisisTokenManager] {hero.name} otrzymał CrisisToken. Suma = {GetTotalCrisisTokens()}");
    }
}
