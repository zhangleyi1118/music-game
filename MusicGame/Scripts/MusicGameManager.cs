using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 保持 TextMeshPro

public class MusicGameManager : MonoBehaviour
{
    [Header("游戏状态")]
    public int score = 0;
    
    [Header("UI组件")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel; // 用于显示最终得分和播放按钮
    public TextMeshProUGUI finalScoreText;
    public UnityEngine.UI.Button playbackButton; // 播放音乐按钮

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
        score = 0;
        gameHasEnded = false;
        playedNotes.Clear();
        
        UpdateUI();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (playbackButton != null)
            playbackButton.onClick.AddListener(PlayRecordedMusic);
        
        // 确保回放源存在
        if (playbackAudioSource == null)
        {
            playbackAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// 由 CubeController 调用
    /// </summary>
    public void RegisterNoteHit(int noteID, int points)
    {
        if (gameHasEnded) return;

        // 1. 录制音符
        playedNotes.Add(noteID);
        
        // 2. 增加分数
        score += points;
        UpdateUI();
        
        // 3. 播放音效 (通过 AudioManager)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCubeHitSound(noteID);
            AudioManager.Instance.PlayJumpSound(); // 玩家的跳跃音效
        }
    }

    /// <summary>
    /// 更新分数显示
    /// </summary>
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    /// <summary>
    /// (示例) 游戏结束
    /// </summary>
    public void EndGame()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {score}";
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
                
                // 等待一个固定延迟，或者 clip 的长度
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