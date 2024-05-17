using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Storage : MonoBehaviour, ICanBePutDownIn, ICanBeGrabbedFrom, IHasInventory, ICanBeClicked {

    [SerializeField] private LayerMask itemLayer;

    private PlacedObjectTypeSO storageBuildingType;
    private Transform raycastPoint;
    private bool canSendItem;

    private Inventory inventory;

    public event Action OnInventoryChanged;

    private void Start() {
        storageBuildingType = GetComponent<BuildingTypeHolder>().buildingType;
        inventory = new Inventory(storageBuildingType.storageData.inventorySlots);

        inventory.OnInventoryChanged += () => OnInventoryChanged?.Invoke();

        canSendItem = true;
        raycastPoint = transform.Find("RaycastPoint");

        

    }

    private void Update() {
        
        HandleSendingItemsToConveyer();
        
    }

    private void HandleSendingItemsToConveyer() {
        if (inventory.GetNextItemToOutput() != null && canSendItem) {
            Vector3 nextGridPosition = raycastPoint.position + raycastPoint.forward;
            PlacedObject_Done nextGridObject = PlacedBuildingManager.Instance.GetPlacedObjectInCell(nextGridPosition);
            if (nextGridObject == null) { return; }
            if (!nextGridObject.TryGetComponent(out ConveyerBelt conveyorBelt)) return;
            bool objectInfront = false;

            float radius = 0.4f;
            float maxDistance = 0.8f;
            if (Physics.SphereCast(raycastPoint.position, radius, raycastPoint.forward, out RaycastHit hit, maxDistance, itemLayer)) {
                objectInfront = true;
            }

            if (!objectInfront) {
                Vector3 spawnPosition = nextGridObject.transform.Find("TurnPoint").position - transform.forward;
                ItemObject outputItem = inventory.GetNextItemToOutput();
                if (inventory.TryRemoveItemObject(outputItem)) {
                    conveyorBelt.AddItem(outputItem, spawnPosition);

                    canSendItem = false;

                    StorageManager.Instance.RemoveItemSOFromGlobalInventory(outputItem.GetItemSO());
                    StartCoroutine(ResetSendItem());
                }

            }


        }
    }

    public bool TryAddItemObjectToInventory(ItemObject itemObject) {
        if (inventory.TryAddItemObject(itemObject)) {
            StorageManager.Instance.AddItemSOToGlobalInventory(itemObject.GetItemSO(), 1);
            itemObject.gameObject.SetActive(false);
            return true;
        }
        return false;
       
    }

    private IEnumerator ResetSendItem() {
        yield return new WaitForSeconds(2f);
        canSendItem = true;
    }

    public bool TryPutDownObject(ItemObject itemObject) {
        if (inventory.IsMaxInventorySlotsReached() && inventory.GetInventorySlotWithSpaceLeft(itemObject.GetItemSO(), 1) == null) return false;

        TryAddItemObjectToInventory(itemObject);
        return true;
    }

    public void GrabObject(ItemObject itemObject) {
        inventory.TryRemoveItemObject(itemObject);
        StorageManager.Instance.RemoveItemSOFromGlobalInventory(itemObject.GetItemSO());
    }

    public ItemObject GetPotentialObject() {
        return inventory.GetNextItemToOutput();
    }

    public Inventory GetInventory() {
        return inventory;
    }

    public List<Inventory> GetInventories() {
        return new List<Inventory> { inventory };
    }

    public void OnClick() {
        LocalStorageUI.Instance.Show(this);
    }
}
