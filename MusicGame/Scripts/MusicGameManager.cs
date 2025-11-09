using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using System.IO; 
using System;   

// 为编辑器添加引用
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MusicGameManager : MonoBehaviour
{
    [Header("游戏状态")]
    private bool isGamePaused = false; 
    public bool IsGamePaused => isGamePaused; 

    [Header("UI组件")]
    public GameObject endLevelPanel; 
    
    [Header("暂停菜单按钮")]
    public Button playbackButton; 
    public Button continueButton; 
    public Button restartButton;  // (功能是“退出”)
    public Button saveButton;     // “保存录音”按钮

    [Header("音乐录制")]
    [Tooltip("玩家踩出的音符序列")]
    public List<int> playedNotes = new List<int>();
    
    [Tooltip("用于回放音乐的AudioSource")]
    public AudioSource playbackAudioSource;
    public float notePlaybackDelay = 0.5f; 

    private Coroutine playbackCoroutine; 

    // 单例
    private static MusicGameManager instance;
    public static MusicGameManager Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    public void InitializeGame()
    {
        isGamePaused = false; 
        Time.timeScale = 1f;  
        playedNotes.Clear();
        
        if (endLevelPanel != null)
            endLevelPanel.SetActive(false); 

        // 绑定所有按钮的点击事件
        if (playbackButton != null)
            playbackButton.onClick.AddListener(PlayRecordedMusic);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(ResumeGame);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(ExitGame); 
            
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveRecording); 
        
        // --- 修改：确保 playbackAudioSource 存在 ---
        // 之前这个检查在 Start() 中，但 InitializeGame() 可能会被重复调用
        if (playbackAudioSource == null)
        {
            playbackAudioSource = gameObject.AddComponent<AudioSource>();
        }
        // --- 修改结束 ---
    }
    
    /// <summary>
    /// 由 CubeController 调用
    /// </summary>
    public void RegisterNoteHit(int noteID)
    {
        if (isGamePaused) return; 

        playedNotes.Add(noteID);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCubeHitSound(noteID);
        }
    }

    /// <summary>
    /// (已修改) 由 CollectibleNote (小圆球) 调用
    /// </summary>
    public void RegisterCollectibleHit(int specialNoteID) 
    {
        if (isGamePaused) return; 
        
        playedNotes.Add(-(specialNoteID + 1)); 
    }

    /// <summary>
    /// 暂停游戏并显示菜单
    /// </summary>
    public void PauseGame()
    {
        if (isGamePaused) return; 
        
        isGamePaused = true;
        Time.timeScale = 0f; 
        
        if (endLevelPanel != null)
        {
            endLevelPanel.SetActive(true);
        }
        
        Debug.Log("游戏暂停!");
        
        if (saveButton != null)
        {
            saveButton.interactable = playedNotes.Count > 0;
        }
    }

    /// <summary>
    /// “继续演奏”按钮调用
    /// </summary>
    public void ResumeGame()
    {
        if (!isGamePaused) return; 

        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
            playbackCoroutine = null;
        }
        if (playbackAudioSource != null && playbackAudioSource.isPlaying)
        {
            playbackAudioSource.Stop();
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopWalkingLoop(); 
        }

        isGamePaused = false;
        Time.timeScale = 1f; 
        
        if (endLevelPanel != null)
        {
            endLevelPanel.SetActive(false);
        }
        
        SetMenuButtonsInteractable(true);
        
        Debug.Log("游戏继续!");
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("退出游戏!");

        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 保存录音功能 (保存到桌面)
    /// </summary>
    public void SaveRecording()
    {
        if (playedNotes.Count == 0)
        {
            Debug.LogWarning("没有录音，无需保存。");
            return;
        }

        NoteRecording recordingData = new NoteRecording();
        recordingData.notes = playedNotes;
        string json = JsonUtility.ToJson(recordingData, true); 

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"MusicRecording_{timeStamp}.json";
        string path = Path.Combine(desktopPath, fileName);
        
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"录音已成功保存到你的桌面: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"保存录音失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 切换暂停/继续
    /// </summary>
    public void TogglePauseMenu()
    {
        if (isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// 播放录制好的音乐 (播放按钮调用)
    /// </summary>
    public void PlayRecordedMusic()
    {
        if (playedNotes.Count == 0)
        {
            Debug.Log("没有录制到音符。");
            return;
        }
        
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
        }
        playbackCoroutine = StartCoroutine(PlaybackRoutine());
    }

    // --- (!!!) 核心修改在这里 (!!!) ---
    /// <summary>
    /// 回放协程 (增加了详细的日志和空值检查)
    /// </summary>
    private IEnumerator PlaybackRoutine()
    {
        SetMenuButtonsInteractable(false); 
        Debug.Log("PlaybackRoutine: 开始回放音乐...");

        // --- 1. 添加了对关键组件的空检查 ---
        if (AudioManager.Instance == null)
        {
            Debug.LogError("PlaybackRoutine: AudioManager.Instance 为空！无法播放。");
            SetMenuButtonsInteractable(true);
            playbackCoroutine = null;
            yield break; // 提前退出协程
        }
        if (playbackAudioSource == null)
        {
            Debug.LogError("PlaybackRoutine: playbackAudioSource 为空！请在 Inspector 中分配它。");
            SetMenuButtonsInteractable(true);
            playbackCoroutine = null;
            yield break; // 提前退出协程
        }
        // --- 检查结束 ---

        AudioManager.Instance.PlayWalkingLoop(); 

        int noteIndex = 0;
        foreach (int noteID in playedNotes)
        {
            AudioClip clip = null;
            string clipSource = "N/A";

            try
            {
                if (noteID >= 0) // Cube 旋律音
                {
                    clipSource = $"Cube (ID: {noteID})";
                    clip = AudioManager.Instance.GetCubeHitClip(noteID);
                }
                else // 小球 特殊音效
                {
                    int specialID = -(noteID + 1); 
                    clipSource = $"Special (ID: {specialID})";
                    clip = AudioManager.Instance.GetSpecialNoteClip(specialID);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"PlaybackRoutine: 在 AudioManager 中获取音符时出错 (Index: {noteIndex}, Source: {clipSource}): {e.Message}");
                continue; // 跳过这个音符
            }

            // --- 2. 修改了空值检查方式并增加了日志 ---
            // 使用 (clip) 而不是 (clip != null) 是检查 Unity Object 是否“真实”存在的更安全方式
            // 这将捕获 "null" 和 "missing" (资源丢失) 两种情况
            if (clip) 
            {
                // Debug.Log($"PlaybackRoutine: 正在播放 Note {noteIndex} (Source: {clipSource})");
                playbackAudioSource.PlayOneShot(clip); 
                yield return new WaitForSecondsRealtime(notePlaybackDelay); 
            }
            else
            {
                // 这将告诉我们问题所在：AudioManager 正在返回 null。
                Debug.LogWarning($"PlaybackRoutine: 跳过 Note {noteIndex} (Source: {clipSource})，因为找到的 AudioClip 为 null。请检查 AudioManager 和它引用的音效文件。");
            }
            // --- 修改结束 ---
            noteIndex++;
        }
        
        Debug.Log("PlaybackRoutine: 回放结束。");
        AudioManager.Instance.StopWalkingLoop(); 
        SetMenuButtonsInteractable(true); 
        playbackCoroutine = null;
    }
    
    /// <summary>
    /// 统一管理按钮状态
    /// </summary>
    private void SetMenuButtonsInteractable(bool state)
    {
        if (playbackButton != null)
            playbackButton.interactable = state;
        if (continueButton != null)
            continueButton.interactable = state;
        if (restartButton != null)
            restartButton.interactable = state;
            
        if (saveButton != null)
        {
            saveButton.interactable = state && (playedNotes.Count > 0);
        }
    }
    
    private void Update()
    {
        // 按 E 键切换暂停菜单
        if (Input.GetKeyDown(KeyCode.E))
        {
            TogglePauseMenu();
        }
    }
}

/// <summary>
/// 用于JSON序列化的辅助类
/// </summary>
[Serializable]
public class NoteRecording
{
    public List<int> notes;
}