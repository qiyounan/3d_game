# 3D Boss战斗游戏 - 构建和部署指南

## 项目概述

这是一个基于Unity开发的3D第一人称Boss战斗手机游戏，玩家需要击败两个Boss：球球和果果。

## 开发环境要求

### Unity版本
- **推荐版本**: Unity 2022.3.10f1 LTS
- **最低版本**: Unity 2021.3.0f1

### 平台SDK要求

#### Android开发
- Android SDK API Level 22 (Android 5.1) 或更高
- Android NDK r21d 或更高
- JDK 8 或更高

#### iOS开发
- Xcode 12.0 或更高
- iOS 11.0 或更高
- macOS 10.15 或更高

## 项目设置

### 1. 导入项目
1. 使用Unity Hub打开项目文件夹
2. 确保Unity版本正确
3. 等待项目导入完成

### 2. 配置构建设置

#### Android配置
1. 打开 `File > Build Settings`
2. 选择 `Android` 平台
3. 点击 `Switch Platform`
4. 在 `Player Settings` 中配置：
   - **Company Name**: YourCompany
   - **Product Name**: 3D Boss Battle
   - **Package Name**: com.yourcompany.bossbattle3d
   - **Version**: 1.0
   - **Bundle Version Code**: 1
   - **Minimum API Level**: Android 5.1 (API level 22)
   - **Target API Level**: Automatic (highest installed)
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64 (推荐)

#### iOS配置
1. 打开 `File > Build Settings`
2. 选择 `iOS` 平台
3. 点击 `Switch Platform`
4. 在 `Player Settings` 中配置：
   - **Company Name**: YourCompany
   - **Product Name**: 3D Boss Battle
   - **Bundle Identifier**: com.yourcompany.bossbattle3d
   - **Version**: 1.0
   - **Build**: 1
   - **Minimum iOS Version**: 11.0
   - **Target Device**: iPhone & iPad
   - **Scripting Backend**: IL2CPP

### 3. 质量设置优化

#### 移动端优化设置
1. 打开 `Edit > Project Settings > Quality`
2. 为移动平台设置以下参数：
   - **Texture Quality**: Half Res
   - **Anisotropic Textures**: Disabled
   - **Anti Aliasing**: Disabled
   - **Soft Particles**: Disabled
   - **Shadows**: Hard Shadows Only
   - **Shadow Resolution**: Low Resolution
   - **Shadow Distance**: 20

#### 图形设置
1. 打开 `Edit > Project Settings > Graphics`
2. 确保使用Built-in Render Pipeline
3. 移除不必要的Shader Variants

## 构建流程

### Android构建

#### 方法1: 直接构建APK
1. 连接Android设备或启动模拟器
2. 在 `Build Settings` 中点击 `Build And Run`
3. 选择输出文件夹
4. 等待构建完成

#### 方法2: 构建Android App Bundle (推荐)
1. 在 `Player Settings > Publishing Settings` 中：
   - 勾选 `Build App Bundle (Google Play)`
2. 点击 `Build`
3. 生成 `.aab` 文件用于Google Play上传

### iOS构建

1. 在 `Build Settings` 中点击 `Build`
2. 选择输出文件夹
3. 等待构建完成
4. 使用Xcode打开生成的项目
5. 在Xcode中配置签名和证书
6. 构建并部署到设备

## 性能优化建议

### 1. 纹理优化
- 使用压缩纹理格式（Android: ETC2, iOS: ASTC）
- 限制纹理最大尺寸为1024x1024
- 使用Mipmap减少远距离渲染开销

### 2. 模型优化
- 保持多边形数量在合理范围内（每个模型<5000三角形）
- 使用LOD系统
- 合并相似材质

### 3. 代码优化
- 避免在Update中进行复杂计算
- 使用对象池管理频繁创建/销毁的对象
- 优化UI更新频率

### 4. 内存优化
- 及时释放不用的资源
- 使用Resources.UnloadUnusedAssets()
- 监控内存使用情况

## 测试指南

### 1. 功能测试
- [ ] 玩家移动和视角控制
- [ ] 虚拟摇杆响应
- [ ] 攻击和技能释放
- [ ] Boss AI行为
- [ ] UI界面交互
- [ ] 音效播放
- [ ] 游戏胜利/失败逻辑

### 2. 性能测试
- [ ] 帧率稳定性（目标30FPS）
- [ ] 内存使用（<500MB）
- [ ] 电池消耗
- [ ] 发热情况
- [ ] 加载时间

### 3. 兼容性测试
- [ ] 不同屏幕尺寸适配
- [ ] 不同Android版本
- [ ] 不同iOS设备
- [ ] 横竖屏切换

## 发布准备

### Android发布
1. **签名配置**
   - 创建Keystore文件
   - 在Player Settings中配置签名

2. **Google Play准备**
   - 准备应用图标（512x512）
   - 准备截图（手机和平板）
   - 编写应用描述
   - 设置年龄分级

### iOS发布
1. **App Store Connect准备**
   - 创建App ID
   - 配置证书和描述文件
   - 准备应用图标和截图
   - 编写应用描述

2. **审核准备**
   - 确保遵循App Store审核指南
   - 准备隐私政策
   - 测试所有功能

## 常见问题解决

### 构建错误
1. **IL2CPP构建失败**
   - 检查NDK版本
   - 清理并重新构建

2. **内存不足**
   - 降低纹理质量
   - 减少同时显示的对象数量

3. **性能问题**
   - 使用Unity Profiler分析
   - 检查Draw Call数量
   - 优化Shader

### 运行时问题
1. **触控不响应**
   - 检查UI Canvas设置
   - 验证EventSystem存在

2. **音效不播放**
   - 检查AudioSource配置
   - 验证音频文件格式

3. **帧率过低**
   - 启用MobileOptimizer
   - 调整质量设置

## 版本控制建议

### Git设置
```gitignore
# Unity生成的文件
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/

# 平台特定文件
*.apk
*.aab
*.ipa

# IDE文件
.vscode/
.idea/
```

### 分支策略
- `main`: 稳定版本
- `develop`: 开发版本
- `feature/*`: 功能分支
- `hotfix/*`: 紧急修复

## 联系信息

如有问题，请联系开发团队：
- 邮箱: dev@yourcompany.com
- 文档: 查看README.md获取更多信息

---

**注意**: 在发布前请务必在真实设备上进行充分测试，确保游戏体验符合预期。
