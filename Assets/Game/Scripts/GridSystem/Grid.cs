using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class Grid
{
    private int width;
    private int height;
    private float cellSize = 100;
    private int[,] gridArray;

    public Grid(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new int[width, height];

    }

    public Vector2 CellToWorld(Vector2 cell)
    {
        cell = ClampCellToGridSize(cell);
        Vector2 worldPos = new Vector2(cell.x, cell.y) * cellSize;


        return worldPos;
    }

    public Vector2 WorldToCell(Vector2 pos)
    {
        Vector2 cell = Vector2.zero;

        pos.x /= cellSize;
        pos.y /= cellSize;

        if (pos.x > 0)
            cell.x = Mathf.FloorToInt(pos.x);
        else
            cell.x = Mathf.CeilToInt(pos.x);

        if (pos.y > 0)
            cell.y = Mathf.FloorToInt(pos.y);
        else
            cell.y = Mathf.CeilToInt(pos.y);

        cell = ClampCellToGridSize(cell);
        return cell;
    }

    public Vector2 ClampCellToGridSize(Vector2 cell)
    {
        cell.x = Math.Clamp(cell.x, -width, width);
        cell.y = Math.Clamp(cell.y, -height, height);
        return cell;
    }
}
