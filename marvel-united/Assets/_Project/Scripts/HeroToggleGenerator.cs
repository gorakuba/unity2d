using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class HeroToggleGenerator : MonoBehaviour
{
    [System.Serializable]
    private class HeroData
    {
        public List<Hero> heroes;
    }

    [System.Serializable]
    private class Hero
    {
        public string id;
        public string name;
        public string description;
        public string imagepath;
    }

    public GameObject togglePrefab;
    public Transform toggleParent;
    public string jsonFileName = "Heroes.json";
    public Sprite defaultSprite;

    public Image heroLargeImage;
    public TMP_Text heroNameText;
    public TMP_Text heroDescriptionText;
    public Image playerOneHeroImage;
    public Image playerTwoHeroImage;

    public Button confirmButton;  // "Next" / "Graj"
    public Button backButton;     // "Back"
    public Button playButton;     // "Graj" (ukryty na start)

    private Dictionary<Toggle, Hero> heroToggles = new Dictionary<Toggle, Hero>();
    private bool isSelectingPlayerOne = true;
    private string playerOneSelectedHeroId = "";
    private string playerTwoSelectedHeroId = "";
    private bool playerTwoConfirmed = false;

    private void Start()
    {
        ProgressManager.LoadProgress();
        LoadHeroes();
        ResetToggles();
        ClearPreviousSelections();  

        confirmButton.onClick.AddListener(ConfirmSelection);
        backButton.onClick.AddListener(GoBack);
        playButton.onClick.AddListener(StartGame);

        backButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
    }

    private void LoadHeroes()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError("Plik JSON nie został znaleziony: " + filePath);
            return;
        }

        string jsonContent = File.ReadAllText(filePath);
        HeroData heroData = JsonConvert.DeserializeObject<HeroData>(jsonContent);

        foreach (Hero hero in heroData.heroes)
        {
            CreateHeroToggle(hero);
        }
    }

    private void CreateHeroToggle(Hero hero)
    {
        GameObject toggleObject = Instantiate(togglePrefab, toggleParent);
        Toggle toggle = toggleObject.GetComponent<Toggle>();

        if (toggle == null)
        {
            Debug.LogError("Prefab nie zawiera komponentu Toggle!");
            return;
        }

        TMP_Text toggleText = toggleObject.GetComponentInChildren<TMP_Text>();
        if (toggleText != null)
        {
            toggleText.text = hero.name;
        }

        Image heroImage = toggleObject.transform.Find("HeroImage")?.GetComponent<Image>();
        if (heroImage != null)
        {
            Sprite loadedSprite = LoadSprite(hero.imagepath);
            heroImage.sprite = loadedSprite != null ? loadedSprite : defaultSprite;
        }

        toggle.interactable = !IsHeroLocked(hero.id);

        toggle.onValueChanged.AddListener(delegate { OnToggleSelected(toggle); });

        heroToggles[toggle] = hero;
    }

    private void ResetToggles()
    {
        foreach (var pair in heroToggles)
        {
            pair.Key.isOn = false;
            pair.Key.interactable = !IsHeroLocked(pair.Value.id);
        }

        if (isSelectingPlayerOne && !string.IsNullOrEmpty(playerTwoSelectedHeroId))
        {
            DisableHeroToggle(playerTwoSelectedHeroId);
        }
        else if (!isSelectingPlayerOne && !string.IsNullOrEmpty(playerOneSelectedHeroId))
        {
            DisableHeroToggle(playerOneSelectedHeroId);
        }
    }

    private void DisableHeroToggle(string heroId)
    {
        foreach (var pair in heroToggles)
        {
            if (pair.Value.id == heroId)
            {
                pair.Key.interactable = false;
            }
        }
    }

    private bool IsHeroLocked(string heroId)
    {
        return (heroId == "spider-man" || heroId == "wasp") &&
               !ProgressManager.Progress.unlockedHeroes.Contains(heroId);
    }
    private void OnToggleSelected(Toggle selectedToggle)
    {
        if (!heroToggles.ContainsKey(selectedToggle)) return;
        
        Hero selectedHero = heroToggles[selectedToggle];

        if (heroLargeImage != null)
        {
            Sprite loadedSprite = LoadSprite(selectedHero.imagepath);
            heroLargeImage.sprite = loadedSprite != null ? loadedSprite : defaultSprite;
        }

        if (heroNameText != null)
        {
            heroNameText.text = selectedHero.name;
        }

        if (heroDescriptionText != null)
        {
            heroDescriptionText.text = selectedHero.description;
        }
    }

    private void ConfirmSelection()
    {
        foreach (var pair in heroToggles)
        {
            if (pair.Key.isOn)
            {
                if (isSelectingPlayerOne)
                {
                    playerOneSelectedHeroId = pair.Value.id;
                    UpdatePlayerOneHeroUI(pair.Value.imagepath);
                    isSelectingPlayerOne = false;
                    ResetToggles();
                    backButton.gameObject.SetActive(true);
                }
                else
                {
                    playerTwoSelectedHeroId = pair.Value.id;
                    UpdatePlayerTwoHeroUI(pair.Value.imagepath);
                    playerTwoConfirmed = true;
                    confirmButton.gameObject.SetActive(false);
                    playButton.gameObject.SetActive(true);

                    Debug.Log($"Gracz pierwszy wybrał: {playerOneSelectedHeroId}");
                    Debug.Log($"Gracz drugi wybrał: {playerTwoSelectedHeroId}");
                }
                return;
            }
        }

        Debug.LogWarning("Nie wybrano postaci!");
    }

    private void GoBack()
    {
        if (playerTwoConfirmed)
        {
            // Cofamy wybór gracza 2
            playerTwoSelectedHeroId = "";
            playerTwoConfirmed = false;
            UpdatePlayerTwoHeroUI(null);
            confirmButton.gameObject.SetActive(true);
            playButton.gameObject.SetActive(false);
            ResetToggles();
        }
        else if (!playerTwoConfirmed && !isSelectingPlayerOne)
        {
            // Powrót do wyboru gracza 1, resetując wybór gracza 1
            playerOneSelectedHeroId = "";
            UpdatePlayerOneHeroUI(null);
            isSelectingPlayerOne = true;
            backButton.gameObject.SetActive(false);
            ResetToggles();
        }
    }

    private void StartGame()
    {
        GameManager.Instance.playerOneHero = playerOneSelectedHeroId;
        GameManager.Instance.playerTwoHero = playerTwoSelectedHeroId;
        SceneManager.LoadScene("VillainSelectionScreen"); // Wstaw właściwą nazwę sceny
    }

    private void UpdatePlayerOneHeroUI(string imagePath)
    {
        if (playerOneHeroImage != null)
        {
            playerOneHeroImage.sprite = LoadSprite(imagePath) ?? defaultSprite;
        }
    }

    private void UpdatePlayerTwoHeroUI(string imagePath)
    {
        if (playerTwoHeroImage != null)
        {
            playerTwoHeroImage.sprite = LoadSprite(imagePath) ?? defaultSprite;
        }
    }

    private void ClearPreviousSelections()
    {
        playerOneSelectedHeroId = "";
        playerTwoSelectedHeroId = "";
        playerTwoConfirmed = false;
    }

    private Sprite LoadSprite(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath)) return null;

        string fullPath = Path.Combine(Application.streamingAssetsPath, imagePath + ".png");

        if (File.Exists(fullPath))
        {
            byte[] imageData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning("Obrazek nie znaleziony: " + fullPath);
            return null;
        }
    }
}
