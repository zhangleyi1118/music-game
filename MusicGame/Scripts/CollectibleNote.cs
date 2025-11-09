using UnityEngine;

public class CollectibleNote : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("这个音符的音效ID (对应AudioManager中的specialNoteSounds索引)")]
    public int specialNoteID = 0;
    public string playerTag = "Player";

    [Header("效果")]
    [Tooltip("收集时实例化的特效预制体（可选）")]
    public GameObject collectEffectPrefab;
    [Tooltip("是否自动飘动")]
    public bool animateFloat = true;
    public float floatSpeed = 1f;
    public float floatAmplitude = 0.25f;

    private Vector3 startPos;
    private bool collected = false;

    private void Start()
    {
        startPos = transform.position;
        // 确保它有一个触发器
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"Collider on {name} is not set to 'Is Trigger'. Forcing it.", this);
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        // 简单的上下飘动效果
        if (animateFloat && !collected)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 防止重复触发
        if (collected) return; 

        if (other.CompareTag(playerTag))
        {
            collected = true;
            OnCollected();
        }
    }

    private void OnCollected()
    {
        // 1. 注册到 GameManager (增加计数)
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.RegisterCollectibleHit();
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

        // 4. 销毁自己 (可以加一个淡出协程，但为了快速实现，先直接销毁)
        Destroy(gameObject);
    }
}