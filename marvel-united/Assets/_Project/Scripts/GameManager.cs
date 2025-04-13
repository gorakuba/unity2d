using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string playerOneHero;
    public string playerTwoHero;
    public string selectedVillain;

    public LocationManager locationManager;
    public ThreatCardSpawner threatCardSpawner;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetGame()
    {
        // 1. Wyczyść poprzednie lokacje
        foreach (Transform slot in locationManager.locationSlots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }

        // 2. Wyczyść Threat Cardy
        foreach (Transform place in threatCardSpawner.threatPlaces)
        {
            foreach (Transform child in place)
            {
                Destroy(child.gameObject);
            }
        }

        // 3. Wyczyść listy
        locationManager.spawnedLocationTransforms.Clear();

        typeof(LocationManager)
            .GetField("spawnedLocations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(locationManager, new List<GameObject>());

        // 4. Uruchom sekwencję ponownie
        locationManager.StartCoroutine("SpawnLocationsWithDelay");
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
    // Opcjonalnie: tylko jeśli to GameScene
    if (scene.name == "GameScene") 
    {
        StartCoroutine(AssignSceneReferences());
    }
}

private IEnumerator AssignSceneReferences()
{
    // Poczekaj jedną klatkę aż scena się wczyta
    yield return null;

    if (locationManager == null)
        locationManager = Object.FindFirstObjectByType<LocationManager>();

    if (threatCardSpawner == null)
        threatCardSpawner = Object.FindFirstObjectByType<ThreatCardSpawner>();
}
}
