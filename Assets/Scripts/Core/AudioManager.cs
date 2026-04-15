using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private int sfxPoolSize = 8;
    [SerializeField] private float bgmVolume = 0.6f;
    [SerializeField] private float sfxVolume = 1f;
    private AudioSource bgmSource;
    private AudioSource[] sfxSources;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    private void InitializeAudioSources()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = Mathf.Clamp01(bgmVolume);

        int poolSize = Mathf.Max(1, sfxPoolSize);
        sfxSources = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = Mathf.Clamp01(sfxVolume);
            sfxSources[i] = sfxSource;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("PlaySound called with null clip.");
            return;
        }

        AudioSource source = GetAvailableSfxSource();
        source.PlayOneShot(clip, Mathf.Clamp01(sfxVolume));
    }

    public void PlayVfxSound(AudioClip audioClip)
    {
        PlaySound(audioClip);
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null)
        {
            return;
        }

        if (bgmSource.clip != backgroundMusic)
        {
            bgmSource.clip = backgroundMusic;
        }

        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSources == null)
        {
            return;
        }

        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i].volume = sfxVolume;
        }
    }

    private AudioSource GetAvailableSfxSource()
    {
        for (int i = 0; i < sfxSources.Length; i++)
        {
            if (!sfxSources[i].isPlaying)
            {
                return sfxSources[i];
            }
        }

        return sfxSources[0];
    }
}