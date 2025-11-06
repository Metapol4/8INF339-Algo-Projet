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
}
