using UnityEngine;

public class LocationButtonHandler : MonoBehaviour
{
    public LocationInfoPanel infoPanel;
    public GameObject locationObject;

    public void OnClick()
    {
        if (locationObject == null)
        {
            Debug.LogError("❌ Brak przypisanego LocationObject!");
            return;
        }

        // Automatyczne debugowanie komponentów
        foreach (var comp in locationObject.GetComponents<MonoBehaviour>())
        {
            Debug.Log("🧩 Komponent: " + comp.GetType().Name);
        }

        // Spróbuj znaleźć skrypty
        var dataHolder = locationObject.GetComponent<LocationDataHolder>();
        var location = locationObject.GetComponent<Location>();

        if (dataHolder == null || location == null)
        {
            Debug.LogError("❌ Nie znaleziono komponentów DataHolder lub Location");
            return;
        }

        // Ręczne podejrzenie pól - popraw je jeśli inaczej się nazywają
        var locationData = dataHolder.data; // <- popraw, jeśli nie masz `data`, tylko np. `Data`
        var threat = location.currentThreat; // <- popraw, jeśli inaczej nazwane

        if (threat == null)
        {
            Debug.LogWarning("⚠️ Brak Threat w tej lokacji");
            return;
        }

        // Debuguj zawartość
        Debug.Log($"✅ SHOW: {locationData.name} | Threat: {threat.data.name}");

        // Wywołanie panelu
        infoPanel.Show(locationData, threat.data, locationData.sprite, threat.data.sprite);
    }
}
