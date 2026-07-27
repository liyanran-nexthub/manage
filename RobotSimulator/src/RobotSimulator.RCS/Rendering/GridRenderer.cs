using RobotSimulator.Core.Models;
using SkiaSharp;

namespace RobotSimulator.RCS.Rendering;

/// <summary>
/// 网格地图渲染器
/// </summary>
public class GridRenderer
{
    private readonly SKPaint _gridPaint;
    private readonly SKPaint _borderPaint;
    private readonly SKPaint _obstaclePaint;
    private readonly SKPaint _coordPaint;

    public GridRenderer()
    {
        _gridPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200),
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        _borderPaint = new SKPaint
        {
            Color = new SKColor(80, 80, 80),
            StrokeWidth = 2,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        _obstaclePaint = new SKPaint
        {
            Color = new SKColor(180, 60, 60, 180),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        _coordPaint = new SKPaint
        {
            Color = new SKColor(150, 150, 150),
            TextSize = 10,
            IsAntialias = true
        };
    }

    /// <summary>
    /// 绘制网格
    /// </summary>
    /// <param name="canvas">画布</param>
    /// <param name="map">地图模型</param>
    /// <param name="pixelsPerMeter">每米对应的像素数</param>
    public void Draw(SKCanvas canvas, GridMap map, float pixelsPerMeter)
    {
        float totalW = map.TotalWidth * pixelsPerMeter;
        float totalH = map.TotalHeight * pixelsPerMeter;
        float cellPx = map.CellSize * pixelsPerMeter;

        // 绘制障碍物
        for (int r = 0; r < map.Rows; r++)
        {
            for (int c = 0; c < map.Cols; c++)
            {
                if (map.Obstacles[r, c])
                {
                    var rect = new SKRect(c * cellPx, r * cellPx, (c + 1) * cellPx, (r + 1) * cellPx);
                    canvas.DrawRect(rect, _obstaclePaint);
                }
            }
        }

        // 绘制网格线
        // 竖线
        for (int c = 0; c <= map.Cols; c++)
        {
            float x = c * cellPx;
            canvas.DrawLine(x, 0, x, totalH, _gridPaint);
        }
        // 横线
        for (int r = 0; r <= map.Rows; r++)
        {
            float y = r * cellPx;
            canvas.DrawLine(0, y, totalW, y, _gridPaint);
        }

        // 绘制外边框（加粗）
        canvas.DrawRect(0, 0, totalW, totalH, _borderPaint);

        // 每隔5格绘制坐标标注
        for (int c = 0; c <= map.Cols; c += 5)
        {
            float x = c * cellPx;
            canvas.DrawText($"{c}", x + 2, 12, _coordPaint);
        }
        for (int r = 0; r <= map.Rows; r += 5)
        {
            float y = r * cellPx;
            canvas.DrawText($"{r}", 2, y + 12, _coordPaint);
        }
    }

    public void Dispose()
    {
        _gridPaint.Dispose();
        _borderPaint.Dispose();
        _obstaclePaint.Dispose();
        _coordPaint.Dispose();
    }
}
