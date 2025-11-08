using UnityEngine;

[CreateAssetMenu(fileName = "MusicGameConfig", menuName = "Music Game/Game Configuration")]
public class MusicGameConfig : ScriptableObject
{
    [Header("音乐设置")]
    public AudioClip[] backgroundMusicClips;
    public float[] musicBPMs;
    
    [Header("游戏参数")]
    public float perfectHitWindow = 0.1f; // 完美命中窗口（秒）
    public float goodHitWindow = 0.2f;    // 良好命中窗口（秒）
    public float cubeSpawnRate = 2.0f;    // 方块生成频率
    public float staffLineSpeed = 5.0f;    // 五线谱移动速度
    public float maxHealth = 100f;        // 最大血条值
    public float staffLineDamage = 20f;   // 五线谱伤害值
    
    [Header("分数设置")]
    public int perfectHitScore = 100;
    public int goodHitScore = 50;
    public int normalHitScore = 20;
    public int comboMultiplier = 10;      // 连击倍率
    
    [Header("音效设置")]
    public AudioClip jumpSound;
    public AudioClip climbSound;
    public AudioClip[] footstepSounds;
    public AudioClip[] cubeHitSounds;
    public AudioClip staffLineHitSound;
    
    [Header("UI设置")]
    public Color perfectHitColor = Color.yellow;
    public Color goodHitColor = Color.green;
    public Color normalHitColor = Color.white;
    public Color comboColor = Color.red;
}