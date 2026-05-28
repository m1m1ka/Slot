# Unity 2D 肉鸽刮彩票项目 MVC架构开发指导文档

本项目是一款**肉鸽刮彩票游戏（Roguelike Scratch Card，核心玩法围绕购买和刮开彩票、构筑流派Build组合、获取增益与数值膨胀）**。为了确保未来增量数值、自动化计算及肉鸽Relic系统等能够无缝扩展且不产生毁灭性耦合，本项目制定了以下严格的开发指南。

## 1. 架构总览

在严格的MVC模式中，系统被分为三个核心部分。各部分的依赖关系是单向或通过事件/接口解耦的。

*   **Model (数据层)**：纯数据与核心业务状态。**不依赖**任何Unity引擎表现层代码（如`GameObject`、`Transform`、`SpriteRenderer`等）。
*   **View (视图层)**：负责渲染表现和收集用户输入。依赖Unity引擎API。它可以监听Model的数据变化，但**绝对不能**直接修改Model的数据。
*   **Controller (逻辑层)**：作为桥梁，负责处理游戏流程逻辑。接收来自View的输入反馈，执行逻辑计算，并更新Model的数据。

### 依赖规则（黄金法则）
*   **Model**：不依赖 View 和 Controller。只暴露事件（`Action` / `delegate`）供外部监听。
*   **View**：可以通过接口或事件监听 Model（只读），将用户输入上报给 Controller。
*   **Controller**：依赖 Model 和 View。负责初始化它们，并将它们绑定在一起。

---

## 2. 目录结构规范

为了在物理层面强制分离，建议在 `Assets/Scripts/` 下建立以下目录结构：

```text
Assets/Scripts/
├── Models/        # 纯C#类或ScriptableObject，定义数据结构和状态事件
├── Views/         # 继承自MonoBehaviour，挂载在预制体和UI元素上，处理渲染和动画
├── Controllers/   # 游戏逻辑脚本，管理生命周期，协调Model和View
├── Core/          # 核心系统（事件总线 EventBus、对象池 ClassPool/PoolManager、配置管理 ConfigManager、时间管理 TimeManager 等）
├── UI/            # UI框架核心（UIManager、UIPanel 等）
├── Configs/       # 配置表接口及数据结构定义（IConfig 等）
└── ScriptableObjects/ # 静态配置文件
```

---

## 3. 模块开发规范

### 3.1 Model (数据层) 开发规范
*   **实现方式**：优先使用**纯C#类**。如果需要配置静态数据，可使用 `ScriptableObject`。
*   **职责**：存储属性（如血量、分数、状态），并在属性改变时触发事件。
*   **禁忌**：绝对不要继承 `MonoBehaviour`。绝对不要在Model中写 `Update()` 循环。绝对不要引用任何 View 组件或UI系统。

### 3.2 View (视图层) 开发规范
*   **实现方式**：**必须继承** `MonoBehaviour`，直接挂载到Unity的 GameObject 上。
*   **资源挂载与对象池**：
    1. **主面板/大窗口（UI 显隐统一规范）**：核心视图（继承自`UIPanel`的 UI 界面等）**必须且只能**通过 `UIManager` 的方法（如 `UI.UIManager.Instance.ShowPanel<T>()`、`HidePanel<T>()` 等）进行统一的加载、弹窗与显隐管理。**严禁**开发者在其他业务逻辑代码中私自手动 `Instantiate` 加载 UI 预制体或自行 `SetActive(true/false)` 修改其显隐状态，以确保 UI 栈层级管理与动画的一致性，防止野生 Canvas 对象的生成。
    2. **资源加载统一入口**：凡是通过 `Resources` 加载的预制体、音频、配置文本等资源，**必须统一通过 `AssetProvider` 访问**，禁止在业务脚本中直接散写 `Resources.Load<T>()`。当前 `AssetProvider` 是项目统一资源入口，未来如迁移到 Addressables，只允许改这一层。
    3. **局部子节点**：所有的UI面板动态生成的子列表条目（例如右侧遗物升级选项、彩票列表等），其对应的预制体必须一律存放于 `Resources/UI/` 目录下。在运行时如果需要向视图生成任何节点，**必须通过对象池系统 (`PoolManager`)** 进行循环提取与软回收，绝不许强用 `Instantiate` 和 `Destroy`。
*   **职责**：
    1.  提供方法供外部（或监听到事件后）调用，如 `PlayHurtAnimation()`，`UpdateHealthBar(float value)`。
    2.  监听Unity的物理碰撞、UI点击等事件，并通过事件或直接调用Controller的方法将“意图”传递出去，**不自己处理逻辑**。
*   **禁忌**：绝对不要在 View 中包含游戏逻辑判断（例如：`if (health < 0) Die();` 逻辑应该在Controller里）。

### 3.3 Controller (逻辑层) 开发规范
*   **实现方式**：**必须继承** `MonoBehaviour`，并且**强制与受其控制的 View 挂载在同一个 GameObject（预制体）上**，以便在 Unity Editor 中直观观察挂载与状态。不再由 Controller 去动态实例化独立表现层。
*   **职责**：
    1.  实例化或接收外部分配传入的 Model 对象。
    2.  在 `Awake()` 或 `Start()` 阶段获取同级挂载的 View 组件（例如 `_view = GetComponent<MyView>();`）。
    3.  订阅 View 的输入事件。
    4.  处理核心游戏逻辑，并调用 Model 的方法修改数据。
*   **禁忌**：虽然 Controller 和 View 现在挂载在同一物理物体上，但**绝对不要**在 Controller 中直接获取/操作 UI 或渲染元素（如直接通过 `GetComponent<Text>().text` 赋值）。必须严格调用 `_view.UpdateText("123")` 保持逻辑与呈现的隔离。

---

## 4. 肉鸽刮彩票游戏 架构实体设计

为了支持随机生成的“刮刮乐彩票（Scratch Card）”组合（例如：1x3小彩票、2x3大彩票、特殊刮区形状等）以及高自由度的**遗物构筑（Relic Build）、数值膨胀乘区**，我们在底层代码设计上强制采用**数据驱动**与**生命周期解耦**机制。

### 4.1. 刮刮乐核心 MVC 划分
*   **Model (`ScratchCardModel.cs`)**：
    纯 C# 类，只负责存储单张刮刮卡的运行时实例状态，例如 `CardId`、`CardTypeId`、`GridWidth`、`GridHeight`、`ScratchTools`、`Cells`、`ScratchProgress`、`State` 等。格子内容由 `ScratchCellModel` 表达。Model 只广播状态变化事件，例如 `OnScratchProgressChanged`、`OnStateChanged`、`OnScratchCompleted`。
*   **View (`ScratchCardView.cs`)**：
    挂载在 UI 预制体上的 `MonoBehaviour`。负责图案显示、涂层 `RawImage` 擦除、DOTween 动画、聚焦表现和用户输入采集。View 不决定图案如何生成，也不决定奖励如何结算，只把“点击”“刮除进度变化”“入场结束”等意图抛给对应的 Controller。
