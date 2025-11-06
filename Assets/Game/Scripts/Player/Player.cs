using System;
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


    private void Start()
    {
        GameManager.Instance.UpdateAlgoText(pathfindAlgo);
    }


    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector3 mousepos = Mouse.current.position.ReadValue();
            mousepos.z = Camera.main.nearClipPlane + 1;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mousepos);

            GridCell cell = GameManager.Instance.GetGrid().WorldToCell(mouseWorld);
            GoToCell(cell);
        }
    }

    public void OnTab(InputAction.CallbackContext context)
    {
        if (context.started)
        {
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

    public void OnStart(InputAction.CallbackContext context)
    {
        if (context.started && !chasingEnemies)
        {
            StartCoroutine(EnemySequence());
        }
    }

    private IEnumerator EnemySequence()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        while (enemies.Length > 0)
        {
            QuickSort(enemies, 0, enemies.Length - 1, sortType);
            Enemy enemy = enemies.First();
            GoToCell(enemy.Cell);
            yield return new WaitUntil(() => !moving);
            enemy.gameObject.SetActive(false);
            enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }
    }

    private void QuickSort(Enemy[] enemies, int low, int high, SortType sortType)
    {
        if (low < high)
        {
            int p = 0;
            switch (sortType)
            {
                case SortType.VALUE:
                    p = PartitionByValue(enemies, low, high);
                    break;
                case SortType.DISTANCE:
                    p = PartitionByDistance(enemies, low, high);
                    break;
                case SortType.DISTANCE_AND_VALUE:
                    p = PartitionByDistanceAndValue(enemies, low, high);
                    break;
                default:
                    p = PartitionByValue(enemies, low, high);
                    break;
            }

            QuickSort(enemies, low, p - 1, sortType);
            QuickSort(enemies, p + 1, high, sortType);
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

    private int PartitionByDistance(Enemy[] enemies, int low, int high)
    {
        int pivot = GetEnemyDistanceToPlayer(enemies[high]);

        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {
            if (GetEnemyDistanceToPlayer(enemies[j]) < pivot)
            {
                i++;
                Swap(enemies, i, j);
            }
        }

        Swap(enemies, i + 1, high);
        return i + 1;
    }

    private int GetEnemyDistanceToPlayer(Enemy enemy)
    {
        int distance = 0;
        Grid grid = GameManager.Instance.GetGrid();

        int source = targetCell.index;

        PathElement[] result = null;
        List<int> path = null;
        switch (pathfindAlgo)
        {
            case PathfindAlgo.BFS:
                result = BFS(source, enemy.Cell.index);
                path = MakePathFromResultBFS(enemy.Cell.index, result);
                distance = path.Count;
                break;
        }

        return distance;
    }

    private int PartitionByDistanceAndValue(Enemy[] enemies, int low, int high)
    {
        throw new NotImplementedException();
    }

    private void Swap(Enemy[] arr, int i, int j)
    {
        Enemy temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
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
        Debug.Log("Going to " + cell.index);

        Grid grid = GameManager.Instance.GetGrid();

        int source = targetCell.index;

        PathElement[] result = null;
        List<int> path = null;
        switch (pathfindAlgo)
        {
            case PathfindAlgo.DIJKSTRA:
                result = Dijkstra(source);
                path = MakePathFromResultDijkstra(source, cell.index, result);
                break;
            case PathfindAlgo.BFS:
                result = BFS(source, cell.index);
                path = MakePathFromResultBFS(cell.index, result);
                break;
        }

        if (path.Count <= 1)
            return;

        targetCell = cell;
        StartCoroutine(GoToCellAnimation(path));
    }

    private List<int> MakePathFromResultBFS(int end, PathElement[] result)
    {
        List<int> path = new List<int>();
        for (int v = end; v != -1; v = result[v].parent)
        {
            path.Add(v);
        }

        path.Reverse();

        return path;
    }

    private List<int> MakePathFromResultDijkstra(int start, int end, PathElement[] result)
    {
        List<int> path = new List<int>();
        int current = end;

        while (current > -1)
        {
            path.Add(current);
            current = result[current].parent;
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
