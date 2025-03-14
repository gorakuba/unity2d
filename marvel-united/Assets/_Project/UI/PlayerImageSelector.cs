using UnityEngine;
using UnityEngine.UI;

public class PlayerImageSelector : MonoBehaviour
{
    public ToggleGroup toggleGroup;
    public Toggle[] toggles;
    public Image switchableImage1;
    public Image switchableImage2;
    public Sprite[] characterSprites;
    public Button startButton; // PRZYCISK PRZEJŚCIA DO GRY

    private Toggle selectedToggle1 = null;
    private Toggle selectedToggle2 = null;

    void Start()
    {
        foreach (Toggle toggle in toggles)
        {
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener(delegate { OnToggleChanged(toggle); });
        }

        startButton.interactable = false; // Wyłącz przycisk na starcie
    }

    void OnToggleChanged(Toggle changedToggle)
    {
        int index = System.Array.IndexOf(toggles, changedToggle);

        if (!changedToggle.isOn)
        {
            if (selectedToggle1 == changedToggle)
            {
                selectedToggle1 = null;
                switchableImage1.sprite = null;
                PlayerPrefs.DeleteKey("Player1Character");
            }
            else if (selectedToggle2 == changedToggle)
            {
                selectedToggle2 = null;
                switchableImage2.sprite = null;
                PlayerPrefs.DeleteKey("Player2Character");
            }

            CheckButtonState(); // Sprawdź, czy przycisk powinien być aktywny
            return;
        }

        if (selectedToggle1 == changedToggle || selectedToggle2 == changedToggle)
        {
            changedToggle.isOn = false;
            return;
        }

        if (selectedToggle1 == null)
        {
            selectedToggle1 = changedToggle;
            switchableImage1.sprite = characterSprites[index];
            PlayerPrefs.SetInt("Player1Character", index);
        }
        else if (selectedToggle2 == null)
        {
            selectedToggle2 = changedToggle;
            switchableImage2.sprite = characterSprites[index];
            PlayerPrefs.SetInt("Player2Character", index);
        }
        else
        {
            selectedToggle2.isOn = false;
            selectedToggle2 = changedToggle;
            switchableImage2.sprite = characterSprites[index];
            PlayerPrefs.SetInt("Player2Character", index);
        }

        PlayerPrefs.Save();
        CheckButtonState(); // Sprawdź stan przycisku po każdym wyborze
    }

    void CheckButtonState()
    {
        // Przyciskiem można przejść dalej tylko, jeśli obaj gracze mają wybrane postacie
        startButton.interactable = (selectedToggle1 != null && selectedToggle2 != null);
    }
}