*   **Controller (`ScratchCardController.cs`)**：
    **必须继承** `MonoBehaviour` 并且**与对应 View 挂载在同一个预制体对象上**。
    负责绑定 `ScratchCardModel` 与 `ScratchCardView`，驱动 `Falling / Idle / Focused / Scratching / Completed` 状态流转，并在 `OnScratchCompleted` 后调用结算策略。后续奖励入账、遗物乘区和全局反馈应通过事件或 Service 继续解耦，不应继续堆进单张卡 Controller。

### 4.2. 动态结算机制与策略模式 (Strategy Pattern)
由于各种肉鸽彩票判赢和多重翻倍规则差异巨大，严禁在 Controller 内部堆砌 `if-else`。  
当前项目统一使用 `IScratchSettlementEvaluator` 作为结算策略接口，并由玩家拥有的 `ScratchToolConfig` 决定要启用哪些结算规则。`ScratchToolSettlementService` 会遍历 `ScratchCardModel.ScratchTools`，再通过 `ScratchSettlementEvaluatorFactory` 根据 `ScratchSettlementType` 选择具体实现。针对新的刮具规则，应新增独立 Evaluator，例如 `FirstRevealedPatternSettlementEvaluator`、`MatchAnyPairSettlementEvaluator`、`MatchAnyThreeSettlementEvaluator`。禁止在 `ScratchCardController` 中按卡种写大量判断。

### 4.3. 全局遗物乘区与 EventBus（流派化）
1. **全局乘区解耦**：
   * 所有与肉鸽“遗物（Relic）”相关的计算（例如：刮开一个特定符号，全局掉率提升 5%），不应该写在 ScratchCard 层。
   * 应该通过 `EventBus` 或后续事件系统广播核心行为，例如 `ScratchCardCompletedEvent`。事件中携带 `ScratchSettlementResult`、卡种 id、格子结果等基础数据。然后由奖励服务、遗物服务、货币服务监听并计算最终入账结果。
2. **UI 统一交互**：
   * 在需要购买彩票或抽取肉鸽遗物时，**强制使用** `UIManager.Instance.ShowPanel<ShopPanel>()` 或类似统一出口弹出选牌界面，保证弹窗生命周期干净且可追溯。

---

## 5. 后续开发建议

1.  **引入事件总线（Event Bus）**：对于跨模块的系统（例如：成就系统需要知道玩家杀死了怪物），直接的Delegate耦合可能不够，建议在 `Core` 目录下实现全局事件驱动。
2.  **避免神级Controller**：采用职责单一原则，分为 `PlayerController`, `EnemyController`, `GamePlayManager`, `LevelController` 等。
3.  **UI隔离**：将主操作UI与对应的玩家GameObject分离，通过MVC逻辑层解耦。

遵循以上规范，即可极大程度保障你在这个2D/3D挂机增量项目中各个系统的独立性与极高的性能可扩展性。

---

## 6. 当前项目架构评估（基于现有代码）

### 6.1 当前优点
1. **分层意识明确**：`Models / Views / Controllers / Core / UI / Configs` 的目录已经具备清晰的职责划分，优于大量常见 Unity 小项目的脚本堆叠式写法。
2. **基础框架雏形已存在**：当前工程已经具备 `UIManager`、`UIPanel`、`PoolManager`、`ClassPool`、`EventBus`、`ConfigManager`、`TimeManager` 等底层框架原型，说明项目已经进入“框架化开发”的方向，而不是纯业务脚本拼接。
3. **MVC 边界基本正确**：`PlayerModel` 作为纯 C# 数据对象、`ShopItemView` 只负责表现和点击意图上报、`MainGameController` 负责监听和调度，这个方向是正确的。
4. **有工程规范文档意识**：这份文档本身就是项目可持续演进的重要基础，后续 AI 和人工协作都应以文档为统一约束来源。

### 6.2 当前核心问题
1. **运行时装配链条已建立但仍属轻量骨架**：当前项目已经接入 `GameBootstrapper + AppRoot`，并可统一初始化 `UIManager / PoolManager / TimeManager / PlayerContext / GameSession`，但更多业务模块尚未完全接入这条运行时链路。
2. **核心状态所有权已开始收敛，但还需全面统一**：当前 `MainGameController` 已改为从 `AppRoot.PlayerContext` 获取 `PlayerModel`，但后续所有新增 Controller、Service、UseCase 仍需统一遵守“从运行时上下文取状态、禁止自行 new 全局状态”的原则。
3. **业务闭环未打通**：现阶段购买逻辑还停留在示例级别，点击购买后没有完整形成“读取配置 -> 校验价格 -> 扣费 -> 生成内容 -> 派发事件 -> 刷新表现”的完整流程。
4. **配置驱动尚未落地**：`ConfigManager` 当前为占位实现，配置解析逻辑并未真正完成，因此项目还没有进入真正的数据驱动阶段。
5. **UI 框架仍偏简化**：当前 `UIManager` 已具备面板缓存和栈管理能力，但还缺少更复杂项目所需的层级、遮罩、输入阻断、异步加载、重复打开防重入等能力。
6. **Controller 有膨胀风险**：`MainGameController` 已经开始同时承担视图初始化、列表生成、按钮监听、资源回收、业务验证等职责，若继续扩展，很容易演化为 God Controller。

### 6.3 当前总体结论
当前项目应定义为：

**“架构方向正确、基础件齐全、但仍处于框架雏形阶段，尚未完全进入可稳定扩展的工程化状态。”**

后续开发不应继续仅增加功能脚本，而应优先补齐运行时装配、全局状态托管、配置驱动、用例层和底层基础设施。

---

## 7. 最需要优先改进的点
以下事项是后续迭代的最高优先级，优先级高于新增玩法表现。

### 7.1 第一优先级：建立统一启动与装配入口
必须引入 `GameBootstrapper`、`AppRoot` 或类似概念，作为全项目唯一可信的初始化入口。

职责至少包括：
1. 初始化全局管理器与全局服务。
2. 初始化运行时数据上下文（如 `PlayerContext`、`GameSession`）。
3. 初始化配置系统。
4. 初始化 UI 根节点、对象池根节点、时间系统等。
5. 统一决定首个场景或首个面板的打开顺序。

禁止继续让各个业务 Controller 自行决定和创建关键全局依赖。

当前项目已采用以下轻量方案：

1. `GameManager`：负责应用级环境初始化。
2. `GameBootstrapper`：负责项目运行时装配。
3. `AppRoot`：负责持有运行时核心对象与上下文。

#### 7.1.1 角色职责边界
必须严格区分以下三类角色，禁止职责混淆：

1. `GameManager`
   只负责应用级、环境级初始化，例如：
   - `Application.targetFrameRate`
   - `Screen.sleepTimeout`
   - 启动入口触发
2. `GameBootstrapper`
   只负责项目运行时装配，例如：
   - 确保 `AppRoot` 存在
   - 确保 `UIManager / PoolManager / TimeManager` 存在
   - 初始化 `PlayerContext / GameSession`
   - 初始化配置系统
   - 打开首屏 UI
3. `AppRoot`
   只负责持有全局运行时引用，不负责复杂业务逻辑。

