using RobotSimulator.Core.Models;

namespace RobotSimulator.RCS;

/// <summary>
/// 键盘输入管理器：维护当前按下的按键，转换为 RobotInput
/// </summary>
public class InputManager
{
    private readonly HashSet<Keys> _pressedKeys = new();
    private readonly object _lock = new();

    /// <summary>按键按下</summary>
    public void OnKeyDown(Keys key)
    {
        lock (_lock)
        {
            _pressedKeys.Add(key);
        }
    }

    /// <summary>按键释放</summary>
    public void OnKeyUp(Keys key)
    {
        lock (_lock)
        {
            _pressedKeys.Remove(key);
        }
    }

    /// <summary>获取当前输入状态</summary>
    public RobotInput GetInput()
    {
        lock (_lock)
        {
            return new RobotInput
            {
                Forward = _pressedKeys.Contains(Keys.W) || _pressedKeys.Contains(Keys.Up),
                Backward = _pressedKeys.Contains(Keys.S) || _pressedKeys.Contains(Keys.Down),
                TurnLeft = _pressedKeys.Contains(Keys.A) || _pressedKeys.Contains(Keys.Left),
                TurnRight = _pressedKeys.Contains(Keys.D) || _pressedKeys.Contains(Keys.Right)
            };
        }
    }

    /// <summary>清除所有按键状态</summary>
    public void Clear()
    {
        lock (_lock)
        {
            _pressedKeys.Clear();
        }
    }
}
