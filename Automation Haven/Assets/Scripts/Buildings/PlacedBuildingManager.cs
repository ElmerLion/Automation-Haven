using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedBuildingManager : MonoBehaviour {

    public static PlacedBuildingManager Instance { get; private set; }

    public event EventHandler OnNewBuildingUnlocked;

    [SerializeField] private List<PlacedObjectTypeSO> startingBuildingTypes;

    private GridBuildingSystem gridSystem;
    private List<PlacedObjectTypeSO> unlockedBuildingTypes;

    private void Awake() {
        Instance = this;

        unlockedBuildingTypes = new List<PlacedObjectTypeSO>();

        foreach (PlacedObjectTypeSO startingBuildingType in startingBuildingTypes) {
            UnlockBuilding(startingBuildingType);
        }
    }

    private void Start() {
        gridSystem = GridBuildingSystem.Instance;
    }

    public void UnlockBuilding(PlacedObjectTypeSO buildingType) {
        if (!unlockedBuildingTypes.Contains(buildingType)) {
            unlockedBuildingTypes.Add(buildingType);
            OnNewBuildingUnlocked?.Invoke(this, EventArgs.Empty);
        }
    }

    public List<PlacedObjectTypeSO> GetUnlockedBuildingTypeList() {
        return unlockedBuildingTypes;
    }

    public PlacedObject_Done GetPlacedObjectInCell(Vector3 worldPosition) {
        Vector2Int gridPosition = gridSystem.GetGridPosition(worldPosition); // Convert world position to grid position

        // Check if there's a conveyer belt in the grid cell
        GridObject gridObject = gridSystem.grid.GetGridObject(gridPosition.x, gridPosition.y);
        if (gridObject != null) {
            PlacedObject_Done placedObject = gridObject.GetPlacedObject();
            if (placedObject != null) {
                return placedObject; 
            }
        }

        return null; 
    }



    public bool IsNextObjectConveyorBelt(PlacedObject_Done placedObject) {
        return IsNextObjectPlacedObject(placedObject)
            && placedObject.transform.GetComponent<ConveyerBelt>() != null;
    }
    public bool IsNextObjectCraftingMachine(PlacedObject_Done placedObject) {
        return IsNextObjectPlacedObject(placedObject)
            && placedObject.transform.GetComponent<CraftingMachine>() != null;
    }
    public bool IsNextObjectStorage(PlacedObject_Done placedObject) {
        return IsNextObjectPlacedObject(placedObject)
            && placedObject.transform.GetComponent<Storage>() != null;
    }
    public bool IsNextObjectPowerProducer(PlacedObject_Done placedObject) {
        return IsNextObjectPlacedObject(placedObject)
            && placedObject.transform.GetComponent<PowerProducer>() != null;
    }

    private bool IsNextObjectPlacedObject(PlacedObject_Done placedObject) {
        return (placedObject != null && placedObject.transform != transform);
    }

    public bool IsNextObjectDeliveryPoint(PlacedObject_Done placedObject) {
        return IsNextObjectPlacedObject(placedObject)
            && placedObject.transform.GetComponent<DeliveryPoint>() != null;
    }

    public bool IsNextObjectPackager(PlacedObject_Done placedObject) {
        return IsNextObjectPlacedObject(placedObject)
            && placedObject.transform.GetComponent<BoxPackager>() != null;
    }

}
