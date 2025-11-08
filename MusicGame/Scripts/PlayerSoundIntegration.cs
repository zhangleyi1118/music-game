using UnityEngine;

public class PlayerSoundIntegration : MonoBehaviour
{
    // 这个脚本用于集成现有的玩家状态机与音乐游戏音效系统
    
    private void Start()
    {
        // 确保音乐游戏管理器存在
        if (MusicGameManager.Instance == null)
        {
            Debug.LogWarning("MusicGameManager 未找到，请确保场景中有 MusicGameManager");
        }
    }
    
    // 以下方法可以在各个状态类中调用
    
    public void PlayJumpSound()
    {
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.PlayJumpSound();
        }
    }
    
    public void PlayClimbSound()
    {
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.PlayClimbSound();
        }
    }
    
    public void PlayFootstepSound()
    {
        if (MusicGameManager.Instance != null)
        {
            MusicGameManager.Instance.PlayFootstepSound();
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