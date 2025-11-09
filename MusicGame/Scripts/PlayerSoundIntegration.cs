using UnityEngine;

public class PlayerSoundIntegration : MonoBehaviour
{
    // 这个脚本用于集成现有的玩家状态机与 AudioManager
    
    private void Start()
    {
        // 确保 AudioManager 存在
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager 未找到，请确保场景中有 AudioManager");
        }
    }
    
    // 以下方法可以在各个状态类中调用
    
    public void PlayJumpSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJumpSound();
        }
    }
    
    public void PlayClimbSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClimbSound();
        }
    }
    
    public void PlayFootstepSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFootstepSound();
        }
    }
    
    // 在跳跃状态中调用
    public void OnJump()
    {
        PlayJumpSound();
    }
    
    // 在攀爬状态中调用
    public void OnClimb()
    {
        PlayClimbSound();
    }
    
    // 在移动状态中调用（脚步音效）
    public void OnFootstep()
    {
        PlayFootstepSound();
    }
}