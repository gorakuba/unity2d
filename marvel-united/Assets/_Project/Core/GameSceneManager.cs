using UnityEngine;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    public Image player1Image;
    public Image player2Image;
    public Sprite[] characterSprites; // Ta sama tablica sprite'ów

    void Start()
    {
        int player1ID = PlayerPrefs.GetInt("Player1Character", -1); // -1 = brak wyboru
        int player2ID = PlayerPrefs.GetInt("Player2Character", -1);

        if (player1ID != -1) player1Image.sprite = characterSprites[player1ID];
        if (player2ID != -1) player2Image.sprite = characterSprites[player2ID];
    }
}
