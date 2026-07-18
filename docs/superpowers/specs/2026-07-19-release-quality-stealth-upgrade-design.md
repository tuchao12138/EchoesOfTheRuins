# 《遗迹回响》成品级潜行体验重构设计

**日期：** 2026-07-19  
**状态：** 用户已批准方案 2  
**目标版本：** Unity 6.3 LTS、URP、Windows 64 位课程发布候选版

## 1. 目标与成功定义

把当前“功能存在但体验割裂”的原型升级为一段 8–12 分钟的完整第三人称潜行垂直切片。玩家必须在进入关卡后的 30 秒内理解：收集三枚能量核心、避开两名石像守卫、解锁并抵达出口。

本轮的“可上线”定义是：可公开演示、可稳定构建、可完成三次端到端测试、具备菜单/暂停/失败/通关/存档流程，并能作为课程提交的 Windows 发布候选版。它不等同于商业 3A 产品。

### 成功标准

- 角色具有可辨认的 Idle、Walk、Run、Sprint、Crouch、Jump、Throw、Hit 动作，不再侧滑。
- 守卫拥有可预测、可躲避的攻击流程，而不是接触后瞬间传送玩家。
- 目标、教学、HUD、世界标记和存档恢复始终显示同一任务状态。
- 玩家能仅凭画面和声音解释自己为何安全、被怀疑、被追击或被命中。
- Windows 构建完成“新游戏、被捕获后继续、关闭重开恢复”三条完整流程。

## 2. 范围

### 本轮包含

- 首分钟任务揭示和分阶段教学。
- 相机相对移动、加减速、平滑转向和语义动画状态。
- 守卫视野、怀疑、追击、攻击预警、挥击、命中、恢复和捕获。
- 统一目标跟踪、世界方向标记和精简 HUD。
- 攻击/受击/警戒/目标完成的音效与 VFX 反馈。
- 现有 JSON 存档、成绩、检查点和设置与新目标状态的兼容。
- EditMode、PlayMode、Windows 构建和三条手动流程验证。

### 本轮不包含

- 玩家武器、生命值战斗、格挡、处决或敌人死亡。
- 多人、联网账号、云存档、在线排行榜或微交易。
- 第二张地图、程序化生成或剧情分支。
- 付费或许可证不明确的第三方代码与资产。

## 3. 玩家体验

### 3.1 核心循环

```text
任务揭示
→ 观察守卫和视野锥
→ 使用掩体、阴影或回响石绕过守卫
→ 收集三枚核心
→ 出口解封并显示世界方向
→ 最终追击
→ 结算本局时间、警报、捕获和回响石使用量
```

### 3.2 首分钟节奏

1. **0–6 秒：任务揭示。** 输入临时锁定，镜头从封印出口扫到第一枚核心，再回到玩家。任务卡显示 `RECOVER 3 CORES / REACH THE SEALED EXIT / ESCAPE`。
2. **6–20 秒：移动。** 只显示 `WASD MOVE`、`SHIFT SPRINT`、`MOUSE LOOK`。完成移动和一次疾跑后自动收起。
3. **20–40 秒：观察。** 玩家到达安全观察点，首次看见守卫、蓝色视野锥和巡逻路线。此时才出现警戒仪。
4. **40–50 秒：阴影。** 玩家接近第一处掩体时提示 `HOLD C TO CROUCH IN SHADOW`。
5. **50–60 秒：干扰。** 只有在守卫封住路线时才提示 `Q THROW ECHO STONE`，守卫进入黄色 Investigate。

教学与任务不是两套独立状态。恢复存档时跳过已经完成的教学阶段，并直接显示与核心数量对应的当前目标。

## 4. 移动与动画设计

### 4.1 数据边界

新增纯逻辑移动模型，保留现有 `CharacterController`：

```text
PlayerInputFrame
→ PlayerLocomotionModel.Tick(...)
→ LocomotionFrame
→ PlayerController 应用位移和旋转
→ CharacterLocomotionAnimator 消费状态
```

- `PlayerInputFrame`：二维移动输入、相机水平朝向、Sprint、Crouch、Jump、Throw。
- `LocomotionFrame`：世界移动方向、当前速度、目标速度、垂直速度、身体朝向和 `LocomotionState`。
- `PlayerLocomotionModel`：负责相机相对方向、加速、刹车、平滑转向和状态选择，不读取 Unity `Input`。
- `PlayerController`：唯一负责读取输入和调用 `CharacterController.Move`。
- `CharacterLocomotionAnimator`：只负责动画参数与反馈，不修改真实位移。

### 4.2 状态与手感

固定状态为 `Idle / Walk / Run / Sprint / Crouch / Jump / Fall / Throw / Hit / Locked`。

