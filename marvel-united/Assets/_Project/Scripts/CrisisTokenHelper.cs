using UnityEngine;

public static class CrisisTokenHelper
{
    public static int GetTotalCrisisTokens()
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
}
