using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
using UnityEngine.SceneManagement; // 1. 引入场景管理

public class MusicGameManager : MonoBehaviour
{
    [Header("游戏状态")]
    // private int orbsCollected = 0; // <-- 2. 已删除
    private bool isGamePaused = false; // <-- 3. 将 'gameHasEnded' 改名为 'isGamePaused'
    
    // (这是我们之前添加的，请确保它在这里，并改用 isGamePaused)
    // 确保小球脚本可以检查游戏是否暂停
    public bool IsGamePaused => isGamePaused; 

    [Header("UI组件")]
    // public TextMeshProUGUI orbCountText; // <-- 4. 已删除
    public GameObject endLevelPanel; // (现在是“暂停菜单”)
    // public TextMeshProUGUI finalOrbCountText; // <-- 5. 已删除
    
    [Header("暂停菜单按钮")]
    public Button playbackButton; // “播放录音”按钮
    public Button continueButton; // “继续演奏”按钮
    public Button restartButton;  // “重新开始” (重新验证) 按钮

    [Header("音乐录制")]
    [Tooltip("玩家踩出的音符序列")]
    public List<int> playedNotes = new List<int>();
    
    [Tooltip("用于回放音乐的AudioSource")]
    public AudioSource playbackAudioSource;
    public float notePlaybackDelay = 0.5f; 

    private Coroutine playbackCoroutine; // 用于停止协程

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
        isGamePaused = false; // 确保游戏开始时未暂停
        Time.timeScale = 1f;  // 确保时间正常流动
        playedNotes.Clear();
        
        // UpdateUI(); // <-- 6. 已删除
        
        if (endLevelPanel != null)
            endLevelPanel.SetActive(false); // <-- 这会在新场景加载时隐藏Panel

        // --- 7. 绑定所有按钮的点击事件 ---
        if (playbackButton != null)
            playbackButton.onClick.AddListener(PlayRecordedMusic);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(ResumeGame);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        // --- 修改结束 ---
        
        if (playbackAudioSource == null)
        {
            playbackAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// 由 CubeController 调用
    /// </summary>
    public void RegisterNoteHit(int noteID)
    {
        if (isGamePaused) return; // <-- 8. 检查是否暂停

        // 1. 录制音符 (正数)
        playedNotes.Add(noteID);
        
        // 2. 播放音效 (通过 AudioManager)
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
        if (isGamePaused) return; // <-- 8. 检查是否暂停
        
        // orbsCollected++; // <-- 9. 已删除
        // UpdateUI(); // <-- 9. 已删除
        
        // 只保留录制功能
        playedNotes.Add(-(specialNoteID + 1)); 
    }

    // private void UpdateUI() { } // <-- 10. 整个函数已删除

    /// <summary>
    /// --- 11. 核心修改：从 EndGame 改为 PauseGame ---
    /// 暂停游戏并显示菜单
    /// </summary>
    public void PauseGame()
    {
        if (isGamePaused) return; // 已经在暂停了
        
        isGamePaused = true;
        Time.timeScale = 0f; // 暂停所有物理和时间
        
        if (endLevelPanel != null)
        {
            endLevelPanel.SetActive(true);
        }
        
        Debug.Log("游戏暂停!");
    }

    /// <summary>
    /// --- 12. 新功能：“继续演奏”按钮调用 ---
    /// </summary>
    public void ResumeGame()
    {
        if (!isGamePaused) return; // 游戏没暂停

        // 停止可能正在播放的音乐
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
            playbackCoroutine = null;
        }
        if (playbackAudioSource.isPlaying)
        {
            playbackAudioSource.Stop();
        }
        if (AudioManager.Instance != null)
        {
            // 停止行走和回放的鼓点
            AudioManager.Instance.StopWalkingLoop(); 
        }

        // 恢复游戏
        isGamePaused = false;
        Time.timeScale = 1f; // 恢复时间流动
        
        if (endLevelPanel != null)
        {
            endLevelPanel.SetActive(false);
        }
        
        // 恢复按钮交互
        SetMenuButtonsInteractable(true);
        
        Debug.Log("游戏继续!");
    }
    
    /// <summary>
    /// --- 13. 新功能：“重新验证/开始”按钮调用 ---
    /// </summary>
    public void RestartGame()
    {
        // 必须在加载场景前重置时间
        Time.timeScale = 1f; 
        isGamePaused = false;
        
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("重新开始!");
    }
    
    /// <summary>
    /// --- 14. 新功能：切换暂停/继续 ---
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
        
        // 防止重复播放
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
        }
        playbackCoroutine = StartCoroutine(PlaybackRoutine());
    }

    private IEnumerator PlaybackRoutine()
    {
        // 15. 播放时禁用所有按钮
        SetMenuButtonsInteractable(false); 

        Debug.Log("开始回放音乐...");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWalkingLoop(); // 播放背景鼓点
        }

        foreach (int noteID in playedNotes)
        {
            AudioClip clip = null;
            
            if (noteID >= 0) // Cube 旋律音
            {
                clip = AudioManager.Instance.GetCubeHitClip(noteID);
            }
            else // 小球 特殊音效
            {
                int specialID = -(noteID + 1); 
                clip = AudioManager.Instance.GetSpecialNoteClip(specialID);
            }

            if (clip != null)
            {
                playbackAudioSource.PlayOneShot(clip);
                // 在 Realtime 中等待，因为 Time.timeScale 是 0
                yield return new WaitForSecondsRealtime(notePlaybackDelay); 
            }
        }
        
        Debug.Log("回放结束。");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopWalkingLoop(); // 停止背景鼓点
        }
        
        // 16. 播放结束后恢复所有按钮
        SetMenuButtonsInteractable(true); 
        playbackCoroutine = null;
    }
    
    /// <summary>
    /// --- 17. 新功能：统一管理按钮状态 ---
    /// </summary>
    private void SetMenuButtonsInteractable(bool state)
    {
        if (playbackButton != null)
            playbackButton.interactable = state;
        if (continueButton != null)
            continueButton.interactable = state;
        if (restartButton != null)
            restartButton.interactable = state;
    }
    
    // (仅用于测试)
    private void Update()
    {
        // --- 18. 核心修改：按 E 键切换暂停菜单 ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            TogglePauseMenu();
        }
    }
}