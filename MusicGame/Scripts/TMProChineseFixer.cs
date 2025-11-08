using UnityEngine;
using TMPro;

// TextMeshPro中文字体修复脚本
public class TMProChineseFixer : MonoBehaviour
{
    [Header("字体设置")]
    [Tooltip("如果为true，自动修复所有TextMeshPro组件")]
    public bool autoFixAllTextMeshPro = true;
    
    [Tooltip("首选字体名称（支持中文的字体）")]
    public string[] preferredFonts = { "Arial", "Microsoft YaHei", "SimHei", "SimSun" };
    
    void Start()
    {
        if (autoFixAllTextMeshPro)
        {
            FixAllTextMeshProComponents();
        }
    }
    
    void FixAllTextMeshProComponents()
    {
        // 查找场景中所有的TextMeshPro组件
        TextMeshProUGUI[] textComponents = FindObjectsOfType<TextMeshProUGUI>(true);
        
        foreach (TextMeshProUGUI textComponent in textComponents)
        {
            FixTextMeshProFont(textComponent);
        }
        
        Debug.Log($"修复了 {textComponents.Length} 个TextMeshPro组件的中文字体");
    }
    
    void FixTextMeshProFont(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        // 检查当前字体是否支持中文
        if (textComponent.font != null && textComponent.font.name.Contains("LiberationSans"))
        {
            // 尝试加载支持中文的字体
            TMP_FontAsset chineseFont = FindChineseFont();
            
            if (chineseFont != null)
            {
                textComponent.font = chineseFont;
                Debug.Log($"已将 {textComponent.name} 的字体更改为 {chineseFont.name}");
            }
            else
            {
                Debug.LogWarning($"未能找到支持中文的字体，请手动设置 {textComponent.name} 的字体");
            }
        }
    }
    
    TMP_FontAsset FindChineseFont()
    {
        // 尝试加载预设的字体
        foreach (string fontName in preferredFonts)
        {
            TMP_FontAsset fontAsset = TMPro.TMP_FontAsset.CreateFontAsset(Resources.GetBuiltinResource<Font>(fontName + ".ttf"));
            if (fontAsset != null)
            {
                return fontAsset;
            }
        }
        
        // 如果没有找到预设字体，尝试使用默认字体
        TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (TMP_FontAsset font in allFonts)
        {
            if (font != null && !font.name.Contains("LiberationSans"))
            {
                return font;
            }
        }
        
        return null;
    }
    
    // 在编辑器中手动调用修复
    [ContextMenu("手动修复TextMeshPro字体")]
    public void ManualFix()
    {
        FixAllTextMeshProComponents();
    }
}