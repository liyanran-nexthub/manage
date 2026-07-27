d:\project\manage\RobotSimulator\
├── RobotSimulator.sln
└── src/
    ├── RobotSimulator.Core/
    │   ├── Models/
    │   │   ├── GridMap.cs          ← 30×30网格地图模型
    │   │   ├── Robot.cs            ← 机器人状态模型
    │   │   └── RobotInput.cs       ← 输入指令结构
    │   └── Simulation/
    │       └── PhysicsEngine.cs    ← 运动学更新（加速/摩擦/边界）
    └── RobotSimulator.RCS/
        ├── Rendering/
        │   ├── GridRenderer.cs     ← 网格线+障碍物+坐标标注
        │   └── RobotRenderer.cs    ← 带方向的三角形机器人
        ├── InputManager.cs         ← 键盘状态管理
        ├── SimulationLoop.cs       ← 16ms Timer 动画循环
        ├── SimulationForm.cs       ← 主窗体（渲染+交互+UI）
        └── Program.cs              ← 入口