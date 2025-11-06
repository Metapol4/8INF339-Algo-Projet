using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private int bounty = 1;
    [SerializeField]
    private GridCell cell;
    [SerializeField]
    private TMP_Text bountyText;

    public int Bounty
    {
        get => bounty; 
        set
        {
            bounty = value;
            bountyText.text = bounty.ToString();
        }
    }
    public GridCell Cell { get => cell; set => cell = value; }
}
