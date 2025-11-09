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

        // 录制音符 (正数)
        playedNotes.Add(noteID);
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCubeHitSound(noteID);
        }
    }

    /// <summary>
    /// 由 CollectibleNote (小圆球) 调用
    /// </summary>
    public void RegisterCollectibleHit(int specialNoteID) 
    {
        if (gameHasEnded) return;
        
        orbsCollected++;
        UpdateUI();
        
        // 录制小球音效 (负数)
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
    /// (已修改) 回放协程
    /// </summary>
    private IEnumerator PlaybackRoutine()
    {
        if (playbackButton != null)
            playbackButton.interactable = false; 

        Debug.Log("开始回放音乐...");

        // --- 核心修改 1：在回放开始时，播放行走循环音 ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWalkingLoop();
        }
        // --- 修改结束 ---

        foreach (int noteID in playedNotes)
        {
            AudioClip clip = null;
            
            if (noteID >= 0) // 正数 = Cube 旋律音
            {
                clip = AudioManager.Instance.GetCubeHitClip(noteID);
            }
            else // 负数 = 小球 特殊音效
            {
                int specialID = -(noteID + 1); 
                clip = AudioManager.Instance.GetSpecialNoteClip(specialID);
            }

            if (clip != null)
            {
                playbackAudioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(notePlaybackDelay); 
            }
        }
        
        Debug.Log("回放结束。");
        
        // --- 核心修改 2：在回放结束后，停止行走循环音 ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopWalkingLoop();
        }
        // --- 修改结束 ---
        
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