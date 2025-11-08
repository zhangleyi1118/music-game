using UnityEngine;

public class FixedTopDownCamera : MonoBehaviour
{
    [Header("摄像机设置")]
    public Transform target; // 跟随的目标（玩家）
    public Vector3 offset = new Vector3(0f, 15f, -5f); // 摄像机偏移
    public float smoothSpeed = 5f; // 平滑移动速度
    
    [Header("视角设置")]
    public float cameraAngle = 45f; // 俯视角角度
    public float distance = 10f; // 摄像机距离
    public float minDistance = 5f; // 最小距离
    public float maxDistance = 20f; // 最大距离
    
    [Header("边界限制")]
    public bool useBounds = true; // 是否使用边界限制
    public Vector2 boundsX = new Vector2(-50f, 50f); // X轴边界
    public Vector2 boundsZ = new Vector2(-50f, 50f); // Z轴边界
    
    private Camera cam;
    private Vector3 desiredPosition;
    private bool isFollowing = true;
    
    private void Start()
    {
        cam = GetComponent<Camera>();
        if (target == null)
        {
            // 自动查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        // 设置初始位置
        SetupInitialPosition();
        
        // 禁用鼠标输入控制
        DisableMouseControl();
    }
    
    private void SetupInitialPosition()
    {
        if (target != null)
        {
            // 计算初始位置
            Vector3 targetPos = GetBoundedTargetPosition();
            desiredPosition = targetPos + CalculateOffset();
            
            // 立即设置位置（不经过平滑）
            transform.position = desiredPosition;
            
            // 设置朝向
            transform.LookAt(targetPos + Vector3.up * 2f);
        }
    }
    
    private void LateUpdate()
    {
        if (isFollowing && target != null)
        {
            FollowTarget();
        }
    }
    
    private void FollowTarget()
    {
        // 获取有边界限制的目标位置
        Vector3 targetPos = GetBoundedTargetPosition();
        
        // 计算期望位置
        desiredPosition = targetPos + CalculateOffset();
        
        // 平滑移动摄像机
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // 保持朝向目标
        transform.LookAt(targetPos + Vector3.up * 2f);
    }
    
    private Vector3 GetBoundedTargetPosition()
    {
        Vector3 targetPos = target.position;
        
        if (useBounds)
        {
            // 应用边界限制
            targetPos.x = Mathf.Clamp(targetPos.x, boundsX.x, boundsX.y);
            targetPos.z = Mathf.Clamp(targetPos.z, boundsZ.x, boundsZ.y);
        }
        
        return targetPos;
    }
    
    private Vector3 CalculateOffset()
    {
        // 根据角度和距离计算偏移
        float radAngle = cameraAngle * Mathf.Deg2Rad;
        
        float xOffset = Mathf.Sin(radAngle) * distance;
        float yOffset = Mathf.Cos(radAngle) * distance;
        
        return new Vector3(xOffset, yOffset, 0f) + offset;
    }
    
    private void DisableMouseControl()
    {
        // 禁用OrbitControls组件（如果存在）
        MonoBehaviour[] orbitControls = GetComponents<MonoBehaviour>();
        foreach (var control in orbitControls)
        {
            if (control.GetType().Name.Contains("Orbit"))
            {
                control.enabled = false;
            }
        }
        
        // 禁用鼠标输入相关组件
        MonoBehaviour[] inputComponents = GetComponents<MonoBehaviour>();
        foreach (var component in inputComponents)
        {
            if (component.GetType().Name.Contains("Mouse") || 
                component.GetType().Name.Contains("Input"))
            {
                component.enabled = false;
            }
        }
        
        // 确保摄像机没有鼠标控制脚本
        Debug.Log("摄像机鼠标控制已禁用，现在是固定俯视角");
    }
    
    // 公共方法用于控制摄像机
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetDistance(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }
    
    public void SetAngle(float newAngle)
    {
        cameraAngle = Mathf.Clamp(newAngle, 10f, 80f);
    }
    
    public void ToggleFollowing(bool follow)
    {
        isFollowing = follow;
    }
    
    // 摄像机震动效果（可选）
    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = originalPos + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPos;
    }
}