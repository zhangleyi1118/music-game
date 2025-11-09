using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 用于加载场景

// 为编辑器添加引用
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 管理开始菜单界面的所有UI逻辑
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    [Header("UI 面板")]
    [Tooltip("包含开始、玩法、退出按钮的主面板")]
    public GameObject mainPanel;
    
    [Tooltip("玩法说明面板")]
    public GameObject howToPlayPanel;   

    [Header("主菜单按钮")]
    [Tooltip("进入游戏按钮")]
    public Button startGameButton;      
    
    [Tooltip("玩法说明按钮")]
    public Button howToPlayButton;    
    
    [Tooltip("退出游戏按钮")]
    public Button exitGameButton;       

    [Header("玩法说明面板")]
    [Tooltip("关闭玩法说明的按钮")]
    public Button closeHowToPlayButton; 

    [Header("要加载的场景名")]
    [Tooltip("你的主游戏场景的名字 (请确保这个场景在 Build Settings 中)")]
    public string mainGameSceneName = "YourMainGameSceneName"; // (!!!) 记得在Inspector中改成你游戏场景的实际名字

    
    void Start()
    {
        // --- 初始化UI状态 ---
        
        // 确保玩法说明窗口一开始是隐藏的
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        // 确保主菜单是显示的
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        // --- 绑定所有按钮的点击事件 ---
        
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }

        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.AddListener(ShowHowToPlay);
        }

        if (exitGameButton != null)
        {
            exitGameButton.onClick.AddListener(ExitGame);
        }

        if (closeHowToPlayButton != null)
        {
            closeHowToPlayButton.onClick.AddListener(HideHowToPlay);
        }
        
        // --- 确保游戏环境正确 ---
        
        // 在主菜单，确保鼠标是可见和可用的
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // 确保游戏时间是正常流动的（以防从暂停的游戏场景返回）
        Time.timeScale = 1f;
    }

    /// <summary>
    /// “进入游戏”按钮调用
    /// </summary>
    public void StartGame()
    {
        // 检查场景名是否为空
        if (string.IsNullOrEmpty(mainGameSceneName))
        {
            Debug.LogError("StartGame: 游戏场景名 (mainGameSceneName) 未在 Inspector 中设置!");
            return;
        }
        
        Debug.Log($"开始加载场景: {mainGameSceneName}");
        SceneManager.LoadScene(mainGameSceneName);
    }

    /// <summary>
    /// “玩法说明”按钮调用
    /// </summary>
    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }
        if (mainPanel != null)
        {
            mainPanel.SetActive(false); // 隐藏主菜单
        }
    }

    /// <summary>
    /// “关闭玩法说明”按钮调用
    /// </summary>
    public void HideHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
        if (mainPanel != null)
        {
            mainPanel.SetActive(true); // 重新显示主菜单
        }
    }

    /// <summary>
    /// “退出游戏”按钮调用
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("退出游戏!");

        // 在 Unity 编辑器中停止运行
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        // 在编译好的游戏中退出程序
        #else
        Application.Quit();
        #endif
    }
}