using RobotSimulator.Core.Models;
using RobotSimulator.Core.Simulation;
using RobotSimulator.RCS.Rendering;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace RobotSimulator.RCS;

/// <summary>
/// 仿真主窗体：SkiaSharp 渲染 + 键盘控制 + 缩放平移
/// </summary>
public class SimulationForm : Form
{
    // --- 核心组件 ---
    private readonly SKControl _skControl;
    private readonly GridMap _map;
    private readonly Robot _robot;
    private readonly PhysicsEngine _physics;
    private readonly SimulationLoop _loop;
    private readonly InputManager _input;
    private readonly GridRenderer _gridRenderer;
    private readonly RobotRenderer _robotRenderer;

    // --- 视图变换 ---
    private float _pixelsPerMeter = 40f;   // 基础缩放：每米40像素
    private float _zoom = 1.0f;            // 缩放倍数
    private float _panX = 50f;             // 平移偏移 X
    private float _panY = 50f;             // 平移偏移 Y
    private bool _isPanning;
    private Point _lastMouse;

    // --- UI 控件 ---
    private readonly TrackBar _speedBar;
    private readonly TrackBar _accelBar;
    private readonly Label _speedLabel;
    private readonly Label _accelLabel;
    private readonly Label _statusLabel;
    private readonly Button _resetBtn;

    public SimulationForm()
    {
        // --- 初始化模型 ---
        _map = new GridMap(30, 30, 0.55f);
        _robot = new Robot(15, 15, _map.CellSize, Direction.Right);  // 地图中心格子
        _physics = new PhysicsEngine();
        _input = new InputManager();
        _gridRenderer = new GridRenderer();
        _robotRenderer = new RobotRenderer();
        _loop = new SimulationLoop(_physics);

        // --- 窗体设置 ---
        Text = "机器人仿真系统 - M1";
        Size = new Size(1100, 800);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;  // 窗体优先接收键盘事件

        // === 顶部控制面板 ===
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(245, 245, 250)
        };

        // 速度滑块
        var lblSpeed = new Label { Text = "速度:", Location = new Point(10, 16), AutoSize = true };
        _speedBar = new TrackBar
        {
            Location = new Point(50, 10),
            Width = 150,
            Minimum = 5,
            Maximum = 30,
            Value = 15,
            TickFrequency = 5
        };
        _speedBar.ValueChanged += (s, e) =>
        {
            _robot.MaxSpeed = _speedBar.Value / 10f;
            _speedLabel!.Text = $"{_robot.MaxSpeed:F1} m/s";
        };
        _speedLabel = new Label
        {
            Text = "1.5 m/s",
            Location = new Point(205, 16),
            AutoSize = true,
            ForeColor = Color.FromArgb(60, 130, 246)
        };

        // 加速度滑块
        var lblAccel = new Label { Text = "加速度:", Location = new Point(280, 16), AutoSize = true };
        _accelBar = new TrackBar
        {
            Location = new Point(335, 10),
            Width = 150,
            Minimum = 5,
            Maximum = 50,
            Value = 10,
            TickFrequency = 10
        };
        _accelBar.ValueChanged += (s, e) =>
        {
            _robot.Acceleration = _accelBar.Value / 10f;
            _accelLabel!.Text = $"{_robot.Acceleration:F1} m/s²";
        };
        _accelLabel = new Label
        {
            Text = "1.0 m/s²",
            Location = new Point(490, 16),
            AutoSize = true,
            ForeColor = Color.FromArgb(60, 130, 246)
        };

        // 重置按钮
        _resetBtn = new Button
        {
            Text = "重置位置",
            Location = new Point(580, 12),
            Size = new Size(80, 26)
        };
        _resetBtn.Click += (s, e) =>
        {
            _robot.Reset(15, 15, _map.CellSize, Direction.Right);
        };

        // 状态标签
        _statusLabel = new Label
        {
            Text = "就绪 | WASD/方向键控制 | 滚轮缩放 | 中键拖拽平移",
            Location = new Point(680, 16),
            AutoSize = true,
            ForeColor = Color.Gray
        };

