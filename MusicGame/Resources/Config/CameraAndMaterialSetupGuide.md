# 摄像机和材质设置指南

## 1. 半透明蓝色材质设置

### 方法一：使用代码自动创建（推荐）
1. **为五线谱添加材质组件**：
   - 选择五线谱GameObject
   - 添加 `StaffLineMaterial` 组件
   - 调整参数：
     - `transparency` = 0.3 (透明度)
     - `staffLineColor` = RGB(51, 128, 255) (蓝色)
     - `enablePulseEffect` = true (启用脉冲效果)

### 方法二：手动创建材质（在Unity编辑器中）
1. **创建新材质**：
   - Project窗口右键 → Create → Material
   - 命名为 "StaffLineBlueTransparent"

2. **设置材质属性**：
   - **Shader**: Standard
   - **Rendering Mode**: Transparent
   - **Albedo**: RGB(51, 128, 255, 77) (蓝色半透明)
   - **Metallic**: 0
   - **Smoothness**: 0.1

3. **应用到五线谱**：
   - 将材质拖拽到五线谱GameObject的Renderer组件

## 2. 固定俯视角摄像机设置

### 步骤一：替换现有摄像机
1. **查找主摄像机**：
   - 在Hierarchy中找到 "Main Camera" 或主摄像机对象

2. **添加固定摄像机脚本**：
   - 移除现有的摄像机控制脚本（如OrbitControls）
   - 添加 `FixedTopDownCamera` 组件

3. **配置摄像机参数**：
   ```csharp
   // 基本设置
   Target = 玩家的Transform（自动查找）
   Offset = (0, 15, -5)
   Smooth Speed = 5
   
   // 视角设置
   Camera Angle = 45
   Distance = 10
   Min Distance = 5
   Max Distance = 20
   
   // 边界设置（可选）
   Use Bounds = true
   Bounds X = (-50, 50)
   Bounds Z = (-50, 50)
   ```

### 步骤二：禁用鼠标控制
- `FixedTopDownCamera` 会自动禁用鼠标控制
- 确保没有其他脚本控制摄像机旋转

## 3. 预制体设置

### 五线谱预制体完整设置：
1. **创建五线谱GameObject**：
   - 创建Quad或Plane
   - 命名为 "StaffLine"

2. **添加组件**：
   - `StaffLineController` (控制逻辑)
   - `StaffLineMaterial` (材质控制)
   - `BoxCollider` (碰撞检测)

3. **材质设置**：
   ```csharp
   // 在StaffLineMaterial中：
   transparency = 0.3f
   staffLineColor = new Color(0.2f, 0.5f, 1f, 1f)
   enablePulseEffect = true
   pulseSpeed = 2f
   ```

### Cube预制体材质设置：
1. **创建不同颜色的Cube**：
   - 普通Cube：白色材质
   - 节奏Cube：闪烁的金色材质
   - 特殊Cube：不同的颜色和效果

2. **添加材质变化脚本**：
   ```csharp
   // 可以创建类似的MaterialController脚本
   // 用于动态改变Cube颜色和效果
   ```

## 4. 场景布置建议

### 摄像机视角优化：
- **高度**：10-15米，确保能看到整个游戏区域
- **角度**：45度俯角，提供良好的视野
- **跟随平滑度**：中等（5-8），避免过于生硬或延迟

### 五线谱视觉效果：
- **透明度**：0.2-0.4，足够透明但可见
- **脉冲效果**：增强节奏感
- **移动速度**：与音乐BPM匹配

## 5. 测试要点

### 摄像机测试：
1. 移动玩家，确保摄像机平滑跟随
2. 测试边界限制功能
3. 验证没有鼠标控制干扰

### 材质测试：
1. 确认五线谱半透明效果
2. 测试脉冲动画
3. 检查碰撞时的视觉效果

## 6. 故障排除

### 常见问题：
1. **材质不透明**：检查Rendering Mode是否为Transparent
2. **摄像机不跟随**：确认Target已正确设置
3. **鼠标仍可控制**：检查是否有残留的摄像机控制脚本

### 解决方案：
- 重新应用材质设置
- 重启Unity编辑器
- 检查组件依赖关系