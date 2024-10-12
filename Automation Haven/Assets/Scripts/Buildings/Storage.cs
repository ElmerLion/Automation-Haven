using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour, ICanBePutDownIn, ICanBeGrabbedFrom, IHasInventory, ICanBeClicked {

    [SerializeField] private LayerMask itemLayer;

    private PlacedObjectTypeSO storageBuildingType;
    private Transform raycastPoint;
    private bool canSendItem;
    private InventoryMonoBehaviour inventoryMonoBehaviour;

    private Inventory storageInventory => inventoryMonoBehaviour.storageInventory;

    public event Action OnInventoryChanged;

    private void Awake() {
        inventoryMonoBehaviour = GetComponent<InventoryMonoBehaviour>();
    }

    private void Start() {
        storageBuildingType = GetComponent<BuildingTypeHolder>().buildingType;

        storageInventory.OnInventoryChanged += () => OnInventoryChanged?.Invoke();

        canSendItem = true;
        raycastPoint = transform.Find("RaycastPoint");

        StorageManager.Instance.ConnectInventory(storageInventory);
    }

    private void Update() {
        
        HandleSendingItemsToConveyer();
        
    }

    private void HandleSendingItemsToConveyer() {
        if (storageInventory.GetNextItemToOutput() != null && canSendItem) {
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
                ItemObject outputItem = storageInventory.GetNextItemToOutput();
                if (storageInventory.TryRemoveItemObject(outputItem)) {
                    conveyorBelt.AddItem(outputItem, spawnPosition);

                    canSendItem = false;

                    StartCoroutine(ResetSendItem());
                }

            }


        }
    }

    public bool TryAddItemObjectToInventory(ItemObject itemObject) {
        if (storageInventory.TryAddItemObject(itemObject)) {
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
        if (storageInventory.IsMaxInventorySlotsReached() && storageInventory.GetInventorySlotWithSpaceLeft(itemObject.GetItemSO(), 1) == null) return false;

        TryAddItemObjectToInventory(itemObject);
        return true;
    }

    public void GrabObject(ItemObject itemObject) {
        storageInventory.TryRemoveItemObject(itemObject);
    }

    public ItemObject GetPotentialObject() {
        return storageInventory.GetNextItemToOutput();
    }

    public Inventory GetInventory() {
        return storageInventory;
    }

    public List<Inventory> GetInventories() {
        return new List<Inventory> { storageInventory };
    }

    public void OnClick() {
        LocalStorageUI.Instance.Show(this);
    }

    private void OnDestroy() {
        StorageManager.Instance.DisconnectInventory(storageInventory);
    }
}
