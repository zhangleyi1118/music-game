using UnityEngine;
using System.Collections;

public class CollectibleNote : MonoBehaviour
{
    [Header("音符设置")]
    [Tooltip("这个音符的音效ID (对应AudioManager中的specialNoteSounds索引)")]
    public int specialNoteID = 0;
    public string playerTag = "Player";

    [Header("移动路径设置")]
    [Tooltip("移动的起始点（右侧点）")]
    public Vector3 startPoint;
    [Tooltip("移动的目标点（左侧点）")]
    public Vector3 endPoint;
    [Tooltip("从右到左的移动速度")]
    public float moveSpeed = 3f;

    [Header("循环设置")]
    [Tooltip("到达左侧后，重置回右侧的最小等待时间")]
    public float minWaitTime = 2f;
    [Tooltip("到达左侧后，重置回右侧的最大等待时间")]
    public float maxWaitTime = 5f;

    [Header("效果")]
    [Tooltip("收集时实例化的特效预制体（可选）")]
    public GameObject collectEffectPrefab;

    // --- 内部状态 ---
    private enum MoveState
    {
        Moving, // 正在从右向左移动
        Waiting, // 在左侧点隐藏并等待
        Stopped // 游戏已结束
    }
    private MoveState currentState = MoveState.Moving;

    private float waitTimer;
    
    // 组件引用
    private Renderer noteRenderer;
    private Collider noteCollider;

    private void Start()
    {
        // 获取渲染器和碰撞体，用于隐藏
        noteRenderer = GetComponent<Renderer>();
        noteCollider = GetComponent<Collider>();

        // 确保碰撞体是触发器
        if (noteCollider != null && !noteCollider.isTrigger)
        {
            noteCollider.isTrigger = true;
        }

        // 游戏开始时，立刻出现在起点
        StartMoving();
    }

    private void Update()
    {
        // 如果游戏已停止，则不执行任何操作
        if (currentState == MoveState.Stopped)
        {
            return;
        }

        // 检查游戏是否结束 (按 'E' 键会触发这个)
        // (这依赖于你在 MusicGameManager.cs 中添加了 'public bool GameHasEnded => gameHasEnded;')
        if (MusicGameManager.Instance != null && MusicGameManager.Instance.GameHasEnded)
        {
            currentState = MoveState.Stopped;
            SetVisible(false); // 游戏结束，隐藏自己
            return;
        }

        // 状态机逻辑
        switch (currentState)
        {
            case MoveState.Moving:
                // --- 移动逻辑 ---
                transform.position = Vector3.MoveTowards(transform.position, endPoint, moveSpeed * Time.deltaTime);

                // 检查是否已（正常）到达终点
                if (Vector3.Distance(transform.position, endPoint) < 0.01f)
                {
                    StartWaiting(); // 到达终点，开始等待
                }
                break;

            case MoveState.Waiting:
                // --- 等待逻辑 ---
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    StartMoving(); // 等待结束，重新开始移动
                }
                break;
        }
    }

    /// <summary>
    /// 开始新一轮的“从右到左”移动
    /// </summary>
    private void StartMoving()
    {
        currentState = MoveState.Moving;
        transform.position = startPoint; // 重置到右侧起点
        SetVisible(true); // 让自己可见可碰
    }

    /// <summary>
    /// 开始随机等待（无论是到达终点还是被碰到）
    /// </summary>
    private void StartWaiting()
    {
        currentState = MoveState.Waiting;
        waitTimer = Random.Range(minWaitTime, maxWaitTime); // 设置随机倒计时
        SetVisible(false); // 隐藏自己
    }

    /// <summary>
    /// 控制小球的可见性和可碰撞性
    /// </summary>
    private void SetVisible(bool visible)
    {
        if (noteRenderer != null)
            noteRenderer.enabled = visible;
        if (noteCollider != null)
            noteCollider.enabled = visible;
    }

    /// <summary>
    /// 玩家触碰检测 (已修正逻辑！)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 只有在移动状态(可见时)才能被收集
        if (currentState != MoveState.Moving) return; 

        if (other.CompareTag(playerTag))
        {
            Player player = other.GetComponent<Player>();

            // --- 【已修正的简单方法】 ---
            // 检查玩家是否存在，并读取 'standValueParameter' 的 'CurrentValue'
            // 根据你的描述，效果反了，所以 0f 才是蹲下
            if (player != null && player.ReusableData.standValueParameter.CurrentValue == 0f) // <-- 关键修改！从 1f 改为 0f
            {
                // 玩家正在蹲下 (值为0f)，不触发收集，直接返回
                return; 
            }
            // --- 检查结束 ---

            // 玩家站着 (值为1f)，正常收集：
            // 1. 立即触发收集效果
            OnCollectedEffects(); 
            
            // 2. 立即进入等待状态
            StartWaiting(); 
        }
    }

    /// <summary>
    /// 被收集时的 *瞬时效果* /// </summary>
    private void OnCollectedEffects()
    {
        // 1. 注册到 GameManager (增加计数)
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.RegisterCollectibleHit(specialNoteID);
        }

        // 2. 播放特殊音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpecialNoteSound(specialNoteID);
        }

        // 3. 播放特效（如果设置了）
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        // (不再销D'hui自己)
    }

    /// <summary>
    /// (可选) 在 Scene 视图中绘制出路径，方便你配置
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPoint, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPoint, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPoint, endPoint);
    }
}