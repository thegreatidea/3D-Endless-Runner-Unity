using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMasterManager : MonoBehaviour
{ 
    [SerializeField]private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;


    private void Start()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMasterVolume();
            SetMusicVolume();
            SetSFXVolume();
            
        }
    }
    public void SetMasterVolume()
    {
        float volume =masterSlider.value;
        audioMixer.SetFloat("masterVolume", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("MasterVolume",volume);
    }
    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SoundFXVolume", volume);
    }

    private void LoadVolume()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SoundFXVolume");
        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }
}
