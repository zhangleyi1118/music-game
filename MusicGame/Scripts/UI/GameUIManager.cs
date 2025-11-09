using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    
    // (移除了 Combo, MaxCombo, HealthBar, HitText, RankText)

    private void Start()
    {
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        UpdateScoreDisplay(0);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = $"分数: {score}";
    }
    
    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText != null)
                finalScoreText.text = $"最终分数: {finalScore}";
        }
    }
    
    // (移除了 UpdateComboDisplay, UpdateMaxComboDisplay, UpdateHealthBar, ShowHitText, GetRank)
}