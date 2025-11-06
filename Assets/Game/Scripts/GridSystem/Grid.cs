using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public struct GridCell
{
    public int index;
    public CellType cellType;
    public Enemy enemy;
    public GridCell(int index = 0, CellType cellType = CellType.GROUND, Enemy enemy = null)
    {
        this.index = index;
        this.cellType = cellType;
        this.enemy = enemy;
    }

    public override bool Equals(object obj)
    {
        return obj is GridCell cell &&
               index == cell.index &&
               cellType == cell.cellType;
    }

    public static bool operator ==(GridCell c1, GridCell c2)
    {
        return c1.Equals(c2);
    }

    public static bool operator !=(GridCell c1, GridCell c2)
    {
        return !c1.Equals(c2);
    }
    public int GetWeight()
    {
        return (int)cellType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(index, cellType);
    }
}

public class Grid
{
    private int width;
    private int height;
    private float cellSize = 100;
    private GridCell[] gridArray;
    private List<PathElement>[] adjacencyList;

    public GridCell[] GridArray { get => gridArray; private set => gridArray = value; }
    public int Width { get => width; private set => width = value; }
    public int Height { get => height; private set => height = value; }
    public float CellSize { get => cellSize; private set => cellSize = value; }
    public List<PathElement>[] AdjacencyList { get => adjacencyList; private set => adjacencyList = value; }

    public Grid(int width, int height, float cellSize)
    {
        this.Width = width;
        this.Height = height;
        this.CellSize = cellSize;

        GridArray = new GridCell[width * height];

        PlaceSpecialCells();

        for (int i = 0; i < GridArray.Length; i++)
        {
            if (GridArray[i].cellType == 0)
                GridArray[i] = new GridCell(i);
        }

        adjacencyList = new List<PathElement>[width * height];

        for (int i = 0; i < width * height; i++)
        {
            adjacencyList[i] = new List<PathElement>();
        }
    }

    private void PlaceSpecialCells()
    {
        int[] walls = new int[] { 4, 14, 24, 50, 51, 52, 53, 54, 55, 80, 81, 91, 77, 87, 97, 78 };
        int[] water = new int[] { 63, 73, 83, 79 };
        int[] elevated = new int[] { 8, 18, 28 };

        foreach (int i in walls)
            gridArray[i] = new GridCell(i, CellType.WALL);
        foreach (int i in water)
            gridArray[i] = new GridCell(i, CellType.WATER);
        foreach (int i in elevated)
            gridArray[i] = new GridCell(i, CellType.ELEVATED);
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

    public bool IsWithinBounds(int index)
    {
        if (index >= gridArray.Length)
            return false;
        if (index < 0)
            return false;
        return true;
    }

    public void AddEdge(int u, int v, int weight)
    {
        adjacencyList[u].Add(new PathElement(u, weight, v));
    }

    public void ComputeAdjencies()
    {
        for (int i = 0; i < gridArray.Length; i++)
        {
            (int dx, int dy)[] directions = new (int, int)[]
        {
            (-1, 0), // up
            (1, 0),  // down
            (0, -1), // left
            (0, 1)   // right
        };


            foreach ((int dx, int dy) direction in directions)
            {
                Tuple<int, int> xy = ConvertIndexToXY(i);
                int nx = xy.Item1 + direction.dx;
                int ny = xy.Item2 + direction.dy;

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    int ni = ConvertXYToIndex(nx, ny);
                    if (gridArray[ni].cellType != CellType.WALL && gridArray[i].cellType != CellType.WALL)
                    {
                        int weight = gridArray[ni].GetWeight();
                        AddEdge(i, ni, weight);
                    }
                }
            }
        }

    }

    public GridCell ConvertXYToGridCell(int x, int y, CellType type = CellType.GROUND)
    {
        return new GridCell(ConvertXYToIndex(x, y), type);
    }

    public int ConvertXYToIndex(int x, int y)
    {
        return (x + width * y);
    }

    public Tuple<int, int> ConvertIndexToXY(int index)
    {
        return new Tuple<int, int>(index % width, index / width);
    }

    public void SetCellType(int index, CellType type)
    {
        if (!IsWithinBounds(index))
            return;
    }

    public List<GridCell> GetWalkableCells()
    {
        List<GridCell> cells = new List<GridCell>();

        foreach (var cell in GridArray)
        {
            if(cell.cellType != CellType.WALL)
                cells.Add(cell);
        }

        return cells;
    }
}
