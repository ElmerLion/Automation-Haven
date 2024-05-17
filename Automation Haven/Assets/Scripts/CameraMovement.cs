using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public static CameraMovement Instance { get; private set; }

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 150.0f;
    [SerializeField] private float maxZoom = 70.0f;
    [SerializeField] private float minZoom = 10.0f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float freeRotationSpeed = 30f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private bool isRotating = false;


    [Header("Movement")]
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float moveSpeed = 10.0f;
    private float sprintSpeedAdd = 0.1f;


    private Vector3 lastMousePosition;
    private bool isPaused = false;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameSaved += Save;
        SaveManager.OnGameLoaded += Load;
    }

    void Update() {
        if (isPaused) return;

        if (Input.GetKey(KeyCode.LeftShift)) {
            currentSpeed = sprintSpeedAdd + moveSpeed;
        } else {
            currentSpeed = moveSpeed;
        }

        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement() {
        // Horizontal Basic Movement
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        moveDir = transform.TransformDirection(moveDir);

        float moveDistance = currentSpeed;
        float playerHeight = 1f;
        float playerRadius = 1f;

        Vector3 boxCastCenter = transform.position + Vector3.up * (playerHeight / 2 - playerRadius);

        // Use the player's height to adjust the size of the BoxCast
        Vector3 boxSize = new Vector3(playerRadius * 2, playerHeight - playerRadius * 2, playerRadius * 2);

        bool canMove = !Physics.BoxCast(boxCastCenter, boxSize / 2, moveDir, Quaternion.identity, moveDistance, collisionsLayerMask);


        if (!canMove) {
            //Cannot move towards moveDir

            //Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x < -.5f || moveDir.x > .5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionsLayerMask);

            if (canMove) {
                //Can only move on the X
                moveDir = moveDirX;
            } else {
                //Cannot move only on the X

                //Attempt Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (moveDir.z < -.5f || moveDir.z > .5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionsLayerMask);

                if (canMove) {
                    //Cant move only on the Z
                    moveDir = moveDirZ;

                } else {
                    //Cannot move in any direction
                }
            }
        }

        if (canMove) {
            transform.position += moveDir * currentSpeed;
        }

        // Vertical Movement
        float upInput = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
        float downInput = Input.GetKey(KeyCode.LeftControl) ? -1.0f : 0.0f;

        Vector3 verticalMove = Vector3.up * (upInput + downInput) * currentSpeed;
        transform.Translate(verticalMove);
    }

    private void HandleRotation() {
        // Camera rotation up and down
        if (Input.GetKey(KeyCode.T)) {
            rotationX -= rotationSpeed;
        }
        if (Input.GetKey(KeyCode.G)) {
            rotationX += rotationSpeed;
        }
        if (Input.GetKey(KeyCode.Q)) {
            rotationY -= rotationSpeed;
        }
        if (Input.GetKey(KeyCode.E)) {
            rotationY += rotationSpeed;
        }

        if (Input.GetMouseButtonDown(2)) // Middle mouse button (scroll wheel)
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        } else if (Input.GetMouseButtonUp(2)) {
            isRotating = false;
        }

        if (isRotating) {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            rotationX -= mouseDelta.y * freeRotationSpeed;
            rotationX = Mathf.Clamp(rotationX, -90.0f, 90.0f);
            rotationY += mouseDelta.x * freeRotationSpeed;

        }
        transform.rotation = Quaternion.Euler(rotationX, rotationY, transform.rotation.eulerAngles.z);

        // Camera zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        Camera mainCamera = Camera.main;

        float newZoom = mainCamera.fieldOfView - scrollInput * zoomSpeed;
        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        mainCamera.fieldOfView = newZoom;
    }

    public void TogglePause() {
        isPaused = !isPaused;
        Debug.Log("Current pause state: " + isPaused);
    }

    public void SetPause(bool value) {
        isPaused = value;
    }



    public void Load(string filePath) {
        transform.position = ES3.Load("cameraPosition", filePath, transform.position);
        transform.rotation = ES3.Load("cameraRotation", filePath, transform.rotation);
    }

    public void Save(string filePath) {
        ES3.Save("cameraPosition", transform.position, filePath);
        ES3.Save("cameraRotation", transform.rotation, filePath);
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= Save;
        SaveManager.OnGameLoaded -= Load;
    }
}
