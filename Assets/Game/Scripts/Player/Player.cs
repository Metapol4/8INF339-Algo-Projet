using System.Collections;
using System.Collections.Generic;
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