禁止：
1. 在 `GameManager` 中直接编写业务初始化细节。
2. 在 `Controller` 中重复创建全局 Manager。
3. 在任意业务脚本中绕过 `GameBootstrapper` 直接初始化核心上下文。

#### 7.1.2 当前标准启动顺序


任何新增系统如果依赖：
1. 运行时上下文
2. UI 管理器
3. 配置系统
4. 对象池系统

都必须默认认为它们在 `Bootstrap()` 之后才可安全访问。

#### 7.1.3 当前 AppRoot 持有规则
当前 `AppRoot` 是统一运行时根节点，后续必须继续承担全局持有职责。

当前已持有：
1. `UIManager`
2. `PoolManager`
3. `TimeManager`
4. `PlayerContext`
5. `GameSession`

后续可继续持有：
1. `AudioManager`
2. `SaveService`
3. `ServiceRegistry`
4. `RuntimeDataStore`

禁止：
1. 让某个 UI Controller 持有全局长期状态。
2. 让某个玩法脚本私有保存 `PlayerContext` 的替代副本。
3. 在多个地方重复 new 同类运行时上下文。

#### 7.1.4 上下文创建与持有约束
后续统一约束如下：

1. `PlayerContext` 由 `GameBootstrapper` 创建，由 `AppRoot` 持有。
2. `GameSession` 由 `GameBootstrapper` 创建，由 `AppRoot` 持有。
3. `Controller` 只允许读取和使用运行时上下文，不拥有其生命周期。
4. 全局状态 Model 不允许由界面 Controller 自行 `new`。

推荐访问方式：
1. `AppRoot.Instance.PlayerContext`
2. `AppRoot.Instance.GameSession`

#### 7.1.5 测试场景与子场景启动约束
后续所有单独运行的测试场景、功能场景、验证场景，都必须满足以下条件之一：

1. 场景中包含 `GameManager`
2. 场景中包含可直接执行 `Bootstrap()` 的 `GameBootstrapper`

否则视为非法启动场景。

原因：
1. `AppRoot` 可能不存在
2. `PlayerContext` 可能未初始化
3. `UIManager / PoolManager / TimeManager` 可能缺失
4. 业务 Controller 会出现空引用或错误状态

#### 7.1.6 Controller 的强制约束
从当前版本开始，所有 Controller 必须遵守以下规则：

1. 禁止自行 new 全局状态型 Model。
2. 禁止自行决定核心 Manager 的初始化顺序。
3. 访问玩家长期状态时，优先从 `AppRoot.PlayerContext` 获取。
4. 访问局内流程状态时，优先从 `AppRoot.GameSession` 获取。

例如：
1. `MainGameController` 应从 `AppRoot.Instance.PlayerContext.Player` 获取 `PlayerModel`
2. 后续 `ShopController`、`UpgradeController`、`GameFlowController` 也必须遵守相同规则

### 7.2 第二优先级：明确运行时数据归属
必须将以下数据从界面 Controller 中剥离出去，纳入统一的运行时上下文：
1. 玩家货币
2. 玩家遗物
3. 玩家升级状态
4. 当前局内构筑
5. 当前关卡或流程状态
6. 局外成长数据

推荐结构：
```text
AppRoot
├── GameServices
├── PlayerContext
├── GameSession
└── UIRoot
```

其中：
1. `PlayerContext` 负责玩家长期状态与账号级数据。
2. `GameSession` 负责单局战斗或单轮玩法中的临时状态。
3. `MainGameController` 只读取和调度这些状态，不拥有这些状态。
4. `PlayerContext`、`GameSession` 推荐使用纯 C# 类实现，不继承 `MonoBehaviour`。
5. `PlayerContext` 允许聚合多个玩家域 Model。
6. `GameSession` 允许聚合多个单局流程 Model。

### 7.3 第三优先级：从样例逻辑升级为配置驱动
当前商店价格、商品数量、命名等内容不能继续写死在 Controller 中。

后续要求：
1. 商店条目来源于配置表。
2. 彩票稀有度、价格、掉落池、奖励规则来源于配置表。
3. 升级项、遗物、Buff、关卡参数来源于配置表。
4. 所有配置在启动时校验主键、字段完整性、重复 ID、非法值。

任何新增玩法功能，如果不能先定义配置结构，则默认视为设计不完整。

### 7.4 第四优先级：引入 UseCase / Service 层
严禁把复杂逻辑全部堆在 `MonoBehaviour Controller` 中。

后续建议将业务逻辑拆为：
1. `BuyScratchCardUseCase`
2. `ScratchRewardSettlementService`
3. `RelicEffectService`
4. `UpgradeApplyService`
5. `CurrencyService`

原则：
1. Controller 只负责接收输入、调用用例、驱动 View。
2. UseCase / Service 负责业务规则。
3. Model 负责状态存储和事件广播。

### 7.5 第五优先级：防止 God Controller
如果某个 Controller 同时负责以下三项以上职责，就必须拆分：
1. 管理多个列表生成
2. 管理多个数据模型
3. 管理复杂购买和升级逻辑
4. 管理场景流程
5. 管理结算与奖励广播
6. 管理多个弹窗状态

拆分后建议：
1. `MainGameController` 只负责主界面总调度。
2. `ShopController` 负责购买列表。
3. `ScratchCardController` 负责单张彩票。
4. `UpgradeController` 负责升级列表。
5. `GameFlowController` 负责流程状态迁移。

---

## 8. 当前项目仍缺少的核心底层框架系统

以下系统被视为“核心底层系统”，缺少它们会显著影响项目后期扩展性、稳定性与 AI 自动迭代质量。

### 8.1 启动与依赖装配系统
必须有统一的启动入口，而不是依赖多个 MonoBehaviour 在场景中各自 `Awake/Start` 触发。

建议补充：
1. `GameBootstrapper`
2. `ServiceRegistry` 或轻量级 Service Locator
3. 可选的依赖注入封装层

### 8.2 运行时数据上下文系统
需要一个显式的全局数据容器，而不是把状态散落在各个 Controller 中。

建议补充：
1. `PlayerContext`
2. `GameSession`
3. `RuntimeDataStore`

### 8.3 存档系统
这是增量、挂机、肉鸽类项目的核心底层能力。

必须包含：
1. 存档读写
2. 自动保存
3. 手动保存
4. 配置版本迁移
5. 存档版本兼容
6. 局外成长与局内数据分离存储
7. 离线收益结算

### 8.4 完整可用的配置系统
当前 `ConfigManager` 尚未真正可用，必须补齐。

至少应支持：
1. JSON / CSV / ScriptableObject 的一种主配置方案
2. 主键索引
3. 全表读取
4. 重复 ID 检测
5. 字段合法性校验
6. 启动时统一加载
7. 失败时阻断进入正式流程

### 8.5 流程状态机系统
项目后期不能只靠“打开哪个面板”来表示游戏状态。

建议建立：
1. `GameStateMachine`
2. `GameState`
3. `Enter / Exit / Update` 生命周期

典型状态包括：
1. Boot
2. MainMenu
3. InGame
4. RewardSettlement
5. Pause
6. Shop
7. GameOver

### 8.6 资源加载系统
当前可以继续小规模使用 `Resources`，但必须明确这是过渡方案。

