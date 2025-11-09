using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 保持 TextMeshPro
using UnityEngine.UI; // 引入 Button

public class MusicGameManager : MonoBehaviour
{
    [Header("游戏状态")]
    public int orbsCollected = 0; // 替换 score
    
    [Header("UI组件")]
    public TextMeshProUGUI orbCountText; // 替换 scoreText
    public GameObject endLevelPanel; // 替换 gameOverPanel
    public TextMeshProUGUI finalOrbCountText; // 替换 finalScoreText
    public Button playbackButton; // 播放音乐按钮

    [Header("音乐录制")]
    [Tooltip("玩家踩出的音符序列")]
    public List<int> playedNotes = new List<int>();
    
    [Tooltip("用于回放音乐的AudioSource")]
    public AudioSource playbackAudioSource;
    public float notePlaybackDelay = 0.5f; // 每个音符播放的间隔

    private bool gameHasEnded = false;

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
        
        // 确保回放源存在
        if (playbackAudioSource == null)
        {
            playbackAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// 由 CubeController 调用 (移除了 points)
    /// </summary>
    public void RegisterNoteHit(int noteID)
    {
        if (gameHasEnded) return;

        // 1. 录制音符
        playedNotes.Add(noteID);
        
        // 2. 播放音效 (通过 AudioManager)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCubeHitSound(noteID);
            // 也许在这里也播放跳跃音效？
            // AudioManager.Instance.PlayJumpSound(); 
        }
    }

    /// <summary>
    /// (新功能) 由 CollectibleNote (小圆球) 调用
    /// </summary>
    public void RegisterCollectibleHit()
    {
        if (gameHasEnded) return;
        
        orbsCollected++;
        UpdateUI();
    }

    /// <summary>
    /// 更新收集物显示
    /// </summary>
    private void UpdateUI()
    {
        if (orbCountText != null)
            orbCountText.text = $"音符: {orbsCollected}"; // 你可以改成你喜欢的文本
    }

    /// <summary>
    /// (示例) 游戏结束
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
    /// 播放录制好的音乐 (这完全符合你的“在线听”需求)
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

    private IEnumerator PlaybackRoutine()
    {
        if (playbackButton != null)
            playbackButton.interactable = false; // 防止重复点击

        Debug.Log("开始回放音乐...");

        foreach (int noteID in playedNotes)
        {
            // 从 AudioManager 获取音效片段
            AudioClip clip = AudioManager.Instance.GetCubeHitClip(noteID);
            if (clip != null)
            {
                playbackAudioSource.PlayOneShot(clip);
                
                // 等待一个固定延迟
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
        // 按 'E' 键结束游戏 (测试用)
        if (Input.GetKeyDown(KeyCode.E))
        {
            EndGame();
        }
    }
}