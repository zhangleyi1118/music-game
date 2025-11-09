using Animancer;
using UnityEngine;

public class PlayerLandState : PlayerMovementState
{
    public PlayerLandState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();

        // --- 核心修改：落地时触发 Cube ---
        CheckForMusicCubeOnLand();
        // --- 修改结束 ---

        // (Your original animation logic)
        if (player.isOnGround.Value)
        {
            AnimancerState state = null;
            int index = 0;
            if (player.verticalSpeed < -15)
            {
                index = 1;
            }
            if (!reusableData.isInPlaceJump)
            {
                if (playerSO.playerMovementData.PlayerJumpFallAndLandData.forwardJumpLand.Length == 1)
                {
                    index = 0;
                }
                state = animancer.Play(playerSO.playerMovementData.PlayerJumpFallAndLandData.forwardJumpLand[index]);
            }
            else
            {
                if (playerSO.playerMovementData.PlayerJumpFallAndLandData.placeJumpLand.Length == 1)
                {
                    index = 0;
                }
                state = animancer.Play(playerSO.playerMovementData.PlayerJumpFallAndLandData.placeJumpLand[index]);
            }
            state.Events(player).SetCallback(playerSO.playerParameterData.moveInterruptEvent, OnInputInterruption);
            state.Events(player).OnEnd = OnStateDefaultEnd;
        }
        else
        {
            OnStateDefaultEnd();
        }
    }

    /// <summary>
    /// (新功能) 向下检测并触发音乐方块
    /// </summary>
    private void CheckForMusicCubeOnLand()
    {
        // 从玩家脚底（稍微向上一点的位置）向下发射射线
        Vector3 rayStart = player.transform.position + Vector3.up * 0.1f;
        
        // 向下检测0.3米（这个距离你可能需要微调）
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 0.3f))
        {
            // 检查是否-碰到了我们设置的 "MusicCube" 标签
            if (hit.collider.CompareTag("MusicCube"))
            {
                // 尝试获取方块的控制器
                CubeController cube = hit.collider.GetComponent<CubeController>();
                if (cube != null)
                {
                    // 找到了！调用它的 OnPlayerLand() 方法来播放旋律音和闪烁
                    cube.OnPlayerLand();
                }
            }
        }
    }
}