长期建议：
1. 封装 `AssetProvider`
2. 中后期迁移到 Addressables
3. 为 UI、特效、卡牌、音频、配置建立统一加载接口

当前项目已采用轻量统一入口：
1. `AssetProvider.Load<T>(path)`
2. `AssetProvider.LoadPrefab(path)`
3. `AssetProvider.LoadAudioClip(path)`
4. `AssetProvider.LoadTextAsset(path)`
5. `AssetProvider.InstantiatePrefab(...)`

当前强制约束：
1. 禁止在业务脚本中直接调用 `Resources.Load<T>()`
2. `UIManager` 加载面板必须走 `AssetProvider`
3. `MainGameController` 加载商店预制体必须走 `AssetProvider`
4. `AudioManager` 加载音频必须走 `AssetProvider`
5. `ConfigManager` 加载配表必须走 `AssetProvider`

后续只有 `AssetProvider` 可以直接接触 `Resources` API。

### 8.7 域事件系统
`EventBus` 只是通信载体，不等于完整事件体系。

必须逐步补充：
1. 明确的事件定义目录
2. 事件命名规范
3. 事件发布者与订阅者边界
4. 场景切换或重开时的清理规则

建议示例：
1. `CurrencyChangedEvent`
2. `ScratchCardPurchasedEvent`
3. `ScratchCardCompletedEvent`
4. `RelicAddedEvent`
5. `GameStateChangedEvent`

### 8.8 音频系统
肉鸽、刮奖、升级反馈都高度依赖音频强化手感。

建议至少具备：
1. BGM 管理
2. SFX 播放
3. 音量分组
4. 静音设置
5. 场景切换音频策略

当前项目已接入轻量 `AudioManager`，并默认由 `GameBootstrapper` 初始化、由 `AppRoot` 持有。

当前能力包括：
1. 播放背景音乐 `PlayMusic`
2. 停止背景音乐 `StopMusic`
3. 背景音乐淡入与淡出
4. 播放音效 `PlaySfx`
5. 多个音效同时播放
6. 音乐音量 / 音效音量控制
7. 音乐静音 / 音效静音
8. 支持直接传入 `AudioClip`
9. 支持通过资源路径播放音频
10. 支持通过语义化 `AudioCueId` 播放音频：`AudioManager.Instance.PlayCue(AudioCueId.Xxx)`
11. 支持在 `AudioManager` 内维护统一 Cue Library，包括音频类型、Resources 路径、音量倍率、音高、冷却时间、BGM 淡入淡出与循环设置

当前约束：
1. 所有音频资源加载必须通过 `AudioManager` -> `AssetProvider`
2. 禁止在业务层直接 `Resources.Load<AudioClip>()`
3. 后续如果接入音频配置表，也必须从 `AudioManager` 统一播放，不得绕过系统直接操作临时 `AudioSource`
4. 业务代码优先调用 `PlayCue(AudioCueId)`，不得在 Controller / View 中散写音频 Resources 路径；`PlayMusic(string)` 与 `PlaySfx(string)` 只作为底层兼容入口或临时调试入口使用
5. 音频文件不由单个业务脚本持有，统一由 `AudioManager` 的 Cue Library 维护；默认资源路径约定为：
   - 音效：`Resources/Audio/Sfx/`
   - 音乐：`Resources/Audio/Music/`
6. 新增音效时必须先新增或复用 `AudioCueId`，再在 `AudioManager` 的 Cue Library 中登记路径与播放参数，最后才允许在业务流程中触发
7. 高频音效必须设置冷却时间，例如刮奖、连点、循环反馈等，禁止在 `Update()` 或拖拽回调中无节制播放
8. `Model` 永远不允许调用音频系统；`Controller` 负责在业务状态变化、成功/失败结果、奖励结算等语义点触发音频；`View` 只允许在纯表现动画自身完成且不涉及业务判断时触发表现音频
9. 如果某个音效代表跨系统事件（例如结算、升级、获得遗物），优先由 Controller / UseCase / Service 在事件发生点播放，或后续通过事件监听服务统一播放，不要让多个界面重复播放同一语义音效

当前默认 Cue 约定：
1. `UiClick` -> `Resources/Audio/Sfx/UI_Click`
2. `UiDenied` -> `Resources/Audio/Sfx/UI_Denied`
3. `ScratchCardPurchased` -> `Resources/Audio/Sfx/ScratchCard_Purchased`
4. `ScratchCardSpawned` -> `Resources/Audio/Sfx/ScratchCard_Spawned`
5. `ScratchCardFocused` -> `Resources/Audio/Sfx/ScratchCard_Focused`
6. `ScratchCardScratching` -> `Resources/Audio/Sfx/ScratchCard_Scratching`
7. `ScratchCardCompleted` -> `Resources/Audio/Sfx/ScratchCard_Completed`
8. `ScratchCardRewardClaimed` -> `Resources/Audio/Sfx/ScratchCard_RewardClaimed`
9. `MainMusic` -> `Resources/Audio/Music/Main`

### 8.9 数值格式化与本地化系统
由于本项目存在大数值成长，必须尽早抽离数值显示规则。

建议补充：
1. `NumberFormatter`
2. 多语言文本入口
3. UI 文本 Key 化，而非硬编码字符串

### 8.10 调试与埋点系统
### 8.10 关卡系统
关卡系统用于约束单局目标和资源消耗边界，例如通关金币要求、购买彩票次数限制、后续可能扩展的时间限制、特殊规则、奖励池等。

当前项目已接入轻量关卡框架：
1. `LevelConfig`：定义关卡静态数据，包括 `Id`、`Name`、`RequiredCoins`、`ScratchCardPurchaseLimit`
2. `LevelProgressModel`：保存当前关卡运行时状态，包括已购买彩票次数、剩余次数、是否通关
3. `LevelDefaultsProvider`：当前阶段提供默认关卡数据，后续应替换为正式配表来源
4. `GameSession.CurrentLevel`：由运行时会话持有当前关卡进度
5. `MainGameController`：在购买入口检查次数限制，在金币变化后判断是否达到通关金币要求
6. `MainGamePanel.UpdateLevelDisplay(...)`：只负责显示关卡状态，不参与规则判断

当前约束：
1. 关卡静态规则必须进入 `LevelConfig` 或后续正式配表，不允许散写在 View 中
2. 当前关卡运行时状态必须由 `GameSession.CurrentLevel` 持有，不允许由某个 UI View 私有保存
3. `LevelProgressModel` 必须保持纯 C#，不引用 Unity UI、Audio、DOTween、GameObject
4. 购买限制必须在 Controller / UseCase 层检查，View 只能提交“购买意图”
5. 通关判断由 Controller 根据 `PlayerModel.Coins` 协调 `LevelProgressModel.EvaluatePass(...)` 完成，避免 `LevelProgressModel` 直接依赖玩家上下文
6. 后续如果购买逻辑继续复杂化，应优先抽出 `BuyScratchCardUseCase`，而不是继续扩大 `MainGameController`

### 8.11 调试与埋点系统
### 8.11 肉鸽卡牌系统
肉鸽卡牌系统负责“通关后 3 选 1 -> 选中后加入玩家卡片区 -> 后续通过效果系统影响结算或生成规则”的完整闭环。

