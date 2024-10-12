using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Plot : MonoBehaviour {

    public static event Action<Plot> OnPlotPurchased;

    public enum State {
        NotUnlocked,
        CanBePurchased,
        Purchased,
    }

    [ES3NonSerializable] public List<Plot> neighbors;
    private List<GridObject> gridObjects;

    private int width;
    private int height;
    [ES3Serializable] public State state;
    [ES3NonSerializable] private GridXZ<GridObject> grid;

    private int price;

    public void Setup(int width, int height) {
        this.height = height;
        this.width = width;

        grid = GridBuildingSystem.Instance.grid;
        gridObjects = new List<GridObject>();

        AssignGridObjects();

        if (state == State.Purchased) {
            foreach (GridObject gridObject in gridObjects) {
                gridObject.canBeBuiltOn = true;
            }
        } 

        OnPlotPurchased += Plot_OnPlotPurchased;
        GameInput.Instance.OnLeftMouseClicked += GameInput_OnLeftMouseClicked;
    }

    private void GameInput_OnLeftMouseClicked(object sender, EventArgs e) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit)) {
            // Check if the hit object is this plot
            Plot hitPlot = hit.collider.GetComponent<Plot>();
            if (hitPlot == this && state == State.CanBePurchased 
                && !GridBuildingSystem.Instance.IsPlacedObjectTypeSelected() && !IsMouseOverUI()) {

                AreYouSureUI.Instance.ShowAreYouSure("Do you want to purchase this plot?\nCost: " + price, PurchasePlot);
            }

            
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(0, 1f, 0), new Vector3(width, 0, height));
    }

    private void Plot_OnPlotPurchased(Plot obj) {
        if (neighbors.Contains(obj)) {
            if (state == State.NotUnlocked) {
                state = State.CanBePurchased;
            }
        }
    }

    private void AssignGridObjects() {
        Vector3 origin = transform.position;

        int startX = Mathf.FloorToInt((origin.x - width / 2) / grid.GetCellSize());
        int endX = Mathf.CeilToInt((origin.x + width / 2) / grid.GetCellSize());
        int startZ = Mathf.FloorToInt((origin.z - height / 2) / grid.GetCellSize());
        int endZ = Mathf.CeilToInt((origin.z + height / 2) / grid.GetCellSize());

        for (int x = startX; x <= endX; x++) {
            for (int z = startZ; z <= endZ; z++) {
                GridObject gridObject = grid.GetGridObject(x, z);
                if (gridObject != null) {
                    gridObjects.Add(gridObject);
                }
            }
        }
    }

    public void PurchasePlot() {
        if (state == State.CanBePurchased) {
            state = State.Purchased;

            foreach (GridObject gridObject in gridObjects) {
                gridObject.canBeBuiltOn = true;
            }

            if (neighbors != null) {
                foreach (Plot neighbor in neighbors) {
                    if (neighbor.state == State.NotUnlocked) {
                        neighbor.state = State.CanBePurchased;
                    }
                }
            }

            
            if (transform.TryGetComponent(out BoxCollider boxCollider)) {
                boxCollider.enabled = false;
            }

            OnPlotPurchased?.Invoke(this);

            GameInput.Instance.OnLeftMouseClicked -= GameInput_OnLeftMouseClicked;
        }
    }

    public void ForcePurchasePlot() {
        state = State.Purchased;

        foreach (GridObject gridObject in gridObjects) {
            gridObject.canBeBuiltOn = true;
        }

        if (neighbors != null) {
            foreach (Plot neighbor in neighbors) {
                if (neighbor.state == State.NotUnlocked) {
                    neighbor.state = State.CanBePurchased;
                }
            }
        }


        if (transform.TryGetComponent(out BoxCollider boxCollider)) {
            boxCollider.enabled = false;
        }

        OnPlotPurchased?.Invoke(this);

        GameInput.Instance.OnLeftMouseClicked -= GameInput_OnLeftMouseClicked;
    }

    private void OnDestroy() {
        OnPlotPurchased -= Plot_OnPlotPurchased;
        GameInput.Instance.OnLeftMouseClicked -= GameInput_OnLeftMouseClicked;
    }

    private bool IsMouseOverUI() {
        return EventSystem.current.IsPointerOverGameObject();
    }


}
