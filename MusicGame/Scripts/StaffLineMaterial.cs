using UnityEngine;

public class StaffLineMaterial : MonoBehaviour
{
    [Header("五线谱材质设置")]
    [Range(0.1f, 0.8f)] public float transparency = 0.3f; // 透明度
    public Color staffLineColor = new Color(0.2f, 0.5f, 1f, 1f); // 五线谱蓝色
    public bool enablePulseEffect = true; // 是否启用脉冲效果
    public float pulseSpeed = 2f; // 脉冲速度
    public float pulseMinAlpha = 0.2f; // 脉冲最小透明度
    public float pulseMaxAlpha = 0.5f; // 脉冲最大透明度
    
    [Header("线条样式")]
    public float lineWidth = 0.1f; // 线条宽度
    public bool useStripedPattern = false; // 是否使用条纹图案
    public Texture2D stripedTexture; // 条纹纹理
    
    private Material material;
    private Renderer rend;
    private float pulseTimer;
    
    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            CreateStaffLineMaterial();
        }
    }
    
    private void Update()
    {
        if (enablePulseEffect && material != null)
        {
            UpdatePulseEffect();
        }
    }
    
    private void CreateStaffLineMaterial()
    {
        // 创建半透明材质
        material = new Material(Shader.Find("Standard"));
        
        // 设置透明渲染模式
        SetupTransparentMaterial();
        
        // 设置基础颜色
        Color finalColor = staffLineColor;
        finalColor.a = transparency;
        material.color = finalColor;
        
        // 设置材质属性
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Glossiness", 0.1f);
        
        // 如果使用条纹图案，设置纹理
        if (useStripedPattern && stripedTexture != null)
        {
            material.mainTexture = stripedTexture;
            material.SetTexture("_MainTex", stripedTexture);
        }
        
        // 应用材质
        rend.material = material;
    }
    
    private void SetupTransparentMaterial()
    {
        // 配置透明材质属性
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
    
    private void UpdatePulseEffect()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        
        // 使用正弦波创建脉冲效果
        float pulseAlpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, 
            (Mathf.Sin(pulseTimer) + 1f) * 0.5f);
        
        Color color = material.color;
        color.a = pulseAlpha;
        material.color = color;
    }
    
    // 公共方法
    public void SetTransparency(float alpha)
    {
        transparency = Mathf.Clamp01(alpha);
        if (material != null)
        {
            Color color = material.color;
            color.a = transparency;
            material.color = color;
        }
    }
    
    public void SetColor(Color newColor)
    {
        staffLineColor = newColor;
        if (material != null)
        {
            Color color = newColor;
            color.a = material.color.a;
            material.color = color;
        }
    }
    
    public void EnablePulse(bool enable)
    {
        enablePulseEffect = enable;
    }
    
    public void SetPulseParameters(float speed, float minAlpha, float maxAlpha)
    {
        pulseSpeed = speed;
        pulseMinAlpha = Mathf.Clamp01(minAlpha);
        pulseMaxAlpha = Mathf.Clamp01(maxAlpha);
    }
    
    // 击中效果（当五线谱被碰触时调用）
    public void PlayHitEffect()
    {
        if (material != null)
        {
            StartCoroutine(HitEffectCoroutine());
        }
    }
    
    private System.Collections.IEnumerator HitEffectCoroutine()
    {
        Color originalColor = material.color;
        
        // 短暂变红
        material.color = Color.red;
        
        yield return new WaitForSeconds(0.1f);
        
        // 恢复原色
        material.color = originalColor;
    }
    
    private void OnDestroy()
    {
        if (material != null)
        {
            DestroyImmediate(material);
        }
    }
}