        topPanel.Controls.AddRange(new Control[]
        {
            lblSpeed, _speedBar, _speedLabel,
            lblAccel, _accelBar, _accelLabel,
            _resetBtn, _statusLabel
        });

        // === SkiaSharp 画布 ===
        _skControl = new SKControl
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        _skControl.PaintSurface += SkControl_PaintSurface;

        // === 事件绑定 ===
        KeyDown += SimulationForm_KeyDown;
        KeyUp += SimulationForm_KeyUp;
        _skControl.MouseWheel += SkControl_MouseWheel;
        _skControl.MouseDown += SkControl_MouseDown;
        _skControl.MouseMove += SkControl_MouseMove;
        _skControl.MouseUp += SkControl_MouseUp;

        // === 组装布局 ===
        Controls.Add(_skControl);
        Controls.Add(topPanel);

        // === 启动仿真循环 ===
        _loop.OnFrame += OnFrame;
        _loop.Start();
    }

    // ==================== 仿真循环 ====================

    private void OnFrame(float dt)
    {
        // 读取输入 → 更新物理 → 触发重绘
        var input = _input.GetInput();
        _physics.Update(_robot, input, dt, _map);

        // 更新状态栏
        _statusLabel.Text =
            $"状态: {_robot.State} | " +
            $"格子: [{_robot.GridRow},{_robot.GridCol}] | " +
            $"位置: ({_robot.X:F2}, {_robot.Y:F2})m | " +
            $"速度: {_robot.Speed:F2} m/s | " +
            $"朝向: {_robot.Facing} | " +
            $"FPS: {_loop.CurrentFps:F0}";

        _skControl.Invalidate();
    }

    // ==================== 渲染 ====================

    private void SkControl_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        // 应用视图变换：平移 → 缩放
        float effectivePpm = _pixelsPerMeter * _zoom;

        canvas.Save();
        canvas.Translate(_panX, _panY);
        canvas.Scale(_zoom);

        // 绘制网格（使用基础 pixelsPerMeter）
        _gridRenderer.Draw(canvas, _map, _pixelsPerMeter);

        // 绘制机器人
        _robotRenderer.Draw(canvas, _robot, _pixelsPerMeter);

        canvas.Restore();
    }

    // ==================== 键盘事件 ====================

    private void SimulationForm_KeyDown(object? sender, KeyEventArgs e)
    {
        _input.OnKeyDown(e.KeyCode);
        e.Handled = true;
    }

    private void SimulationForm_KeyUp(object? sender, KeyEventArgs e)
    {
        _input.OnKeyUp(e.KeyCode);
        e.Handled = true;
    }

    // ==================== 鼠标事件（缩放 + 平移）====================

    private void SkControl_MouseWheel(object? sender, MouseEventArgs e)
    {
        // 以鼠标位置为中心缩放
        float oldZoom = _zoom;
        float zoomDelta = e.Delta > 0 ? 1.1f : 0.9f;
        _zoom = Math.Clamp(_zoom * zoomDelta, 0.2f, 5.0f);

        // 调整平移，使鼠标下的点不动
        float ratio = _zoom / oldZoom;
        _panX = e.X - (e.X - _panX) * ratio;
        _panY = e.Y - (e.Y - _panY) * ratio;

        _skControl.Invalidate();
    }

    private void SkControl_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Left)
        {
            _isPanning = true;
            _lastMouse = e.Location;
            Cursor = Cursors.SizeAll;
        }
    }

    private void SkControl_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isPanning)
        {
            _panX += e.X - _lastMouse.X;
            _panY += e.Y - _lastMouse.Y;
            _lastMouse = e.Location;
            _skControl.Invalidate();
        }
    }

    private void SkControl_MouseUp(object? sender, MouseEventArgs e)
    {
        _isPanning = false;
        Cursor = Cursors.Default;
    }

    // ==================== 资源释放 ====================

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _loop.Dispose();
        _gridRenderer.Dispose();
        _robotRenderer.Dispose();
        base.OnFormClosing(e);
    }
}
