using UnityEngine;

// 改进的半透明材质脚本，使用URP兼容的Shader
public class SimpleTransparentMaterial : MonoBehaviour
{
    [Header("材质设置")]
    [Range(0f, 1f)] public float transparency = 0.3f;
    public Color color = new Color(0.2f, 0.5f, 1f, 1f);
    
    void Start()
    {
        ApplyTransparentMaterial();
    }
    
    void ApplyTransparentMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // 尝试使用URP的Unlit/Transparent Shader，如果不存在则使用Legacy Shader
            Material mat;
            
            // 先尝试URP Shader
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader != null)
            {
                mat = new Material(urpShader);
                // 设置URP材质属性
                mat.SetFloat("_Surface", 1); // Transparent surface
                mat.SetFloat("_Blend", 0);   // Alpha blend
                mat.SetFloat("_AlphaClip", 0); // No alpha clip
                mat.SetFloat("_Smoothness", 0.1f);
                mat.SetFloat("_Metallic", 0f);
            }
            else
            {
                // 回退到Legacy Shader
                mat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
            }
            
            // 设置颜色和透明度
            Color finalColor = color;
            finalColor.a = transparency;
            mat.color = finalColor;
            
            rend.material = mat;
        }
    }
    
    // 在Inspector中值变化时实时更新
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyTransparentMaterial();
        }
    }
}