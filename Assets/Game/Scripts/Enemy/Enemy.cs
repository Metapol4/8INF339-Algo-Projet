using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private int bounty = 1;
    [SerializeField]
    private GridCell cell;

    public int Bounty { get => bounty; set => bounty = value; }
    public GridCell Cell { get => cell; set => cell = value; }
}
