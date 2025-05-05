// LocationButtonAutoBinder.cs
using UnityEngine;
using UnityEngine.UI;

public class LocationButtonAutoBinder : MonoBehaviour
{
    [Header("Referencje")]
    public LocationManager locationManager;
    public Button[] locationButtons;

    void Awake()
    {
        if (locationManager == null)
            locationManager = FindAnyObjectByType<LocationManager>();
        if (locationManager != null)
            locationManager.OnLocationsReady += BindButtons;
        else
            Debug.LogError("LocationButtonAutoBinder: nie znaleziono LocationManager");
    }

    void OnDestroy()
    {
        if (locationManager != null)
            locationManager.OnLocationsReady -= BindButtons;
    }

    private void BindButtons()
    {
        ClearButtonBindings();
        var roots = locationManager.LocationRoots;
        int count = Mathf.Min(locationButtons.Length, roots.Count);

        for (int i = 0; i < count; i++)
        {
            var btn = locationButtons[i];
            var handler = btn.GetComponent<LocationButtonHandler>();
            if (handler == null)
            {
                Debug.LogWarning($"Brakuje LocationButtonHandler na przycisku {i}");
                continue;
            }
            handler.locationObject = roots[i].gameObject;
        }

        Debug.Log("LocationButtonAutoBinder: przypisano przyciski");
    }

    private void ClearButtonBindings()
    {
        foreach (var btn in locationButtons)
        {
            var handler = btn.GetComponent<LocationButtonHandler>();
            if (handler != null)
                handler.locationObject = null;
        }
    }
        public void RebindButtons()
    {
        BindButtons();
    }
}