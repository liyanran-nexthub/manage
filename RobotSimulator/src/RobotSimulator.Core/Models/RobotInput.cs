namespace RobotSimulator.Core.Models;

/// <summary>
/// 机器人输入指令（每帧从 InputManager 读取）
/// </summary>
public struct RobotInput
{
    /// <summary>前进（W / Up）</summary>
    public bool Forward;

    /// <summary>后退（S / Down）</summary>
    public bool Backward;

    /// <summary>左转（A / Left）</summary>
    public bool TurnLeft;

    /// <summary>右转（D / Right）</summary>
    public bool TurnRight;

    public static RobotInput None => new()
    {
        Forward = false,
        Backward = false,
        TurnLeft = false,
        TurnRight = false
    };
}
