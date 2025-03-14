using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class Hero
{
    public string name;        // Nazwa bohatera
    public string description; // Opis bohatera
    public string imagePath;   // Ścieżka do obrazka bohatera
}

[System.Serializable]
public class HeroList
{
    public List<Hero> heroes;  // Lista bohaterów
}

public class HeroLoader : MonoBehaviour
{
    public TMP_Text heroNameText;      // Pole tekstowe dla nazwy bohatera (TextMeshPro)
    public TMP_Text heroDescriptionText; // Pole tekstowe dla opisu bohatera (TextMeshPro)
    public Image heroImage;            // Obrazek bohatera
    public List<Toggle> heroToggles;   // Lista toggle’ów dla wyboru bohaterów

    private Dictionary<Toggle, Hero> heroDictionary = new Dictionary<Toggle, Hero>();

    void Start()
    {
        LoadHeroesFromJSON();

        // Przypisanie listenerów do toggle'ów
        foreach (var toggle in heroToggles)
        {
            toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(toggle); });
        }
    }

    void LoadHeroesFromJSON()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("Data/Heroes/heroes");

        if (jsonText != null)
        {
            HeroList heroList = JsonUtility.FromJson<HeroList>(jsonText.text);

            // Przypisanie danych bohaterów do toggle'ów
            for (int i = 0; i < heroToggles.Count && i < heroList.heroes.Count; i++)
            {
                heroDictionary[heroToggles[i]] = heroList.heroes[i];
            }
        }
        else
        {
            Debug.LogError("Nie znaleziono pliku JSON w ścieżce Resources/Data/Heroes/heroes.json");
        }
    }

    void OnToggleValueChanged(Toggle selectedToggle)
    {
        if (selectedToggle.isOn && heroDictionary.ContainsKey(selectedToggle))
        {
            Hero hero = heroDictionary[selectedToggle];
            heroNameText.text = hero.name;
            heroDescriptionText.text = hero.description;
            
            // Załaduj obrazek z pełnej ścieżki
            if (!string.IsNullOrEmpty(hero.imagePath))
            {
                string fullPath = Path.Combine(Application.dataPath, hero.imagePath + ".png");
                if (File.Exists(fullPath))
                {
                    byte[] imageData = File.ReadAllBytes(fullPath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageData);
                    heroImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    Debug.LogError($"Nie znaleziono obrazka pod ścieżką: {fullPath}");
                }
            }
        }
    }
}
