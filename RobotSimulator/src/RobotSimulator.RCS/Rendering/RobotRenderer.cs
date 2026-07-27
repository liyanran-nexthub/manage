using RobotSimulator.Core.Models;
using SkiaSharp;

namespace RobotSimulator.RCS.Rendering;

/// <summary>
/// 机器人渲染器：绘制带方向的三角形
/// </summary>
public class RobotRenderer
{
    private readonly SKPaint _bodyPaint;
    private readonly SKPaint _outlinePaint;
    private readonly SKPaint _directionPaint;
    private readonly SKPaint _labelPaint;

    public RobotRenderer()
    {
        _bodyPaint = new SKPaint
        {
            Color = new SKColor(60, 130, 246, 220),  // 蓝色半透明
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        _outlinePaint = new SKPaint
        {
            Color = new SKColor(30, 80, 180),
            StrokeWidth = 2,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        _directionPaint = new SKPaint
        {
            Color = new SKColor(255, 220, 50),  // 黄色方向指示
            StrokeWidth = 2,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        _labelPaint = new SKPaint
        {
            Color = new SKColor(40, 40, 40),
            TextSize = 11,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };
    }

    /// <summary>
    /// 绘制单个机器人
    /// </summary>
    /// <param name="canvas">画布</param>
    /// <param name="robot">机器人</param>
    /// <param name="pixelsPerMeter">每米像素数</param>
    public void Draw(SKCanvas canvas, Robot robot, float pixelsPerMeter)
    {
        float cx = robot.X * pixelsPerMeter;
        float cy = robot.Y * pixelsPerMeter;
        float size = robot.Radius * 2f * pixelsPerMeter;  // 机器人尺寸（像素）
        float halfSize = size / 2f;

        canvas.Save();
        canvas.Translate(cx, cy);
        canvas.RotateDegrees(robot.Heading * 180f / MathF.PI);

        // 三角形顶点（朝向 +X 方向）
        var path = new SKPath();
        path.MoveTo(halfSize, 0);                          // 尖端（前方）
        path.LineTo(-halfSize * 0.7f, -halfSize * 0.7f);   // 左后
        path.LineTo(-halfSize * 0.7f, halfSize * 0.7f);    // 右后
        path.Close();

        canvas.DrawPath(path, _bodyPaint);
        canvas.DrawPath(path, _outlinePaint);

        // 方向指示线（从中心到尖端）
        canvas.DrawLine(0, 0, halfSize * 1.2f, 0, _directionPaint);

        path.Dispose();
        canvas.Restore();

        // 绘制标签（不随旋转）
        canvas.DrawText(robot.Name, cx, cy - halfSize - 5, _labelPaint);
    }

    public void Dispose()
    {
        _bodyPaint.Dispose();
        _outlinePaint.Dispose();
        _directionPaint.Dispose();
        _labelPaint.Dispose();
    }
}
