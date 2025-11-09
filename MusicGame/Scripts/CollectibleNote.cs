using UnityEngine;
using System.Collections;

public class CollectibleNote : MonoBehaviour
{
    [Header("音符设置")]
    
    // --- 核心修改 1：从单个ID改为ID列表 ---
    [Tooltip("这个音符 *可能* 触发的音效ID列表 (从AudioManager的specialNoteSounds中选)")]
    // 我们给它一个默认值 {0}，这样即使你不配置，它也能像以前一样工作
    public int[] possibleSpecialNoteIDs = { 0 }; 
    // --- 修改结束 ---

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
        noteRenderer = GetComponent<Renderer>();
        noteCollider = GetComponent<Collider>();

        if (noteCollider != null && !noteCollider.isTrigger)
        {
            noteCollider.isTrigger = true;
        }

        StartMoving();
    }

    private void Update()
    {
        if (currentState == MoveState.Stopped || 
            (MusicGameManager.Instance != null && MusicGameManager.Instance.IsGamePaused))
        {
            // 如果游戏已停止(玩家按了E)或已被收集，则不执行任何操作
            return;
        }

        switch (currentState)
        {
            case MoveState.Moving:
                transform.position = Vector3.MoveTowards(transform.position, endPoint, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, endPoint) < 0.01f)
                {
                    StartWaiting(); 
                }
                break;

            case MoveState.Waiting:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    StartMoving(); 
                }
                break;
        }
    }

    private void StartMoving()
    {
        currentState = MoveState.Moving;
        transform.position = startPoint; 
        SetVisible(true); 
    }

    private void StartWaiting()
    {
        currentState = MoveState.Waiting;
        waitTimer = Random.Range(minWaitTime, maxWaitTime); 
        SetVisible(false); 
    }

    private void SetVisible(bool visible)
    {
        if (noteRenderer != null)
            noteRenderer.enabled = visible;
        if (noteCollider != null)
            noteCollider.enabled = visible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState != MoveState.Moving) return; 

        if (other.CompareTag(playerTag))
        {
            // Player player = other.GetComponent<Player>(); // <-- 1. 删除
            // if (player != null && player.ReusableData.standValueParameter.CurrentValue == 0f) // <-- 2. 删除
            // { // <-- 3. 删除
            //     return; 
            // } // <-- 4. 删除
            
            OnCollectedEffects(); 
            StartWaiting(); 
        }
    }

    /// <summary>
    /// 被收集时的 *瞬时效果*
    /// </summary>
    private void OnCollectedEffects()
    {
        // --- 核心修改 2：在这里随机选择一个ID ---
        int chosenNoteID = 0; // 默认ID为0

        if (possibleSpecialNoteIDs != null && possibleSpecialNoteIDs.Length > 0)
        {
            // 从列表中随机选择一个索引
            int randomIndex = Random.Range(0, possibleSpecialNoteIDs.Length);
            // 获取该索引对应的音效ID
            chosenNoteID = possibleSpecialNoteIDs[randomIndex];
        }
        else
        {
            Debug.LogWarning("CollectibleNote 没有配置 possibleSpecialNoteIDs! 默认使用 ID 0。", this);
        }
        // --- 修改结束 ---


        // 1. 注册到 GameManager (使用我们刚选中的 chosenNoteID)
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.RegisterCollectibleHit(chosenNoteID);
        }

        // 2. 播放特殊音效 (使用我们刚选中的 chosenNoteID)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpecialNoteSound(chosenNoteID);
        }

        // 3. 播放特效
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }
    }


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