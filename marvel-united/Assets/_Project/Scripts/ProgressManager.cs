using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerProgress
{
    public int wins = 0;
    public List<string> unlockedHeroes = new List<string>();
    public List<string> unlockedVillains = new List<string>();
}

public static class ProgressManager
{
    public static PlayerProgress Progress = new PlayerProgress();

    private static string FilePath => Path.Combine(Application.streamingAssetsPath, "PlayerProgress.json");

    public static void LoadProgress()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            Progress = JsonUtility.FromJson<PlayerProgress>(json);
        }
        else
        {
            Progress = new PlayerProgress();
            SaveProgress();
        }
    }

    public static void SaveProgress()
    {
        string json = JsonUtility.ToJson(Progress, true);
        File.WriteAllText(FilePath, json);
    }
}