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

            Vector2 cell = GameManager.Instance.GetGrid().WorldToCell(mouseWorld);
            Debug.Log("Going to " + cell);
            GoToCell(cell);
        }
    }

    private void GoToCell(Vector2 cell)
    {
        transform.position = GameManager.Instance.GetGrid().CellToWorld(cell);
    }
}

