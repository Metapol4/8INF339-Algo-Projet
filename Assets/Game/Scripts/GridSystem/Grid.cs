using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public struct GridCell
{
    public int index;
    public CellType cellType;
    public GridCell(int index = 0, CellType cellType = CellType.GROUND)
    {
        this.index = index;
        this.cellType = cellType;
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
}

public class Grid
{
    private int width;
    private int height;
    private float cellSize = 100;
    private GridCell[] gridArray;
    private List<Tuple<int, int>>[] adjacencyList;

    public GridCell[] GridArray { get => gridArray; private set => gridArray = value; }
    public int Width { get => width; private set => width = value; }
    public int Height { get => height; private set => height = value; }
    public float CellSize { get => cellSize; private set => cellSize = value; }
    public List<Tuple<int, int>>[] AdjacencyList { get => adjacencyList; private set => adjacencyList = value; }

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

        Debug.Log("CLL: " + gridArray[5].cellType);

        adjacencyList = new List<Tuple<int, int>>[width * height];

        for (int i = 0; i < width * height; i++)
        {
            adjacencyList[i] = new List<Tuple<int, int>>();
        }
    }

    public void PlaceSpecialCells()
    {
        gridArray[4] = new GridCell(4, CellType.WALL);
        gridArray[14] = new GridCell(14, CellType.WALL);
        gridArray[24] = new GridCell(24, CellType.WALL);

        gridArray[50] = new GridCell(50, CellType.WALL);
        gridArray[51] = new GridCell(51, CellType.WALL);
        gridArray[52] = new GridCell(52, CellType.WALL);
        gridArray[53] = new GridCell(53, CellType.WALL);
        gridArray[54] = new GridCell(54, CellType.WALL);
        gridArray[55] = new GridCell(55, CellType.WALL);

        gridArray[80] = new GridCell(80, CellType.WALL);
        gridArray[81] = new GridCell(81, CellType.WALL);
        gridArray[91] = new GridCell(91, CellType.WALL);

        gridArray[77] = new GridCell(77, CellType.WALL);
        gridArray[87] = new GridCell(87, CellType.WALL);
        gridArray[97] = new GridCell(97, CellType.WALL);
        gridArray[78] = new GridCell(78, CellType.WALL);

        gridArray[63] = new GridCell(63, CellType.WATER);
        gridArray[73] = new GridCell(73, CellType.WATER);
        gridArray[83] = new GridCell(83, CellType.WATER);

        gridArray[79] = new GridCell(79, CellType.WATER);

        gridArray[8] = new GridCell(8, CellType.ELEVATED);
        gridArray[18] = new GridCell(18, CellType.ELEVATED);
        gridArray[28] = new GridCell(28, CellType.ELEVATED);
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
        adjacencyList[u].Add(new Tuple<int, int>(v, weight));
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
                        Debug.Log("WGT: " + weight);
                        AddEdge(i, ni, weight);
                    }
                }
            }
        }

        foreach (var adjency in adjacencyList)
        {
            foreach (var item in adjency)
            {
                Debug.Log(item);
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
}
