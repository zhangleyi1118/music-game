using UnityEngine;
using System.Collections.Generic;

public class CubeManager : MonoBehaviour
{
    public static CubeManager Instance { get; private set; }
    
    [Header("方块管理")]
    [Space(10)]
    [Header("检测设置")]
    public LayerMask obstacleLayerMask = 1; // 障碍物层级
    public float sphereCastRadius = 0.5f; // 球形检测半径
    public float maxDetectionDistance = 2f; // 最大检测距离
    
    private List<CubeController> allCubes = new List<CubeController>();
    private Transform playerTransform;
    private CharacterBase characterBase;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 查找玩家和角色控制器
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            characterBase = player.GetComponent<CharacterBase>();
        }
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        
        // 检测玩家前方的障碍物
        CheckObstacleAhead();
        
        // 更新所有方块的状态
        UpdateAllCubes();
    }
    
    private void CheckObstacleAhead()
    {
        if (characterBase == null) return;
        
        // 检测玩家移动方向上的障碍物
        Vector3 moveDirection = characterBase.AnimationVelocity.normalized;
        if (moveDirection.magnitude < 0.1f) return;
        
        // 使用球形检测
        RaycastHit[] hits = Physics.SphereCastAll(
            playerTransform.position, 
            sphereCastRadius, 
            moveDirection, 
            maxDetectionDistance, 
            obstacleLayerMask
        );
        
        // 处理检测到的障碍物
        foreach (RaycastHit hit in hits)
        {
            CubeController cube = hit.collider.GetComponent<CubeController>();
            if (cube != null && cube.isObstacle)
            {
                // 障碍物阻挡逻辑
                HandleObstacleBlock(cube, hit.distance);
            }
        }
    }
    
    private void HandleObstacleBlock(CubeController obstacleCube, float distance)
    {
        // 根据距离决定阻挡强度
        float blockingStrength = 1f - Mathf.Clamp01(distance / maxDetectionDistance);
        
        // 这里可以实现更复杂的阻挡逻辑
        // 比如降低移动速度、播放阻挡动画等
        
        if (blockingStrength > 0.8f)
        {
            // 完全阻挡
            Debug.Log($"完全阻挡在障碍物前: {obstacleCube.name}");
        }
        else if (blockingStrength > 0.3f)
        {
            // 部分阻挡
            Debug.Log($"部分阻挡: {obstacleCube.name}");
        }
    }
    
    private void UpdateAllCubes()
    {
        foreach (CubeController cube in allCubes)
        {
            // 可以根据需要更新方块状态
            // 比如基于距离的视觉反馈等
        }
    }
    
    // 注册方块
    public void RegisterCube(CubeController cube)
    {
        if (!allCubes.Contains(cube))
        {
            allCubes.Add(cube);
        }
    }
    
    // 取消注册方块
    public void UnregisterCube(CubeController cube)
    {
        if (allCubes.Contains(cube))
        {
            allCubes.Remove(cube);
        }
    }
    
    // 根据类型获取所有方块
    public List<CubeController> GetCubesByType(CubeController.DetectionMode detectionMode)
    {
        return allCubes.FindAll(cube => cube.detectionMode == detectionMode);
    }
    
    // 在Scene视图中绘制检测范围
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        
        // 绘制球形检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerTransform.position, sphereCastRadius);
        
        // 绘制检测方向
        if (characterBase != null && characterBase.AnimationVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerTransform.position, characterBase.AnimationVelocity.normalized * maxDetectionDistance);
        }
    }
}