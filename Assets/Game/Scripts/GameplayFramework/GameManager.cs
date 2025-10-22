using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private Player player;

    private Grid grid = new Grid(10,10,1);

    private void Awake()
    {
        if(GameManager.Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        player.transform.position = grid.CellToWorld(Vector2.zero);
    }

    public Grid GetGrid()
    {
        return grid;
    }
}
