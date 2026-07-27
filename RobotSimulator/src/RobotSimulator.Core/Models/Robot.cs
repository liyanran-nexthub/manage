namespace RobotSimulator.Core.Models;

/// <summary>机器人状态枚举</summary>
public enum RobotState
{
    Idle,       // 空闲（在格子中心）
    Moving,     // 沿网格移动中
    Turning,    // 转向中（90°旋转）
    Waiting     // 等待（碰撞避让等，后续里程碑用）
}

/// <summary>朝向枚举（仅4个方向）</summary>
public enum Direction
{
    Right = 0,  // →  (0°)
    Down = 1,   // ↓  (90°)
    Left = 2,   // ←  (180°)
    Up = 3      // ↑  (270°)
}

/// <summary>
/// 机器人模型：沿网格移动，位置锁定在格子中心
/// </summary>
public class Robot
{
    private static int _nextId = 1;

    /// <summary>唯一ID</summary>
    public int Id { get; }

    /// <summary>名称</summary>
    public string Name { get; set; }

    // --- 网格坐标（逻辑位置）---

    /// <summary>当前所在行</summary>
    public int GridRow { get; set; }

    /// <summary>当前所在列</summary>
    public int GridCol { get; set; }

    // --- 世界坐标（渲染用，平滑插值）---

    /// <summary>X 坐标（米，世界坐标）</summary>
    public float X { get; set; }

    /// <summary>Y 坐标（米，世界坐标）</summary>
    public float Y { get; set; }

    // --- 运动目标 ---

    /// <summary>目标X（正在移动时的目标格子中心）</summary>
    public float TargetX { get; set; }

    /// <summary>目标Y</summary>
    public float TargetY { get; set; }

    /// <summary>是否正在移动到目标</summary>
    public bool IsMovingToTarget { get; set; }

    // --- 朝向 ---

    /// <summary>当前朝向（4方向）</summary>
    public Direction Facing { get; set; } = Direction.Right;

    /// <summary>渲染用朝向角度（弧度，平滑插值）</summary>
    public float Heading { get; set; }

    /// <summary>目标朝向角度（转向时的目标）</summary>
    public float TargetHeading { get; set; }

    /// <summary>是否正在转向</summary>
    public bool IsTurning { get; set; }

    // --- 速度参数 ---

    /// <summary>当前速度（米/秒）</summary>
    public float Speed { get; set; }

    /// <summary>最大速度（米/秒），默认 1.5</summary>
    public float MaxSpeed { get; set; } = 1.5f;

    /// <summary>加速度（米/秒²），默认 1.0</summary>
    public float Acceleration { get; set; } = 1.0f;

    /// <summary>转向速率（弧度/秒），默认 4.0（快速转90°）</summary>
    public float TurnRate { get; set; } = 4.0f;

    /// <summary>当前状态</summary>
    public RobotState State { get; set; } = RobotState.Idle;

    /// <summary>机器人半径（米），用于碰撞检测</summary>
    public float Radius { get; set; } = 0.2f;

    public Robot()
    {
        Id = _nextId++;
        Name = $"Robot-{Id:D3}";
    }

    public Robot(int gridRow, int gridCol, float cellSize, Direction facing = Direction.Right) : this()
    {
        GridRow = gridRow;
        GridCol = gridCol;
        Facing = facing;
        X = (gridCol + 0.5f) * cellSize;
        Y = (gridRow + 0.5f) * cellSize;
        TargetX = X;
        TargetY = Y;
        Heading = FacingToAngle(facing);
        TargetHeading = Heading;
    }

    /// <summary>重置到指定网格位置</summary>
    public void Reset(int gridRow, int gridCol, float cellSize, Direction facing = Direction.Right)
    {
        GridRow = gridRow;
        GridCol = gridCol;
        Facing = facing;
        X = (gridCol + 0.5f) * cellSize;
        Y = (gridRow + 0.5f) * cellSize;
        TargetX = X;
        TargetY = Y;
        Heading = FacingToAngle(facing);
        TargetHeading = Heading;
        Speed = 0;
        IsMovingToTarget = false;
        IsTurning = false;
        State = RobotState.Idle;
    }

    /// <summary>Direction 转弧度角度</summary>
    public static float FacingToAngle(Direction dir)
    {
        return dir switch
        {
            Direction.Right => 0f,
            Direction.Down => MathF.PI / 2f,
            Direction.Left => MathF.PI,
            Direction.Up => MathF.PI * 3f / 2f,
            _ => 0f
        };
    }

    /// <summary>获取当前朝向前方一格的网格坐标</summary>
    public (int row, int col) GetFrontCell()
    {
        return Facing switch
        {
            Direction.Right => (GridRow, GridCol + 1),
            Direction.Down => (GridRow + 1, GridCol),
            Direction.Left => (GridRow, GridCol - 1),
            Direction.Up => (GridRow - 1, GridCol),
            _ => (GridRow, GridCol)
        };
    }
}
