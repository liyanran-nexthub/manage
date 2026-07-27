using System.Diagnostics;
using RobotSimulator.Core.Models;
using RobotSimulator.Core.Simulation;

namespace RobotSimulator.RCS;

/// <summary>
/// 仿真循环：用 WinForms Timer 驱动，每帧更新物理 + 触发重绘
/// </summary>
public class SimulationLoop : IDisposable
{
    private readonly System.Windows.Forms.Timer _timer;
    private readonly PhysicsEngine _physics;
    private readonly Stopwatch _stopwatch;
    private long _lastTickMs;

    /// <summary>每帧触发的事件（参数：deltaTime 秒）</summary>
    public event Action<float>? OnFrame;

    /// <summary>是否正在运行</summary>
    public bool IsRunning => _timer.Enabled;

    /// <summary>帧率（FPS）</summary>
    public float CurrentFps { get; private set; }

    public SimulationLoop(PhysicsEngine physics)
    {
        _physics = physics;
        _stopwatch = Stopwatch.StartNew();
        _lastTickMs = _stopwatch.ElapsedMilliseconds;

        _timer = new System.Windows.Forms.Timer
        {
            Interval = 16  // ≈ 60fps
        };
        _timer.Tick += Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        long nowMs = _stopwatch.ElapsedMilliseconds;
        float dt = (nowMs - _lastTickMs) / 1000f;
        _lastTickMs = nowMs;

        // 防止 dt 过大（如窗口拖动卡顿时）
        dt = Math.Clamp(dt, 0.001f, 0.1f);

        // 计算 FPS
        CurrentFps = dt > 0 ? 1f / dt : 0;

        OnFrame?.Invoke(dt);
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
