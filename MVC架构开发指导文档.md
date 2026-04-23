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
    2. **局部子节点**：所有的UI面板动态生成的子列表条目（例如右侧遗物升级选项、彩票列表等），其对应的预制体必须一律存放于 `Resources/UI/` 目录下。在运行时如果需要向视图生成任何节点，**必须通过对象池系统 (`PoolManager`)** 进行循环提取与软回收，绝不许强用 `Instantiate` 和 `Destroy`。
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

为了支持随机生成的“刮刮乐彩票（Scratch Card）”组合（例如：1x3小彩票、3x3大彩票、特殊刮区形状等）以及高自由度的**遗物构筑（Relic Build）、数值膨胀乘区**，我们在底层代码设计上强制采用**数据驱动**与**生命周期解耦**机制。

### 4.1. 刮刮乐核心 MVC 划分
*   **Model (`ScratchCardModel.cs`)**：
    纯 C# 类，只负责存储当前彩票的**稀有度 (Rarity)**、**状态 (未刮/刮开中/已结算)** 以及**隐藏的矩阵奖励数据 (`int[,] RewardGrid`)**。自身提供事件（Action）对外广播刮开进度的局部状态。
*   **View (`ScratchCardView.cs`)**：
    挂载在 2D 预制体上的 `MonoBehaviour`。负责遮罩（Mask）的擦除表现和监听用户手指/鼠标的涂抹交互，把“某行某列已被完全刮开”或“擦除进度达到100%”的业务意图抛给对应的 Controller 处理。
*   **Controller (`ScratchCardController.cs`)**：
    **必须继承** `MonoBehaviour` 并且**与对应 View 挂载在同一个预制体对象上**。
    在 `Awake` 阶段通过 `GetComponent` 自动获取同物体的 View 组件并接收其输入边界反馈。负责业务调度与调用奖励结算器。当判断整张彩票刮开完成时，向外派发事件（走 EventBus 触发全局金币获取特效与遗物加成计算）。

### 4.2. 动态结算机制与策略模式 (Strategy Pattern)
由于各种肉鸽彩票判赢和多重翻倍规则差异巨大，严禁在 Controller 内部堆砌 `if-else`。  
利用 **策略模式**：创建一个只负责纯计算的接口 `IWinEvaluator`。针对不同的彩票（连线赢、找同花色、累计符号），创建不同的实现类（如 `MatchThreeEvaluator`），并在初始化 Controller 时根据 `ScratchCardConfig` 动态注入其所需计算规则策略即可。

### 4.3. 全局遗物乘区与 EventBus（流派化）
1. **全局乘区解耦**：
   * 所有与肉鸽“遗物（Relic）”相关的计算（例如：刮开一个特定符号，全局掉率提升 5%），不应该写在 ScratchCard 层。
   * 应该通过 `EventBus` 广播核心行为：“这张彩票刚刚提供了这些基础金币 `amount`”。然后由一个全局的 `PlayerModel / RelicManager` 监听到此事件，读取玩家身上的遗物集合，计算最终膨胀后的数值再入账玩家账户。
2. **UI 统一交互**：
   * 在需要购买彩票或抽取肉鸽遗物时，**强制使用** `UIManager.Instance.ShowPanel<ShopPanel>()` 或类似统一出口弹出选牌界面，保证弹窗生命周期干净且可追溯。

---

## 5. 后续开发建议

1.  **引入事件总线（Event Bus）**：对于跨模块的系统（例如：成就系统需要知道玩家杀死了怪物），直接的Delegate耦合可能不够，建议在 `Core` 目录下实现全局事件驱动。
2.  **避免神级Controller**：采用职责单一原则，分为 `PlayerController`, `EnemyController`, `GamePlayManager`, `LevelController` 等。
3.  **UI隔离**：将主操作UI与对应的玩家GameObject分离，通过MVC逻辑层解耦。

遵循以上规范，即可极大程度保障你在这个2D/3D挂机增量项目中各个系统的独立性与极高的性能可扩展性。