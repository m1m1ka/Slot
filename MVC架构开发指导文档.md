# Unity 2D项目 MVC架构开发指导文档

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
├── Views/         # 继承自MonoBehaviour，挂载在预制体和UI上，处理渲染和动画
├── Controllers/   # 游戏逻辑脚本，管理生命周期，协调Model和View
├── Core/          # 核心系统（事件总线、对象池、公共接口等）
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

## 4. 后续开发建议

1.  **引入事件总线（Event Bus）**：对于跨模块的系统（例如：成就系统需要知道玩家杀死了怪物），直接的Delegate耦合可能不够，建议在 `Core` 目录下实现全局事件驱动。
2.  **避免神级Controller**：采用职责单一原则，分为 `PlayerController`, `EnemyController`, `GamePlayManager`, `LevelController` 等。
3.  **UI隔离**：将主操作UI与对应的玩家GameObject分离，通过MVC逻辑层解耦。

遵循以上规范，即可极大程度保障你在这个2D项目中各个系统的独立性。