当前项目已接入轻量肉鸽卡牌框架：
1. `RogueCardConfig`：肉鸽卡静态配置，定义 `Id`、`Name`、`Description`、`Rarity` 和 `Effects`
2. `RogueCardEffectConfig`：单个效果配置，定义 `EffectType`、`TargetId`、`Value`
3. `RogueCardEffectType`：效果类型枚举，例如 `IncreasePatternBaseScore`、`IncreaseScratchCardMultiplier`
4. `RogueCardInventoryModel`：玩家已获得肉鸽卡库存，属于纯 C# Model，由 `PlayerContext.RogueCards` 持有
5. `RogueCardRewardOfferModel`：一次 3 选 1 奖励候选集合
6. `RogueCardDefaultsProvider`：当前阶段默认肉鸽卡池，后续应替换为正式配表
7. `RogueCardRewardService`：负责从卡池生成奖励候选，当前默认生成 3 张不重复卡
8. `IRogueCardEffect`：单个效果运行时处理器接口
9. `RogueCardEffectService`：效果分发入口，拿到卡牌后按 `EffectType` 找对应处理器执行
10. `RogueCardEffectContext`：效果执行上下文，聚合 `PlayerContext`、`GameSession` 等运行时对象
11. `MainGamePanel.ShowRogueCardChoices(...)`：只负责展示 3 选 1 UI，并把点击选择上报给 Controller
12. `MainGamePanel.RefreshOwnedRogueCards(...)`：只负责刷新底部玩家已拥有卡片区
13. `MainGameController`：监听关卡通关，生成 3 选 1，接收选择结果，写入玩家库存，并调用效果服务

当前跑通流程：
1. `LevelProgressModel` 达成通关金币要求
2. `MainGameController.HandleLevelPassStateChanged(true)` 收到通关事件
3. `RogueCardRewardService.CreateRewardOffer(3)` 生成三张候选卡
4. `MainGamePanel.ShowRogueCardChoices(...)` 弹出三选一界面
5. 玩家点击其中一张，View 触发 `OnRogueRewardCardSelected(cardId)`
6. `MainGameController` 从当前候选中找到被选中的 `RogueCardConfig`
7. `RogueCardInventoryModel.AddCard(...)` 把卡加入玩家库存
8. `RogueCardEffectService.ApplyCard(...)` 调用效果系统入口
9. `MainGamePanel.RefreshOwnedRogueCards(...)` 刷新底部卡片区
10. `MainGamePanel.HideRogueCardChoices()` 关闭三选一界面

当前约束：
1. 肉鸽卡静态数据必须进入 `RogueCardConfig` 或正式配表，不允许写死在 View / Controller 中
2. 玩家已拥有肉鸽卡必须由 `PlayerContext.RogueCards` 持有，不允许某个 UI 面板私有保存
3. `RogueCardInventoryModel`、`RogueCardRewardOfferModel` 必须保持纯 C#，不引用 Unity UI、Audio、DOTween、GameObject
4. View 只能展示卡牌与上报选择，不允许决定卡牌效果、不允许修改玩家库存
5. Controller 只负责流程协调：通关后生成候选、接收选择、写入库存、调用效果服务
6. 具体效果必须通过 `IRogueCardEffect` 新增独立处理器，不允许在 `MainGameController`、`ScratchCardController` 或结算器里堆 `if cardId == ...`
7. 如果效果影响图案基础分，优先在后续的图案分数计算服务或生成/结算上下文中读取效果结果，不要直接改静态配置对象
8. 如果效果影响倍率，必须先区分“计分时倍率”和“总倍率”：计分时倍率只影响某一次图案计分，总倍率影响整张刮刮卡最终奖励；不要把两类倍率混用，也不要把倍率逻辑塞回单张卡 Controller
9. 后续 UI 美术化时，可以替换为 `Resources/UI/` 下的肉鸽卡预制体，但仍必须保持 View 只表现、Controller 只协调、Model 只存状态

### 8.12 调试与埋点系统
后续平衡性调优、AI 回归测试、问题定位都需要它。

建议补充：
1. 分级日志系统
2. 开发调试面板
3. 关键事件埋点
4. 性能统计入口

### 8.13 测试基础设施
后续所有纯 C# 业务层都应可测试。

最低要求：
1. `EditMode` 单元测试
2. 购买逻辑测试
3. 配置读取测试
4. 奖励结算测试
5. 遗物乘区计算测试

---

## 9. 推荐的后续演进顺序

为了避免“边写功能边推翻框架”，后续应严格按以下顺序推进：

1. **先补启动与全局上下文**
   先完成 `AppRoot / Bootstrapper / PlayerContext / GameSession`。
2. **再补可用配置系统**
   让所有商店条目、彩票参数、奖励参数改为配置驱动。
3. **再拆业务用例层**
   将购买、结算、升级、遗物效果从 Controller 中拆到 UseCase / Service。
4. **再扩充 UI 框架**
   补齐 UI 层级、遮罩、弹窗规则、异步加载和防重入。
5. **再补存档和流程状态机**
   将项目从“可运行 Demo”升级为“可持续迭代项目”。

如果顺序颠倒，极易出现：
1. 数据写死
2. Controller 膨胀
3. 状态到处散落
4. AI 每轮迭代都重复造轮子
5. 后期重构成本指数级增加

---

## 10. 后续 AI 迭代开发强制约束

以下规则为后续所有 AI 代码生成、修改、重构时的强制执行规范。

### 10.1 先补框架，再堆玩法
当底层系统缺失时，AI 不应直接跳过框架去添加业务功能，而应优先补齐框架缺口。

### 10.2 任何新增功能优先判断归属层级
AI 在新增代码前，必须先判断该逻辑属于：
1. Model
2. View
3. Controller
4. Core
5. UseCase / Service
6. Config

禁止把不确定归属的逻辑默认塞进 Controller。

### 10.3 新增玩法必须优先配置驱动
凡是以下内容，原则上不得写死：
1. 价格
2. 掉落概率
3. 奖励数值
4. 稀有度参数
5. UI 显示文案
6. Buff / Relic 效果参数

### 10.4 新增全局状态必须先确定归属
新增玩家状态、局内状态、战斗状态时，必须优先写入 `PlayerContext / GameSession / RuntimeData`，而不是临时挂在某个界面 Controller 上。

### 10.5 新增跨模块通信优先定义事件类型
当系统间需要解耦时，AI 应优先：
1. 定义事件结构
2. 明确发布方
3. 明确订阅方
4. 明确事件生命周期

而不是直接在多个系统间建立硬引用。

### 10.6 新增复杂业务优先抽离 UseCase
当逻辑具备以下任一特征时，应优先抽成 UseCase / Service：
1. 包含多个判断分支
2. 涉及多种状态修改
3. 涉及配置读取
4. 涉及奖励或消耗计算
5. 未来可能被多个入口复用

### 10.7 所有 AI 修改都应优先保护现有分层边界
即使临时实现更快，也不得为了赶功能而破坏：
1. Model 纯净性
2. View 的只表现原则
3. Controller 的中介职责
4. Core 的通用性

---

