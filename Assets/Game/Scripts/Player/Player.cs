using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

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
public class Player : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 0.15f;
    [SerializeField]
    private GridCell targetCell;
    [SerializeField]
    private bool moving = false;
    [SerializeField]
    private PathfindAlgo pathfindAlgo = PathfindAlgo.DIJKSTRA;
    [SerializeField]
    private bool chasingEnemies = false;
    [SerializeField]
    private SortType sortType = SortType.VALUE;
    [SerializeField]
    private float bountyWeight = 1.0f;
    [SerializeField]
    private float distanceWeight = 0.3f;
    List<GridCell> enemyCells;


    private void Start()
    {
        GameManager.Instance.UpdateAlgoText(pathfindAlgo);
        GameManager.Instance.UpdateSortText(sortType);
        ResetMap();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (chasingEnemies)
                return;

            Vector3 mousepos = Mouse.current.position.ReadValue();
            mousepos.z = Camera.main.nearClipPlane + 1;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mousepos);

            GridCell cell = GameManager.Instance.GetGrid().WorldToCell(mouseWorld);
            GoToCell(cell);
        }
    }

    public void OnChangeAlgo(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (chasingEnemies)
                return;

            switch (pathfindAlgo)
            {
                case PathfindAlgo.DIJKSTRA:
                    pathfindAlgo = PathfindAlgo.BFS;
                    break;
                case PathfindAlgo.BFS:
                    pathfindAlgo = PathfindAlgo.DIJKSTRA;
                    break;
            }
            GameManager.Instance.UpdateAlgoText(pathfindAlgo);
        }
    }

    public void OnSortTypeChange(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (chasingEnemies)
                return;

            switch (sortType)
            {
                case SortType.NONE:
                    sortType = SortType.VALUE;
                    break;
                case SortType.VALUE:
                    sortType = SortType.DISTANCE;
                    break;
                case SortType.DISTANCE:
                    sortType = SortType.DISTANCE_AND_VALUE;
                    break;
                case SortType.DISTANCE_AND_VALUE:
                    sortType = SortType.NONE;
                    break;
            }
            GameManager.Instance.UpdateSortText(sortType);
        }
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ResetMap();
        }
    }

    public void ResetMap()
    {
        if (chasingEnemies)
            return;
        Grid grid = GameManager.Instance.GetGrid();
        transform.position = grid.CellToWorld(grid.GridArray[0]);
        targetCell = grid.GridArray[0];
        GameManager.Instance.ResetMap();

        enemyCells = grid.GetCellsWithEnemies();
        int i = 0;
        foreach (GridCell cell in enemyCells)
        {
            cell.enemy.UpdateIdText(i);
            i++;
        }
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        if (context.started && !chasingEnemies)
        {
            StartCoroutine(EnemySequence());
        }
    }

    public void OnStartDP(InputAction.CallbackContext context)
    {
        if (context.started && !chasingEnemies)
        {
            DPSequence();
        }
    }

    // adapted from Karp algorithm
    private void DPSequence()
    {
        // setup
        List<GridCell> tmpCells = new List<GridCell>();
        tmpCells.Add(targetCell);
        tmpCells.AddRange(enemyCells);

        int[,] distances = PrecomputeDistances(tmpCells);
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

        StartCoroutine(EnemySequenceTSP(tmpCells, order));
    }

    private int[,] PrecomputeDistances(List<GridCell> points)
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

    private IEnumerator EnemySequenceTSP(List<GridCell> enemyCells, List<int> order)
    {
        chasingEnemies = true;
        Grid grid = GameManager.Instance.GetGrid();
        foreach (int index in order)
        {
            GoToCell(enemyCells[index]);
            yield return new WaitUntil(() => !moving);
            Enemy enemy = enemyCells[index].enemy;
            if (enemy)
                enemy.gameObject.SetActive(false);
        }
        chasingEnemies = false;
    }

    private IEnumerator EnemySequence()
    {
        chasingEnemies = true;
        Grid grid = GameManager.Instance.GetGrid();
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Enemy enemy = null;
        while (enemies.Length > 0)
        {
            switch (sortType)
            {
                case SortType.NONE:
                    enemy = enemies.First();
                    GoToCell(enemy.Cell);
                    break;
                case SortType.VALUE:
                    SortByValue(enemies);
                    enemy = enemies.First();
                    GoToCell(enemy.Cell);
                    break;
                case SortType.DISTANCE:
                    List<PathElement> path = SortByDistance(enemies).First();
                    targetCell = grid.GridArray[path.Last().index];
                    StartCoroutine(GoToCellAnimation(path));
                    enemy = grid.GridArray[path.First().index].enemy;
                    break;
                case SortType.DISTANCE_AND_VALUE:
                    SortByDistanceAndValue(enemies);
                    enemy = enemies.First();
                    GoToCell(enemy.Cell);
                    break;
            }
            yield return new WaitUntil(() => !moving);

            if (enemy)
                enemy.gameObject.SetActive(false);

            enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }
        chasingEnemies = false;
    }

    private void SortByValue(Enemy[] enemies)
    {
        QuickSortByValue(enemies, 0, enemies.Length - 1);
    }


    private void QuickSortByValue(Enemy[] enemies, int low, int high)
    {
        if (low < high)
        {
            int p = PartitionByValue(enemies, low, high);

            QuickSortByValue(enemies, low, p - 1);
            QuickSortByValue(enemies, p + 1, high);
        }
    }

    private int PartitionByValue(Enemy[] enemies, int low, int high)
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

    private List<PathElement>[] SortByDistance(Enemy[] enemies)
    {
        int distance = 0;
        Grid grid = GameManager.Instance.GetGrid();

        int source = targetCell.index;

        List<List<PathElement>> paths = new List<List<PathElement>>();

        foreach (Enemy enemy in enemies)
        {

            PathElement[] result = null;
            List<PathElement> path = null;
            switch (pathfindAlgo)
            {
                case PathfindAlgo.BFS:
                    result = BFS(source, enemy.Cell.index);
                    path = MakePathFromResult(enemy.Cell, result);
                    distance = path.Count;
                    break;
                case PathfindAlgo.DIJKSTRA:
                    result = Dijkstra(source);
                    path = MakePathFromResult(enemy.Cell, result);
                    distance = path.Count;
                    break;
            }

            if (path != null)
                paths.Add(path);
        }
        List<PathElement>[] arrayPaths = paths.ToArray();
        QuickSortByDistance(arrayPaths, 0, paths.Count - 1);
        return arrayPaths;
    }

    private void QuickSortByDistance(List<PathElement>[] paths, int low, int high)
    {
        if (low < high)
        {
            int p = PartitionByDistance(paths, low, high);

            QuickSortByDistance(paths, low, p - 1);
            QuickSortByDistance(paths, p + 1, high);
        }
    }

    private int PartitionByDistance(List<PathElement>[] paths, int low, int high)
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

    private int GetEnemyDistanceToPlayer(List<PathElement> path)
    {
        int distance = 0;

        foreach (PathElement e in path)
        {
            distance += e.weight;
        }

        return distance;
    }


    private void SortByDistanceAndValue(Enemy[] enemies)
    {
        int distance = 0;
        Grid grid = GameManager.Instance.GetGrid();

        int source = targetCell.index;

        List<List<PathElement>> paths = new List<List<PathElement>>();

        foreach (Enemy enemy in enemies)
        {

            PathElement[] result = null;
            List<PathElement> path = null;
            switch (pathfindAlgo)
            {
                case PathfindAlgo.BFS:
                    result = BFS(source, enemy.Cell.index);
                    path = MakePathFromResult(enemy.Cell, result);
                    distance = path.Count;
                    break;
                case PathfindAlgo.DIJKSTRA:
                    result = Dijkstra(source);
                    path = MakePathFromResult(enemy.Cell, result);
                    distance = path.Count;
                    break;
            }

            if (path != null)
                paths.Add(path);
        }
        List<PathElement>[] arrayPaths = paths.ToArray();
        if (enemies.Length == paths.Count)
            QuickSortByDistanceAndValue(enemies, arrayPaths, 0, paths.Count - 1);
    }

    private void QuickSortByDistanceAndValue(Enemy[] enemies, List<PathElement>[] paths, int low, int high)
    {
        if (low < high)
        {
            int p = PartitionByDistanceAndValue(enemies, paths, low, high);

            QuickSortByDistanceAndValue(enemies, paths, low, p - 1);
            QuickSortByDistanceAndValue(enemies, paths, p + 1, high);
        }
    }

    private int PartitionByDistanceAndValue(Enemy[] enemies, List<PathElement>[] paths, int low, int high)
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

    private float GetWeightedScore(Enemy enemy, int distance)
    {
        return bountyWeight * enemy.Bounty - distanceWeight * distance;
    }

    private void Swap(Enemy[] arr, int i, int j)
    {
        Enemy temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }

    private void Swap(List<PathElement>[] arr, int i, int j)
    {
        List<PathElement> temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }

    private IEnumerator GoToCellAnimation(List<PathElement> path)
    {
        if (!moving)
        {
            moving = true;
            Grid grid = GameManager.Instance.GetGrid();
            foreach (var i in path)
            {
                transform.position = grid.CellToWorld(grid.GridArray[i.index]);
                yield return new WaitForSeconds(moveSpeed);
            }
            moving = false;
        }
    }

    private IEnumerator GoToCellAnimation(List<int> path)
    {
        if (!moving)
        {
            moving = true;
            Grid grid = GameManager.Instance.GetGrid();
            foreach (var i in path)
            {
                transform.position = grid.CellToWorld(grid.GridArray[i]);
                yield return new WaitForSeconds(moveSpeed);
            }
            moving = false;
        }
    }

    private void GoToCell(GridCell cell)
    {
        if (moving)
            return;

        Grid grid = GameManager.Instance.GetGrid();

        int source = targetCell.index;

        PathElement[] result = null;
        switch (pathfindAlgo)
        {
            case PathfindAlgo.DIJKSTRA:
                result = Dijkstra(source);
                break;
            case PathfindAlgo.BFS:
                result = BFS(source, cell.index);
                break;
        }
        List<PathElement> path = MakePathFromResult(cell, result);

        if (path.Count <= 1)
            return;

        targetCell = cell;
        StartCoroutine(GoToCellAnimation(path));
    }

    private List<PathElement> MakePathFromResult(GridCell end, PathElement[] result)
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

    private PathElement[] BFS(int source, int dest)
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

    private PathElement[] Dijkstra(int source)
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
}
