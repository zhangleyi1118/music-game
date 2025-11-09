using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour
{
    [Header("音符设置")]
    [Tooltip("音符ID (对应AudioManager中的cubeHitSounds索引)")]
    public int noteID = 0;
    [Tooltip("是否为特殊方块（例如黑键，仅用于视觉区分）")]
    public bool isSpecial = false;

    [Header("触发设置")]
    public float jumpTriggerHeight = 0.5f;
    public string playerTag = "Player";

    [Header("效果设置")]
    [Tooltip("发光持续时间（秒）")]
    public float glowDuration = 2.0f;
    [Tooltip("消失后重生延迟（秒）")]
    public float respawnDelay = 3.0f; 

    [Tooltip("普通音符颜色（白键）")]
    public Color normalColor = Color.white;
    [Tooltip("特殊音符颜色（黑键）")]
    public Color specialColor = Color.black;
    [Tooltip("触发后发光颜色")]
    public Color glowColor = Color.cyan;

    [Header("特殊方块循环效果 (Idle)")]
    [Tooltip("（此功能可选）显示状态持续时间（秒）")]
    public float idleShowDuration = 0.5f;
    [Tooltip("（此功能可选）消失状态持续时间（秒）")]
    public float idleHideDuration = 0.5f;

    // 内部状态
    private Renderer cubeRenderer;
    private Collider cubeCollider;
    private Color originalColor;
    private Transform playerTransform;
    private Coroutine glowCoroutine;
    private Coroutine idleLoopCoroutine;
    private Light cubeLight; // 发光组件

    public enum CubeState
    {
        Idle,           // 待机状态
        Triggered       // 被触发（发光和消失中）
    }
    private CubeState currentState = CubeState.Idle;


    private void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        cubeCollider = GetComponent<Collider>();

        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // 添加光源
        SetupLightComponent();
        
        // 根据是否为Special，设置初始状态
        // 确保在SetupIdleState之前设置好originalColor
        originalColor = isSpecial ? specialColor : normalColor;
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

        // 停止所有可能在运行的协程
        if (idleLoopCoroutine != null)
            StopCoroutine(idleLoopCoroutine);
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        // 确保可见
        SetVisualsActive(true);
        SetGlowEffect(false, originalColor); // 恢复原始颜色

        if (isSpecial)
        {
            // 特殊方块/黑键 -> 开始闪烁 (如果你不希望黑键闪烁，可以注释掉下面这行)
            idleLoopCoroutine = StartCoroutine(IdleLoopRoutine());
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
        if (currentState == CubeState.Triggered) return;

        currentState = CubeState.Triggered;

        // 1. 停止空闲闪烁
        if (idleLoopCoroutine != null)
            StopCoroutine(idleLoopCoroutine);

        // 2. 播放音效 & 录制 (不再传递分数)
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.RegisterNoteHit(noteID);
        }

        // 3. 开始发光 & 消失 & 重生 协程
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);
        glowCoroutine = StartCoroutine(GlowAndRespawnRoutine());
    }

    /// <summary>
    /// 核心：发光，消失，然后重生
    /// </summary>
    private IEnumerator GlowAndRespawnRoutine()
    {
        // 1. 立刻开始发光
        SetGlowEffect(true, glowColor);

        // 2. 等待发光时间
        yield return new WaitForSeconds(glowDuration);

        // 3. 发光结束 & 消失
        SetGlowEffect(false, originalColor); // 恢复
        SetVisualsActive(false); // 隐藏

        // 4. 等待重生时间
        yield return new WaitForSeconds(respawnDelay);

        // 5. 重新进入待机状态
        SetVisualsActive(true); // 显现
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
            yield return new WaitForSeconds(idleShowDuration);

            if (currentState != CubeState.Idle || !isSpecial) break;

            // --- 2. 消失状态 ---
            SetMaterialAlpha(0f);
            yield return new WaitForSeconds(idleHideDuration);

            if (currentState != CubeState.Idle || !isSpecial) break;
        }
        // 结束后确保恢复
        SetMaterialAlpha(1f);
    }

    // --- 效果辅助方法 ---

    private void SetVisualsActive(bool active)
    {
        if (cubeRenderer != null)
            cubeRenderer.enabled = active;
        if (cubeCollider != null)
            cubeCollider.enabled = active;
        if (cubeLight != null)
            cubeLight.enabled = active;
        
        // 如果是禁用，也关闭发光效果
        if (!active && cubeLight != null)
        {
             cubeLight.intensity = 0f;
        }
    }

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
            cubeLight.enabled = true; // 即使关闭发光，组件也保持启用，仅控制强度
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