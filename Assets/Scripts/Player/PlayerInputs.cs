using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class PlayerInputs : MonoBehaviour
{
    public Vector2 Move { get; set; } = Vector2.zero;
    public bool Jump { get; set; } = false;

    public void Start()
    {
        var actions = InputManager.Controls.PlayerActions;

        actions.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        actions.Move.canceled += _ => Move = Vector2.zero;

        actions.Jump.performed += _ => Jump = true;
        actions.Jump.canceled += _ => Jump = false;
    }
}