## 11. 推荐目标目录结构（后续演进版）

建议后续将目录逐步演进为：

```text
Assets/Scripts/
├── App/
│   ├── AppRoot.cs
│   ├── GameBootstrapper.cs
│   └── ServiceRegistry.cs
├── Core/
│   ├── EventBus/
│   ├── Pool/
│   ├── Time/
│   ├── Save/
│   ├── Config/
│   ├── Audio/
│   └── Utils/
├── Runtime/
│   ├── PlayerContext.cs
│   ├── GameSession.cs
│   └── RuntimeDataStore.cs
├── Models/
├── Views/
├── Controllers/
├── UseCases/
├── Services/
├── UI/
├── Configs/
├── Events/
└── Tests/
```

说明：
1. `App/` 负责启动和装配。
2. `Runtime/` 负责当前运行时数据。
3. `UseCases/` 负责业务流程。
4. `Services/` 负责可复用业务能力。
5. `Events/` 负责定义跨系统事件。
6. `Tests/` 负责纯逻辑层回归验证。

---

## 12. 本文档在后续开发中的使用方式

后续所有 AI 或人工开发，在新增功能前，必须先按以下顺序自检：

1. 这是新功能，还是在补框架？
2. 这个逻辑属于哪一层？
3. 这个功能是否应先定义配置？
4. 这个状态应归谁持有？
5. 这个模块是否会让某个 Controller 继续膨胀？
6. 这个改动是否破坏现有 MVC 边界？
7. 这个功能是否需要事件、存档、状态机或测试支持？

如果以上问题未明确，则不应直接进入编码阶段，应先补设计与框架约束。
---

## 13. 刮刮卡系统流程说明（创建 -> 初始化 -> 刮奖 -> 结算）

本节专门说明当前项目里“刮刮卡”这条链路的完整职责分工。后续 AI 或人工继续扩展刮刮卡玩法时，必须优先遵守这一节，而不是直接把新逻辑继续堆进 `MainGameController` 或 `ScratchCardController`。

### 13.1 总体流程图

当前一张刮刮卡的运行流程为：

1. `MainGameController` 收到购买请求
2. 根据购买入口选择 `ScratchCardTypeConfig`
3. 根据卡种配置读取：
   - 图案池 `ScratchPatternPoolConfig`
   - 可刮区域模板 `ScratchAreaTemplateConfig`
   - 玩家当前拥有的刮具集合 `PlayerContext.ScratchTools`
   - 预制体路径 `PrefabPath`
4. 通过 `ScratchCardGenerator` 生成本张卡的格子数据 `ScratchCellModel`
5. 用这些数据创建运行时实例 `ScratchCardModel`
6. 从 `AssetProvider` 加载对应刮刮卡预制体
7. 从对象池 `PoolManager` 生成刮刮卡对象
8. `ScratchCardController.Initialize(...)` 绑定 `Model + View`
9. `ScratchCardView` 初始化图案显示、涂层遮罩、入场动画、聚焦动画和刮除输入
10. 玩家划过涂层，`ScratchCardView` 擦除遮罩并把进度上报给 `ScratchCardController`
11. `ScratchCardController` 驱动 `ScratchCardModel` 更新刮开进度与状态
12. `ScratchCardModel` 达到完成条件后触发 `OnScratchCompleted`
13. `ScratchCardController` 调用 `ScratchToolSettlementService`，由玩家拥有的刮具集合触发对应结算策略 `IScratchSettlementEvaluator`
14. 输出结算结果 `ScratchSettlementResult`

### 13.2 各层职责划分

#### 13.2.1 配置层：定义“这是什么卡”

这一层只描述规则，不参与运行时交互。

1. `ScratchCardTypeConfig`
   作用：
   - 定义卡种 id、名称、价格、图案池、区域模板、预制体路径
   - 表示“玩家购买的是哪一种卡”

2. `ScratchPatternConfig`
   作用：
   - 定义图案 id、名称、基础分、图集路径、切片名
   - 表示“世界里有哪些基础图案”

3. `ScratchPatternPoolConfig`
   作用：
   - 定义某种卡可出现哪些图案，不定义卡种自己的独立权重
   - 表示“这张卡会从什么池子里抽图案”

补充底层规则：
- 所有刮刮卡必须共用同一套全局图案权重表。
- `ScratchPatternPoolConfig` 只表示该卡种允许出现哪些图案，不再为不同卡种维护独立概率。
- 实际抽取时，先取当前卡种允许图案集合，再读取这些图案的全局权重并在集合内部重新归一化。
- UI 展示概率、实际随机生成、后续数据表校验必须使用同一套归一化规则，禁止在 View 或单张卡 Controller 中另写概率。

4. `ScratchAreaTemplateConfig`
   作用：
   - 定义宽高、可刮格子索引
   - 表示“这张卡的布局是什么，哪些格子真的可刮”

5. `ScratchToolConfig`
   作用：
   - 定义刮具 id、名称、描述与 `ScratchSettlementType`
   - 表示“玩家当前构筑里有哪些结算规则”

6. `ScratchSettlementType`
   作用：
   - 定义结算规则类型
   - 表示“某个刮具按什么规则算分”

当前这些配置的默认来源是：

1. `ScratchCardDefaultsProvider`
2. `ScratchPatternDefaultProvider`

后续正式接入配表时，应优先替换这两个 Provider 的数据来源，而不是改动生成器、View 或 Controller 主流程。

#### 13.2.2 创建层：负责“生成这张卡”

这一层负责把静态配置转成一张可玩的运行时实例。

1. `MainGameController`
   当前作用：
   - 接收购买入口
   - 根据 `sourceSlotId` 选择卡种
   - 读取卡种配置、区域模板、预制体
   - 调用生成器创建格子数据
   - 创建 `ScratchCardModel`
   - 从对象池生成预制体并初始化 `ScratchCardController`

2. `ScratchCardGenerator`
   作用：
   - 根据 `ScratchCardTypeConfig + ScratchAreaTemplateConfig`
   - 从图案池按权重随机抽图案
   - 生成每个格子的 `ScratchCellModel`

约束：

1. `ScratchCardModel` 不允许自己读配置
2. `ScratchCardView` 不允许自己生成业务数据
3. 图案抽取和格子生成必须集中在生成层完成

#### 13.2.3 运行时数据层：负责“保存这张卡的状态”

1. `ScratchCardModel`
   作用：
   - 保存一张卡的运行时实例信息
   - 包括：
     - `CardId`
     - `SourceSlotId`
     - `CardTypeId`
     - `CardTypeName`
     - `GridWidth`
     - `GridHeight`
     - `AreaTemplateId`
     - `ScratchTools`
     - `Cells`
     - `TotalBaseScore`
     - `ScratchProgress`
     - `State`
   - 对外广播：
     - `OnScratchProgressChanged`
     - `OnStateChanged`
     - `OnScratchCompleted`

2. `ScratchCellModel`
   作用：
   - 保存单个格子的运行时数据
   - 包括：
     - 行列位置
     - 图案 id / 名称
     - 基础分
     - 是否可刮
     - 是否已刮开

约束：

