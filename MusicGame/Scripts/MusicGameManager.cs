using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 添加TextMeshPro支持

public class MusicGameManager : MonoBehaviour
{
    [Header("音乐设置")]
    public AudioSource backgroundMusic;
    public AudioClip[] levelMusic;
    public float[] bpmArray; // 每分钟节拍数
    public float currentBPM = 120f;
    
    [Header("节拍系统")]
    public float beatInterval; // 节拍间隔时间
    public float currentBeatTime = 0f;
    public int currentBeat = 0;
    public bool isPlaying = false;
    
    [Header("游戏状态")]
    public int score = 0;
    public int combo = 0;
    public int maxCombo = 0;
    public bool gameOver = false;
    
    [Header("UI组件")]
    [Tooltip("TextMeshPro文本组件")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI maxComboText;
    public Slider healthBar;
    public GameObject gameOverPanel;
    
    [Header("音效")]
    public AudioSource effectAudio;
    public AudioClip jumpSound;
    public AudioClip climbSound;
    public AudioClip[] footstepSounds;
    public AudioClip[] cubeHitSounds;
    
    private static MusicGameManager instance;
    public static MusicGameManager Instance => instance;
    
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
        InitializeGame();
    }
    
    public void InitializeGame()
    {
        score = 0;
        combo = 0;
        maxCombo = 0;
        gameOver = false;
        
        // 计算节拍间隔
        beatInterval = 60f / currentBPM;
        
        // 开始播放音乐
        if (backgroundMusic != null && levelMusic.Length > 0)
        {
            backgroundMusic.clip = levelMusic[0];
            backgroundMusic.Play();
            isPlaying = true;
        }
        
        // 初始化UI
        UpdateUI();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    private void Update()
    {
        if (!isPlaying || gameOver) return;
        
        // 更新节拍计时
        currentBeatTime += Time.deltaTime;
        
        // 检查是否到达下一节拍
        if (currentBeatTime >= beatInterval)
        {
            currentBeatTime = 0f;
            currentBeat++;
            OnBeat();
        }
    }
    
    private void OnBeat()
    {
        // 这里可以触发节拍事件，比如生成新的五线谱线
        Debug.Log("节拍: " + currentBeat);
        
        // 每隔4个节拍生成五线谱
        if (currentBeat % 4 == 0)
        {
            SpawnStaffLine();
        }
    }
    
    private void SpawnStaffLine()
    {
        // 生成五线谱逻辑
        // 这里需要创建五线谱对象
        Debug.Log("生成五线谱");
    }
    
    public void AddScore(int points, bool perfectHit = false)
    {
        if (perfectHit)
        {
            points *= 2; // 完美命中双倍分数
            combo++;
            if (combo > maxCombo)
                maxCombo = combo;
        }
        else
        {
            combo = 0; // 重置连击
        }
        
        score += points;
        UpdateUI();
    }
    
    public void TakeDamage(float damage)
    {
        if (healthBar != null)
        {
            healthBar.value -= damage;
            
            if (healthBar.value <= 0)
            {
                GameOver();
            }
        }
    }
    
    private void GameOver()
    {
        gameOver = true;
        isPlaying = false;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        Debug.Log("游戏结束! 最终分数: " + score + " 最大连击: " + maxCombo);
    }
    
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
        
        if (comboText != null)
            comboText.text = $"Combo: {combo}";
        
        if (maxComboText != null)
            maxComboText.text = $"Max Combo: {maxCombo}";
    }
    
    // 音效方法
    public void PlayJumpSound()
    {
        if (effectAudio != null && jumpSound != null)
        {
            effectAudio.PlayOneShot(jumpSound);
        }
    }
    
    public void PlayClimbSound()
    {
        if (effectAudio != null && climbSound != null)
        {
            effectAudio.PlayOneShot(climbSound);
        }
    }
    
    public void PlayFootstepSound()
    {
        if (effectAudio != null && footstepSounds.Length > 0)
        {
            int index = Random.Range(0, footstepSounds.Length);
            effectAudio.PlayOneShot(footstepSounds[index]);
        }
    }
    
    public void PlayCubeHitSound(int cubeType)
    {
        if (effectAudio != null && cubeHitSounds.Length > cubeType)
        {
            effectAudio.PlayOneShot(cubeHitSounds[cubeType]);
        }
    }
}