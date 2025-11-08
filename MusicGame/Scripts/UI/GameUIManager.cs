using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI maxComboText;
    public TextMeshProUGUI hitText;
    public Slider healthBar;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalComboText;
    public TextMeshProUGUI rankText;
    
    [Header("UI动画")]
    public Animator comboAnimator;
    public Animator hitTextAnimator;
    
    [Header("颜色设置")]
    public Color perfectHitColor = Color.yellow;
    public Color goodHitColor = Color.green;
    public Color normalHitColor = Color.white;
    public Color comboColor = Color.red;
    
    private int currentScore = 0;
    private int currentCombo = 0;
    private int maxCombo = 0;
    
    private void Start()
    {
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        UpdateScoreDisplay(0);
        UpdateComboDisplay(0);
        UpdateMaxComboDisplay(0);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (hitText != null)
            hitText.gameObject.SetActive(false);
    }
    
    public void UpdateScoreDisplay(int score)
    {
        currentScore = score;
        if (scoreText != null)
            scoreText.text = $"分数: {score}";
    }
    
    public void UpdateComboDisplay(int combo)
    {
        currentCombo = combo;
        if (comboText != null)
        {
            comboText.text = $"连击: {combo}";
            
            // 连击数颜色变化
            if (combo >= 10)
                comboText.color = comboColor;
            else if (combo >= 5)
                comboText.color = perfectHitColor;
            else
                comboText.color = normalHitColor;
                
            // 触发连击动画
            if (comboAnimator != null && combo > 0)
                comboAnimator.SetTrigger("Combo");
        }
    }
    
    public void UpdateMaxComboDisplay(int maxCombo)
    {
        this.maxCombo = maxCombo;
        if (maxComboText != null)
            maxComboText.text = $"最大连击: {maxCombo}";
    }
    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }
    
    public void ShowHitText(string text, HitType hitType)
    {
        if (hitText != null && hitTextAnimator != null)
        {
            hitText.gameObject.SetActive(true);
            hitText.text = text;
            
            // 根据命中类型设置颜色
            switch (hitType)
            {
                case HitType.Perfect:
                    hitText.color = perfectHitColor;
                    break;
                case HitType.Good:
                    hitText.color = goodHitColor;
                    break;
                case HitType.Normal:
                    hitText.color = normalHitColor;
                    break;
                case HitType.Miss:
                    hitText.color = Color.red;
                    break;
            }
            
            hitTextAnimator.SetTrigger("ShowHit");
        }
    }
    
    public void ShowGameOver(int finalScore, int finalCombo)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText != null)
                finalScoreText.text = $"最终分数: {finalScore}";
                
            if (finalComboText != null)
                finalComboText.text = $"最终连击: {finalCombo}";
                
            if (rankText != null)
                rankText.text = GetRank(finalScore);
        }
    }
    
    private string GetRank(int score)
    {
        if (score >= 10000) return "SSS";
        if (score >= 8000) return "SS";
        if (score >= 6000) return "S";
        if (score >= 4000) return "A";
        if (score >= 2000) return "B";
        if (score >= 1000) return "C";
        return "D";
    }
    
    public enum HitType
    {
        Perfect,
        Good,
        Normal,
        Miss
    }
}