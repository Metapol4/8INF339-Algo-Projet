using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector3 mousepos = Mouse.current.position.ReadValue();
            mousepos.z = Camera.main.nearClipPlane + 1;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mousepos);

            GridCell cell = GameManager.Instance.GetGrid().WorldToCell(mouseWorld);
            Debug.Log("Going to " + cell.x + ", " + cell.y);
            GoToCell(cell);
        }
    }

    private void GoToCell(GridCell cell)
    {
        transform.position = GameManager.Instance.GetGrid().CellToWorld(cell);
    }
}

