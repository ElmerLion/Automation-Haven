using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour {

    public static GameInput Instance { get; private set; }

    public event EventHandler OnLeftMouseClicked;
    public event EventHandler OnPauseAction;
    public event EventHandler OnShowMorePerformedAction;
    public event EventHandler OnShowMoreCanceledAction;

    private PlayerInputActions playerInputActions;

    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        playerInputActions.Enable();

        playerInputActions.Player.Pause.performed += Pause_performed;
        playerInputActions.Player.ShowMore.performed += ShowMore_performed;
        playerInputActions.Player.ShowMore.canceled += ShowMore_canceled;
        playerInputActions.Player.LeftMouse.performed += LeftClick_performed;
    }

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }



    public Vector2 GetMovementVectorNormalized() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    private void LeftClick_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnLeftMouseClicked?.Invoke(this, EventArgs.Empty);
    }

    private void ShowMore_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnShowMoreCanceledAction?.Invoke(this, EventArgs.Empty);
    }

    private void ShowMore_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnShowMorePerformedAction?.Invoke(this, EventArgs.Empty);
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }
}
