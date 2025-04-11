using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class VillainToggleGenerator : MonoBehaviour
{
    [System.Serializable]
    private class VillainData
    {
        public List<Villain> villains;
    }

    [System.Serializable]
    private class Villain
    {
        public string id;
        public string name;
        public Dictionary<string, int> health_per_players;
        public string bam_effect;
        public string overflow;
        public string villainous_plot;
        public bool additional_win_condition;
        public string additional_win_condition_script;
        public string imagePath;
    }

    public GameObject togglePrefab;
    public Transform toggleParent;
    public string jsonFileName = "Villains.json";
    public Sprite defaultSprite;

    public Image villainLargeImage;
    public TMP_Text villainNameText;
    public TMP_Text villainHealthText;
    public TMP_Text villainBamEffectText;
    public TMP_Text villainOverflowText;
    public TMP_Text villainWinConditionText;

    public Button confirmButton;
    public Button playButton;

    private Dictionary<Toggle, Villain> villainToggles = new Dictionary<Toggle, Villain>();
    private string selectedVillainId = "";

    private void Start()
    {
        LoadVillains();
        playButton.gameObject.SetActive(false); // Ukrycie "Play" na starcie
        confirmButton.onClick.AddListener(ConfirmSelection);
        playButton.onClick.AddListener(StartGame);
    }

    private void LoadVillains()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError("Plik JSON nie został znaleziony: " + filePath);
            return;
        }

        string jsonContent = File.ReadAllText(filePath);
        VillainData villainData = JsonConvert.DeserializeObject<VillainData>(jsonContent);

        foreach (Villain villain in villainData.villains)
        {
            CreateVillainToggle(villain);
        }
    }

    private void CreateVillainToggle(Villain villain)
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
            toggleText.text = villain.name;
        }

        Image villainImage = toggleObject.transform.Find("VillainImage")?.GetComponent<Image>();
        if (villainImage != null)
        {
            Sprite loadedSprite = LoadSprite(villain.imagePath);
            villainImage.sprite = loadedSprite != null ? loadedSprite : defaultSprite;
        }

        toggle.onValueChanged.AddListener(delegate { OnToggleSelected(toggle); });

        villainToggles[toggle] = villain;
    }

    private void OnToggleSelected(Toggle selectedToggle)
    {
        if (!villainToggles.ContainsKey(selectedToggle)) return;
        
        Villain selectedVillain = villainToggles[selectedToggle];
        selectedVillainId = selectedVillain.id;

        // Aktualizacja UI z informacjami o Villainie
        if (villainLargeImage != null)
        {
            Sprite loadedSprite = LoadSprite(selectedVillain.imagePath);
            villainLargeImage.sprite = loadedSprite != null ? loadedSprite : defaultSprite;
        }

        if (villainNameText != null)
        {
            villainNameText.text = selectedVillain.name;
        }

        if (villainHealthText != null)
        {
            int healthForTwoPlayers = selectedVillain.health_per_players.ContainsKey("2") ? selectedVillain.health_per_players["2"] : 0;
            villainHealthText.text = $"Health (2 Players): {healthForTwoPlayers}";
        }

        if (villainBamEffectText != null)
        {
            villainBamEffectText.text = $"BAM Effect: {selectedVillain.bam_effect}";
        }

        if (villainOverflowText != null)
        {
            villainOverflowText.text = $"Overflow: {selectedVillain.overflow}";
        }

        if (villainWinConditionText != null)
        {
            string winCondition = selectedVillain.additional_win_condition ? selectedVillain.villainous_plot : "Standard Victory Condition";
            villainWinConditionText.text = $"Win Condition: {winCondition}";
        }
    }

    private void ConfirmSelection()
    {
        if (string.IsNullOrEmpty(selectedVillainId))
        {
            Debug.LogWarning("Nie wybrano Villaina!");
            return;
        }
        // Zapisanie Villaina w GameManager
        GameManager.Instance.selectedVillain = selectedVillainId;

        // Ukrycie "Confirm", pokazanie "Play"
        confirmButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(true);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // Wstaw poprawną nazwę sceny
    }

    private Sprite LoadSprite(string imagePath)
    {
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
