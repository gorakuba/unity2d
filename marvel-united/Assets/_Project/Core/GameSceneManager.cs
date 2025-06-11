using UnityEngine;
using UnityEngine.UI;

// Handles loading and saving of player progress.
// ProgressManager is responsible for reading a JSON file with unlocked characters.

public class GameSceneManager : MonoBehaviour
{
    public Image player1Image;
    public Image player2Image;
    public Sprite[] characterSprites; // Ta sama tablica sprite'ów

    void Start()
    {
        ProgressManager.LoadProgress();
        int player1ID = PlayerPrefs.GetInt("Player1Character", -1); // -1 = brak wyboru
        int player2ID = PlayerPrefs.GetInt("Player2Character", -1);

        if (player1ID != -1) player1Image.sprite = characterSprites[player1ID];
        if (player2ID != -1) player2Image.sprite = characterSprites[player2ID];
    }
    
    public void UnlockHero(string heroId)
    {
        if (!ProgressManager.Progress.unlockedHeroes.Contains(heroId))
        {
            ProgressManager.Progress.unlockedHeroes.Add(heroId);
            ProgressManager.SaveProgress();
        }
    }

    public void UnlockVillain(string villainId)
    {
        if (!ProgressManager.Progress.unlockedVillains.Contains(villainId))
        {
            ProgressManager.Progress.unlockedVillains.Add(villainId);
            ProgressManager.SaveProgress();
        }
    }
}
