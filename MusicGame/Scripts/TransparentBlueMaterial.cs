using UnityEngine;

public class TransparentBlueMaterial : MonoBehaviour
{
    [Header("材质设置")]
    [Range(0f, 1f)] public float alpha = 0.3f; // 透明度
    public Color baseColor = new Color(0f, 0.4f, 1f, 1f); // 基础蓝色
    
    private Material material;
    private Renderer rend;
    
    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            CreateTransparentMaterial();
        }
    }
    
    private void CreateTransparentMaterial()
    {
        // 创建新的半透明材质
        material = new Material(Shader.Find("Standard"));
        
        // 设置为半透明渲染模式
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        
        // 设置颜色和透明度
        Color finalColor = baseColor;
        finalColor.a = alpha;
        material.color = finalColor;
        
        // 应用材质
        rend.material = material;
    }
    
    // 动态调整透明度
    public void SetAlpha(float newAlpha)
    {
        alpha = Mathf.Clamp01(newAlpha);
        if (material != null)
        {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }
    
    // 动态调整颜色
    public void SetColor(Color newColor)
    {
        baseColor = newColor;
        if (material != null)
        {
            Color color = newColor;
            color.a = alpha;
            material.color = color;
        }
    }
    
    private void OnDestroy()
    {
        if (material != null)
        {
            DestroyImmediate(material);
        }
    }
}