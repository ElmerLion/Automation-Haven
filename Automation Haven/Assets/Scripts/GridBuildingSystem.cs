using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;

public class GridBuildingSystem : MonoBehaviour {

    public static GridBuildingSystem Instance { get; private set; }

    [SerializeField] private GameObject gridObjectVisual;

    private const string placedObjectsKey = "placedObjects";

    public event EventHandler OnSelectedChanged;
    public event EventHandler<ObjectPlacedEventArgs> OnObjectPlaced;
    public event EventHandler<ObjectDestroyedEventArgs> OnObjectDestroyed;
    public event EventHandler<ObjectDestroyedEventArgs> OnBeforeObjectDestroyed;

    public class ObjectPlacedEventArgs : EventArgs {
        public PlacedObject_Done placedObject;

        public ObjectPlacedEventArgs(PlacedObject_Done placedObject) {
            this.placedObject = placedObject;
        }

        public Vector2Int GridPosition => placedObject.origin;
    }

    public class ObjectDestroyedEventArgs : EventArgs {
        public PlacedObject_Done placedObject;
    }


    public GridXZ<GridObject> grid;
    private PlacedObjectTypeSO placedObjectTypeSO;
    private PlacedObjectTypeSO.Dir dir;
    private List<GameObject> placedObjects;
    private int gridWidth;
    private int gridHeight;

    private void Awake() {
        Instance = this;

        placedObjectTypeSO = null;// placedObjectTypeSOList[0];
    }

