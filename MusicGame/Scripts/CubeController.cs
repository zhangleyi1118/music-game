using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour
{
    [Header("方块设置")]
    public int cubeType = 0; // 方块类型，对应不同音效
    public int baseScore = 100;

    [Header("跳跃触发设置")]
    public float jumpTriggerHeight = 0.5f; // 跳跃触发高度阈值

    [Header("发光效果设置")]
    [Tooltip("发光持续时间（秒）")]
    public float glowDuration = 5.0f; // 发光持续时间
    [Tooltip("发光颜色")]
    public Color glowColor = Color.yellow; // 发光颜色
    [Tooltip("闪烁频率（每秒闪烁次数）")]
    public float flashFrequency = 2.0f; // 闪烁频率
    [Tooltip("渐隐效果持续时间（秒）")]
    public float fadeDuration = 1.0f; // 渐隐效果持续时间
    [Tooltip("是否启用闪烁效果")]
    public bool enableFlashing = true; // 是否启用闪烁
    [Tooltip("是否启用渐隐效果")]
    public bool enableFadeOut = true; // 是否启用渐隐
    [Tooltip("是否完全消失")]
    public bool disappearAfterGlow = true; // 发光后是否消失

    [Header("空闲状态循环效果")]
    [Tooltip("是否启用空闲状态循环显示/消失")]
    public bool enableIdleLoop = true; // 是否启用空闲状态循环
    [Tooltip("显示状态持续时间（秒）")]
    public float idleShowDuration = 1.0f; // 显示状态持续时间
    [Tooltip("消失状态持续时间（秒）")]
    public float idleHideDuration = 1.0f; // 消失状态持续时间

    [Header("检测模式")]
    public DetectionMode detectionMode = DetectionMode.Physics; // 默认改为物理碰撞模式
    public string playerTag = "Player"; // 玩家标签

    [Header("物理碰撞设置")]
    public bool isObstacle = false; // 是否是障碍物（物理阻挡）
    public ColliderType colliderType = ColliderType.Box; // 碰撞器类型
    public Vector3 colliderSize = Vector3.one; // 碰撞器大小

    private Renderer cubeRenderer;
    private Color originalColor;
    private Transform playerTransform;
    private Collider cubeCollider;
    private CubeState currentState = CubeState.Idle;
    private Coroutine glowCoroutine;
    private Coroutine idleLoopCoroutine;
    private Light cubeLight; // 发光效果组件
    private CharacterBase playerCharacter; // 角色控制器引用

    public enum DetectionMode
    {
        Distance,       // 距离检测模式（音乐游戏方块）
        Physics,        // 物理碰撞模式（普通障碍物）
        Hybrid          // 混合模式（同时具有物理碰撞和距离检测）
    }

    public enum ColliderType
    {
        Box,
        Sphere,
        Capsule
    }

    public enum CubeState
    {
        Idle,           // 待机状态
        Jumped,         // 已被跳跃触发
        Glowing,        // 发光状态
        Disappearing    // 消失状态
    }

    private void Start()
    {
        cubeRenderer = GetComponent<Renderer>();

        if (cubeRenderer != null)
        {
            // !! 优化 !! 确保获取的是材质实例，避免修改共享材质
            originalColor = cubeRenderer.material.color;
        }

        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
            playerCharacter = player.GetComponent<CharacterBase>();
        }

        // 根据检测模式设置碰撞器
        SetupCollider();

        // 添加发光效果组件
        AddLightComponent();

        // 开始空闲状态循环
        if (enableIdleLoop)
        {
            StartIdleLoop();
        }
    }

    private void Update()
    {
        if (currentState == CubeState.Idle && playerTransform != null && playerCharacter != null)
        {
            // 无论方块是否可见，都检测跳跃
            CheckJumpTrigger();
        }
    }

    private void CheckJumpTrigger()
    {
        // 检查玩家是否在跳跃状态并且与方块顶部接触
        if (playerCharacter != null)
        {
            // 玩家正在跳跃，检查是否在方块上方
            Vector3 playerPosition = playerTransform.position;
            Vector3 cubePosition = transform.position;

            // 计算玩家与方块的水平距离
            Vector2 horizontalDistance = new Vector2(
                playerPosition.x - cubePosition.x,
                playerPosition.z - cubePosition.z
            );

            // 检查玩家是否在方块正上方，并且跳跃高度超过阈值
            // 放宽条件：只需要玩家在方块上方且有一定高度差
            float heightDiff = playerPosition.y - cubePosition.y;
            bool isInRange = horizontalDistance.magnitude < 1.0f;
            bool isAboveCube = playerPosition.y > cubePosition.y;
            bool isHighEnough = heightDiff > jumpTriggerHeight;

            // 调试输出
            if (Time.frameCount % 60 == 0) // 每60帧输出一次调试信息
            {
                Debug.Log($"Cube {gameObject.name}: Range={isInRange}, Above={isAboveCube}, Height={heightDiff:F2}, Trigger={jumpTriggerHeight:F2}, HighEnough={isHighEnough}");
            }

            if (isInRange && isAboveCube && isHighEnough)
            {
                // 跳跃触发方块
                OnJumpTriggered();
            }
        }
    }

    private void OnJumpTriggered()
    {
        if (currentState != CubeState.Idle) return;

        currentState = CubeState.Jumped;

        // 播放音效
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.PlayJumpSound();
            MusicGameManager.Instance.PlayCubeHitSound(cubeType);
            MusicGameManager.Instance.AddScore(baseScore, false);
        }

        // 开始发光效果
        StartGlowEffect();
    }

    private void StartGlowEffect()
    {
        currentState = CubeState.Glowing;

        // 停止空闲状态循环
        if (idleLoopCoroutine != null)
            StopCoroutine(idleLoopCoroutine);
        
        // !! 关键修复 !!
        // 无论方块在被触发时是否可见（Alpha=0），
        // 都必须立即强制设为可见（Alpha=1）并重置颜色，
        // 以便接下来的发光效果能正确显示。
        if (cubeRenderer != null)
        {
            SetMaterialAlpha(1f); // 强制设为不透明
            cubeRenderer.material.color = originalColor; // 恢复原始色（GlowIntensity会覆盖）
        }
        if (cubeCollider != null)
        {
            cubeCollider.enabled = true; // 确保碰撞器也启用
        }
        // !! 修复结束 !!

        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        glowCoroutine = StartCoroutine(GlowRoutine());
    }

    private void StartIdleLoop()
    {
        if (idleLoopCoroutine != null)
            StopCoroutine(idleLoopCoroutine);

        idleLoopCoroutine = StartCoroutine(IdleLoopRoutine());
    }

    private System.Collections.IEnumerator GlowRoutine()
    {
        // 立即发光
        SetGlowEffect(true);

        float timer = 0f;
        while (timer < glowDuration)
        {
            // 闪烁效果
            if (enableFlashing)
            {
                // !! 优化 !! 使用 Sin 的绝对值来实现更清晰的“闪烁” (0 -> 1 -> 0)
                float flashValue = Mathf.Abs(Mathf.Sin(timer * flashFrequency * Mathf.PI));
                SetGlowIntensity(flashValue);
            }
            else
            {
                // 如果不启用闪烁，保持最大亮度
                SetGlowIntensity(1f);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 闪烁结束，根据设置决定是否消失
        if (disappearAfterGlow)
        {
            currentState = CubeState.Disappearing;
            if (enableFadeOut)
            {
                StartCoroutine(DisappearRoutine());
            }
            else
            {
                // 直接消失
                gameObject.SetActive(false);
            }
        }
        else
        {
            // 不消失，恢复原状并重新开始空闲循环
            SetGlowEffect(false);
            currentState = CubeState.Idle;

            if (enableIdleLoop)
            {
                StartIdleLoop();
            }
        }
    }

    // !! 优化/重写 !!
    // 这是完全重写的空闲循环协程，以匹配你的需求
    // (显示 X 秒，消失 Y 秒)
    private System.Collections.IEnumerator IdleLoopRoutine()
    {
        // 确保当前是空闲状态
        currentState = CubeState.Idle;

        // 确保方块开始时是可见的
        gameObject.SetActive(true);
        SetGlowEffect(false); // 移除Glow
        SetMaterialAlpha(1f); // 设为可见
        if (cubeCollider != null) cubeCollider.enabled = true; // 确保碰撞器启用

        // 循环，直到状态改变或循环被禁用
        while (currentState == CubeState.Idle && enableIdleLoop)
        {
            // --- 1. 显示状态 ---
            // 确保完全显示（以防万一）
            SetMaterialAlpha(1f);
            if (cubeRenderer != null) cubeRenderer.enabled = true; // 确保渲染器启用
            if (cubeCollider != null) cubeCollider.enabled = true;

            // 等待“显示”时间
            yield return new WaitForSeconds(idleShowDuration);

            // 在等待后检查状态是否已改变（例如被玩家触发）
            if (currentState != CubeState.Idle || !enableIdleLoop)
                break;

            // --- 2. 消失状态 ---
            // 立即设置为“消失”（透明）
            SetMaterialAlpha(0f);
            
            // 注意：这里我们不禁用 cubeRenderer，因为 SetMaterialAlpha(0) 应该使其不可见。
            // 如果 SetMaterialAlpha 不起作用 (材质不是 Transparent)，
            // 你应该用 cubeRenderer.enabled = false; 替换 SetMaterialAlpha(0f)
            // 并在上面显示状态时用 cubeRenderer.enabled = true;
            
            if (cubeCollider != null) cubeCollider.enabled = false; // 消失时禁用碰撞器

            // 等待“消失”时间
            yield return new WaitForSeconds(idleHideDuration);

            // 再次检查状态
            if (currentState != CubeState.Idle || !enableIdleLoop)
                break;
            
            // 循环将自动回到显示状态
        }
    }

    private System.Collections.IEnumerator DisappearRoutine()
    {
        // 渐隐效果
        float timer = 0f;

        while (timer < fadeDuration)
        {
            float alpha = 1f - (timer / fadeDuration);
            SetMaterialAlpha(alpha);
            SetLightIntensity(alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        // 完全消失
        gameObject.SetActive(false);
    }

    private void AddLightComponent()
    {
        cubeLight = gameObject.AddComponent<Light>();
        cubeLight.type = LightType.Point;
        cubeLight.range = 3f;
        cubeLight.intensity = 0f;
        cubeLight.color = glowColor;
        cubeLight.enabled = false;
    }

    private void SetGlowEffect(bool enable)
    {
        if (cubeLight != null)
        {
            cubeLight.enabled = enable;
            cubeLight.intensity = enable ? 1f : 0f;
        }

        if (cubeRenderer != null)
        {
            if (enable)
            {
                cubeRenderer.material.color = glowColor;
            }
            else
            {
                cubeRenderer.material.color = originalColor;
            }
        }
    }

    private void SetGlowIntensity(float intensity)
    {
        if (cubeLight != null)
        {
            cubeLight.intensity = intensity;
        }

        if (cubeRenderer != null)
        {
            // !! 优化 !! 使用 Color.Lerp 来在原始颜色和发光色之间插值
            // 这样“暗”的时候是 originalColor，而不是黑色
            Color currentColor = Color.Lerp(originalColor, glowColor, intensity);
            
            // 保持Alpha不变，只修改RGB
            float currentAlpha = cubeRenderer.material.color.a;
            currentColor.a = currentAlpha;
            
            cubeRenderer.material.color = currentColor;
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

    private void SetLightIntensity(float intensity)
    {
        if (cubeLight != null)
        {
            cubeLight.intensity = intensity;
        }
    }

    private void SetupCollider()
    {
        // 移除任何现有碰撞器
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider != null)
        {
            // 缓存碰撞器引用
            cubeCollider = existingCollider;
        }

        // 根据检测模式设置碰撞器
        switch (detectionMode)
        {
            case DetectionMode.Physics:
            case DetectionMode.Hybrid:
                // 物理碰撞模式需要碰撞器
                if (cubeCollider == null) CreateCollider(); // 只在没有时创建
                if (cubeCollider != null) cubeCollider.enabled = true; // 确保启用
                break;
            case DetectionMode.Distance:
                // 距离检测模式不需要碰撞器
                if (cubeCollider != null) cubeCollider.enabled = false; // 禁用
                break;
        }

        // 确保碰撞器属性设置正确
        if (cubeCollider != null)
        {
            cubeCollider.isTrigger = !isObstacle;
        }
    }

    private void CreateCollider()
    {
        // 再次检查，防止重复添加
        cubeCollider = GetComponent<Collider>();
        if (cubeCollider != null) return;

        switch (colliderType)
        {
            case ColliderType.Box:
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = colliderSize;
                cubeCollider = boxCollider;
                break;
            case ColliderType.Sphere:
                SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = colliderSize.x * 0.5f;
                cubeCollider = sphereCollider;
                break;
            case ColliderType.Capsule:
                CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.height = colliderSize.y;
                capsuleCollider.radius = colliderSize.x * 0.5f;
                cubeCollider = capsuleCollider;
                break;
        }

        if (cubeCollider != null)
        {
            // 设置碰撞器属性
            cubeCollider.isTrigger = !isObstacle; // 障碍物不是触发器，音乐方块是触发器
        }
    }

    // 重置方块状态
    public void ResetCube()
    {
        currentState = CubeState.Idle;

        // 停止所有协程
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        if (idleLoopCoroutine != null)
            StopCoroutine(idleLoopCoroutine);

        // 恢复视觉和物理状态
        gameObject.SetActive(true);
        SetGlowEffect(false); // 移除光效并恢复原始颜色
        
        if (cubeRenderer != null)
        {
            // !! 关键修复 !!
            // 确保重置时 Alpha 也是 1
            SetMaterialAlpha(1f);
            cubeRenderer.material.color = originalColor; // 确保颜色也重置
        }
        
        if (cubeCollider != null)
        {
            // !! 关键修复 !!
            // 确保碰撞器也已启用
            cubeCollider.enabled = true;
        }

        // 重新开始空闲循环
        if (enableIdleLoop)
        {
            StartIdleLoop();
        }
    }

    // 碰撞检测方法（用于物理碰撞模式）
    private void OnTriggerEnter(Collider other)
    {
        if (detectionMode == DetectionMode.Physics || detectionMode == DetectionMode.Hybrid)
        {
            if (other.CompareTag(playerTag) && !isObstacle)
            {
                // 保持物理碰撞但不触发跳跃逻辑
                // 跳跃逻辑由专门的跳跃检测处理
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (detectionMode == DetectionMode.Physics || detectionMode == DetectionMode.Hybrid)
        {
            if (collision.gameObject.CompareTag(playerTag) && isObstacle)
            {
                // 障碍物碰撞，可以在这里处理阻挡逻辑
                Debug.Log($"Player collided with obstacle cube: {gameObject.name}");
            }
        }
    }

    // 公开方法：设置检测模式
    public void SetDetectionMode(DetectionMode mode)
    {
        detectionMode = mode;
        SetupCollider();
    }

    // 公开方法：设置是否为障碍物
    public void SetAsObstacle(bool obstacle)
    {
        isObstacle = obstacle;
        if (cubeCollider != null)
        {
            cubeCollider.isTrigger = !obstacle;
        }
    }

    // 在Scene视图中绘制检测范围
    private void OnDrawGizmosSelected()
    {
        // 绘制跳跃触发高度
        Gizmos.color = Color.cyan;
        Vector3 triggerTop = transform.position + Vector3.up * jumpTriggerHeight;
        Gizmos.DrawLine(transform.position, triggerTop);
        Gizmos.DrawWireSphere(triggerTop, 0.2f);

        // 绘制发光范围
        if (cubeLight != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, cubeLight.range);
        }
    }

    // 在Inspector中更改时实时更新
    private void OnValidate()
    {
        // 在OnValidate中完全避免任何组件操作
        // 碰撞器的实际创建和更新将在运行时通过Start()方法处理

        // 只进行数据验证，不进行任何组件操作
        if (jumpTriggerHeight < 0) jumpTriggerHeight = 0.1f;
        if (glowDuration < 0) glowDuration = 1.0f;
        if (flashFrequency < 0) flashFrequency = 1.0f;

        // 确保碰撞器尺寸合理
        if (colliderSize.x < 0.1f) colliderSize.x = 0.1f;
        if (colliderSize.y < 0.1f) colliderSize.y = 0.1f;
        if (colliderSize.z < 0.1f) colliderSize.z = 0.1f;
    }
}