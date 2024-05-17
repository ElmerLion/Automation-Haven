using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;

public class GridBuildingSystem : MonoBehaviour {

    public static GridBuildingSystem Instance { get; private set; }

    private const string placedObjectsKey = "placedObjects";

    public event EventHandler OnSelectedChanged;
    public event EventHandler<ObjectPlacedEventArgs> OnObjectPlaced;
    public event EventHandler OnObjectDestroyed;

    public class ObjectPlacedEventArgs : EventArgs {
        public PlacedObject_Done placedObject;

        public ObjectPlacedEventArgs(PlacedObject_Done placedObject) {
            this.placedObject = placedObject;
        }

        public Vector2Int GridPosition => placedObject.origin;
    }


    public GridXZ<GridObject> grid;
    private PlacedObjectTypeSO placedObjectTypeSO;
    private PlacedObjectTypeSO.Dir dir;
    private List<GameObject> placedObjects;

    private void Awake() {
        Instance = this;

        int gridWidth = 180;
        int gridHeight = 180;
        float cellSize = 1f;

        grid = new GridXZ<GridObject>(gridWidth, gridHeight, cellSize, new Vector3(0, 0, 0), (GridXZ<GridObject> g, int x, int y) => new GridObject(g, x, y));

        placedObjectTypeSO = null;// placedObjectTypeSOList[0];
    }

    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
    }




    private void Update() {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 999f);

        if (Input.GetMouseButtonDown(0) && placedObjectTypeSO != null) {
            Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
            grid.GetXZ(mousePosition, out int x, out int z);

            Vector2Int placedObjectOrigin = new Vector2Int(x, z);
            placedObjectOrigin = grid.ValidateGridPosition(placedObjectOrigin);

            // Test Can Build
            List<Vector2Int> gridPositionList = placedObjectTypeSO.GetGridPositionList(placedObjectOrigin, dir);
            
            bool canBuild = true;
            foreach (Vector2Int gridPosition in gridPositionList) {
                if (!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild()) {
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

        if (Input.GetKeyDown(KeyCode.R)) {
            dir = PlacedObjectTypeSO.GetNextDir(dir);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) { DeselectObjectType(); }


        if (Input.GetMouseButtonDown(1)) {
            Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
            if (grid.GetGridObject(mousePosition) != null) {
                // Valid Grid Position
                PlacedObject_Done placedObject = grid.GetGridObject(mousePosition).GetPlacedObject();
                if (placedObject != null) {
                    // Demolish
                    
                    foreach (ItemAmount itemAmount in placedObject.GetPlacedObjectTypeSO().buildingCostList.requiredResources) {
                        StorageManager.Instance.AddItemAmountToGlobalInventory(itemAmount);
                    }

                    placedObjects.Remove(placedObject.gameObject);
                    placedObject.DestroySelf();
                    

                    List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();
                    foreach (Vector2Int gridPosition in gridPositionList) {
                        grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
                    }
                    OnObjectDestroyed?.Invoke(this, EventArgs.Empty);
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
