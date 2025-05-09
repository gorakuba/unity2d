using UnityEngine;

public class SetupManager : MonoBehaviour
{
    [Header("Prefaby postaci")]
    public GameObject playerCharacterPrefab;
    public GameObject villainPrefab;

    private LocationManager _locMan;

    public static HeroController hero1Controller;
    public static HeroController hero2Controller;
    public static VillainController villainController;

    [Header("Hero Movement System")]
    public HeroMovementManager movementManager;
    

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

        hero1Controller = SpawnHero(GameManager.Instance.playerOneHero, slots.heroSlot1, false);
        hero2Controller = SpawnHero(GameManager.Instance.playerTwoHero, slots.heroSlot2, true);
        villainController = SpawnVillain(GameManager.Instance.selectedVillain, slots.villainSlot);

        if (movementManager != null)
        {
            movementManager.playerOneObject = hero1Controller.transform;
            movementManager.playerTwoObject = hero2Controller.transform;
        }
        else
        {
            Debug.LogError("SetupManager: Brakuje referencji do HeroMovementManager!");
        }
    }

    private HeroController SpawnHero(string heroId, Transform slot, bool isPlayerTwo)
{
    var go = Instantiate(playerCharacterPrefab, slot.position, Quaternion.identity, slot);
    var hc = go.GetComponent<HeroController>();
    var cm = FindAnyObjectByType<CardManager>();

    hc.Initialize(heroId, GameManager.Instance, cm, isPlayerTwo);

    return hc;
}


    private VillainController SpawnVillain(string villainId, Transform slot)
    {
        var go = Instantiate(villainPrefab, slot.position, Quaternion.identity, slot);
        var vc = go.GetComponent<VillainController>();
        vc.Initialize(villainId, 0);
        return vc;
    }
}
