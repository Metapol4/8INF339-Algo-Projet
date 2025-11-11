using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private Player player;
    [SerializeField]
    private SpriteRenderer groundSprite;
    [SerializeField]
    private SpriteRenderer wallSprite;
    [SerializeField]
    private SpriteRenderer waterSprite;
    [SerializeField]
    private SpriteRenderer elevatedSprite;
    [SerializeField]
    private TMP_Text algoText;
    [SerializeField]
    private TMP_Text sortText;
    [SerializeField]
    private Enemy enemyPrefab;
    [SerializeField]
    private int nbOfRandomEnemies = 5;
    [SerializeField]
    private int randomBountyMax = 10;

    private Grid grid;

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        grid = new Grid(10, 10, 1);

        player.transform.position = grid.CellToWorld(new GridCell());

        for (int i = 0; i < grid.Width; i++)
        {
            for (int j = 0; j < grid.Height; j++)
            {
                int index = grid.ConvertXYToIndex(i, j);
                SpriteRenderer renderer;
                switch (grid.GridArray[index].cellType)
                {
                    case CellType.GROUND:
                        renderer = Instantiate<SpriteRenderer>(groundSprite);
                        break;
                    case CellType.WALL:
                        renderer = Instantiate<SpriteRenderer>(wallSprite);
                        break;
                    case CellType.WATER:
                        renderer = Instantiate<SpriteRenderer>(waterSprite);
                        break;
                    case CellType.ELEVATED:
                        renderer = Instantiate<SpriteRenderer>(elevatedSprite);
                        break;
                    default:
                        renderer = Instantiate<SpriteRenderer>(groundSprite);
                        break;
                }
                Vector3 position = grid.CellToWorld(i, j);
                renderer.transform.position = position;
            }
        }
        grid.ComputeAdjencies();

        PlaceRandomEnemies();
    }


    public void PlaceRandomEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        List<GridCell> walkableCells = grid.GetWalkableCells();

        for (int i = 0; i < nbOfRandomEnemies; i++)
        {
            int randomIndex = Random.Range(0, walkableCells.Count - 1);
            int randomCell = walkableCells[randomIndex].index;
            int randomBounty = Random.Range(1, randomBountyMax);

            grid.GridArray[randomCell].enemy = Instantiate(enemyPrefab);
            grid.GridArray[randomCell].enemy.Cell = grid.GridArray[randomCell];
            grid.GridArray[randomCell].enemy.transform.position = grid.CellToWorld(grid.GridArray[randomCell]);
            grid.GridArray[randomCell].enemy.Bounty = randomBounty;
        }
    }

    public void PlaceScriptedEnemies()
    {
        GridCell[] gridArray = grid.GridArray;

        int[] enemies = new int[] { 8, 12, 60, 99 };
        int[] bounties = new int[] { 1, 2, 3, 4 };

        for(int i = 0; i < enemies.Length; i++) 
        {
            gridArray[enemies[i]].enemy = Instantiate(enemyPrefab);
            gridArray[enemies[i]].enemy.Cell = gridArray[enemies[i]];
            gridArray[enemies[i]].enemy.transform.position = grid.CellToWorld(gridArray[enemies[i]]);
            gridArray[enemies[i]].enemy.Bounty = bounties[i];
        }
    }

    public void UpdateAlgoText(PathfindAlgo algo)
    {
        switch (algo)
        {
            case PathfindAlgo.DIJKSTRA:
                algoText.text = "DIJKSTRA";
                break;
            case PathfindAlgo.BFS:
                algoText.text = "BFS";
                break;
        }
    }

    public void UpdateSortText(SortType sortType)
    {
        switch (sortType)
        {
            case SortType.NONE:
                sortText.text = "NONE";
                break;
            case SortType.VALUE:
                sortText.text = "VALUE";
                break;
            case SortType.DISTANCE:
                sortText.text = "DISTANCE";
                break;
            case SortType.DISTANCE_AND_VALUE:
                sortText.text = "DISTANCE AND VALUE";
                break;
        }
    }


    public void ResetMap()
    {
        PlaceRandomEnemies();
    }

    public Grid GetGrid()
    {
        return grid;
    }
}
