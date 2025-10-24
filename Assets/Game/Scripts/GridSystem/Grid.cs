using System;
using UnityEngine;

public struct GridCell
{
    public int x;
    public int y;
    public GridCell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class Grid
{
    private int width;
    private int height;
    private float cellSize = 100;
    private int[,] gridArray;

    public int[,] GridArray { get => gridArray; private set => gridArray = value; }
    public int Width { get => width; private set => width = value; }
    public int Height { get => height; private set => height = value; }
    public float CellSize { get => cellSize; private set => cellSize = value; }

    public Grid(int width, int height, float cellSize)
    {
        this.Width = width;
        this.Height = height;
        this.CellSize = cellSize;

        GridArray = new int[width, height];

    }

    public Vector2 CellToWorld(GridCell cell)
    {
        cell = ClampCellToGridSize(cell);
        Vector2 worldPos = new Vector2(cell.x, cell.y) * CellSize;

        return worldPos;
    }

    public GridCell WorldToCell(Vector2 pos)
    {
        Debug.Log(pos);
        GridCell cell;
        cell.x = 0;
        cell.y = 0;

        pos.x /= CellSize;
        pos.y /= CellSize;

        cell.x = Mathf.RoundToInt(pos.x);
        cell.y = Mathf.RoundToInt(pos.y);

        return cell;
    }

    public GridCell ClampCellToGridSize(GridCell cell)
    {
        cell.x = Math.Clamp(cell.x, 0, Width - 1);
        cell.y = Math.Clamp(cell.y, 0, Height - 1);
        return cell;
    }

    public bool IsWithinBounds(GridCell cell)
    {
        if (cell.x >= width || cell.y >= width)
            return false;
        if (cell.x < 0 || cell.y < 0)
            return false;
        return true;
    }

    public void SetValue(GridCell cell, int value)
    {
        if (!IsWithinBounds(cell))
            return;
        GridArray[cell.x, cell.y] = value;
    }
}
