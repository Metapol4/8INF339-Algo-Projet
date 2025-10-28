using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private GridCell currentCell;
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector3 mousepos = Mouse.current.position.ReadValue();
            mousepos.z = Camera.main.nearClipPlane + 1;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mousepos);

            GridCell cell = GameManager.Instance.GetGrid().WorldToCell(mouseWorld);
            //Debug.Log("Going to " + cell.x + ", " + cell.y);
            GoToCell(cell);
        }
    }

    private void GoToCell(GridCell cell)
    {
        Grid grid = GameManager.Instance.GetGrid();

        //int source = 0; //currentCell.y * grid.Width + currentCell.x;
        //int[] distances = Dijkstra(source);
        transform.position = grid.CellToWorld(cell);
        //currentCell = cell;

        //for (int i = 0; i < distances.Length; i++)
        //{
        //    Debug.Log($"{i}\t{distances[i]}");
        //}
    }

    private int[] Dijkstra(int source)
    {
        Grid grid = GameManager.Instance.GetGrid();
        int vertices = grid.AdjacencyList.Length;
        int[] distances = new int[vertices];
        bool[] shortestPathTreeSet = new bool[vertices];

        for (int i = 0; i < vertices; i++)
        {
            distances[i] = int.MaxValue;
            shortestPathTreeSet[i] = false;
        }

        distances[source] = 0;

        for (int i = 0; i < vertices - 1; i++)
        {
            int u = MinimumDistance(distances, shortestPathTreeSet);
            shortestPathTreeSet[u] = true;

            foreach(var neighbor in grid.AdjacencyList[u])
            {
                int v = neighbor.Item1;
                int weight = neighbor.Item2;

                if (!shortestPathTreeSet[v] && distances[u] != int.MaxValue && distances[u] < distances[v])
                {
                    distances[v] = distances[u] + weight;
                }
            }
        }

        return distances;
    }

    private static int MinimumDistance(int[] distances, bool[] shortestPathTreeSet)
    {
        int min = int.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < distances.Length; i++)
        {
            if (!shortestPathTreeSet[i] && distances[i] <= min)
            {
                min = distances[i];
                minIndex = i;
            }
        }
        return minIndex;
    }
}
