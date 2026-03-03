using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private MusicList[] musicList;
    [HideInInspector] public AudioSource audioSource;
    public static MusicManager instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        if (instance == null)
        {
            instance = this;
        }
        
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        PlayMusicForScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public static void PlaySound(MusicType music, float volume = 1.0f)
    {
        StopMusic();
        AudioClip[] clips = instance.musicList[(int)music].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if (randomClip == null) return;
        instance.audioSource.clip = randomClip;
        instance.audioSource.volume = volume;
        instance.audioSource.Play();

        Debug.Log($"current Music: {randomClip.name}");
    }

    private void PlayMusicForScene(int sceneIndex)
    {
        switch (sceneIndex)
        {
            case 1:
                PlaySound(MusicType.MAINMENU, 0.3f);
                break;
            default:
                PlaySound(MusicType.INGAME, 0.3f);
                break;
        }
        switch (SceneManager.GetActiveScene().name)
        {
            case "Death_Screen": // during the death screen there shouldn't be any music
                StopMusic();
                break;
            case "Starting_Video_Scene": // during the starting video there shouldn't be any music
                StopMusic();
                break;
            case "Tutorial_Video": // during the tutorial video there shouldn't be any music
                StopMusic();
                break;
            case "Credits":
                StopMusic();
                PlaySound(MusicType.MAINMENU, 0.3f);
                break;
            default:
                break;

        }
    }

    public static void PauseMusic()
    {
        instance.audioSource.Pause();
    }

    public static void ResumeMusic()
    {
        instance.audioSource.UnPause();
    }

    public static void StopMusic()
    {
        instance.audioSource.Stop();
    }

    public static void PlaySoundWithDelay(MusicType music, float volume = 1.0f, float delay = 0f)
    {
        instance.StartCoroutine(PlaySoundCoroutine(music, volume, delay));
    }

    private static IEnumerator PlaySoundCoroutine(MusicType music, float volume = 1.0f, float delay = 0f)
    {
        AudioClip[] clips = instance.musicList[(int)music].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        yield return new WaitForSeconds(delay);
        instance.audioSource.PlayOneShot(randomClip, volume);
        Debug.Log(randomClip.name);
    }
    

    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(MusicType)); 
        Array.Resize(ref musicList, names.Length);
        for (int i = 0; i < musicList.Length; i++)
        {
            musicList[i].name = names[i];
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerHealth.onPlayerDeath += OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
       StopMusic();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        PlayMusicForScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerHealth.onPlayerDeath -= OnPlayerDeath;
    }
}

public enum MusicType
{ 
    MAINMENU = 0,
    PAUSEMENU = 1,
    INGAME = 2
}


[System.Serializable]
public struct MusicList
{
    public AudioClip[] Sounds => sounds;
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}

