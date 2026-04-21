# Unity 2D 挂机增量老虎机项目 MVC架构开发指导文档

本项目是一款**挂机增量点击游戏（核心玩法围绕老虎机的购买、摇奖、升级与自动化触发）**。为了确保未来增量数值、自动化计算及离线收益等系统能够无缝扩展且不产生毁灭性耦合，本项目制定了以下严格的开发指南。

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
    1. **主面板/大窗口**：核心视图（继承自`UIPanel`的主界面等）必须通过 `UI.UIManager.Instance.ShowPanel<T>()` 进行加载和管理。严控野生Canvas对象的生成。
    2. **局部子节点**：所有的UI面板及其动态生成的子列表条目（例如已购买的老虎机单元、右侧升级选项等），其对应的预制体必须一律存放于 `Resources/UI/` 目录下。在运行时如果需要向视图生成任何节点，**必须通过对象池系统 (`PoolManager`)** 进行循环提取与软回收，绝不许强用 `Instantiate` 和 `Destroy`。
*   **职责**：
    1.  提供方法供外部（或监听到事件后）调用，如 `PlayHurtAnimation()`，`UpdateHealthBar(float value)`。
    2.  监听Unity的物理碰撞、UI点击等事件，并通过事件或直接调用Controller的方法将“意图”传递出去，**不自己处理逻辑**。
*   **禁忌**：绝对不要在 View 中包含游戏逻辑判断（例如：`if (health < 0) Die();` 逻辑应该在Controller里）。

### 3.3 Controller (逻辑层) 开发规范
*   **实现方式**：可以是挂载在空物体上的 `MonoBehaviour`（作为入口/生命周期管理者），或者由入口统一管理的普通C#类。
*   **职责**：
    1.  实例化/获取 Model 对象。
    2.  实例化/获取 View 对象。
    3.  订阅 View 的输入事件。
    4.  处理核心游戏逻辑，并调用 Model 的方法修改数据。
*   **禁忌**：不要在 Controller 中直接操作 UI 元素（如 `text.text = "123"`），应当调用 `View.UpdateText("123")`。

---

## 4. 老虎机实体与挂机机制架构设计

为了支持无穷种类的老虎机（例如：1x3单线、3x3多线、特殊Scatter计算等）以及性能攸关的**下线挂机收益与屏幕外裁剪计算**，我们在底层代码设计上强制采用**数据驱动**与**生命周期分离**机制。

### 4.1. 老虎机核心 MVC 划分
*   **Model (`SlotMachineModel.cs`)**：
    纯 C# 类，只负责存储当前老虎机的**等级 (Level)**、**状态 (Idle/Spinning/Cooldown/Auto)** 以及**当前的矩阵开奖结果 (`int[,] CurrentGrid`)**。并通过事件向外广播状态改变。
*   **View (`SlotMachineView.cs`)**：
    挂载在老虎机 3D 预制体上的 `MonoBehaviour`。本质是“皮囊”，负责动画表现（摇杆拉动、转轮旋转动画）和监听用户的 3D 点击交互，并把点击意图抛给对应的 Controller。
*   **Controller (`SlotMachineController.cs`)**：
    纯 C# 逻辑大脑（或者由上层 `MainGameController` 持有的轻量级对象）。每个被购买的老虎机都会永久持有一个对应的 Controller 实例。Controller 内部监听 Model 的状态变化并计算中奖概率及金币派发。

### 4.2. 动态计算机制与策略模式 (Strategy Pattern)
由于各种老虎机连线判赢规则差异巨大，我们严禁在 Controller 内部堆砌 `if-else`。  
利用 **策略模式**：创建一个只负责纯计算的接口 `ISlotEvaluator`。针对不同的老虎机（单线、九线），创建不同的实现类（如 `LineMatchEvaluator`），并在初始化 `SlotMachineController` 时根据配置表（`SlotMachineConfig`）动态注入其所需计算规则策略即可。

### 4.3. 屏幕外休眠与离线挂机核心思想
为了保证大量老虎机同时触发不会引发内存耗尽或卡顿：
1. **View 与 Controller 生命周期的解耦**：
   * **在视口内**：Controller 正常获取到对应的 3D `SlotMachineView` 表现层实体，并驱使其播放转轮动画。
   * **滑出视口 (Off-screen)**：为了节省渲染开销，`SlotMachineView` 必须被对象池 (`PoolManager`) 回收销毁，只保留唯一的 Controller 实力常驻后台。
   * **离线挂机后台结算**：即使 `View` 为 null，`Controller` 内部仍然按照配置好的**冷却时间**和**摇奖概率**，在纯后台悄无声息地进行结算判断（`RequestSpin()`），并根据策略结果更新玩家全局金额属性，实现真假挂机的无缝过渡。
2. **唯一销毁出口**：
   * 只有当玩家选择**主动出售该老虎机**时，对应的 `SlotMachineController` 才可以被释放卸载，清理掉内存占用，终止该老虎机的后台收益机制。

---

## 5. 后续开发建议

1.  **引入事件总线（Event Bus）**：对于跨模块的系统（例如：成就系统需要知道玩家杀死了怪物），直接的Delegate耦合可能不够，建议在 `Core` 目录下实现全局事件驱动。
2.  **避免神级Controller**：采用职责单一原则，分为 `PlayerController`, `EnemyController`, `GamePlayManager`, `LevelController` 等。
3.  **UI隔离**：将主操作UI与对应的玩家GameObject分离，通过MVC逻辑层解耦。

遵循以上规范，即可极大程度保障你在这个2D/3D挂机增量项目中各个系统的独立性与极高的性能可扩展性。