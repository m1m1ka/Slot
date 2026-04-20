# Instructions（AI开发指引）

本文件用于在仓库本地为 AI 提供统一开发约束，核心规则如下：

1. **严格遵循 MVC 分层**：Model（数据）、View（表现）、Controller（逻辑）必须职责分离，禁止跨层直接耦合。
2. **Model 纯数据化**：不依赖 `MonoBehaviour` 与 Unity 视图组件，仅通过事件暴露状态变化。
3. **View 仅负责表现与输入上报**：不承载业务判断，不直接修改 Model。
4. **Controller 统一编排**：接收输入、执行业务逻辑、更新 Model，并驱动 View 刷新。
5. **目录结构遵循约定**：`Assets/Scripts/Models`、`Views`、`Controllers`、`Core`、`ScriptableObjects`。
6. **开发前先阅读详细规范**：详见仓库根目录中的 `MVC架构开发指导文档.md`。
