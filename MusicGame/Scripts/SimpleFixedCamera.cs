using UnityEngine;

// 强化的固定摄像机脚本，完全禁用Cinemachine和其他控制
public class SimpleFixedCamera : MonoBehaviour
{
    [Header("摄像机设置")]
    public Transform target; // 拖拽玩家对象到这里
    public float height = 15f; // 摄像机高度
    public float distance = 10f; // 摄像机距离
    [Range(30f, 45f)] public float tiltAngle = 35f; // 俯视角度（30-45度）
    public float smoothSpeed = 5f; // 移动平滑度
    
    private Vector3 offset; // 固定的摄像机偏移
    
    void Start()
    {
        // 强制禁用所有控制脚本
        ForceDisableAllControls();
        
        // 自动查找玩家
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        // 计算摄像机偏移
        CalculateOffset();
        
        // 设置初始位置
        SetInitialPosition();
    }
    
    void LateUpdate()
    {
        if (target != null)
        {
            // 摄像机跟随目标，保持固定偏移
            Vector3 targetPosition = target.position + offset;
            
            // 平滑移动摄像机
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
            
            // 设置摄像机旋转（俯视玩家）
            Vector3 lookDirection = target.position - transform.position;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
            }
        }
    }
    
    void CalculateOffset()
    {
        if (target != null)
        {
            // 计算基于俯视角度的偏移
            Vector3 direction = Quaternion.Euler(tiltAngle, 0, 0) * Vector3.back;
            offset = direction * distance;
            offset.y = height;
        }
    }
    
    void SetInitialPosition()
    {
        if (target != null)
        {
            // 直接设置位置和旋转
            transform.position = target.position + offset;
            
            Vector3 lookDirection = target.position - transform.position;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
    
    void ForceDisableAllControls()
    {
        // 禁用所有可能控制摄像机的脚本（更彻底的检查）
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string scriptName = script.GetType().Name;
            string scriptNamespace = script.GetType().Namespace ?? "";
            
            // 禁用Cinemachine相关脚本
            if (scriptNamespace.Contains("Cinemachine") ||
                scriptName.Contains("Orbit") || 
                scriptName.Contains("Mouse") ||
                scriptName.Contains("Rotate") ||
                scriptName.Contains("FreeLook") ||
                scriptName.Contains("Follow") ||
                scriptName.Contains("Camera") && !scriptName.Contains("SimpleFixedCamera"))
            {
                Debug.Log($"禁用摄像机控制脚本: {scriptName}");
                script.enabled = false;
            }
        }
        
        // 检查并禁用可能存在的虚拟摄像机
        GameObject[] vcams = GameObject.FindGameObjectsWithTag("MainCamera");
        foreach (GameObject vcam in vcams)
        {
            MonoBehaviour[] vcamScripts = vcam.GetComponents<MonoBehaviour>();
            foreach (var script in vcamScripts)
            {
                if (script.GetType().Namespace?.Contains("Cinemachine") == true)
                {
                    script.enabled = false;
                    Debug.Log($"禁用虚拟摄像机脚本: {script.GetType().Name}");
                }
            }
        }
    }
    
    // 在Inspector中调整角度时实时更新
    void OnValidate()
    {
        CalculateOffset();
        if (Application.isPlaying && target != null)
        {
            SetInitialPosition();
        }
    }
}