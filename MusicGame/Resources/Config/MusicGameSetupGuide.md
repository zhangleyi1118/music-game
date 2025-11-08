# 音乐游戏配置指南

## 1. 场景设置步骤

### 第一步：创建UI Canvas
1. 在Hierarchy中右键 -> UI -> Canvas
2. 设置Canvas Scaler：UI Scale Mode = Scale With Screen Size, Reference Resolution = 1920x1080
3. 添加以下UI元素：
   - ScoreText (TextMeshPro)
   - ComboText (TextMeshPro) 
   - MaxComboText (TextMeshPro)
   - HealthBar (Slider)
   - GameOverPanel (Panel)

### 第二步：添加音乐游戏管理器
1. 创建空GameObject命名为"MusicGameManager"
2. 添加`MusicGameManager`组件
3. 将UI元素拖拽到对应槽位

### 第三步：设置玩家和音效
1. 确保玩家预制体有Player标签
2. 添加AudioSource组件到玩家
3. 设置背景音乐和音效文件

## 2. 预制体创建指南

### Cube预制体创建：
1. 创建Cube GameObject
2. 添加`CubeController`组件
3. 设置碰撞器和触发器
4. 保存为预制体：`Assets/MusicGame/Prefabs/Cube.prefab`

### 五线谱预制体：
1. 创建Quad GameObject
2. 添加`StaffLineController`组件
3. 设置材质为半透明蓝色
4. 设置碰撞器
5. 保存为预制体：`Assets/MusicGame/Prefabs/StaffLine.prefab`