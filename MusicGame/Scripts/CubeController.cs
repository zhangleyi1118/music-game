using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour
{
    [Header("旋律音设置 (跳跃落地)")]
    [Tooltip("旋律音高 ID (对应 AudioManager 中的 cubeHitSounds 索引, 13-72)")]
    public int noteID = 13; 

    [Header("节奏音设置 (行走)")]
    [Tooltip("节奏音 ID (对应 AudioManager 中的 walkingNoteSounds 索引, 0=底鼓, 1=军鼓, 等)")]
    public int walkingNoteID = 0;

    [Header("视觉效果")]
    [Tooltip("是否为特殊方块（例如黑键，仅用于视觉）")]
    public bool isSpecial = false;
    [Tooltip("普通音符颜色（白键）")]
    public Color normalColor = Color.white;
    [Tooltip("特殊音符颜色（黑键）")]
    public Color specialColor = Color.black;
    [Tooltip("触发后发光颜色")]
    public Color glowColor = Color.cyan;
    [Tooltip("发光持续时间（秒）")]
    public float glowDuration = 0.5f; // 缩短闪烁时间

    // 内部状态
    private Renderer cubeRenderer;
    private Color originalColor;
    private Coroutine glowCoroutine;
    private Light cubeLight; // 发光组件

    public enum CubeState
    {
        Idle,           // 待机状态
        Glowing         // 正在发光
    }
    private CubeState currentState = CubeState.Idle;

    private void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer == null)
        {
            Debug.LogError("CubeController 缺少 Renderer 组件!", this);
            return;
        }

        // 确保你的 "MusicCube" 标签已经应用到这个物体上
        if (!CompareTag("MusicCube"))
        {
            Debug.LogWarning($"请将 {name} 的 Tag 设置为 'MusicCube'", this);
        }

        // 设置光源
        SetupLightComponent();
        
        // 设置初始颜色
        originalColor = isSpecial ? specialColor : normalColor;
        SetGlowEffect(false, originalColor);
    }

    /// <summary>
    /// 核心：由 PlayerLandState (玩家落地状态) 调用
    /// </summary>
    public void OnPlayerLand()
    {
        // 如果正在发光，则重置协程，实现连续触发
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }
        
        // 1. 播放音效 & 录制 (通过 MusicGameManager)
        if (MusicGameManager.Instance != null)
        {
            // RegisterNoteHit 会同时播放音效和录制
            MusicGameManager.Instance.RegisterNoteHit(noteID);
        }

        // 2. 开始发光协程
        glowCoroutine = StartCoroutine(GlowRoutine());
    }

    /// <summary>
    // 核心：发光，然后恢复
    /// </summary>
    private IEnumerator GlowRoutine()
    {
        currentState = CubeState.Glowing;

        // 1. 立刻开始发光
        SetGlowEffect(true, glowColor);

        // 2. 等待发光时间
        yield return new WaitForSeconds(glowDuration);

        // 3. 发光结束, 恢复
        SetGlowEffect(false, originalColor); 
        currentState = CubeState.Idle;
        glowCoroutine = null;
    }

    // --- 效果辅助方法 ---

    private void SetupLightComponent()
    {
        cubeLight = GetComponent<Light>();
        if (cubeLight == null)
        {
            cubeLight = gameObject.AddComponent<Light>();
        }
        cubeLight.type = LightType.Point;
        cubeLight.range = 3f;
        cubeLight.intensity = 0f;
        cubeLight.enabled = false;
    }

    private void SetGlowEffect(bool enable, Color color)
    {
        if (cubeLight != null)
        {
            cubeLight.enabled = true;
            cubeLight.intensity = enable ? 1.5f : 0f;
            cubeLight.color = color;
        }

        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = color;
            if (enable)
            {
                // 开启HDR自发光 (如果你的材质支持)
                cubeRenderer.material.EnableKeyword("_EMISSION");
                cubeRenderer.material.SetColor("_EmissionColor", color * 2f);
            }
            else
            {
                // 关闭自发光
                cubeRenderer.material.DisableKeyword("_EMISSION");
                cubeRenderer.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}