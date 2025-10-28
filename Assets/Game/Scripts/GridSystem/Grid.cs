using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public struct GridCell
{
    public int index;
    //public int y;
    public CellType cellType;
    public GridCell(int index = 0, /*int y = 0,*/ CellType cellType = CellType.GROUND)
    {
        this.index = index;
        //this.y = y;
        this.cellType = cellType;
    }
}

public class Grid
{
    private int width;
    private int height;
    private float cellSize = 100;
    private int[] gridArray;
    //private int[,] gridArray;
    private List<Tuple<int, int>>[,] adjacencies;

    private List<Tuple<int, int>>[] adjacencyList;

    public int[] GridArray { get => gridArray; private set => gridArray = value; }
    //public int[,] GridArray { get => gridArray; private set => gridArray = value; }
    public int Width { get => width; private set => width = value; }
    public int Height { get => height; private set => height = value; }
    public float CellSize { get => cellSize; private set => cellSize = value; }
    public List<Tuple<int, int>>[] AdjacencyList { get => adjacencyList; private set => adjacencyList = value; }

    public Grid(int width, int height, float cellSize)
    {
        this.Width = width;
        this.Height = height;
        this.CellSize = cellSize;

        GridArray = new int[width * height];

        adjacencyList = new List<Tuple<int, int>>[width * height];

        for (int i = 0; i < width * height; i++)
        {
            adjacencyList[i] = new List<Tuple<int, int>>();
        }
    }

    public Vector2 CellToWorld(int x, int y)
    {
        return CellToWorld(ConvertXYToGridCell(x, y));
    }

    public Vector2 CellToWorld(GridCell cell)
    {
        cell = ClampCellToGridSize(cell);

        Tuple<int, int> pos = ConvertIndexToXY(cell.index);

        Vector2 worldPos = new Vector2(pos.Item1, pos.Item2) * CellSize;

        return worldPos;
    }

    public GridCell WorldToCell(Vector2 pos)
    {
        GridCell cell = new GridCell();

        pos.x /= CellSize;
        pos.y /= CellSize;

        cell.index = Mathf.RoundToInt(pos.x) + width * Mathf.RoundToInt(pos.y);

        return cell;
    }

    public GridCell ClampCellToGridSize(GridCell cell)
    {
        cell.index = Math.Clamp(cell.index, 0, gridArray.Length - 1);
        return cell;
    }

    public bool IsWithinBounds(GridCell cell)
    {
        if (cell.index >= gridArray.Length )
            return false;
        if (cell.index < 0)
            return false;
        return true;
    }

    public void SetValue(GridCell cell, int value)
    {
        if (!IsWithinBounds(cell))
            return;
        GridArray[cell.index] = value;
    }

    public void AddEdge(int u, int v, int weight)
    {
        adjacencyList[u].Add(new Tuple<int, int>(v, weight));
    }

    public void ComputeAdjencies()
    {
        AddEdge(0, 1, 10);
        AddEdge(0, 4, 5);
        AddEdge(1, 2, 1);
        AddEdge(1, 4, 2);
        AddEdge(2, 3, 4);
        AddEdge(3, 0, 7);
        AddEdge(3, 2, 6);
        AddEdge(4, 1, 3);
        AddEdge(4, 2, 9);
        AddEdge(4, 3, 2);
        /*
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<Tuple<int, int>> current = GetBlankAdjacentCells(i, j);
                foreach (Tuple<int, int> item in current)
                {
                    adjacencies[i, j].Add(item);
                }
            }
        }*/
    }

    public GridCell ConvertXYToGridCell(int x, int y, CellType type = CellType.GROUND)
    {
        return new GridCell(x + width * y, type);
    }

    public Tuple<int, int> ConvertIndexToXY(int index)
    {
        return new Tuple<int, int>(index % width, index / width);
    }

    private List<Tuple<int, int>> GetBlankAdjacentCells(int x, int y)
    {
        (int dx, int dy)[] directions = new (int, int)[]
        {
            (-1, 0), // up
            (1, 0),  // down
            (0, -1), // left
            (0, 1)   // right
        };

        List<Tuple<int, int>> result = new List<Tuple<int, int>>();

        foreach ((int dx, int dy) direction in directions)
        {
            int nx = x + direction.dx;
            int ny = y + direction.dy;

            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                result.Add(Tuple.Create(nx, ny));
            }
        }

        return result;
    }

    public List<Tuple<int, int>>[] GetAdjencies()
    {
        return adjacencies.Cast<List<Tuple<int, int>>>().ToArray();
    }
}
