using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class LocationButtonAutoBinder : MonoBehaviour
{
    public LocationInfoPanel infoPanel;
    public LocationManager locationManager;
    public Button[] locationButtons;
        public GameObject locationObject;

private void Start()
{
    ClearButtonBindings(); // wyzeruj stare przypisania
    StartCoroutine(InitButtonsWithDelay());

}

private IEnumerator InitButtonsWithDelay()
{
    
    yield return new WaitForSeconds(5f); // ⏱️ Czekamy 5 sekund

    var locationManager = FindAnyObjectByType<LocationManager>();
    if (locationManager == null)
    {
        Debug.LogError("Nie znaleziono LocationManager!");
        yield break;
    }

    if (locationButtons.Length != locationManager.spawnedLocationTransforms.Count)
    {
        Debug.LogError($"Mismatch: {locationButtons.Length} przycisków, " +
                       $"{locationManager.spawnedLocationTransforms.Count} lokacji!");
    }

    for (int i = 0; i < locationButtons.Length; i++)
    {
        if (i >= locationManager.spawnedLocationTransforms.Count)
        {
            Debug.LogError($"❌ Brakuje zespawnowanej lokacji dla przycisku {i + 1}");
            continue;
        }

        var handler = locationButtons[i].GetComponent<LocationButtonHandler>();
        if (handler == null)
        {
            Debug.LogError($"❌ Brakuje LocationButtonHandler na przycisku {i + 1}");
            continue;
        }

        handler.locationObject = locationManager.spawnedLocationTransforms[i].gameObject;
        Debug.Log($"✅ Przypisano lokację {handler.locationObject.name} do przycisku {i + 1}");
    }
}
public void RebindButtons()
{
    StartCoroutine(InitButtonsWithDelay());
}
public void ClearButtonBindings()
{
    foreach (var button in locationButtons)
    {
        var handler = button.GetComponent<LocationButtonHandler>();
        if (handler != null)
        {
            handler.locationObject = null;
        }
    }
}
}
