using System.Collections.Generic;
using UnityEngine;
using Utils;

public static class Algorithms
{
    private static float bountyWeight = 1.0f;
    private static float distanceWeight = 0.3f;

    // adapted from Held-Karp algorithm
    public static List<int> TSP(GridCell targetCell, List<GridCell> enemyCells, PathfindAlgo pathfindAlgo)
    {
        // setup
        List<GridCell> tmpCells = new List<GridCell>();
        tmpCells.Add(targetCell);
        tmpCells.AddRange(enemyCells);

        int[,] distances = PrecomputeDistances(tmpCells, pathfindAlgo);
        int n = distances.GetLength(0);

        int full = (1 << n);
        int infinite = int.MaxValue / 4;

        int[,] dp = new int[full, tmpCells.Count];
        int[,] parent = new int[full, tmpCells.Count];

        for (int i = 0; i < full; i++)
        {
            for (int j = 0; j < tmpCells.Count; j++)
            {
                dp[i, j] = infinite;
                parent[i, j] = -1;
            }
        }

        dp[1, 0] = 0;


        // transitions
        for (int transMask = 1; transMask < full; transMask++)
        {
            if ((transMask & 1) == 0)
                continue;

            for (int j = 1; j < n; j++)
            {
                if ((transMask & (1 << j)) == 0)
                    continue;

                int previousMask = transMask ^ (1 << j);

                for (int k = 0; k < n; k++)
                {
                    if ((previousMask & (1 << k)) == 0)
                        continue;

                    int cost = dp[previousMask, k] + distances[k, j];

                    if (cost < dp[transMask, j])
                    {
                        dp[transMask, j] = cost;
                        parent[transMask, j] = k;
                    }
                }
            }
        }

        //get enemy order
        int fullMask = full - 1;
        int minCost = infinite;
        int lastParent = 0;

        for (int j = 1; j < n; j++)
        {
            int cost = dp[fullMask, j] + distances[j, 0];
            if (cost < minCost)
            {
                minCost = cost;
                lastParent = j;
            }
        }

        List<int> order = new List<int>();
        int currentMask = fullMask;
        int currentParent = lastParent;

        while (currentParent != 0)
        {
            order.Add(currentParent);
            int previousParent = parent[currentMask, currentParent];
            currentMask ^= (1 << currentParent);
            currentParent = previousParent;
        }
        order.Add(0);
        order.Reverse();
        order.Add(0);

        return order;
    }