1. `ScratchCardModel` 和 `ScratchCellModel` 都属于纯数据层
2. 不允许在 Model 里直接操作 Unity UI、Texture、DOTween、Input
3. Model 只负责存状态和广播状态变化

#### 13.2.4 表现与交互层：负责“让玩家看到并刮它”

1. `ScratchCardView`
   作用：
   - 根据 `ScratchCellModel` 绑定图案显示
   - 通过 `AssetProvider.LoadSpriteFromAtlas(...)` 统一加载图案资源
   - 初始化多个刮层 `RawImage`
   - 为每个刮层创建运行时可擦除纹理
   - 处理：
     - 入场动画
     - 聚焦放大
     - 移动到屏幕中央
     - 划过刮除
     - 自动清空涂层
   - 把“点击卡片”“刮除进度变化”“入场结束”等意图抛给 Controller

2. `MainGamePanel`
   作用：
   - 提供刮刮卡生成容器 `ScratchCardRoot`
   - 提供聚焦遮罩层容器 `FocusOverlayRoot`
   - 提供随机落点和顶部出生点计算
   - 管理聚焦遮罩显示与层级切换

约束：

1. `ScratchCardView` 不负责决定奖励怎么算
2. `ScratchCardView` 不负责决定该生成什么图案
3. 所有图案资源加载必须走 `AssetProvider`
4. 所有刮刮卡对象创建与回收必须走 `PoolManager`
5. 所有动画统一使用 DOTween

#### 13.2.5 控制层：负责“连接 Model 和 View”

1. `ScratchCardController`
   作用：
   - 接收 `Initialize(model, spawnFrom, spawnTo)`
   - 绑定 Model 事件
   - 绑定 View 事件
   - 控制状态切换：
     - `Falling`
     - `Idle`
     - `Focused`
     - `Scratching`
     - `Completed`
   - 接收 View 上报的刮除进度
   - 把进度写回 `ScratchCardModel`
   - 在完成时触发结算
   - 通知外部聚焦状态变化，驱动主界面的遮罩显示与层级调整

2. `MainGameController`
   当前仍然承担“主界面总调度 + 刮刮卡创建入口”的职责
   后续如果购买逻辑继续复杂化，应把“创建刮刮卡”进一步抽到 `UseCase` 或 `Factory`

约束：

1. `ScratchCardController` 不负责图案随机生成
2. `ScratchCardController` 不负责静态配置定义
3. `MainGameController` 只负责主流程调度，不应该逐渐膨胀成结算中心

#### 13.2.6 结算层：负责“这张卡怎么算”

1. `IScratchSettlementEvaluator`
   作用：
   - 定义统一结算接口

2. `ScratchSettlementEvaluatorFactory`
   作用：
   - 根据 `ScratchSettlementType` 返回对应结算器

3. `ScratchToolSettlementService`
   作用：
   - 遍历 `ScratchCardModel.ScratchTools`
   - 聚合每个刮具对应 Evaluator 的 `ScratchSettlementResult`
   - 同一图案允许在不同刮具规则下分别计分；同一刮具规则内部必须自行保证不会重复触发同一次规则

4. 当前已有结算器
   - `FirstRevealedPatternSettlementEvaluator`
   - `MatchAnyPairSettlementEvaluator`
   - `SumScoreSettlementEvaluator`
   - `MatchAnyThreeSettlementEvaluator`
   - `RowSumBonusSettlementEvaluator`

5. `ScratchSettlementResult`
   作用：
   - 承载最终结算结果
   - 包括：
     - `ScoreBeforeRewardMultiplier`：所有刮具规则结算出的分数汇总，尚未应用总倍率
     - `FinalScore`
     - `Summary`
     - `WinningPatternIds`
     - `ScoredCellIndices`
     - `ScoredCellScoreMultipliers`：每个计分格子对应的计分时倍率，只描述该次图案计分，不代表整张刮刮卡总倍率

约束：

1. 结算规则必须策略化
2. 禁止在 `ScratchCardController` 里堆大量 `if cardType == ...`
3. 新增结算方式时，优先新增一个 evaluator，而不是改坏原有结算链

### 13.3 当前项目中的关键文件与作用

#### 13.3.1 配置与默认数据

1. `Assets/Scripts/Configs/ScratchCardTypeConfig.cs`
   定义卡种配置结构
2. `Assets/Scripts/Configs/ScratchPatternConfig.cs`
   定义图案配置结构
3. `Assets/Scripts/Configs/ScratchPatternPoolConfig.cs`
   定义图案池与权重结构
4. `Assets/Scripts/Configs/ScratchAreaTemplateConfig.cs`
   定义区域模板结构
5. `Assets/Scripts/Configs/ScratchSettlementType.cs`
   定义结算策略枚举
6. `Assets/Scripts/Configs/ScratchToolConfig.cs`
   定义刮具配置结构
7. `Assets/Scripts/Core/DataSupport/ScratchToolDefaultsProvider.cs`
   当前临时刮具默认数据提供者
8. `Assets/Scripts/Core/DataSupport/ScratchCardDefaultsProvider.cs`
   当前临时卡种 / 区域 / 图案池默认数据提供者
9. `Assets/Scripts/Core/DataSupport/ScratchPatternDefaultProvider.cs`
   当前临时图案默认数据提供者

#### 13.3.2 生成与运行时数据

1. `Assets/Scripts/Core/DataSupport/ScratchCardGenerator.cs`
   负责生成格子数据
2. `Assets/Scripts/Models/ScratchCardModel.cs`
   负责整张卡实例状态
3. `Assets/Scripts/Models/ScratchCellModel.cs`
   负责单格实例状态
4. `Assets/Scripts/Models/ScratchToolInventoryModel.cs`
   负责玩家已拥有刮具集合

#### 13.3.3 交互与表现

1. `Assets/Scripts/Views/ScratchCardView.cs`
   负责图案显示、刮层擦除、动画、输入
2. `Assets/Scripts/Controllers/ScratchCardController.cs`
   负责单张卡的状态流转和结算触发
3. `Assets/Scripts/Views/MainGamePanel.cs`
   负责主界面里的刮刮卡容器、遮罩、层级与坐标区域
4. `Assets/Scripts/Controllers/MainGameController.cs`
   负责从购买入口生成刮刮卡实例

#### 13.3.4 结算与资源

1. `Assets/Scripts/Core/Services/IScratchSettlementEvaluator.cs`
   统一结算接口
2. `Assets/Scripts/Core/Services/ScratchSettlementEvaluatorFactory.cs`
   结算器工厂
3. `Assets/Scripts/Core/Services/ScratchToolSettlementService.cs`
   玩家刮具聚合结算服务
4. `Assets/Scripts/Core/Services/FirstRevealedPatternSettlementEvaluator.cs`
   第一个刮开图案计分规则
5. `Assets/Scripts/Core/Services/MatchAnyPairSettlementEvaluator.cs`
   一对相同图案计分规则
6. `Assets/Scripts/Core/Services/SumScoreSettlementEvaluator.cs`
   累加基础分
7. `Assets/Scripts/Core/Services/MatchAnyThreeSettlementEvaluator.cs`
   三消类示例规则
8. `Assets/Scripts/Core/Services/RowSumBonusSettlementEvaluator.cs`
   行加成类示例规则
