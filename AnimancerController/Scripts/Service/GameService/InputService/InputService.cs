using UnityEngine;
/**************************************************************************
作者: HuHu
邮箱: 3112891874@qq.com
功能: 玩家输入控制类，基于InputSystem
**************************************************************************/
public class InputService : MonoSingleton<InputService>
{
    public InputMap inputMap;
    protected override void Awake()
    {
        base.Awake();
        if (inputMap == null)
        {
            inputMap = new InputMap();
        }
        
        // 确保inputMap不为null后再启用
        if (inputMap != null)
        {
            inputMap.Enable();
        }
    }
    
    // --- 核心修改在这里 ---
    // 我们必须移除 Disable() 调用，
    // 因为它在场景重新加载时会永久禁用输入系统。
    private void OnDestroy()
    {
        /*
        if (inputMap != null)
        {
            inputMap.Disable();
        }
        */
    }
    // --- 修改结束 ---

    /// <summary>
    /// 封装走和跑以及PC和安卓的区别的移动量
    /// </summary>
    public Vector2 GetMoveHorizontalValue
    {
        get
        {
            if (inputMap == null) return Vector2.zero;
            
            //安卓
#if UNITY_ANDROID
                return inputMap.Player.Move.ReadValue<Vector2>();
            //非安卓
#elif !UNITY_ANDROID
            Vector2 dir = inputMap.Player.Move.ReadValue<Vector2>();
            bool isShift = inputMap.Player.Shift.ReadValue<float>()!=0;

            if (dir != Vector2.zero && isShift)
            {
                dir.y = 0;
                return dir.normalized;
            }
            else if (dir != Vector2.zero && !isShift)
            {
                dir.y = 0;
                return dir.normalized;
            }
            else
            {
                return Vector2.zero;
            }
#else
                return Vector2.zero; // 默认值
#endif
        }
    }
    public Vector2 GetMoveVerticalValue
    {
        get
        {
            if (inputMap == null) return Vector2.zero;
            
            Vector2 dir = inputMap.Player.Move.ReadValue<Vector2>();

            if (dir != Vector2.zero)
            {
                dir.x = 0;
                return dir.normalized;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

    public bool Interactive => inputMap != null && inputMap.Player.Interactive.ReadValue<float>()!= 0;

    public bool Shift
    {
       get
        {
           return inputMap != null && inputMap.Player.Shift.ReadValue<float>() != 0;
        }
    }

    public Vector2 Move
    {
        get
        {
            if (inputMap == null) return Vector2.zero;
            
            Vector2 vector2 = inputMap.Player.Move.ReadValue<Vector2>();
            if (vector2.x > 0)
            {
                vector2.x = 1;
            }
            else if (vector2.x < 0)
            {
                vector2.x = -1;
            }
            else
            {
                vector2.x = 0;
            }
            if (vector2.y > 0)
            {
                vector2.y = 1;
            }
            else if (vector2.y < 0)
            {
                vector2.y = -1;
            }
            else
            {
                vector2.y = 0;
            }
            return vector2;
        }
    }
    public Vector2 Scroll => inputMap != null ? inputMap.Player.Scroll.ReadValue<Vector2>() : Vector2.zero;

}