    private static int[,] PrecomputeDistances(List<GridCell> points, PathfindAlgo pathfindAlgo)
    {
        Grid grid = GameManager.Instance.GetGrid();
        int n = points.Count;
        int[,] dist = new int[n, n];


        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    dist[i, j] = 0;
                }
                else
                {
                    int startIndex = points[i].index;
                    int endIndex = points[j].index;

                    switch (pathfindAlgo)
                    {
                        case PathfindAlgo.BFS:
                            PathElement[] bfs = BFS(startIndex, endIndex);
                            dist[i, j] = MakePathFromResult(grid.GridArray[endIndex], bfs).Count;
                            break;
                        case PathfindAlgo.DIJKSTRA:
                            PathElement[] dijstra = Dijkstra(startIndex);
                            List<PathElement> path = MakePathFromResult(grid.GridArray[endIndex], dijstra);
                            dist[i, j] = GetEnemyDistanceToPlayer(path);
                            break;
                    }
                }
            }
        }

        return dist;
    }

    public static PathElement[] BFS(int source, int dest)
    {
        Grid grid = GameManager.Instance.GetGrid();
        bool[] visited = new bool[grid.AdjacencyList.Length];
        PathElement[] elements = new PathElement[grid.AdjacencyList.Length];
        Queue<int> queue = new Queue<int>();

        for (int i = 0; i < elements.Length; i++)
        {
            elements[i] = new PathElement(i, 0, -1);
        }

        visited[source] = true;
        elements[source] = new PathElement(source, 0, -1);
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            int v = queue.Dequeue();

            if (v == dest)
                break;

            foreach (PathElement adjacent in grid.AdjacencyList[v])
            {
                int neighbor = adjacent.parent;

                if (!visited[neighbor])
                {
                    visited[neighbor] = true;
                    elements[neighbor] = new PathElement(neighbor, elements[v].weight + 1, v);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return elements;
    }

    public static PathElement[] Dijkstra(int source)
    {
        Grid grid = GameManager.Instance.GetGrid();
        int vertices = grid.AdjacencyList.Length;
        PathElement[] distances = new PathElement[vertices];
        bool[] shortestPathTreeSet = new bool[vertices];

        for (int i = 0; i < vertices; i++)
        {
            distances[i] = new PathElement(i, int.MaxValue, -1);
            shortestPathTreeSet[i] = false;
        }

        distances[source].weight = 0;

        for (int i = 0; i < vertices - 1; i++)
        {
            int u = MinimumDistance(distances, shortestPathTreeSet);
            shortestPathTreeSet[u] = true;

            foreach (var neighbor in grid.AdjacencyList[u])
            {
                int v = neighbor.parent;
                int weight = neighbor.weight;

                if (!shortestPathTreeSet[v] && distances[u].weight != int.MaxValue && distances[u].weight < distances[v].weight)
                {
                    distances[v].weight = distances[u].weight + weight;
                    distances[v].parent = u;
                }
            }
        }

        return distances;
    }

    private static int MinimumDistance(PathElement[] distances, bool[] shortestPathTreeSet)
    {
        int min = int.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < distances.Length; i++)
        {
            if (!shortestPathTreeSet[i] && distances[i].weight <= min)
            {
                min = distances[i].weight;
                minIndex = i;
            }
        }
        return minIndex;
    }

    public static List<PathElement> MakePathFromResult(GridCell end, PathElement[] result)
    {
        List<PathElement> path = new List<PathElement>();

        int currentIndex = end.index;

        while (currentIndex != -1)
        {
            PathElement element = result[currentIndex];
            path.Add(element);
            currentIndex = element.parent;
        }

        path.Reverse();

        return path;
    }

    private static int GetEnemyDistanceToPlayer(List<PathElement> path)
    {
        int distance = 0;

        foreach (PathElement e in path)
        {
            distance += e.weight;
        }

        return distance;
    }

    public static void QuickSortByValue(Enemy[] enemies, int low, int high)
    {
        if (low < high)
        {
            int p = PartitionByValue(enemies, low, high);

            QuickSortByValue(enemies, low, p - 1);
            QuickSortByValue(enemies, p + 1, high);
        }
    }

    private static int PartitionByValue(Enemy[] enemies, int low, int high)
    {
        int pivot = enemies[high].Bounty;

        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {
            if (enemies[j].Bounty > pivot)
            {
                i++;
                Swap(enemies, i, j);
            }
        }

        Swap(enemies, i + 1, high);
        return i + 1;
    }

    public static void QuickSortByDistance(List<PathElement>[] paths, int low, int high)
    {
        if (low < high)
        {
            int p = PartitionByDistance(paths, low, high);

            QuickSortByDistance(paths, low, p - 1);
            QuickSortByDistance(paths, p + 1, high);
        }
    }

    private static int PartitionByDistance(List<PathElement>[] paths, int low, int high)
    {
        int pivot = GetEnemyDistanceToPlayer(paths[high]);

        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {
            if (GetEnemyDistanceToPlayer(paths[j]) < pivot)
            {
                i++;
                Swap(paths, i, j);
            }
        }

        Swap(paths, i + 1, high);
        return i + 1;
    }

    public static void QuickSortByDistanceAndValue(Enemy[] enemies, List<PathElement>[] paths, int low, int high)
    {
        if (low < high)
        {
            int p = PartitionByDistanceAndValue(enemies, paths, low, high);

            QuickSortByDistanceAndValue(enemies, paths, low, p - 1);
            QuickSortByDistanceAndValue(enemies, paths, p + 1, high);
        }
    }

    private static int PartitionByDistanceAndValue(Enemy[] enemies, List<PathElement>[] paths, int low, int high)
    {
        float pivot = GetWeightedScore(enemies[high], GetEnemyDistanceToPlayer(paths[high]));

        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {
            if (GetWeightedScore(enemies[j], GetEnemyDistanceToPlayer(paths[j])) > pivot)
            {
                i++;
                Swap(enemies, i, j);
                Swap(paths, i, j);
            }
        }

        Swap(enemies, i + 1, high);
        Swap(paths, i + 1, high);
        return i + 1;
    }

    private static float GetWeightedScore(Enemy enemy, int distance)
    {
        return bountyWeight * enemy.Bounty - distanceWeight * distance;
    }

    private static void Swap(Enemy[] arr, int i, int j)
    {
        Enemy temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }

    private static void Swap(List<PathElement>[] arr, int i, int j)
    {
        List<PathElement> temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }
}
