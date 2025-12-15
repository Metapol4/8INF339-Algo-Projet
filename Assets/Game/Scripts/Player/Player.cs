using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

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
            CalculateTSP();
        }
    }

    private void CalculateTSP()
    {
        List<GridCell> tmpCells = new List<GridCell>();
        tmpCells.Add(targetCell);
        tmpCells.AddRange(enemyCells);

        List<int> order = Algorithms.TSP(targetCell, enemyCells, pathfindAlgo);

        StartCoroutine(EnemySequenceTSP(tmpCells, order));
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
        Algorithms.QuickSortByValue(enemies, 0, enemies.Length - 1);
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
                    result = Algorithms.BFS(source, enemy.Cell.index);
                    path = Algorithms.MakePathFromResult(enemy.Cell, result);
                    distance = path.Count;
                    break;
                case PathfindAlgo.DIJKSTRA:
                    result = Algorithms.Dijkstra(source);
                    path = Algorithms.MakePathFromResult(enemy.Cell, result);
                    distance = path.Count;
                    break;
            }

            if (path != null)
                paths.Add(path);
        }
        List<PathElement>[] arrayPaths = paths.ToArray();
        Algorithms.QuickSortByDistance(arrayPaths, 0, paths.Count - 1);
        return arrayPaths;
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
                    result = Algorithms.BFS(source, enemy.Cell.index);
                    path = Algorithms.MakePathFromResult(enemy.Cell, result);
                    distance = path.Count;
                    break;
                case PathfindAlgo.DIJKSTRA:
                    result = Algorithms.Dijkstra(source);
                    path = Algorithms.MakePathFromResult(enemy.Cell, result);
                    distance = path.Count;
                    break;
            }

            if (path != null)
                paths.Add(path);
        }
        List<PathElement>[] arrayPaths = paths.ToArray();
        if (enemies.Length == paths.Count)
            Algorithms.QuickSortByDistanceAndValue(enemies, arrayPaths, 0, paths.Count - 1);
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
                result = Algorithms.Dijkstra(source);
                break;
            case PathfindAlgo.BFS:
                result = Algorithms.BFS(source, cell.index);
                break;
        }
        List<PathElement> path = Algorithms.MakePathFromResult(cell, result);

        if (path.Count <= 1)
            return;

        targetCell = cell;
        StartCoroutine(GoToCellAnimation(path));
    }
}
