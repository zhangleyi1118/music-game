using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

public class MusicGameManager : MonoBehaviour
{
    [Header("游戏状态")]
    public int orbsCollected = 0; 
    
    [Header("UI组件")]
    public TextMeshProUGUI orbCountText; 
    public GameObject endLevelPanel; 
    public TextMeshProUGUI finalOrbCountText; 
    public Button playbackButton; 

    [Header("音乐录制")]
    [Tooltip("玩家踩出的音符序列")]
    public List<int> playedNotes = new List<int>();
    
    [Tooltip("用于回放音乐的AudioSource")]
    public AudioSource playbackAudioSource;
    public float notePlaybackDelay = 0.5f; 

    private bool gameHasEnded = false;
    // (这是我们上一步添加的，请确保它在这里)
    public bool GameHasEnded => gameHasEnded; 

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
        orbsCollected = 0;
        gameHasEnded = false;
        playedNotes.Clear();
        
        UpdateUI();
        
        if (endLevelPanel != null)
            endLevelPanel.SetActive(false);

        if (playbackButton != null)
            playbackButton.onClick.AddListener(PlayRecordedMusic);
        
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
        if (gameHasEnded) return;

        // 1. 录制音符 (正数)
        playedNotes.Add(noteID);
        
        // 2. 播放音效 (通过 AudioManager)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCubeHitSound(noteID);
        }
    }

    /// <summary>
    /// (新功能) 由 CollectibleNote (小圆球) 调用
    /// --- 核心修改：添加 specialNoteID 参数 ---
    /// </summary>
    public void RegisterCollectibleHit(int specialNoteID) // <-- 修改点 1：添加参数
    {
        if (gameHasEnded) return;
        
        orbsCollected++;
        UpdateUI();
        
        // --- 修改点 2：录制小球音效 ---
        // 技巧：我们将 ID 存为负数来区分。
        // (specialNoteID 0 会被存为 -1)
        // (specialNoteID 1 会被存为 -2)
        playedNotes.Add(-(specialNoteID + 1)); 
    }

    /// <summary>
    /// 更新收集物显示
    /// </summary>
    private void UpdateUI()
    {
        if (orbCountText != null)
            orbCountText.text = $"音符: {orbsCollected}"; 
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    public void EndGame()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;
        
        if (endLevelPanel != null)
        {
            endLevelPanel.SetActive(true);
            if (finalOrbCountText != null)
                finalOrbCountText.text = $"最终收集: {orbsCollected}";
        }
        
        Debug.Log("游戏结束! 录制的音符数量: " + playedNotes.Count);
    }
    
    /// <summary>
    /// 播放录制好的音乐
    /// </summary>
    public void PlayRecordedMusic()
    {
        if (playedNotes.Count == 0)
        {
            Debug.Log("没有录制到音符。");
            return;
        }
        
        StartCoroutine(PlaybackRoutine());
    }

    /// <summary>
    /// --- 核心修改：让协程能识别负数ID ---
    /// </summary>
    private IEnumerator PlaybackRoutine()
    {
        if (playbackButton != null)
            playbackButton.interactable = false; 

        Debug.Log("开始回放音乐...");

        foreach (int noteID in playedNotes)
        {
            // --- 修改点 3：检查 ID 是正数还是负数 ---
            AudioClip clip = null;
            
            if (noteID >= 0) // 正数 = Cube 旋律音
            {
                clip = AudioManager.Instance.GetCubeHitClip(noteID);
            }
            else // 负数 = 小球 特殊音效
            {
                // 把 -1 转回 0, -2 转回 1 ...
                int specialID = -(noteID + 1); 
                clip = AudioManager.Instance.GetSpecialNoteClip(specialID); // <-- 我们需要在 AudioManager 中创建这个新函数
            }
            // --- 修改结束 ---

            if (clip != null)
            {
                playbackAudioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(notePlaybackDelay); 
            }
        }
        
        Debug.Log("回放结束。");
        if (playbackButton != null)
            playbackButton.interactable = true;
    }
    
    // (仅用于测试)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EndGame();
        }
    }
}