using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI组件")]
    public Text finalScoreText;
    public Text maxComboText;
    public Text accuracyText;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("评级系统")]
    public GameObject[] ratingStars; // 星级评价
    public string[] ratingTitles = {"D", "C", "B", "A", "S", "SS", "SSS"};
    
    private void Start()
    {
        // 绑定按钮事件
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }
    
    public void ShowGameOverScreen(int finalScore, int maxCombo, float accuracy)
    {
        // 更新分数显示
        if (finalScoreText != null)
            finalScoreText.text = "最终分数: " + finalScore.ToString("N0");
            
        if (maxComboText != null)
            maxComboText.text = "最大连击: " + maxCombo;
            
        if (accuracyText != null)
            accuracyText.text = "准确率: " + (accuracy * 100).ToString("F1") + "%";
        
        // 计算评级
        int rating = CalculateRating(finalScore, maxCombo, accuracy);
        DisplayRating(rating);
        
        gameObject.SetActive(true);
    }
    
    private int CalculateRating(int score, int combo, float accuracy)
    {
        // 基于分数、连击和准确率计算评级
        float ratingScore = 0f;
        
        // 分数权重 40%
        ratingScore += Mathf.Clamp01(score / 10000f) * 0.4f;
        
        // 连击权重 30%
        ratingScore += Mathf.Clamp01(combo / 50f) * 0.3f;
        
        // 准确率权重 30%
        ratingScore += accuracy * 0.3f;
        
        // 转换为星级 (0-6)
        return Mathf.FloorToInt(ratingScore * 6f);
    }
    
    private void DisplayRating(int rating)
    {
        // 显示星级评价
        for (int i = 0; i < ratingStars.Length; i++)
        {
            if (ratingStars[i] != null)
                ratingStars[i].SetActive(i <= rating);
        }
        
        // 显示评级标题
        if (accuracyText != null)
            accuracyText.text += "\n评级: " + ratingTitles[Mathf.Clamp(rating, 0, ratingTitles.Length - 1)];
    }
    
    private void RestartGame()
    {
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void ReturnToMainMenu()
    {
        // 返回主菜单（如果有的话）
        SceneManager.LoadScene("MainMenu"); // 需要创建主菜单场景
    }
}