- 相机水平旋转与角色身体旋转解耦。
- 移动方向根据相机平面的 forward/right 计算。
- 有移动输入时，身体以平滑角速度朝移动方向旋转；A/D 不再保持正面侧滑。
- 普通移动从静止加速到 Run；轻输入或低速度使用 Walk；Shift 进入 Sprint；C 进入 Crouch。
- 松开输入后使用刹车减速，禁止一帧归零。
- 动画使用 KayKit CC0 已有的 Idle、Walking、Running、Jump、Throw、Hit 语义动作；`applyRootMotion = false`。
- 脚步声由动画事件或足部接触状态触发，不按固定计时器播放。

若正式模型缺少任一必需语义动作，编辑器验证必须失败并指出缺失名称；运行时允许回退到 Idle，但不得静默使用“最长动画片段”替代全部状态。

## 5. 守卫、视野与攻击设计

### 5.1 状态机

```text
Patrol
↔ Investigate
→ Search
→ Chase
→ AttackTelegraph
→ Strike
→ Recovery
→ Capture（仅命中）
→ Patrol / Chase
```

- `GuardianBrain` 保持纯逻辑，输入感知数据并输出状态、怀疑值和攻击阶段。
- `GuardianAI` 负责 NavMeshAgent、视线射线、目标距离和场景表现。
- `GuardianAttackSequence` 负责攻击时序、命中窗口和单次命中事件。
- `GameManager` 只在受击表现结束后执行检查点重置。

### 5.2 攻击规则

- 攻击启动距离：1.7 米。
- 预警时间：0.6 秒；守卫停下、转向玩家、武器亮红并播放短促警告音。
- Strike 命中窗口：0.15 秒；只进行一次短距离球形检测和墙体遮挡检查。
- Recovery：0.6 秒；攻击落空后继续 Chase，命中后进入 Capture。
- 玩家离开范围或躲到墙后，攻击必须落空。
- 已进入 Chase 后，近战攻击不再要求玩家仍处于完整视野锥，只要求距离和近战射线无遮挡。
- 命中后玩家进入 Hit/Locked，播放踉跄、屏幕暗红边缘和短暂淡出；1.0 秒后回到检查点。
- 开场保护期间守卫不积累怀疑、不进入攻击；HUD 显示短暂 `SAFE ENTRY`，避免玩家误判 AI 失效。

### 5.3 视野反馈

- Patrol：蓝色视野锥。
- Investigate/Search：琥珀色视野锥。
- Chase/Attack：红色视野锥和方向警告。
- 可见视野锥略长于实际检测距离，给玩家留出容错。
- 视野锥必须被墙体裁切；守卫不能隔墙发现或攻击玩家。

## 6. 目标、HUD 与世界引导

### 6.1 目标模型

`ObjectiveTracker` 成为任务状态的唯一来源。固定阶段：

```text
Briefing → Move → Observe → Hide → Distract → CollectCores → ReachExit → Complete
```

`ObjectiveData` 包含：阶段、标题、简短说明、进度、目标世界位置、是否显示距离、是否锁定输入。核心、出口、教学、保存和 HUD 只能通过目标事件协作，不直接互相修改文本。

### 6.2 HUD 层级

- **左上：** 单个当前任务、核心 `n/3`；不再显示重复游戏标题。
- **上中：** 两名守卫中威胁最高者的怀疑条、状态文字和方向。
- **准星附近：** `E` 交互提示和目标距离。
- **右下：** 三个真实回响石图标；不使用乱码字符模拟图标。
- **短时教学：** 屏幕中下方单行显示，完成后淡出，不与任务卡长期并存。
- **被捕获：** 攻击命中后才显示 `STRUCK / RETURNING TO CHECKPOINT`。

HUD 使用现有蓝灰、青色和暖金配色，但减少深色矩形面积。状态不能只靠颜色表达，必须同时提供文字、图标、音效和动画变化。

### 6.3 世界引导

- 第一核心和当前出口拥有短时脉冲标记与距离。
- 每枚核心使用区域名：`COURTYARD CORE`、`SHADOW CHAMBER CORE`、`ALTAR CORE`。
- 收集核心后标记自动切换到下一目标；3/3 时出口产生青金色光束、解锁音效和 3 秒方向标记。
- 世界标记被相机外时显示屏幕边缘方向箭头；进入 8 米范围后逐渐淡出。

## 7. 事件和数据流

公开事件保持单向：

```text
PlayerLocomotionChanged(LocomotionFrame frame)
PlayerStateChanged(bool crouching, bool inShadow, float visibility)
GuardianAlertChanged(string guardianId, GuardianState state, float suspicion)
GuardianAttackChanged(string guardianId, GuardianAttackPhase phase)
PlayerStruck(string guardianId)
ObjectiveChanged(ObjectiveData objective)
EchoStoneUsed(Vector3 impactPosition)
```

`GuardianAttackPhase` 固定为 `None / Telegraph / Strike / Recovery / HitConfirmed`，用于驱动武器光、音效、受击和镜头反馈；`GuardianState` 仍负责 AI 决策，两者由 `GuardianAttackSequence` 在单一位置映射，禁止 HUD 自行推断攻击阶段。

