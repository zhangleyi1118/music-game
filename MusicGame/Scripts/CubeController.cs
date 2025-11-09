using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour
{
    [Header("音符设置")]
    [Tooltip("音符ID (对应AudioManager中的cubeHitSounds索引)")]
    public int noteID = 0;
    [Tooltip("是否为特殊方块（重音）")]
    public bool isSpecial = false;

    [Header("基础得分")]
    public int baseScore = 100;
    public int specialScore = 200;

    [Header("触发设置")]
    public float jumpTriggerHeight = 0.5f;
    public string playerTag = "Player";

    [Header("效果设置")]
    [Tooltip("发光持续时间（秒）")]
    public float glowDuration = 5.0f;
    [Tooltip("普通音符颜色")]
    public Color normalColor = Color.white;
    [Tooltip("重音/闪烁颜色")]
    public Color specialColor = Color.yellow;
    [Tooltip("触发后发光颜色")]
    public Color glowColor = Color.cyan;

    [Header("重音循环效果 (Idle)")]
    [Tooltip("显示状态持续时间（秒）")]
    public float idleShowDuration = 0.5f;
    [Tooltip("消失状态持续时间（秒）")]
    public float idleHideDuration = 0.5f;

    // 内部状态
    private Renderer cubeRenderer;
    private Color originalColor;
    private Transform playerTransform;
    private CharacterBase playerCharacter;
    private Coroutine idleLoopCoroutine;
    private Coroutine glowCoroutine;
    private bool isGlowing = false;
    private Light cubeLight; // 发光组件

    public enum CubeState
    {
        Idle,           // 待机状态
        Glowing         // 被触发后的发光状态
    }
    private CubeState currentState = CubeState.Idle;


    private void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            // 确保获取的是材质实例
            originalColor = cubeRenderer.material.color;
        }

        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
            playerCharacter = player.GetComponent<CharacterBase>();
        }

        // 添加光源
        SetupLightComponent();
        
        // 根据是否为Special，设置初始状态
        SetupIdleState();
    }

    private void Update()
    {
        // 只有在空闲状态才检测跳跃
        if (currentState == CubeState.Idle && playerTransform != null)
        {
            CheckJumpTrigger();
        }
    }

    /// <summary>
    /// 设置初始待机状态（闪烁或静止）
    /// </summary>
    private void SetupIdleState()
    {
        currentState = CubeState.Idle;
        isGlowing = false;

        // 停止所有可能在运行的协程
        if (idleLoopCoroutine != null)
            StopCoroutine(idleLoopCoroutine);
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        // 确保可见
        SetMaterialAlpha(1f);
        gameObject.SetActive(true);

        if (isSpecial)
        {
            // 特殊方块/重音 -> 开始闪烁
            originalColor = specialColor;
            idleLoopCoroutine = StartCoroutine(IdleLoopRoutine());
        }
        else
        {
            // 普通方块 -> 保持静止颜色
            originalColor = normalColor;
            SetGlowEffect(false, normalColor);
        }
    }

    private void CheckJumpTrigger()
    {
        // (您的跳跃检测逻辑已保留)
        Vector3 playerPosition = playerTransform.position;
        Vector3 cubePosition = transform.position;

        Vector2 horizontalDistance = new Vector2(
            playerPosition.x - cubePosition.x,
            playerPosition.z - cubePosition.z
        );

        float heightDiff = playerPosition.y - cubePosition.y;
        bool isInRange = horizontalDistance.magnitude < 1.0f; // 范围检查
        bool isAboveCube = playerPosition.y > cubePosition.y;
        bool isHighEnough = heightDiff > jumpTriggerHeight;

        if (isInRange && isAboveCube && isHighEnough)
        {
            OnJumpTriggered();
        }
    }

    /// <summary>
    /// 核心：当方块被踩踏时
    /// </summary>
    private void OnJumpTriggered()
    {
        // 如果正在发光，不允许再次触发
        if (currentState == CubeState.Glowing) return;

        currentState = CubeState.Glowing;
        isGlowing = true;

        // 1. 停止空闲闪烁
        if (idleLoopCoroutine != null)
            StopCoroutine(idleLoopCoroutine);

        // 2. 强制设为可见
        SetMaterialAlpha(1f);

        // 3. 播放音效 & 加分
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.RegisterNoteHit(noteID, isSpecial ? specialScore : baseScore);
        }

        // 4. 开始发光 & 变身协程
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);
        glowCoroutine = StartCoroutine(GlowAndChangeRoutine());
    }

    /// <summary>
    /// 核心：发光5秒，然后随机变身
    /// </summary>
    private IEnumerator GlowAndChangeRoutine()
    {
        // 1. 立刻开始发光
        SetGlowEffect(true, glowColor);

        // 2. 等待发光时间
        yield return new WaitForSeconds(glowDuration);

        // 3. 发光结束
        SetGlowEffect(false, originalColor); // 恢复
        isGlowing = false;

        // 4. 随机变身
        // 70% 几率变普通, 30% 几率变特殊 (你可以调整这个概率)
        isSpecial = (Random.Range(0, 10) < 3);

        // 随机一个新的音符ID
        if (AudioManager.Instance != null)
        {
            noteID = Random.Range(0, AudioManager.Instance.GetMaxNoteTypes());
        }

        // 5. 重新进入待机状态
        SetupIdleState();
    }


    /// <summary>
    /// 空闲状态的闪烁循环 (用于 'isSpecial' 方块)
    /// </summary>
    private IEnumerator IdleLoopRoutine()
    {
        while (currentState == CubeState.Idle && isSpecial)
        {
            // --- 1. 显示状态 ---
            SetMaterialAlpha(1f);
            if (cubeRenderer != null) cubeRenderer.enabled = true;
            yield return new WaitForSeconds(idleShowDuration);

            // 检查状态 (可能在等待时被触发)
            if (currentState != CubeState.Idle || !isSpecial) break;

            // --- 2. 消失状态 ---
            SetMaterialAlpha(0f);
            yield return new WaitForSeconds(idleHideDuration);

            if (currentState != CubeState.Idle || !isSpecial) break;
        }
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
            cubeLight.enabled = enable;
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

    private void SetMaterialAlpha(float alpha)
    {
        if (cubeRenderer != null && cubeRenderer.material != null)
        {
            Color color = cubeRenderer.material.color;
            color.a = alpha;
            cubeRenderer.material.color = color;
        }
    }
}