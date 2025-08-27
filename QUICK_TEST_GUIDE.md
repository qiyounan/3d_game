# 快速测试指南 - 在电脑上试玩游戏

## 🎮 立即开始测试

### 1. Unity中直接运行
1. 打开Unity编辑器
2. 确保场景是 `Assets/Scenes/GameScene.unity`
3. 点击播放按钮 ▶️
4. 游戏将在Unity编辑器中运行

### 2. PC端控制方式

#### 移动控制
- **W/A/S/D** - 前后左右移动
- **鼠标移动** - 视角控制
- **空格键** - 跳跃
- **Shift** - 跑步（如果启用）

#### 战斗控制
- **鼠标左键** - 普通攻击
- **1/2/3键** - 使用技能1/2/3
- **R键** - 重新加载（如果需要）

#### 调试快捷键（开发版本）
- **F1** - 切换调试信息显示
- **F2** - 开启/关闭上帝模式
- **F3** - 击杀所有Boss
- **F4** - 重置游戏
- **ESC** - 暂停游戏

### 3. 快速设置步骤

#### 创建基础场景
1. 在Unity中创建新的空场景
2. 添加以下基础对象：

```
场景层级结构：
├── Main Camera (删除，会被玩家摄像机替代)
├── Directional Light
├── Player (空对象)
│   └── 添加 PlayerSetup 脚本
├── Boss_QiuQiu (空对象)
│   └── 添加 BossSetup 脚本 (类型设为QiuQiu)
├── Boss_GuoGuo (空对象)
│   └── 添加 BossSetup 脚本 (类型设为GuoGuo)
├── GameManager (空对象)
│   └── 添加 GameManager 脚本
├── AudioManager (空对象)
│   └── 添加 AudioManager 脚本
├── EffectManager (空对象)
│   └── 添加 EffectManager 脚本
├── DebugManager (空对象)
│   └── 添加 DebugManager 脚本
└── Canvas (UI)
    └── 添加 UIManager 脚本
```

#### 自动设置脚本
大部分组件会自动配置：
- `PlayerSetup` 会自动创建玩家控制器和摄像机
- `BossSetup` 会自动创建Boss模型和UI
- `GameManager` 会管理游戏流程

### 4. 预期游戏体验

#### 游戏开始
- 玩家在场景中心生成
- 两个Boss（红色球体和绿色胶囊）在周围生成
- 可以看到Boss头上的名称和血条

#### 战斗体验
- 使用鼠标控制视角，WASD移动
- 鼠标左键攻击Boss
- 数字键1/2/3使用技能
- Boss会主动攻击玩家
- 击败所有Boss获得胜利

#### Boss特色
- **球球（红色）**：会滚动冲撞、跳跃攻击
- **果果（绿色）**：会投掷攻击、自我治疗

### 5. 常见问题解决

#### 如果没有看到Boss
1. 检查Boss对象是否添加了BossSetup脚本
2. 确保BossSetup的autoSetup为true
3. 查看Console是否有错误信息

#### 如果控制不响应
1. 确保Player对象有PlayerSetup脚本
2. 检查是否有多个摄像机冲突
3. 确保场景中有EventSystem（UI需要）

#### 如果没有UI显示
1. 检查Canvas是否存在
2. 确保UIManager脚本已添加
3. 在移动端UI会自动隐藏，PC端主要靠键盘鼠标

#### 性能问题
1. 降低Unity编辑器的质量设置
2. 关闭不必要的窗口
3. 使用Game视图而不是Scene视图测试

### 6. 调试信息

按F1键可以看到：
- 当前FPS
- 玩家生命值
- Boss状态
- 性能信息

### 7. 快速修改建议

#### 调整游戏难度
编辑 `GameBalance.cs` 中的数值：
- 增加玩家血量：`player.maxHealth`
- 降低Boss血量：`qiuqiuBoss.maxHealth`
- 调整攻击伤害：`player.attackDamage`

#### 修改Boss行为
在Boss脚本中调整：
- 攻击频率：`attackCooldown`
- 移动速度：`moveSpeed`
- 攻击范围：`attackRange`

### 8. 构建PC版本（可选）

如果想要独立的exe文件：
1. File > Build Settings
2. 选择 "PC, Mac & Linux Standalone"
3. 点击 "Build"
4. 选择输出文件夹
5. 运行生成的exe文件

### 9. 预期测试时间

- **基础设置**：5-10分钟
- **第一次游玩**：10-15分钟
- **调试和调整**：根据需要

### 10. 测试重点

重点测试以下功能：
- [ ] 玩家移动和视角控制
- [ ] 攻击命中检测
- [ ] Boss AI反应
- [ ] 技能释放效果
- [ ] 生命值系统
- [ ] 游戏胜利/失败逻辑

---

**提示**：如果遇到任何问题，可以查看Unity Console窗口的错误信息，或者使用F1键查看调试信息。大部分功能都有自动设置，应该能够快速体验到游戏效果！
