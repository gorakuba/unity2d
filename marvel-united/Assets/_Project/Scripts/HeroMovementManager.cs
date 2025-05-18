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

    private void OnMoveSelected(LocationController newLocation)
    {
        foreach (var neighbor in currentLocation.neighbors)
            neighbor.DisableMoveButton();

        if (currentHero == null)
        {
            Debug.LogError("Nie przypisano obiektu bohatera!");
            return;
        }

        Transform slot = newLocation.GetHeroSlot(playerIndex);
        currentHero.SetParent(slot);          // ← nowy parent
        currentHero.localPosition = Vector3.zero;  // ← ustalona pozycja względem slotu
        currentHero.localRotation = Quaternion.identity; // ← reset rotacji jeśli trzeba

        if (playerIndex == 1)
            GameManager.Instance.locationManager.characterSlots.heroSlot1 = slot;
        else
            GameManager.Instance.locationManager.characterSlots.heroSlot2 = slot;

        currentLocation = newLocation;
        OnMoveCompleted?.Invoke();
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

        foreach (var neighbor in currentLocation.neighbors)
            neighbor.DisableMoveButton();
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
