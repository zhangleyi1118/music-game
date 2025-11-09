using Animancer;
using UnityEngine;
/*************************************************
作者: HuHu
邮箱: 3112891874@qq.com
功能: 玩家控制核心组件
*************************************************/
[RequireComponent(typeof(AnimancerComponent))]
public class Player : CharacterBase
{
    public PlayerSO playerSO;
    //移动业务
    public  AnimancerComponent animancer { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerReusableData ReusableData { get; private set; }
    public PlayerReusableLogic ReusableLogic { get; private set; }
    public Transform camTransform { get; private set; }

    //服务模块
    public InputService InputService { get; private set; }
    public TimerService TimerService { get; private set; }

    // --- 核心修改：为节奏层添加射线检测距离 ---
    [Header("Music Game Settings")]
    [Tooltip("脚步射线检测距离")]
    public float footstepRaycastDistance = 0.3f;
    // --- 修改结束 ---


    protected override void Awake()
    {
        base.Awake();
        InputService = InputService.Instance;
        TimerService = TimerService.Instance;

        camTransform = Camera.main.transform;
        animancer = GetComponent<AnimancerComponent>();
        if (animancer == null)
        {
            Debug.LogError("未指定Animancer组件，无法播放动画！！");
            return;
        }
        //创建参数
        ReusableData = new PlayerReusableData(animancer, playerSO);
        //创建逻辑
        ReusableLogic = new PlayerReusableLogic(this);
        //创建状态机
        StateMachine = new PlayerStateMachine(this);
        //设置默认开始状态
        StateMachine.ChangeState(StateMachine.idleState);
    }
    protected override void Update()
    {
        base.Update();
        StateMachine?.OnUpdate();
    }
    protected override void OnAnimatorMove()
    {
        base.OnAnimatorMove();
        StateMachine?.OnAnimationUpdate();
    }
    public void AnimationEnd()
    {
        StateMachine?.OnAnimationEnd();
    }

    // --- 核心修改：添加此公共方法 ---
    /// <summary>
    /// (新功能) 此方法由行走/奔跑动画的 Animation Event 调用
    /// </summary>
    public void OnFootstep()
    {
        // 仅在地面状态（Idle或Move）时播放脚步节奏
        if (StateMachine.currentState != StateMachine.idleState && 
            StateMachine.currentState != StateMachine.moveLoopState &&
            StateMachine.currentState != StateMachine.moveStartState &&
            StateMachine.currentState != StateMachine.moveEndState)
        {
            return;
        }
        
        // 从玩家位置向下发射射线
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, footstepRaycastDistance))
        {
            // 检查是否走在 "MusicCube" 上
            if (hit.collider.CompareTag("MusicCube"))
            {
                CubeController cube = hit.collider.GetComponent<CubeController>();
                if (cube != null && AudioManager.Instance != null)
                {
                    // 播放此 Cube 指定的 "行走节奏音"
                    AudioManager.Instance.PlayWalkingSound(cube.walkingNoteID);
                }
            }
            // (可选) 在这里你还可以为"非MusicCube"的地面添加普通脚步声
            // else 
            // {
            //     AudioManager.Instance.PlayFootstepSound(); 
            // }
        }
    }
    // --- 修改结束 ---
}