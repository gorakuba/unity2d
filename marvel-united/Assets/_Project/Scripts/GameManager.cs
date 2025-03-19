using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string playerOneHero;
    public string playerTwoHero;
    public string selectedVillain; // 🔹 Nowa zmienna dla Villaina

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Zapewnia, że GameManager nie zniknie po zmianie sceny
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
