using Unity.VisualScripting;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{

    public static SoundFXManager Instance;
    [SerializeField] private AudioSource _audioSource;
    private void Awake()
    {
        if (Instance == null) { 
        Instance = this;
        }
    }

    public void PlaySoundEffect(AudioClip audioClip,Transform spawnTransform,float volume)
    {
        //spawn the gameobject
        AudioSource audioSource = Instantiate(_audioSource, spawnTransform.position,Quaternion.identity);

        //assingn the audioClip
        audioSource.clip = audioClip;

        //assign  Volume
        audioSource.volume = volume;

        //play the sound
        audioSource.Play();

        //get the length of sound FX Clip
        float clipLenght = audioSource.clip.length;

        //destroy the clip after playing clip
        Destroy(audioSource.gameObject,clipLenght );

    }
    public void PlayRandimSoundEffect(AudioClip[] audioClips,Transform spawnTransform,float volume)
    {
        //assign random index
        int rand =Random.Range(0,audioClips.Length);

        //spawn the gameobject
        AudioSource audioSource = Instantiate(_audioSource, spawnTransform.position,Quaternion.identity);

        //assingn the audioClip
        audioSource.clip = audioClips[rand];

        //assign  Volume
        audioSource.volume = volume;

        //play the sound
        audioSource.Play();

        //get the length of sound FX Clip
        float clipLenght = audioSource.clip.length;

        //destroy the clip after playing clip
        Destroy(audioSource.gameObject,clipLenght );

    }
}
