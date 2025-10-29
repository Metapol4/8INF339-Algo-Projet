using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public struct DijkstraResult
    {
        public int weight;
        public int previousIndex;

        public DijkstraResult(int weight = 1, int previousIndex = -1)
        {
            this.weight = weight;
            this.previousIndex = previousIndex;
        }
    }
    [SerializeField]
    private float moveSpeed = 0.15f;
    [SerializeField]
    private GridCell targetCell;
    [SerializeField]
    private bool moving = false;
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector3 mousepos = Mouse.current.position.ReadValue();
            mousepos.z = Camera.main.nearClipPlane + 1;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mousepos);

            GridCell cell = GameManager.Instance.GetGrid().WorldToCell(mouseWorld);
            Debug.Log("Going to " + cell.index);
            GoToCell(cell);
        }
    }

    private IEnumerator GoToCellAnimation(List<int> path)
    {
        if (!moving)
        {
            moving = true;
            Grid grid = GameManager.Instance.GetGrid();
            foreach (int i in path)
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
        DijkstraResult[] result = Dijkstra(source);

        for (int i = 0; i < result.Length; i++)
        {
            Debug.Log("I: " + i + " Weight: " + result[i].weight + " Prev: " + result[i].previousIndex);
        }
        List<int> path = MakePathFromResult(source, cell.index, result);


        targetCell = cell;
        StartCoroutine(GoToCellAnimation(path));
    }

    private List<int> MakePathFromResult(int start, int end, DijkstraResult[] result)
    {
        List<int> path = new List<int>();
        List<DijkstraResult> resultList = result.ToList();
        int current = end;

        while (current != -1)
        {
            Debug.Log("PTH:" + result[current].weight);
            path.Add(current);
            current = result[current].previousIndex;
        }

        path.Reverse();
        return path;
    }

    private DijkstraResult[] Dijkstra(int source)
    {
        Grid grid = GameManager.Instance.GetGrid();
        int vertices = grid.AdjacencyList.Length;
        DijkstraResult[] distances = new DijkstraResult[vertices];
        bool[] shortestPathTreeSet = new bool[vertices];

        for (int i = 0; i < vertices; i++)
        {
            distances[i] = new DijkstraResult(int.MaxValue, -1);
            shortestPathTreeSet[i] = false;
        }

        distances[source].weight = 0;

        for (int i = 0; i < vertices - 1; i++)
        {
            int u = MinimumDistance(distances, shortestPathTreeSet);
            shortestPathTreeSet[u] = true;

            foreach (var neighbor in grid.AdjacencyList[u])
            {
                int v = neighbor.Item1;
                int weight = neighbor.Item2;

                if (!shortestPathTreeSet[v] && distances[u].weight != int.MaxValue && distances[u].weight < distances[v].weight)
                {
                    distances[v].weight = distances[u].weight + weight;
                    distances[v].previousIndex = u;
                }
            }
        }

        return distances;
    }

    private static int MinimumDistance(DijkstraResult[] distances, bool[] shortestPathTreeSet)
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
