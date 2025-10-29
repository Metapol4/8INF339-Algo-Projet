using UnityEngine;

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
                    case Utils.CellType.GROUND:
                        renderer = Instantiate<SpriteRenderer>(groundSprite);
                        break;
                    case Utils.CellType.WALL:
                        renderer = Instantiate<SpriteRenderer>(wallSprite);
                        break;
                    case Utils.CellType.WATER:
                        renderer = Instantiate<SpriteRenderer>(waterSprite);
                        break;
                    case Utils.CellType.ELEVATED:
                        renderer = Instantiate<SpriteRenderer>(elevatedSprite);
                        break;
                    default:
                        renderer = Instantiate<SpriteRenderer>(groundSprite);
                        break;
                }
                Vector3 position = grid.CellToWorld(i, j);
                renderer.transform.position = position;
                //Debug.Log(i + " " + j);
            }
        }
        grid.ComputeAdjencies();
    }

    public Grid GetGrid()
    {
        return grid;
    }
}
