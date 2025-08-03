using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System.Linq;

public enum SoundType
{
    Click,
    Collect,
    OfficeBackground,
    MachineOn,
    MachineOff,
    Music
}

[System.Serializable]
public class Sound
{
    public string name;
    public SoundType soundType;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Configuration")]
    [SerializeField] private List<Sound> sounds = new List<Sound>();
    
    [Header("Click Sound Variations")]
    [SerializeField] private List<AudioClip> clickVariations = new List<AudioClip>();
    
    [Header("Music Configuration")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    private AudioSource musicSource;
    private bool musicWasPlaying = false;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSounds();
            InitializeMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.spatialBlend;
        }
    }

    private void InitializeMusic()
    {
        // Create dedicated AudioSource for music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f; // 2D sound
        
        // Add music to the sounds list for compatibility
        Sound musicSound = sounds.FirstOrDefault(s => s.soundType == SoundType.Music);
        if (musicSound == null)
        {
            musicSound = new Sound
            {
                name = "Background Music",
                soundType = SoundType.Music,
                clip = backgroundMusic,
                volume = musicVolume,
                loop = true,
                spatialBlend = 0f
            };
            musicSound.source = musicSource;
            sounds.Add(musicSound);
        }
        
        // Start playing music by default
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    public void PlaySound(SoundType type)
    {
        Sound sound = sounds.FirstOrDefault(s => s.soundType == type);
        if (sound != null)
        {
            // Special case for click which uses random variations
            if (type == SoundType.Click && clickVariations.Count > 0)
            {
                int randomIndex = Random.Range(0, clickVariations.Count);
                sound.source.clip = clickVariations[randomIndex];
            }
            
            sound.source.Play();
            
            // Handle music when OfficeBackground plays
            if (type == SoundType.OfficeBackground)
            {
                musicWasPlaying = musicSource.isPlaying;
                StopMusic();
            }
        }
        else
        {
            Debug.LogWarning($"Sound of type {type} not found!");
        }
    }

    public void PlaySoundAtPosition(SoundType type, Vector3 position)
    {
        Sound sound = sounds.FirstOrDefault(s => s.soundType == type);
        if (sound != null)
        {
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume);
        }
        else
        {
            Debug.LogWarning($"Sound of type {type} not found!");
        }
    }

    public void StopSound(SoundType type)
    {
        Sound sound = sounds.FirstOrDefault(s => s.soundType == type);
        if (sound != null && sound.source.isPlaying)
        {
            sound.source.Stop();
            
            // Resume music when OfficeBackground stops
            if (type == SoundType.OfficeBackground && musicWasPlaying)
            {
                PlayMusic();
            }
        }
    }

    public void PauseSound(SoundType type)
    {
        Sound sound = sounds.FirstOrDefault(s => s.soundType == type);
        if (sound != null && sound.source.isPlaying)
        {
            sound.source.Pause();
        }
    }

    public void ResumeSound(SoundType type)
    {
        Sound sound = sounds.FirstOrDefault(s => s.soundType == type);
        if (sound != null && !sound.source.isPlaying)
        {
            sound.source.UnPause();
        }
    }
}
