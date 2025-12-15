using System;

namespace Utils
{
    public enum PathfindAlgo
    {
        DIJKSTRA,
        BFS
    }
    public enum SortType
    {
        NONE,
        VALUE,
        DISTANCE,
        DISTANCE_AND_VALUE
    }
    public enum CellType
    {
        GROUND = 1,
        WALL = 999,
        WATER = 5,
        ELEVATED = 10,
    }

    public struct PathElement
    {
        public int weight;
        public int parent;
        public int index;

        public PathElement(int index = 0, int weight = 1, int parent = -1)
        {
            this.index = index;
            this.weight = weight;
            this.parent = parent;
        }
    }
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
}
