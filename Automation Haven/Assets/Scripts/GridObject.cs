using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridObject {

    private GridXZ<GridObject> grid;
    private int x;
    private int y;
    public PlacedObject_Done placedObject;
    public int powerNetworkId;

    private List<Transform> itemsInGridObject;

    public GridObject(GridXZ<GridObject> grid, int x, int y) {
        this.grid = grid;
        this.x = x;
        this.y = y;
        placedObject = null;
    }

    public override string ToString() {
        return x + ", " + y + "\n" + placedObject;
    }

    public void SetPlacedObject(PlacedObject_Done placedObject) {
        this.placedObject = placedObject;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void ClearPlacedObject() {
        placedObject = null;
        grid.TriggerGridObjectChanged(x, y);
    }

    public PlacedObject_Done GetPlacedObject() {
        return placedObject;
    }

    public void SetPowerNetworkId(int powerNetworkId) {
        this.powerNetworkId = powerNetworkId;
    }
    public void ClearPowerNetworkId() {
        powerNetworkId = 0;
    }
    public int GetPowerNetworkId() {
        return powerNetworkId;
    }

    public bool CanBuild() {
        return placedObject == null || placedObject.transform.GetComponent<ResourceNode>() != null;
    }

    public bool IsResourceNode() {
        return placedObject.transform.GetComponent<ResourceNode>() != null;
    }

    public void AddItem(Transform item) {
        if (itemsInGridObject == null) {
            itemsInGridObject = new List<Transform>();
        }
        itemsInGridObject.Add(item);
    }
    public void RemoveItem(Transform item) {
        itemsInGridObject.Remove(item);
    }

}
