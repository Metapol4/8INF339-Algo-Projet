using TMPro;
using UnityEngine;
using Utils;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private int bounty = 1;
    [SerializeField]
    private GridCell cell;
    [SerializeField]
    private TMP_Text bountyText;
    [SerializeField]
    private TMP_Text idText;

    public int Bounty
    {
        get => bounty;
        set
        {
            bounty = value;
            if (bountyText != null)
                bountyText.text = bounty.ToString();
        }
    }
    public GridCell Cell { get => cell; set => cell = value; }

    public void UpdateIdText(int id)
    {
        if (idText != null)
            idText.text = id.ToString();
    }
}
