using UnityEngine;
using UnityEngine.UI;

public class LastCardsPanelUI : MonoBehaviour
{
    public static LastCardsPanelUI Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject panelRoot;
    public Image currentCardImage;
    public Image previousCardImage;

    private void Awake()
    {
        Instance = this;

        if (panelRoot == null)
            panelRoot = GameManager.Instance?.FindObjectInScene("LastCardsPanel");
        if (currentCardImage == null)
        {
            var go = GameManager.Instance?.FindObjectInScene("Card_played");
            currentCardImage = go != null ? go.GetComponent<Image>() : null;
        }
        if (previousCardImage == null)
        {
            var go = GameManager.Instance?.FindObjectInScene("Card_played_before");
            previousCardImage = go != null ? go.GetComponent<Image>() : null;
        }

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show(Sprite currentCard, Sprite previousCard)
    {
        if (panelRoot == null) return;

        if (currentCardImage != null)
        {
            currentCardImage.sprite = currentCard;
            currentCardImage.preserveAspect = true;
        }

        if (previousCardImage != null)
        {
            if (previousCard != null)
            {
                previousCardImage.sprite = previousCard;
                previousCardImage.preserveAspect = true;
                previousCardImage.gameObject.SetActive(true);
            }
            else
            {
                previousCardImage.gameObject.SetActive(false);
            }
        }

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}