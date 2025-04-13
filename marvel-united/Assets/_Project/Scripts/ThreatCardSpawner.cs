using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class ThreatCardSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject threatCardPrefab;
    public Transform[] threatPlaces;
    public ThreatCardTextureDatabase textureDatabase;
    [SerializeField] private LocationManager locationManager;
    public GameObject tokenHealthPrefab;
    [SerializeField] private Transform[] locationObjects;

    [Header("JSON")]
    public TextAsset villainJson;

private void Start()
{
    Debug.Log("[START] ThreatCardSpawner wystartował");

    if (locationManager != null)
    {
        locationManager.OnLocationsReady += OnLocationsReady;
    }
    else
    {
        Debug.LogWarning("LocationManager nie jest przypisany!");
    }
}
private void OnLocationsReady()
{
    locationObjects = locationManager.spawnedLocationTransforms.ToArray();
    SpawnThreatCardsForSelectedVillain();
}

    void SpawnThreatCardsForSelectedVillain()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }

        string villainId = GameManager.Instance.selectedVillain;
        Debug.Log($"Wybrany Zbir: {villainId}");

        if (string.IsNullOrEmpty(villainId))
        {
            Debug.LogError("Brak ID Zbira w GameManager");
            return;
        }

        VillainsRoot data = JsonUtility.FromJson<VillainsRoot>(villainJson.text);
        VillainData villain = data.villains.FirstOrDefault(v => v.id == villainId);

        if (villain == null)
        {
            Debug.LogError($"Nie znaleziono Zbira o ID: {villainId}");
            return;
        }

        List<ThreatCard> selectedThreats = villain.threats.OrderBy(x => Random.value).Take(6).ToList();
        Debug.Log($"Wylosowano {selectedThreats.Count} kart zagrożeń");

        for (int i = 0; i < 6; i++)
        {
            SpawnSingleThreatCard(selectedThreats[i], i, villainId);
        }
    }

    void SpawnSingleThreatCard(ThreatCard threat, int index, string villainId)
    {
        if (index >= threatPlaces.Length || index >= locationObjects.Length)
        {
            Debug.LogWarning($"Index {index} poza zakresem!");
            return;
        }

        GameObject card = Instantiate(threatCardPrefab);
        card.transform.SetParent(threatPlaces[index], false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;

        ThreatCardInstance instance = card.AddComponent<ThreatCardInstance>();
        instance.data = threat;
        Debug.Log($"[{threat.id}] minion: {threat.minion}, health: {threat.minion_health} (type: {threat.minion_health?.GetType()})");

        GameObject locationObj = locationObjects[index].gameObject;
        instance.assignedLocation = locationObj;
        

        var display = card.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.frontTexture = textureDatabase.GetTexture(villainId, threat.id);
            display.backTexture = textureDatabase.GetBackTexture(villainId);
        }

        Location location = locationObj.GetComponentInChildren<Location>();
        if (location != null)
        {
            location.AssignThreatCard(instance);
        }
        else
        {
            Debug.LogWarning($"Brak komponentu Location na obiekcie {locationObj.name}");
        }

        // 🔍 Tu debugujemy warunek
        if (threat.minion)
        {
            Debug.Log($"[{threat.id}] to minion!");

            if (!string.IsNullOrEmpty(threat.minion_health))
            {
                Debug.Log($"minion_health is not null or empty: {threat.minion_health}");

                if (int.TryParse(threat.minion_health, out int health))
                {
                    Debug.Log($"[TOKEN] Wywołuję coroutine dla {threat.id}, health: {health}");
                    StartCoroutine(SpawnMinionTokens(instance, health));
                }
                else
                {
                    Debug.LogWarning($"Nie udało się sparsować minion_health '{threat.minion_health}' dla {threat.id}");
                }
            }
            else
            {
                Debug.LogWarning($"minion_health jest puste dla {threat.id}");
            }
        }
    }

    private IEnumerator SpawnMinionTokens(ThreatCardInstance instance, int health)
{
    // ⏳ Czekaj 1 sekundę zanim zaczniesz generować żetony
    yield return new WaitForSeconds(2.5f);

    Transform slot = instance.transform.Find("Slot_Health");
    if (slot == null)
    {
        Debug.LogWarning("Brak Slot_Health na karcie!");
        yield break;
    }
    for (int i = 0; i < health; i++)
    {
        GameObject token = Instantiate(tokenHealthPrefab, slot);
        token.transform.localPosition = new Vector3(0, 0, i * 0.0000424f);
        token.transform.localRotation = Quaternion.identity;
        token.transform.localScale = Vector3.one;

        instance.currentMinionHealth++; // jeśli chcesz zliczać

        yield return new WaitForSeconds(0.2f);
    }
}


}
