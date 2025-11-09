using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("音频源设置")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource playerSource;
    
    [Header("音乐设置")]
    public AudioClip[] levelMusic;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1.0f;
    
    [Header("音效库")]
    public AudioClip jumpSound;
    public AudioClip climbSound;
    public AudioClip[] footstepSounds;
    
    [Tooltip("旋律音库 (钢琴音) - 数组长度至少为73，以匹配NoteID 13-72")]
    public AudioClip[] cubeHitSounds; 
    
    [Tooltip("节奏音库 (鼓点) - 0=底鼓, 1=军鼓, 2=镲片, etc.")]
    public AudioClip[] walkingNoteSounds; // <--- 添加这一行 (节奏音)

    [Tooltip("特殊收集物音效库 (小圆球)")]
    public AudioClip[] specialNoteSounds; 
    
    public AudioClip staffLineHitSound;
    public AudioClip gameOverSound;
    
    private static AudioManager instance;
    public static AudioManager Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupAudioSources();
    }
    
    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = sfxVolume;
        }
        
        if (playerSource == null)
        {
            playerSource = gameObject.AddComponent<AudioSource>();
            playerSource.volume = sfxVolume * 0.8f;
        }
    }
    
    public void PlayMusic(int levelIndex = 0)
    {
        if (levelMusic.Length > levelIndex && levelMusic[levelIndex] != null)
        {
            musicSource.clip = levelMusic[levelIndex];
            musicSource.Play();
        }
    }
    
    public void StopMusic()
    {
        musicSource.Stop();
    }
    
    public void PlayJumpSound()
    {
        if (jumpSound != null)
            playerSource.PlayOneShot(jumpSound);
    }
    
    public void PlayClimbSound()
    {
        if (climbSound != null)
            playerSource.PlayOneShot(climbSound);
    }
    
    public void PlayFootstepSound()
    {
        if (footstepSounds.Length > 0)
        {
            int index = Random.Range(0, footstepSounds.Length);
            playerSource.PlayOneShot(footstepSounds[index]);
        }
    }
    
    public void PlayCubeHitSound(int cubeType = 0)
    {
        if (cubeHitSounds.Length > cubeType && cubeType >= 0 && cubeHitSounds[cubeType] != null)
            sfxSource.PlayOneShot(cubeHitSounds[cubeType]);
        else if (cubeType != 0)
            Debug.LogWarning($"Cube Hit Sound (Melody) for noteID {cubeType} is missing!");
    }

    /// <summary>
    /// (新功能) 播放行走节奏音
    /// </summary>
    public void PlayWalkingSound(int noteID)
    {
        if (walkingNoteSounds != null && walkingNoteSounds.Length > noteID && noteID >= 0 && walkingNoteSounds[noteID] != null)
        {
            // 使用 playerSource 播放节奏音可能感觉更真实
            playerSource.PlayOneShot(walkingNoteSounds[noteID]);
        }
    }

    public void PlaySpecialNoteSound(int noteID) 
    {
        if (specialNoteSounds != null && specialNoteSounds.Length > noteID && noteID >= 0 && specialNoteSounds[noteID] != null)
        {
            sfxSource.PlayOneShot(specialNoteSounds[noteID]);
        }
    }
    
    public void PlayStaffLineHitSound()
    {
        if (staffLineHitSound != null)
            sfxSource.PlayOneShot(staffLineHitSound);
    }
    
    public void PlayGameOverSound()
    {
        if (gameOverSound != null)
            sfxSource.PlayOneShot(gameOverSound);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        playerSource.volume = sfxVolume * 0.8f;
    }

    public int GetMaxNoteTypes()
    {
        if (cubeHitSounds == null) return 0;
        return cubeHitSounds.Length;
    }

    public AudioClip GetCubeHitClip(int noteID)
    {
        if (cubeHitSounds != null && cubeHitSounds.Length > noteID && noteID >= 0)
        {
            return cubeHitSounds[noteID];
        }
        return null;
    }
}