`ThreatCoordinator` 汇总两名守卫，只向 HUD 提供最高威胁守卫；`CanvasHud` 不再直接轮询单个 `GuardianAI`。UI 仅显示事件数据，不负责游戏状态转换。

现有 `SaveService` 继续保存核心、检查点、设置和成绩；存档结构升级为 v4，并新增当前教学/目标阶段。旧存档缺失新字段时使用确定规则迁移：起始检查点且 0 枚核心为 `Briefing`；其他 0–2 枚为 `CollectCores`；3 枚为 `ReachExit`；已完成记录为 `Complete`。

## 8. 视觉与声音反馈

- 玩家轮廓始终比背景亮一个层级，护符保持青色识别点。
- 守卫武器和视野锥承担威胁信息，不额外增加大面积屏幕面板。
- 攻击预警使用武器红光、地面短弧、守卫动作和音效四重反馈。
- 核心、出口和回响石使用不同音色，玩家闭眼也能区分事件类型。
- 主菜单移除 `COURSEWORK BUILD` 的主视觉强调，保留小号版本信息；背景加入缓慢月光、守卫剪影或遗迹焦点运动。

## 9. 错误处理与可靠性

- 正式场景中任一守卫不在 NavMesh 上时，PlayMode 测试失败；运行时守卫停用并记录明确错误，禁止直线穿墙回退。
- 缺少必需动画、材质、相机、EventSystem、核心或出口引用时，构建前验证失败。
- 目标世界对象被销毁或不可用时，HUD 继续显示文字目标并隐藏距离，不产生空引用。
- 攻击事件必须具有单次命中保护，避免一段动画多次重置玩家。
- 暂停、结算、任务揭示和受击期间通过统一输入锁阻止玩家移动，不单独修改多个脚本开关。
- 损坏存档继续使用已有备份和默认回退；目标阶段按核心数量重建。

## 10. 验证计划

### EditMode

- 相机旋转 90°时，W 产生正确世界方向。
- A/D 输入让身体朝运动方向转身。
- 加速和刹车连续且不超过目标速度。
- Walk、Run、Sprint、Crouch、Jump、Hit 状态转换正确。
- 正式角色具备所有必需语义动画。
- Chase 进入攻击范围后先进入 AttackTelegraph，不立即 Capture。
- 预警结束前不能命中；离开范围或隔墙时攻击落空；单次攻击最多命中一次。
- 目标阶段、核心进度、旧存档恢复和 3/3 出口目标一致。
- ThreatCoordinator 始终选择威胁最高的守卫。

### PlayMode

- 主菜单按钮、焦点和模态窗口可用。
- 首分钟揭示镜头结束后正确恢复输入。
- 玩家移动不侧滑，动画和实际速度对应。
- 两名守卫均 `NavigationReady == true`，视野受墙体遮挡。
- 回响石触发 Investigate；怀疑条、视野锥和音效同步。
- 守卫攻击预警、挥空、命中、受击淡出和检查点恢复完整运行。
- 收集 3 枚核心后世界标记切换到出口，通关进入结算。

### Windows 发布候选验证

1. 新游戏无警报通关。
2. 被攻击并返回检查点后继续通关。
3. 收集部分核心后关闭游戏，重开 Continue 并通关。

每次记录构建版本、测试时间、结果、异常、截图和录像路径。发布候选必须无编译错误、粉色 Shader、黑屏、缺失相机、无效 NavMesh 和存档异常。

## 11. GitHub 参考与许可证边界

- Unity `Standard-Assets-Characters`：参考 Input → Brain → Motor 分层、相机相对移动和动画参数；Unity Companion License，只能用于 Unity 项目。
- Unity `open-project-1`：参考事件驱动输入、目标和 UI；Apache-2.0。
- `HenrySpartGlobal/Unity_Stealth_Game`：只参考可见视野锥、连续警觉和视觉容错；未发现根许可证，不复制代码或资产。
- `kkevn/cs426_ShadowNinjas`：只参考目标揭示镜头、攻击动作同步和课程项目迭代证据；未发现根许可证，且属于课程项目，不复制代码、关卡或美术。
- `GMostyn-Parry/Third-Person-Stealth`：只参考状态/动作/条件分离和相机碰撞概念；未发现根许可证，不复制代码。

最终仓库只提交本项目代码、项目设置、实际使用的轻量 CC0/Unity 许可资产、许可证记录、测试结果和课程证据。

## 12. 交付顺序

1. 移动模型与语义动画。
2. 守卫攻击和受击流程。
3. 统一目标、首分钟和世界引导。
4. HUD、警戒聚合、声音和 VFX。
5. 存档迁移、完整测试、Windows 构建和课程证据。

每个阶段都必须先写失败测试、实现最小行为、运行相关测试，再运行完整测试；不以截图或“场景中存在组件”代替行为验证。
