using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private Player player;
    [SerializeField]
    private SpriteRenderer gridSprite;

    private Grid grid = new Grid(10, 10, 1);

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        player.transform.position = grid.CellToWorld(new GridCell());

        for (int i = 0; i < grid.Width; i++)
        {
            for (int j = 0; j < grid.Height; j++)
            {
                SpriteRenderer square = Instantiate<SpriteRenderer>(gridSprite);
                Vector3 position = grid.CellToWorld(i, j);
                square.transform.position = position;
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
