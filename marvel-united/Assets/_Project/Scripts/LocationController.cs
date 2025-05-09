using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationController : MonoBehaviour
{
    [Tooltip("Sąsiednie lokacje (clockwise i counter-clockwise)")]
    public List<LocationController> neighbors;

    [Header("Przycisk ruchu (ustaw w prefabie)")]
    public Button moveButton;

    [Header("Efekty cząsteczkowe (opcjonalne)")]
    public GameObject moveParticles;

    private System.Action<LocationController> onMoveCallback;

    public void EnableMoveButton(System.Action<LocationController> callback)
    {
        if (moveButton == null) return;

        moveButton.gameObject.SetActive(true);
        if (moveParticles != null) moveParticles.SetActive(true);

        onMoveCallback = callback;
        moveButton.onClick.RemoveAllListeners();
        moveButton.onClick.AddListener(() => onMoveCallback?.Invoke(this));
    }

    public void DisableMoveButton()
    {
        if (moveButton == null) return;

        moveButton.onClick.RemoveAllListeners();
        moveButton.gameObject.SetActive(false);
        if (moveParticles != null) moveParticles.SetActive(false);
    }

    public Transform GetHeroSlot(int playerIndex)
    {
        return transform.Find(playerIndex == 1 ? "Hero_Slot_1" : "Hero_Slot_2");
    }
}
