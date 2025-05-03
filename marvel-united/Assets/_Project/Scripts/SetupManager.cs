using UnityEngine;

public class SetupManager : MonoBehaviour
{
    [Header("Prefaby postaci")]
    public GameObject playerCharacterPrefab;
    public GameObject villainPrefab;

    private LocationManager _locMan;

    private void Awake()
    {
        _locMan = FindAnyObjectByType<LocationManager>();
        if (_locMan == null)
            Debug.LogError("SetupManager: nie znaleziono LocationManager!");
        else
            _locMan.OnLocationsReady += InitSpawn;
    }

    private void OnDestroy()
    {
        if (_locMan != null)
            _locMan.OnLocationsReady -= InitSpawn;
    }

    private void InitSpawn()
    {
        var slots = _locMan.characterSlots;
        if (slots.heroSlot1 == null || slots.heroSlot2 == null || slots.villainSlot == null)
        {
            Debug.LogError("SetupManager: któryś ze slotów jest NULL!");
            return;
        }

        // Spawn bohaterów
        SpawnHero(GameManager.Instance.playerOneHero, slots.heroSlot1);
        SpawnHero(GameManager.Instance.playerTwoHero, slots.heroSlot2);

        // Spawn zbira
        SpawnVillain(GameManager.Instance.selectedVillain, slots.villainSlot);
    }

    private void SpawnHero(string heroId, Transform slot)
    {
        var go = Instantiate(playerCharacterPrefab, slot.position, Quaternion.identity, slot);
        go.GetComponent<HeroController>()?.Initialize(heroId);
    }

    private void SpawnVillain(string villainId, Transform slot)
    {
        var go = Instantiate(villainPrefab, slot.position, Quaternion.identity, slot);
        var vc = go.GetComponent<VillainController>();
        vc.Initialize(villainId, /*startIndex=*/0);
    }
}
