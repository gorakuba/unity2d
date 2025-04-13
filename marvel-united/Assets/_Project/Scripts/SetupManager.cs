using UnityEngine;

public class SetupManager : MonoBehaviour
{
    public GameObject playerCharacterPrefab;
    public GameObject villainPrefab;
private void Awake()
{
    LocationManager locMan = FindAnyObjectByType<LocationManager>();
    locMan.OnLocationsReady += InitSpawn;
}
void InitSpawn()
{
    CharacterSlots slots = FindAnyObjectByType<LocationManager>().GetCharacterSlots();

    if (slots.heroSlot1 == null || slots.heroSlot2 == null || slots.villainSlot == null)
    {
        Debug.LogError("❌ Jeden z slotów postaci jest NULL!");
        return;
    }

    SpawnHero(GameManager.Instance.playerOneHero, slots.heroSlot1);
    SpawnHero(GameManager.Instance.playerTwoHero, slots.heroSlot2);
    SpawnVillain(GameManager.Instance.selectedVillain, slots.villainSlot);
}
    void SpawnHero(string heroName, Transform location)
    {
        GameObject heroGO = Instantiate(playerCharacterPrefab, location.position, Quaternion.identity, location);
        heroGO.GetComponent<HeroController>()?.Initialize(heroName);
    }

void SpawnVillain(string villainName, Transform location)
{
    GameObject villainGO = Instantiate(villainPrefab, location.position, Quaternion.identity, location);
    villainGO.GetComponent<VillainController>()?.Initialize(villainName);
}
}
