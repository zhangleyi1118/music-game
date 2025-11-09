using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("音频源设置")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource playerSource;
    
    // --- 核心修改 1：为行走循环添加专属音源 ---
    [Tooltip("用于播放行走/奔跑循环音效的音源")]
    public AudioSource walkingLoopSource; 
    // --- 修改结束 ---
    
    [Header("音乐设置")]
    public AudioClip[] levelMusic;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1.0f;
    
    [Header("音效库")]
    public AudioClip jumpSound;
    public AudioClip climbSound;
    public AudioClip[] footstepSounds; // (这个现在可以不用了，但我们先留着)
    
    [Tooltip("旋律音库 (钢琴音) - 数组长度至少为73，以匹配NoteID 13-72")]
    public AudioClip[] cubeHitSounds; 
    
    // --- 核心修改 2：添加循环音效文件 ---
    [Tooltip("行走/奔跑的循环音效 (例如一个鼓点)")]
    public AudioClip walkingLoopSound; 
    // --- 修改结束 ---

    [Tooltip("节奏音库 (鼓点) - 0=底鼓, 1=军鼓, 2=镲片, etc.")]
    public AudioClip[] walkingNoteSounds; // (这个现在也不需要了)

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

        // --- 核心修改 3：设置新的循环音源 ---
        if (walkingLoopSource == null)
        {
            Debug.LogWarning("WalkingLoopSource 未在 Inspector 中指定，正在自动创建。");
            walkingLoopSource = gameObject.AddComponent<AudioSource>();
        }
        // 关键：确保它是循环播放的
        walkingLoopSource.loop = true; 
        walkingLoopSource.volume = playerSource.volume; // 和玩家音量保持一致
        // --- 修改结束 ---
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
        // (这个函数现在没用了)
    }
    
    public void PlayCubeHitSound(int cubeType = 0)
    {
        if (cubeHitSounds.Length > cubeType && cubeType >= 0 && cubeHitSounds[cubeType] != null)
            sfxSource.PlayOneShot(cubeHitSounds[cubeType]);
        else if (cubeType != 0)
            Debug.LogWarning($"Cube Hit Sound (Melody) for noteID {cubeType} is missing!");
    }

    public void PlayWalkingSound(int noteID)
    {
        // (这个函数现在也没用了)
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
        
        // (也更新循环音源的音量)
        if(walkingLoopSource != null)
            walkingLoopSource.volume = playerSource.volume;
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

    // (这是我们上一步为录制小球添加的，请保留)
    public AudioClip GetSpecialNoteClip(int noteID)
    {
        if (specialNoteSounds != null && specialNoteSounds.Length > noteID && noteID >= 0 && specialNoteSounds[noteID] != null)
        {
            return specialNoteSounds[noteID];
        }
        if (specialNoteSounds != null && specialNoteSounds.Length > 0 && specialNoteSounds[0] != null)
        {
            return specialNoteSounds[0];
        }
        return null; 
    }
    
    // --- 核心修改 4：添加两个新的公共函数来控制循环 ---

    /// <summary>
    /// 开始播放行走循环音效
    /// </summary>
    public void PlayWalkingLoop()
    {
        if (walkingLoopSource.isPlaying || walkingLoopSound == null) return;
        
        walkingLoopSource.clip = walkingLoopSound;
        walkingLoopSource.Play();
    }

    /// <summary>
    /// 停止播放行走循环音效
    /// </summary>
    public void StopWalkingLoop()
    {
        if (walkingLoopSource.isPlaying)
        {
            walkingLoopSource.Stop();
        }
    }
    // --- 修改结束 ---
}