using RobotSimulator.Core.Models;

namespace RobotSimulator.Core.Simulation;

/// <summary>
/// 物理引擎：网格锁定运动
/// 机器人只能在格子中心之间移动，转向只能转90°
/// </summary>
public class PhysicsEngine
{
    /// <summary>到达目标的距离阈值（米）</summary>
    private const float ArrivalThreshold = 0.005f;

    /// <summary>转向完成的角度阈值（弧度）</summary>
    private const float TurnThreshold = 0.02f;

    /// <summary>
    /// 更新单个机器人的物理状态（网格模式）
    /// </summary>
    public void Update(Robot robot, RobotInput input, float dt, GridMap map)
    {
        // === 正在转向 ===
        if (robot.IsTurning)
        {
            UpdateTurning(robot, dt);
            return;
        }

        // === 正在移动到目标格子 ===
        if (robot.IsMovingToTarget)
        {
            UpdateMoving(robot, dt);
            return;
        }

        // === 空闲状态：处理输入 ===

        // 左转 / 右转（只在空闲时响应，一次转90°）
        if (input.TurnLeft)
        {
            StartTurn(robot, -1);  // 逆时针
            return;
        }
        if (input.TurnRight)
        {
            StartTurn(robot, +1);  // 顺时针
            return;
        }

        // 前进：向前方格子移动
        if (input.Forward)
        {
            TryMoveForward(robot, map);
            return;
        }

        // 后退：向后方向移动（不改变朝向）
        if (input.Backward)
        {
            TryMoveBackward(robot, map);
            return;
        }

        // 无输入 → 空闲
        robot.State = RobotState.Idle;
        robot.Speed = 0;
    }

    /// <summary>开始转向（90°）</summary>
    private void StartTurn(Robot robot, int direction)
    {
        // direction: -1=左转(逆时针), +1=右转(顺时针)
        int newFacing = ((int)robot.Facing + direction + 4) % 4;
        robot.Facing = (Direction)newFacing;
        robot.TargetHeading = Robot.FacingToAngle(robot.Facing);
        robot.IsTurning = true;
        robot.State = RobotState.Turning;
    }

    /// <summary>更新转向过程（平滑旋转）</summary>
    private void UpdateTurning(Robot robot, float dt)
    {
        float diff = robot.TargetHeading - robot.Heading;

        // 处理跨越 0/2π 的情况
        if (diff > MathF.PI) diff -= MathF.PI * 2f;
        if (diff < -MathF.PI) diff += MathF.PI * 2f;

        float step = robot.TurnRate * dt;

        if (MathF.Abs(diff) <= step || MathF.Abs(diff) < TurnThreshold)
        {
            // 转向完成
            robot.Heading = robot.TargetHeading;
            robot.IsTurning = false;
            robot.State = RobotState.Idle;
        }
        else
        {
            robot.Heading += MathF.Sign(diff) * step;
            robot.Heading = NormalizeAngle(robot.Heading);
        }
    }

    /// <summary>尝试向前移动一格</summary>
    private void TryMoveForward(Robot robot, GridMap map)
    {
        var (targetRow, targetCol) = robot.GetFrontCell();

        if (!map.IsWalkable(targetRow, targetCol))
            return;  // 前方不可通行，不动

        // 设置移动目标
        robot.GridRow = targetRow;
        robot.GridCol = targetCol;
        robot.TargetX = (targetCol + 0.5f) * map.CellSize;
        robot.TargetY = (targetRow + 0.5f) * map.CellSize;
        robot.IsMovingToTarget = true;
        robot.Speed = 0;  // 从0开始加速
        robot.State = RobotState.Moving;
    }

    /// <summary>尝试向后移动一格（不改变朝向）</summary>
    private void TryMoveBackward(Robot robot, GridMap map)
    {
        // 后方 = 朝向的反方向
        int backFacing = ((int)robot.Facing + 2) % 4;
        var (targetRow, targetCol) = backFacing switch
        {
            0 => (robot.GridRow, robot.GridCol + 1),
            1 => (robot.GridRow + 1, robot.GridCol),
            2 => (robot.GridRow, robot.GridCol - 1),
            3 => (robot.GridRow - 1, robot.GridCol),
            _ => (robot.GridRow, robot.GridCol)
        };

        if (!map.IsWalkable(targetRow, targetCol))
            return;

        robot.GridRow = targetRow;
        robot.GridCol = targetCol;
        robot.TargetX = (targetCol + 0.5f) * map.CellSize;
        robot.TargetY = (targetRow + 0.5f) * map.CellSize;
        robot.IsMovingToTarget = true;
        robot.Speed = 0;
        robot.State = RobotState.Moving;
    }

    /// <summary>更新移动过程（加速→匀速→减速→到达）</summary>
    private void UpdateMoving(Robot robot, float dt)
    {
        float dx = robot.TargetX - robot.X;
        float dy = robot.TargetY - robot.Y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);

        // 已到达目标
        if (dist < ArrivalThreshold)
        {
            robot.X = robot.TargetX;
            robot.Y = robot.TargetY;
            robot.IsMovingToTarget = false;
            robot.Speed = 0;
            robot.State = RobotState.Idle;
            return;
        }

        // 计算减速距离：v²/(2a)，在此距离内需要开始减速
        float brakeDist = (robot.Speed * robot.Speed) / (2f * robot.Acceleration);

        if (dist <= brakeDist)
        {
            // 减速阶段
            robot.Speed -= robot.Acceleration * dt;
            if (robot.Speed < 0.05f) robot.Speed = 0.05f;  // 最低速度保证能到达
        }
        else
        {
            // 加速阶段
            robot.Speed += robot.Acceleration * dt;
        }

        // 限速
        robot.Speed = Math.Clamp(robot.Speed, 0f, robot.MaxSpeed);

        // 计算本帧移动距离
        float moveDist = robot.Speed * dt;
        if (moveDist > dist) moveDist = dist;  // 不超过目标

        // 沿方向移动
        float nx = dx / dist;
        float ny = dy / dist;
        robot.X += nx * moveDist;
        robot.Y += ny * moveDist;

        robot.State = RobotState.Moving;
    }

    /// <summary>归一化角度到 [0, 2π)</summary>
    private static float NormalizeAngle(float angle)
    {
        const float twoPi = MathF.PI * 2f;
        angle %= twoPi;
        if (angle < 0) angle += twoPi;
        return angle;
    }
}
