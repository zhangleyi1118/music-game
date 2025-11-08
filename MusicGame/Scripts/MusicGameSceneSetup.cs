using UnityEngine;

public class MusicGameSceneSetup : MonoBehaviour
{
    [Header("场景设置")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;
    public MusicGameManager gameManager;
    public CubeSpawner cubeSpawner;
    public StaffLineSpawner staffLineSpawner;
    
    [Header("背景音乐")]
    public AudioClip levelMusic;
    public float musicBPM = 120f;
    
    private void Start()
    {
        SetupScene();
    }
    
    private void SetupScene()
    {
        // 确保玩家存在
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            
            // 设置玩家标签
            player.tag = "Player";
        }
        
        // 设置游戏管理器
        if (gameManager != null)
        {
            // 设置背景音乐
            if (levelMusic != null)
            {
                gameManager.levelMusic = new AudioClip[] { levelMusic };
            }
            
            // 设置BPM
            gameManager.currentBPM = musicBPM;
        }
        
        // 连接节拍事件
        if (cubeSpawner != null && gameManager != null)
        {
            // 这里需要修改 MusicGameManager 以支持事件订阅
        }
        
        if (staffLineSpawner != null && gameManager != null)
        {
            // 这里需要修改 MusicGameManager 以支持事件订阅
        }
    }
    
    public void StartGame()
    {
        if (gameManager != null)
        {
            // 开始游戏逻辑
            gameManager.InitializeGame();
        }
    }
    
    public void ResetGame()
    {
        // 重置方块生成器
        if (cubeSpawner != null)
        {
            cubeSpawner.ResetAllCubes();
        }
        
        // 重置五线谱生成器
        if (staffLineSpawner != null)
        {
            staffLineSpawner.ResetAllStaffLines();
        }
        
        // 重置玩家位置
        if (playerSpawnPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = playerSpawnPoint.position;
                player.transform.rotation = playerSpawnPoint.rotation;
            }
        }
    }
}