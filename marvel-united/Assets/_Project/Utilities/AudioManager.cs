using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;

    void Start()
    {
        // Ustawienie wartości suwaka na podstawie zapisanych ustawień
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume");
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
        }
        else
        {
            volumeSlider.value = 1f;
            SetVolume(1f);
        }

        // Podłączenie suwaka do funkcji zmieniającej głośność
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        // Konwersja wartości 0-1 na decybele (-80dB do 0dB)
        float dB = Mathf.Log10(volume) * 20;
        audioMixer.SetFloat("MusicVolume", dB);

        // Zapis ustawienia
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
}
