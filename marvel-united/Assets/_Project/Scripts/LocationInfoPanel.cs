using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationInfoPanel : MonoBehaviour
{
    public GameObject panelRoot;

    public Image locationImage;
    public TMP_Text locationNameText;

    public Image threatImage;
    public TMP_Text threatNameText;

   public void Show(LocationData location, ThreatCard threat, Sprite locationSprite, Sprite threatSprite)
{
    Debug.Log($"🧪 SHOW: {location.name} | Sprite: {locationSprite}");
    Debug.Log($"🧪 THREAT: {threat.name} | Sprite: {threatSprite}");

    locationImage.sprite = locationSprite;
    locationImage.color = Color.white;

    threatImage.sprite = threatSprite;
    threatImage.color = Color.white;

    locationNameText.text = location.name;
    threatNameText.text = threat.name;

    panelRoot.SetActive(true);
}

    public void Hide()
    {
        panelRoot.SetActive(false);
    }
}
