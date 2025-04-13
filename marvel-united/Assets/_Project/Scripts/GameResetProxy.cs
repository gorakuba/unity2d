using UnityEngine;

public class GameResetProxy : MonoBehaviour
{
    public void CallResetGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null!");
        }
    }
}