9. `Assets/Scripts/Core/DataSupport/ScratchSettlementResult.cs`
   结算结果数据
10. `Assets/Scripts/Core/Services/AssetProvider.cs`
   图案图集、预制体等资源统一加载入口

### 13.4 当前这套链路最容易混乱的点

后续开发时最容易混乱的是下面几件事，必须特别注意：

1. `MainGameController` 是“创建入口”，不是“整套规则中心”
2. `ScratchCardController` 是“单张卡协调器”，不是“图案生成器”
3. `ScratchCardView` 是“输入和表现层”，不是“业务结算层”
4. `ScratchCardModel` 是“实例状态容器”，不是“配置读取器”
5. `ScratchCardDefaultsProvider` / `ScratchPatternDefaultProvider` 只是当前默认数据源，后续应被正式配表替换
6. `ScratchToolSettlementService + IScratchSettlementEvaluator` 才是未来扩展多种刮具结算规则的稳定入口

### 13.5 刮具构筑与结算规则

刮刮卡本身只决定“价格、图案池、布局、预制体、商店图标”等静态内容，不再决定结算方式。结算方式被抽离为玩家肉鸽构筑的一部分，以“刮具”形式存在：

1. `PlayerContext.ScratchTools` 持有玩家当前拥有的刮具集合。
2. `ScratchToolConfig.SettlementType` 指向一个具体结算规则。
3. 创建 `ScratchCardModel` 时，`MainGameController` 将玩家当前刮具集合注入 `ScratchCardModel.ScratchTools`。
4. 刮开图案和完成刮刮卡时，`ScratchCardController` 只能调用 `ScratchToolSettlementService`，不允许再按卡种或刮具类型写分支。
5. 同一图案允许在不同结算规则下反复触发。例如第一个刮开的水果可以被“默认刮具”计分，也可以在之后凑成一对时被“配对刮具”再次计分。
6. 同一结算规则内部必须记录已消耗的计分对象，不能让同一格子重复参与同一规则。例如“配对刮具”中有 3 个相同图案时只能组成 1 对；有 4 个相同图案时可以组成 2 对，但第一对的两个格子不能再次参与第二对。
7. 每新增一种刮具，应优先新增一个 `IScratchSettlementEvaluator` 实现，再在 `ScratchSettlementEvaluatorFactory` 注册，不应修改 `ScratchCardController` 主流程。

#### 13.5.1 倍率术语必须明确区分

当前游戏内存在两类不同倍率，文档、配表、UI 文案和代码命名必须明确区分：

1. **计分时倍率**（Score Multiplier）
   - 只作用于某一次“图案计分事件”。
   - 它不改变整张刮刮卡的最终总倍率，也不影响其他图案的计分。
   - 典型例子：配对刮具中“一对相同图案计分，成对图案分数 x2”。这里的 `x2` 只属于该配对规则产生的这一次计分。
   - 数据落点应在 `ScratchSettlementResult.ScoredCellScoreMultipliers` 或具体 `IScratchSettlementEvaluator` 的局部计算中表达。

2. **总倍率**（Reward Multiplier）
   - 作用于整张刮刮卡最终入账奖励。
   - 所有已经被刮具结算进 `ScoreBeforeRewardMultiplier` 的图案分数，都会统一受到总倍率影响。
   - 典型例子：倍率图案被刮开后，使当前刮刮卡总倍率 `+0.5`；最终奖励由 `ScratchPatternScoreService.ApplyFinalScoreRules(...)` 统一应用。
   - 数据落点应在 `ScratchCardModel.RewardMultiplier` 或全局/本卡运行时 modifier 中表达，不应写进单个图案的计分时倍率。

结算顺序应保持为：

```text
单个图案基础分
-> 当前刮具规则产生的计分时倍率
-> 汇总为 ScoreBeforeRewardMultiplier
-> 当前刮刮卡总倍率 RewardMultiplier
-> FinalScore
```

禁止事项：

1. 不要把配对刮具的 `x2` 写成总倍率。
2. 不要把倍率图案提供的总倍率写成某个格子的计分时倍率。
3. UI 文案中只写“倍率”时必须补充上下文；优先使用“计分倍率”或“总倍率”。

### 13.6 后续扩展时的推荐改造方向

如果刮刮卡玩法继续变复杂，建议按下面顺序演进：

1. 先把 `ScratchCardDefaultsProvider` 和 `ScratchPatternDefaultProvider` 替换为正式配表读取
2. 再把 `MainGameController` 里的创建逻辑抽成 `CreateScratchCardUseCase` 或 `BuyScratchCardUseCase`
3. 再把“购买扣费”和“结算奖励入账”拆到独立 `Service`
4. 最后再做更细粒度的逐格刮开判定、奖励动画、事件广播和存档接入

### 13.7 动态图案与特殊图案效果

后续部分肉鸽卡会在刮刮卡生成时动态加入额外图案，例如总倍率图案、好脸图案、坏脸图案。该能力必须遵守下面的分层规则：

1. 肉鸽卡只通过 `IRogueCardEffect` 写入 `RogueCardRunModifierModel`，不直接修改静态 `ScratchPatternConfig` 或 `ScratchPatternPoolConfig`
2. 动态加入图案使用 `RogueCardEffectType.AddScratchPatternToPool`
   - `TargetIds` 表示要加入的 `PatternId`
   - `Value` 表示加入图案的基础权重
   - `CardTypeIds` 可选；为空时对所有刮刮卡生效，填写时只对指定卡种生效
3. `ScratchCardGenerator` 负责把卡种原始图案池与肉鸽动态加入图案合并，再统一应用概率修正并归一化抽取
4. Focus 面板概率展示必须使用同一套合并后的权重，禁止在 View 或 Controller 中重复计算概率规则
5. 特殊图案效果写在 `ScratchPatternConfig.EffectType / EffectValue` 中，并由 `ScratchPatternScoreService` 统一解释
6. 结算器需要通过 `ScratchPatternScoreService.GetCellScore(...)` 和 `ScratchPatternScoreService.ApplyFinalScoreRules(...)` 处理特殊图案，不允许在各处按图案 id 写死规则

当前支持的特殊图案效果：

1. `None`：普通图案，按基础分计分
2. `AddRewardMultiplierOnRevealed`：当该图案被刮开时，为当前刮刮卡追加**总倍率**，追加值读取 `EffectValue`
3. `ScoreHighestPatternBaseScoreMultiplier`：当该图案计分时，获得本张刮刮卡最高基础分图案的分数乘以 `EffectValue`；这里属于单次图案计分的特殊计算，不是总倍率。好脸图案默认可配为 `2`
4. `ForceFinalRewardZero`：只要该图案出现在本张可刮区域，最终入账金币强制为 `0`

示例配置：

```json
{
  "effectType": "AddScratchPatternToPool",
  "targetIds": [11],
  "cardTypeIds": [1, 3],
  "value": 10
}
```

### 13.7 当前阶段一句话记忆法

可以用这一句来记整条链路：

**配置层定义卡种，生成层创建实例，Model 保存状态，View 负责刮和显示，Controller 负责协调，Evaluator 负责结算。**
