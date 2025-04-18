using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public class ThreatCardInstance : MonoBehaviour
{
    public ThreatCard data;
    public GameObject assignedLocation;
    public int currentMinionHealth = 0;

    private void Start()
    {
        if (data != null)
            StartCoroutine(LoadThreatSpriteAsync(data));
    }

    private IEnumerator LoadThreatSpriteAsync(ThreatCard card)
{
    string villainId = GameManager.Instance.selectedVillain;
    string address = GetThreatSpriteAddress(villainId, card.id);

    var handle = Addressables.LoadAssetAsync<Sprite>(address);
    yield return handle;

    if (handle.Status == AsyncOperationStatus.Succeeded)
    {
        card.sprite = handle.Result;
        Debug.Log($"✅ Załadowano sprite threat: {address}");
    }
    else
    {
        Debug.LogError($"❌ Nie udało się załadować sprite'a: {address}");
    }
}
private string GetThreatSpriteAddress(string villainId, string cardId)
{
    // Threat ID: "threat_4" → indeks: "4"
    string index = cardId.Split('_')[1]; // "4"
    return $"Villain/{villainId}/Threats/Card_{index}";
}
}
