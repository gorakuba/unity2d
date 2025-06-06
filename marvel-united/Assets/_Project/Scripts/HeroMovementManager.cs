using System.Collections.Generic;
using UnityEngine;

public class HeroMovementManager : MonoBehaviour
{
    public Transform playerOneObject;
    public Transform playerTwoObject;
    public System.Action OnMoveCompleted;

    private Transform currentHero => GameManager.Instance.CurrentPlayerIndex == 1 ? playerOneObject : playerTwoObject;
    private int playerIndex => GameManager.Instance.CurrentPlayerIndex;

    private LocationController currentLocation;
    public void PrepareHeroMovement()
    {
        UpdateCurrentLocation();

        if (currentLocation == null)
        {
            Debug.LogWarning("Nie znaleziono aktualnej lokacji gracza!");
            return;
        }

        foreach (var neighbor in currentLocation.neighbors)
            neighbor.EnableMoveButton(OnMoveSelected);
    }
    public void PrepareHeroTeleport()
    {
        UpdateCurrentLocation();

        if (currentLocation == null)
        {
            Debug.LogWarning("Nie znaleziono aktualnej lokacji gracza!");
            return;
        }

        foreach (var loc in GameManager.Instance.locationManager.LocationRoots)
        {
            var ctrl = loc.GetComponent<LocationController>();
            if (ctrl != null)
                ctrl.EnableMoveButton(OnMoveSelected);
        }
    }
    private void OnMoveSelected(LocationController newLocation)
    {
        DisableAllMoveButtons();

        if (currentHero == null)
        {
            Debug.LogError("Nie przypisano obiektu bohatera!");
            return;
        }

        Transform slot = newLocation.GetHeroSlot(playerIndex);
        currentHero.SetParent(slot);          // ← nowy parent
        currentHero.localPosition = Vector3.zero;  // ← ustalona pozycja względem slotu
        currentHero.localRotation = Quaternion.identity; // ← reset rotacji jeśli trzeba

        var heroCtrl = currentHero.GetComponent<HeroController>();
        
        if (heroCtrl != null)
            heroCtrl.CurrentLocation = newLocation;
            Debug.Log($"[HeroMovement] Bohater {heroCtrl.HeroId} przesunięty do lokacji {newLocation.name}");

        if (playerIndex == 1)
            GameManager.Instance.locationManager.characterSlots.heroSlot1 = slot;
        else
            GameManager.Instance.locationManager.characterSlots.heroSlot2 = slot;

        currentLocation = newLocation;
        OnMoveCompleted?.Invoke();
    }
    private void DisableAllMoveButtons()
    {
        var locMan = GameManager.Instance?.locationManager;
        if (locMan == null) return;

        foreach (var loc in locMan.LocationRoots)
        {
            var ctrl = loc.GetComponent<LocationController>();
            if (ctrl != null)
                ctrl.DisableMoveButton();
        }
    }

    public void TeleportHero(HeroController hero, LocationController newLocation)
    {
        if (hero == null || newLocation == null) return;

        int index = hero == SetupManager.hero1Controller ? 1 : 2;
        Transform slot = newLocation.GetHeroSlot(index);
        hero.transform.SetParent(slot);
        hero.transform.localPosition = Vector3.zero;
        hero.transform.localRotation = Quaternion.identity;
        hero.CurrentLocation = newLocation;

        if (index == 1)
            GameManager.Instance.locationManager.characterSlots.heroSlot1 = slot;
        else
            GameManager.Instance.locationManager.characterSlots.heroSlot2 = slot;
    }

    private void UpdateCurrentLocation()
    {
        var slot = playerIndex == 1
            ? GameManager.Instance.locationManager.characterSlots.heroSlot1
            : GameManager.Instance.locationManager.characterSlots.heroSlot2;

        foreach (var loc in GameManager.Instance.locationManager.LocationRoots)
        {
            var ctrl = loc.GetComponent<LocationController>();
            if (ctrl == null) continue;

            var s = ctrl.GetHeroSlot(playerIndex);
            if (s == slot)
            {
                currentLocation = ctrl;
                return;
            }
        }
    }
    public void CancelHeroMovement()
    {
        if (currentLocation == null) return;

        DisableAllMoveButtons();
    }
        public LocationController GetCurrentLocation()
        {
            var slot = playerIndex == 1
                ? GameManager.Instance.locationManager.characterSlots.heroSlot1
                : GameManager.Instance.locationManager.characterSlots.heroSlot2;

            foreach (var loc in GameManager.Instance.locationManager.LocationRoots)
            {
                var ctrl = loc.GetComponent<LocationController>();
                if (ctrl == null) continue;

                var heroSlot = ctrl.GetHeroSlot(playerIndex);
                if (heroSlot == slot)
                    return ctrl;
            }

            Debug.LogWarning("GetCurrentLocation: nie znaleziono lokacji dla aktywnego gracza!");
            return null;
        }



}
