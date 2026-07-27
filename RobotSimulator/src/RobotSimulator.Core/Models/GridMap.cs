namespace RobotSimulator.Core.Models;

/// <summary>
/// 网格地图模型：30×30 网格，每格 0.55 米
/// </summary>
public class GridMap
{
    /// <summary>行数</summary>
    public int Rows { get; set; } = 30;

    /// <summary>列数</summary>
    public int Cols { get; set; } = 30;

    /// <summary>每个网格的物理尺寸（米）</summary>
    public float CellSize { get; set; } = 0.55f;

    /// <summary>地图总宽度（米）</summary>
    public float TotalWidth => Cols * CellSize;

    /// <summary>地图总高度（米）</summary>
    public float TotalHeight => Rows * CellSize;

    /// <summary>障碍物标记 [row, col]，true 表示不可通行</summary>
    public bool[,] Obstacles { get; set; }

    public GridMap()
    {
        Obstacles = new bool[Rows, Cols];
    }

    public GridMap(int rows, int cols, float cellSize)
    {
        Rows = rows;
        Cols = cols;
        CellSize = cellSize;
        Obstacles = new bool[rows, cols];
    }

    /// <summary>判断某个网格是否可通行</summary>
    public bool IsWalkable(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Cols)
            return false;
        return !Obstacles[row, col];
    }

    /// <summary>世界坐标(米)转网格坐标</summary>
    public (int row, int col) WorldToGrid(float x, float y)
    {
        int col = (int)(x / CellSize);
        int row = (int)(y / CellSize);
        return (row, col);
    }

    /// <summary>网格坐标转世界坐标(米)，返回格子中心</summary>
    public (float x, float y) GridToWorld(int row, int col)
    {
        float x = (col + 0.5f) * CellSize;
        float y = (row + 0.5f) * CellSize;
        return (x, y);
    }
}
