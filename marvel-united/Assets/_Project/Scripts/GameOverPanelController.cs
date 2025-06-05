using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameOverPanelController : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Button playAgainButton;
    public Button backButton;

    private Canvas[] canvasesToHide;
    private Canvas ownCanvas;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(LoadHeroSelection);
        if (backButton != null)
            backButton.onClick.AddListener(LoadMainMenu);

        ownCanvas = GetComponentInParent<Canvas>(true);
        canvasesToHide = UnityEngine.Object
            .FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(c => c != ownCanvas)
            .ToArray();
    }

    public void ShowVictory()
    {
        Show("WYGRAŁEŚ!");
    }

    public void ShowDefeat()
    {
        Show("PRZEGRAŁEŚ!");
    }

    private void Show(string msg)
    {
        if (messageText != null)
            messageText.text = msg;

        if (canvasesToHide != null)
        {
            foreach (var canvas in canvasesToHide)
                if (canvas != null)
                    canvas.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
    }

    private void LoadHeroSelection()
    {
        SceneManager.LoadScene("HeroSelectionScreen");
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}