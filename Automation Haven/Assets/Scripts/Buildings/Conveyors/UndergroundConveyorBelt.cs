using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlacedObjectTypeSO;
using static UnityEngine.UI.Image;

public class UndergroundConveyorBelt : MonoBehaviour {

    [SerializeField] private Transform findUndergroundBeltRaycastTransform;
    [SerializeField] private Transform nextConveyorBeltRaycast;
    [SerializeField] private LayerMask itemLayer;

    private PlacedObject_Done placedObject;

    private UndergroundConveyorBelt inputUndergroundBelt;
    private UndergroundConveyorBelt outputUndergroundBelt;
    private ConveyerBelt nextConveyorBelt;
    private float speed = 0.8f;
    private int maxCellRange = 6;
    private int cellRange;
    private int maxItemsPerCell = 3;
    private int checkRange = 1;

    private List<ItemObject> itemsInTransit = new List<ItemObject>();
    private Dictionary<ItemObject, float> itemTravelTimeDictionary = new Dictionary<ItemObject, float>();

    private void Start() {
        placedObject = GetComponent<PlacedObject_Done>();

        if (Physics.Raycast(findUndergroundBeltRaycastTransform.position, -findUndergroundBeltRaycastTransform.forward, out RaycastHit hit, maxCellRange)) {
            if (hit.transform.TryGetComponent(out UndergroundConveyorBelt foundConveyorBelt)) {
                inputUndergroundBelt = foundConveyorBelt;
                inputUndergroundBelt.SetOutputUndergroundConveyorBelt(this);
                cellRange = Mathf.RoundToInt(hit.distance);
                inputUndergroundBelt.SetCelLRange(cellRange);
                Debug.Log("Cell Range: " + cellRange + " Max items allowed stored: " + cellRange * maxItemsPerCell);

                outputUndergroundBelt = this;
                
                FindOutputConveyorBelt();

                Debug.Log("Found Underground Conveyor Belt at " + hit.transform.position);
            }
        }

        GridBuildingSystem.Instance.OnObjectPlaced += GridBuildingSystem_OnObjectPlaced;
    }


    private void Update() { 
        if (itemsInTransit.Count > 0) {
            HandleItemsInTransit();
        }
    }

    private void HandleItemsInTransit() {
        List<ItemObject> toBeRemoved = new List<ItemObject>();

        foreach (ItemObject item in itemsInTransit) {
            float travelTime = itemTravelTimeDictionary[item];
            //Debug.Log("Checking item " + item.GetItemSO().nameString + " travel time: " + travelTime);

            if (travelTime > 0) {
                travelTime -= Time.deltaTime;
                itemTravelTimeDictionary[item] = travelTime;
            }

            if (travelTime <= 0) {
                ConveyerBelt nextConveyorBelt = outputUndergroundBelt.GetOutputConveyorBelt();
                //Debug.Log("Next conveyor belt: " + nextConveyorBelt.transform.position);

                /*if (nextConveyorBelt == null) {
                    outputUndergroundBelt.FindOutputConveyorBelt();
                }*/

                if (outputUndergroundBelt != null && nextConveyorBelt!= null) {

                    if (!outputUndergroundBelt.CanOutput()) {
                        continue;
                    }

                    Vector3 spawnPosition = nextConveyorBelt.GetEntryPoints()[0].position + (nextConveyorBelt.transform.forward / 7);

                    nextConveyorBelt.AddItem(item, spawnPosition);

                    toBeRemoved.Add(item);
                }
            }
        }

        foreach (ItemObject item in toBeRemoved) {
            RemoveItem(item);
        }
    }

    private void GridBuildingSystem_OnObjectPlaced(object sender, GridBuildingSystem.ObjectPlacedEventArgs e) {
        if (nextConveyorBelt == null && IsAdjacent(e.GridPosition)) {
            outputUndergroundBelt.FindOutputConveyorBelt();
        }
    }

    public ConveyerBelt FindOutputConveyorBelt() {
        Vector3 nextGridPosition = nextConveyorBeltRaycast.transform.position + nextConveyorBeltRaycast.transform.forward;
        PlacedObject_Done placedObject = PlacedBuildingManager.Instance.GetPlacedObjectInCell(nextGridPosition);

        if (placedObject != null) {
            if (placedObject.TryGetComponent(out ConveyerBelt foundConveyorBelt)) {
                nextConveyorBelt = foundConveyorBelt;
                return foundConveyorBelt;
            }
        }

        return null;
    }

    public bool CanOutput() {
        if (Physics.Raycast(nextConveyorBeltRaycast.position, nextConveyorBeltRaycast.forward, out RaycastHit hit, 0.5f, itemLayer)) {
            return false;
        }
        return true;
    }

    public void SetInputUndergroundConveyorBelt(UndergroundConveyorBelt inputUndergroundBelt) {
        this.inputUndergroundBelt = inputUndergroundBelt;
    }

    public void SetCelLRange(int cellRange) {
        this.cellRange = cellRange;
    }

    public void SetOutputUndergroundConveyorBelt(UndergroundConveyorBelt outputUndergroundBelt) {
        this.outputUndergroundBelt = outputUndergroundBelt;
    }

    public ConveyerBelt GetOutputConveyorBelt() {
        return nextConveyorBelt;
    }

    public List<ItemObject> GetItems() {
        return itemsInTransit;
    }

    public bool TryAddItem(ItemObject item) {
        if (itemsInTransit.Count >= maxItemsPerCell * cellRange) return false;
        Debug.Log("Adding item to underground belt");
        item.gameObject.SetActive(false);

        itemsInTransit.Add(item);

        float travelTime = cellRange / speed;
        itemTravelTimeDictionary[item] = travelTime;
        return true;
    }

    public void RemoveItem(ItemObject item) {
        itemsInTransit.Remove(item);
        itemTravelTimeDictionary.Remove(item);
    }

    private bool IsAdjacent(Vector2Int otherPosition) {
        List<Vector2Int> myPositions = GetGridPositionList();
        foreach (Vector2Int pos in myPositions) {
            if (Vector2Int.Distance(pos, otherPosition) <= checkRange) {
                return true;
            }
        }
        return false;
    }

    public List<Vector2Int> GetGridPositionList() {
        return placedObject.placedObjectTypeSO.GetGridPositionList(placedObject.origin, placedObject.dir);
    }
}
