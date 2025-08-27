# Unity场景快速设置指南

## 🎮 为什么看不到游戏界面？

你现在看到的是空场景，因为我们只创建了脚本文件，还没有在Unity中搭建游戏对象。需要手动创建游戏场景。

## 🚀 快速设置步骤（5分钟搞定）

### 第1步：创建基础场景对象

在Unity的Hierarchy窗口中，右键点击空白处，按以下顺序创建：

#### 1. 创建玩家
```
右键 → Create Empty → 重命名为 "Player"
选中Player → 在Inspector中点击 "Add Component"
搜索并添加 "PlayerSetup" 脚本
确保 PlayerSetup 的 "Auto Setup" 勾选为 true
```

#### 2. 创建第一个Boss（球球）
```
右键 → Create Empty → 重命名为 "Boss_QiuQiu"
选中Boss_QiuQiu → 添加 "BossSetup" 脚本
在BossSetup组件中：
- Boss Type 设置为 "QiuQiu"
- Auto Setup 勾选为 true
```

#### 3. 创建第二个Boss（果果）
```
右键 → Create Empty → 重命名为 "Boss_GuoGuo"  
选中Boss_GuoGuo → 添加 "BossSetup" 脚本
在BossSetup组件中：
- Boss Type 设置为 "GuoGuo"
- Auto Setup 勾选为 true
```

#### 4. 创建游戏管理器
```
右键 → Create Empty → 重命名为 "GameManager"
选中GameManager → 添加 "GameManager" 脚本
```

#### 5. 创建UI系统
```
右键 → UI → Canvas （如果没有的话）
选中Canvas → 添加 "UIManager" 脚本
```

#### 6. 创建音效管理器
```
右键 → Create Empty → 重命名为 "AudioManager"
选中AudioManager → 添加 "AudioManager" 脚本
```

#### 7. 创建调试管理器
```
右键 → Create Empty → 重命名为 "DebugManager"
选中DebugManager → 添加 "DebugManager" 脚本
```

### 第2步：调整Boss位置

选中Boss对象，在Inspector的Transform组件中设置位置：
- **Boss_QiuQiu**: Position设为 (5, 0, 5)
- **Boss_GuoGuo**: Position设为 (-5, 0, -5)

### 第3步：点击播放按钮

点击Unity顶部的播放按钮 ▶️

## 🎯 预期效果

设置完成后，你应该看到：

1. **玩家视角**：第一人称摄像机视角
2. **红色球体**：球球Boss（会自动生成）
3. **绿色胶囊**：果果Boss（会自动生成）
4. **Boss名称和血条**：Boss头上显示名称和血条
5. **地面**：自动生成的竞技场地面

## 🎮 控制方式

- **WASD** - 移动
- **鼠标** - 视角控制
- **鼠标左键** - 攻击
- **1/2/3键** - 使用技能
- **F1键** - 显示调试信息

## 🔧 如果还是看不到

### 检查清单：
- [ ] 确保所有脚本都已编译无错误
- [ ] 确保PlayerSetup的Auto Setup已勾选
- [ ] 确保BossSetup的Auto Setup已勾选
- [ ] 点击播放按钮后等待几秒（脚本需要初始化）

### 常见问题：

#### 1. 看到黑屏
**原因**：没有摄像机
**解决**：确保Player对象有PlayerSetup脚本，它会自动创建摄像机

#### 2. 看不到Boss
**原因**：Boss没有正确生成
**解决**：
- 检查Console窗口是否有错误
- 确保BossSetup脚本已添加
- 确保Boss Type设置正确

#### 3. 无法移动
**原因**：玩家控制器没有正确设置
**解决**：确保Player对象有PlayerSetup脚本

### 调试方法：

1. **查看Console窗口**：
   - Window → General → Console
   - 查看是否有红色错误信息

2. **按F1键**：
   - 显示调试信息
   - 查看玩家和Boss状态

3. **检查Scene视图**：
   - 点击Scene标签
   - 查看是否有对象生成

## 🎨 可选：美化场景

如果基础功能正常，可以添加：

### 添加光照
```
右键 → Light → Directional Light
设置Rotation为 (50, -30, 0)
```

### 添加地面
```
右键 → 3D Object → Plane
设置Scale为 (3, 1, 3)
重命名为 "Ground"
```

### 添加天空盒
```
Window → Rendering → Lighting
在Environment标签中设置Skybox Material
```

## 📝 完整的Hierarchy结构

设置完成后，你的Hierarchy应该看起来像这样：

```
Scene
├── Main Camera (会被PlayerSetup删除)
├── Directional Light
├── Player
│   ├── Player Camera (自动生成)
│   └── Attack Point (自动生成)
├── Boss_QiuQiu
│   ├── QiuQiu Model (自动生成)
│   └── Name Canvas (自动生成)
├── Boss_GuoGuo
│   ├── GuoGuo Model (自动生成)
│   └── Name Canvas (自动生成)
├── GameManager
├── AudioManager
├── DebugManager
├── Canvas
│   └── EventSystem
└── Ground (可选)
```

## ⚡ 超快速设置（1分钟版本）

如果你想最快体验游戏：

1. 创建空对象 "Player" + PlayerSetup脚本
2. 创建空对象 "Boss_QiuQiu" + BossSetup脚本（类型设为QiuQiu）
3. 创建空对象 "GameManager" + GameManager脚本
4. 点击播放 ▶️

这样就能看到基本的游戏效果了！

---

**提示**：所有Setup脚本都有自动配置功能，会自动创建必要的子对象和组件，所以设置过程很简单！
