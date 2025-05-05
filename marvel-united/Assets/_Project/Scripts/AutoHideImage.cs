using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AutoHideImage : MonoBehaviour
{
    public float displayTime = 2f;
    public Image image; // <== to przypisujemy w Inspectorze

    public void ShowForDuration(Sprite sprite)
    {
        image.sprite = sprite;
        gameObject.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        gameObject.SetActive(false);
    }
}
