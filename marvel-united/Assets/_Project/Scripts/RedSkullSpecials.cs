using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IVillainSpecials
{
    IEnumerator ExecuteSpecial(string specialId);
}
public class RedSkullSpecials : IVillainSpecials
{
    public IEnumerator ExecuteSpecial(string specialId)
    {
        switch (specialId)
        {
            case "Red_Skull_Special_1":
                yield return Special_HailHydra();
                break;
            case "Red_Skull_Special_2":
                yield return Special_HydraInsurgency();
                break;
            default:
                Debug.LogWarning($"[RedSkullSpecials] Unknown special ID: {specialId}");
                break;
        }
    }

    // -------------------------------------
    // Red_Skull_Special_1 → HAIL HYDRA (usuń cywili + obrażenia + fear)
    // -------------------------------------

private IEnumerator Special_HailHydra()
{
    Debug.Log("🔥 [Red Skull Special 1] Remove civilians in hero locations + Damage + Fear");

    var hero1 = SetupManager.hero1Controller;
    var hero2 = SetupManager.hero2Controller;

    int removedCivilians = 0;

    // 1️⃣ Znajdź wszystkich bohaterów
    List<GameObject> heroes = new List<GameObject>();
    if (hero1 != null) heroes.Add(hero1.gameObject);
    if (hero2 != null) heroes.Add(hero2.gameObject);

    // 2️⃣ Znajdź wszystkie unikalne lokacje bohaterów
    HashSet<Transform> uniqueLocations = new HashSet<Transform>();

    foreach (var hero in heroes)
    {
        var heroLocationRoot = GetLocationRoot(hero.transform);
        var realLocation = FindRealLocation(heroLocationRoot);

        if (realLocation != null)
        {
            uniqueLocations.Add(realLocation);
        }
    }

    // 3️⃣ Dla każdej unikalnej lokalizacji → usuń cywili
    foreach (var location in uniqueLocations)
    {
        Debug.Log($"[LOCATION] Working in real location: {location.name}");

        List<GameObject> civiliansToRemove = new List<GameObject>();
        foreach (Transform slot in location)
        {
            if (slot.name.StartsWith("Slot_") && slot.childCount > 0)
            {
                var token = slot.GetChild(0).gameObject;
                Debug.Log($"[CHECK] Found token: {token.name} in {slot.name}");

                if (token.name.Contains("Civillian"))
                {
                    civiliansToRemove.Add(token);
                }
            }
        }

        // Usuń cywili
        foreach (var civ in civiliansToRemove)
        {
            Debug.Log($"[REMOVE] Destroying: {civ.name}");
            Object.Destroy(civ);
            removedCivilians++;
        }
    }

    // 4️⃣ Dodaj Fear za usuniętych cywili
    if (removedCivilians > 0)
    {
        Debug.Log($"[Fear] +{removedCivilians} Fear (removed civilians)");
        DashboardLoader.Instance.MoveFearTrack(removedCivilians);
    }

    // 5️⃣ Zadanie obrażeń → dopiero po wszystkim
    foreach (var hero in heroes)
    {
                if (hero.GetComponent<HeroController>()?.IsStunned == true)
            continue;
        yield return hero.GetComponent<HeroDamageHandler>().TakeDamageCoroutine();
    }
}



private Transform FindRealLocation(Transform slotRoot)
{
    // Szukamy głębiej w slotRoot i wszystkich jego dzieciach
    foreach (Transform child in slotRoot.GetComponentsInChildren<Transform>())
    {
        if (child.name.Contains("(Clone)"))
        {
            Debug.Log($"[LOCATION] Found real location prefab: {child.name}");
            return child;
        }
    }

    Debug.LogWarning($"[LOCATION] No real location prefab found inside: {slotRoot.name}");
    return null; // lepiej zwrócić null niż zły obiekt
}





    // -------------------------------------
    // Red_Skull_Special_2 → HYDRA INSURGENCY (crisis tokens → fear)
    // -------------------------------------
private IEnumerator Special_HydraInsurgency()
{
    Debug.Log("🔥 [Red Skull Special 2] Crisis Tokens → Fear");

    int crisisTokens = CrisisTokenManager.Instance.GetTotalCrisisTokens();

    if (crisisTokens > 0)
    {
        Debug.Log($"[Fear] +{crisisTokens} Fear (crisis tokens)");
        DashboardLoader.Instance.MoveFearTrack(crisisTokens);
    }

    yield return null;
}

    // -------------------------------------
    // Helper → znajdź lokację bohatera
    // -------------------------------------
    private Transform GetLocationRoot(Transform t)
    {
        while (t.parent != null && !t.parent.name.StartsWith("Location_PLACE"))
            t = t.parent;
        return t;
    }
}
