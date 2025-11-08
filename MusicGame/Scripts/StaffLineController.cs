using UnityEngine;

public class StaffLineController : MonoBehaviour
{
    [Header("五线谱设置")]
    public float moveSpeed = 5f;
    public float damage = 10f;
    public bool isActive = true;
    
    [Header("视觉效果")]
    public ParticleSystem hitEffect;
    public AudioClip hitSound;
    
    private void Update()
    {
        if (isActive)
        {
            // 向左移动
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
            
            // 检查是否超出屏幕
            if (transform.position.x < -15f)
            {
                Destroy(gameObject);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            // 对玩家造成伤害
            if (MusicGameManager.Instance != null)
            {
                MusicGameManager.Instance.TakeDamage(damage);
            }
            
            // 播放击中效果
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // 播放击中音效
            if (hitSound != null && MusicGameManager.Instance != null)
            {
                MusicGameManager.Instance.effectAudio.PlayOneShot(hitSound);
            }
            
            // 销毁五线谱
            Destroy(gameObject);
        }
    }
    
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }
}