    public void SetupGrid(int width, int height) {
        gridWidth = width;
        gridHeight = height;
        grid = new GridXZ<GridObject>(gridWidth, gridHeight, 1f, new Vector3(0, 0, 0), (GridXZ<GridObject> g, int x, int y) => new GridObject(g, x, y));
    }

    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
    }

    private void GenerateDebugObject() {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                Vector3 gridPosition = grid.GetWorldPosition(x, y) + new Vector3(0.5f, 0, 0.5f);
                GameObject gridObjectVisualInstance = Instantiate(gridObjectVisual, gridPosition, Quaternion.identity);
                Debug.Log("Spawned gridObjectVisual at " + gridPosition + " for " + x + " " + y + " grid object.");

                GridObjectVisual gridObjectVisualScript = gridObjectVisualInstance.GetComponent<GridObjectVisual>();
                GridObject gridObject = grid.GetGridObject(x, y);
                gridObjectVisualScript.Setup(gridObject);
            }
        }
    }


    private void Update() {
        HandleBuildingPlacing();


        if (Input.GetKeyDown(KeyCode.R)) {
            dir = PlacedObjectTypeSO.GetNextDir(dir);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) { DeselectObjectType(); }

        HandleBuildingDestroying();
        
    }

    private void HandleBuildingPlacing() {
        if (Input.GetMouseButtonDown(0) && placedObjectTypeSO != null) {
            Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
            grid.GetXZ(mousePosition, out int x, out int z);

            Vector2Int placedObjectOrigin = new Vector2Int(x, z);
            placedObjectOrigin = grid.ValidateGridPosition(placedObjectOrigin);

            // Test Can Build
            List<Vector2Int> gridPositionList = placedObjectTypeSO.GetGridPositionList(placedObjectOrigin, dir);

            bool canBuild = true;
            foreach (Vector2Int gridPosition in gridPositionList) {
                GridObject gridObject = grid.GetGridObject(gridPosition.x, gridPosition.y);
                if (gridObject == null || !gridObject.CanBuild()) {
                    canBuild = false;
                    break;
                }
            }

            if (!IsMouseOverUI()) {
                if (canBuild) {

                    if (!StorageManager.Instance.CanAffordItems(placedObjectTypeSO.buildingCostList.requiredResources)) {
                        UtilsClass.CreateWorldTextPopup("Can't Afford!", mousePosition);
                        return;
                    }
                    foreach (ItemAmount itemAmount in placedObjectTypeSO.buildingCostList.requiredResources) {
                        StorageManager.Instance.RemoveItemAmountFromGlobalInventory(itemAmount);
                    }

                    Vector2Int rotationOffset = placedObjectTypeSO.GetRotationOffset(dir);
                    Vector3 placedObjectWorldPosition = grid.GetWorldPosition(placedObjectOrigin.x, placedObjectOrigin.y) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    PlacedObject_Done placedObject = PlacedObject_Done.Create(placedObjectWorldPosition, placedObjectOrigin, dir, placedObjectTypeSO);
                    placedObjects.Add(placedObject.gameObject);

                    foreach (Vector2Int gridPosition in gridPositionList) {
                        grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedObject(placedObject);
                    }

                    OnObjectPlaced?.Invoke(this, new ObjectPlacedEventArgs(placedObject));

                    //DeselectObjectType();
                } else {
                    // Cannot build here
                    UtilsClass.CreateWorldTextPopup("Cannot Build Here!", mousePosition);
                }
            }

        }
    }

    private void HandleBuildingDestroying() {
        if (Input.GetMouseButtonDown(1)) {

            Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
            if (grid.GetGridObject(mousePosition) != null) {
                // Valid Grid Position

                GridObject gridObject = grid.GetGridObject(mousePosition);
                PlacedObject_Done placedObject = gridObject.GetPlacedObject();

                if (gridObject.IsResourceNode()) {
                    return;
                }

                if (placedObject != null) {
                    // Demolish
                    foreach (ItemAmount itemAmount in placedObject.GetPlacedObjectTypeSO().buildingCostList.requiredResources) {
                        StorageManager.Instance.AddItemAmountToGlobalInventory(itemAmount);
                    }

                    OnBeforeObjectDestroyed?.Invoke(this, new ObjectDestroyedEventArgs {
                        placedObject = placedObject
                    });

                    placedObjects.Remove(placedObject.gameObject);
                    placedObject.DestroySelf();


                    List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();
                    foreach (Vector2Int gridPosition in gridPositionList) {
                        grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
                    }
                    OnObjectDestroyed?.Invoke(this, new ObjectDestroyedEventArgs {
                        placedObject = placedObject
                    });
                }
            }
        }
    }

    private bool IsMouseOverUI() {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void SelectObjectType(PlacedObjectTypeSO placedObjectTypeSO) {
        this.placedObjectTypeSO = placedObjectTypeSO;
        RefreshSelectedObjectType();
    }

    public void DeselectObjectType() {
        placedObjectTypeSO = null; RefreshSelectedObjectType();
    }

    private void RefreshSelectedObjectType() {
        OnSelectedChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlacedObjectTypeSelected() {
        return placedObjectTypeSO != null;
    }


    public Vector2Int GetGridPosition(Vector3 worldPosition) {
        grid.GetXZ(worldPosition, out int x, out int z);
        return new Vector2Int(x, z);
    }

    public Vector3 GetMouseWorldSnappedPosition() {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        grid.GetXZ(mousePosition, out int x, out int z);

        if (placedObjectTypeSO != null) {
            Vector2Int rotationOffset = placedObjectTypeSO.GetRotationOffset(dir);
            Vector3 placedObjectWorldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
            return placedObjectWorldPosition;
        } else {
            return mousePosition;
        }
    }

    public Quaternion GetPlacedObjectRotation() {
        if (placedObjectTypeSO != null) {
            return Quaternion.Euler(0, placedObjectTypeSO.GetRotationAngle(dir), 0);
        } else {
            return Quaternion.identity;
        }
    }

    public PlacedObjectTypeSO GetPlacedObjectTypeSO() {
        return placedObjectTypeSO;
    }

    public void AddPlacedObject(PlacedObject_Done placedObject) {
        placedObjects.Add(placedObject.gameObject);
    }



    private List<GameObject> LoadPlacedObjects(string filePath) {
        placedObjects = ES3.Load<List<GameObject>>(placedObjectsKey, filePath);

        foreach (GameObject placedObject in placedObjects) {
            PlacedObject_Done placedObjectScript = placedObject.GetComponent<PlacedObject_Done>();
            Vector2Int origin = placedObjectScript.origin;
            PlacedObjectTypeSO placedObjectTypeSO = placedObjectScript.GetPlacedObjectTypeSO();
            PlacedObjectTypeSO.Dir dir = placedObjectScript.dir;

            List<Vector2Int> gridPositionList = placedObjectTypeSO.GetGridPositionList(origin, dir);
            foreach (Vector2Int gridPosition in gridPositionList) {
                grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedObject(placedObjectScript);
            }
        }
        Debug.Log("Placed objects loaded.");
        return placedObjects;
    }

    private void SaveManager_OnGameSaved(string filePath) {
        ES3.Save(placedObjectsKey, placedObjects, filePath);
        Debug.Log("Grid and placed objects saved.");
    }

    private void SaveManager_OnGameLoaded(string filePath) {
        if (ES3.KeyExists(placedObjectsKey, filePath)) {
            placedObjects = LoadPlacedObjects(filePath);
        } else {
            placedObjects = new List<GameObject>();
        }
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded -= SaveManager_OnGameLoaded;
    }

}
