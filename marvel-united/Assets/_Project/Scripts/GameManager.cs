using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public int CurrentPlayerIndex = 1;
    public static GameManager Instance;

    [Header("Wybrane postacie")]
    public string playerOneHero;
    public string playerTwoHero;
    public string selectedVillain;

    [Header("Referencje do managerów i UI")]
    public LocationManager    locationManager;
    public ThreatCardSpawner  threatCardSpawner;
    public CardManager        cardManager;
    public HeroCardDisplay    displayPlayer1;
    public HeroCardDisplay    displayPlayer2;
    public VillainCardDisplay villainDisplay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// Wywołaj np. z przycisku „Reset”
    /// </summary>
    public void ResetGame()
    {
        // 1) resetujemy lokacje + threat‐karty (poprzez eventy)
        locationManager.ResetSpawnedLocations();

        // 2) odpalamy ponowny spawn i po nim odświeżamy UI
        StartCoroutine(ResetAndSetupRoutine());
    }

    private IEnumerator ResetAndSetupRoutine()
    {
        // a) Spawn lokacji, tokenów i threat‐kart
        yield return StartCoroutine(locationManager.SpawnLocationsWithDelay());

        // b) odświeżamy ręce graczy i pierwszy card zbira
        cardManager.RollAllCards();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "GameScene") return;
        StartCoroutine(AssignSceneReferences());
    }

    private IEnumerator AssignSceneReferences()
    {
        yield return null;
        if (locationManager == null)    locationManager   = FindAnyObjectByType<LocationManager>();
        if (threatCardSpawner == null)  threatCardSpawner = FindAnyObjectByType<ThreatCardSpawner>();
        if (cardManager == null)        cardManager       = FindAnyObjectByType<CardManager>();

        GameObject hero1 = FindObjectInScene("Hero1CardsInfo");
        if (hero1 != null)
        {
            displayPlayer1 = hero1.GetComponentInChildren<HeroCardDisplay>(true);
            displayPlayer1?.Initialize(this, cardManager);
            displayPlayer1?.ShowCards();
        }
        else
            Debug.LogError("Nie znaleziono Hero1CardsInfo!");

        GameObject hero2 = FindObjectInScene("Hero2CardsInfo");
        if (hero2 != null)
        {
            displayPlayer2 = hero2.GetComponentInChildren<HeroCardDisplay>(true);
            displayPlayer2?.Initialize(this, cardManager);
            displayPlayer2?.ShowCards();
        }
        else
            Debug.LogError("Nie znaleziono Hero2CardsInfo!");

        GameObject villain = FindObjectInScene("VillainCardInfo");
        if (villain != null)
        {
            villainDisplay = villain.GetComponentInChildren<VillainCardDisplay>(true);
            villainDisplay?.Initialize(this, cardManager);
            villainDisplay?.ShowFirstCard();
        }

        else
            Debug.LogError("Nie znaleziono VillainCardInfo!");

    }
    public string GetHeroName(string heroId)
{
    switch (heroId)
    {
        case "iron_man": return "Iron Man";
        case "spider-man": return "Spider-Man";
        case "black_panther": return "Black Panther";
        case "wasp": return "Wasp";
        case "captain_marvel": return "Captain Marvel";
        case "captain_america": return "Captain America";
        
        default: return heroId;
    }
}
public GameObject FindObjectInScene(string name)
{
    Scene activeScene = SceneManager.GetActiveScene();
    GameObject[] rootObjects = activeScene.GetRootGameObjects();

    foreach (var root in rootObjects)
    {
        GameObject result = FindChildRecursive(root.transform, name);
        if (result != null)
            return result;
    }
    return null;
}
private GameObject FindChildRecursive(Transform parent, string name)
{
    if (parent.name == name)
        return parent.gameObject;

    foreach (Transform child in parent)
    {
        GameObject result = FindChildRecursive(child, name);
        if (result != null)
            return result;
    }
    return null;
}
}
