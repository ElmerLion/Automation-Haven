using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoxPackager : MonoBehaviour, ICanBeClicked, IHasInventory {

    public event Action OnInputInventoryChanged;
    public event Action OnOutputInventoryChanged;

    public enum Mode {
        Packaging,
        Unpackaging
    }

    [SerializeField] private LayerMask itemLayer;

    private Inventory inputInventory;
    private Inventory outputInventory;

    private Transform outputRaycastPointTransform;
    private int weightPerBox = 10;
    private bool isOutputting = false;

    private Mode mode;


    private void Start() {
        isOutputting = false;
        
        OnInputInventoryChanged += BoxPackager_OnInputInventoryChanged;
        OnOutputInventoryChanged += OutputFirstItemObject;

        outputRaycastPointTransform = transform.Find("OutputPoint");
        mode = Mode.Packaging;

        inputInventory = new Inventory(10);
        outputInventory = new Inventory(10);

        inputInventory.OnInventoryChanged += () => OnInputInventoryChanged?.Invoke();
        outputInventory.OnInventoryChanged += () => OnOutputInventoryChanged?.Invoke();
    }

    private void BoxPackager_OnInputInventoryChanged() {
        if (mode == Mode.Packaging) {
            CreateNewBox();
        }
        if (mode == Mode.Unpackaging) {
            TryUnpackageInputBoxes();
        }
    }

    private void Update() {
        if (outputInventory.GetActiveInventorySlots().Count > 0) {
            OutputFirstItemObject();
        }
        if (inputInventory.GetActiveInventorySlots().Count > 0) {
            if (mode == Mode.Packaging) {
                CreateNewBox();
            }
            if (mode == Mode.Unpackaging) {
                TryUnpackageInputBoxes();
            }
        }
    }

    private bool TryCreateNewBox(out List<ItemAmount> inputItemAmountList, out List<ItemObject> inputItemObjects) {
        inputItemAmountList = new List<ItemAmount>();
        inputItemObjects = new List<ItemObject>();

        float totaltWeight = 0f;
        foreach (Inventory.InventorySlot inventorySlot in inputInventory.GetActiveInventorySlots()) {
            foreach (ItemObject itemObject in inventorySlot.itemObjectList) {
                float potentialWeight = totaltWeight + itemObject.GetItemSO().weight;

                // Check if adding this item exceeds the weight limit
                if (potentialWeight > weightPerBox) {
                    // If weight limit is exceeded, we're done adding items
                    return inputItemAmountList.Count > 0;
                }

                // Add this item to the new box
                totaltWeight += itemObject.GetItemSO().weight;
                inputItemObjects.Add(itemObject);

                // Update the list of item amounts to reflect the items added to the new box
                ItemAmount itemInItemAmountList = ItemAmount.GetItemSOInItemAmountList(itemObject.GetItemSO(), inputItemAmountList);
                if (itemInItemAmountList != null) {
                    itemInItemAmountList.amount++;
                } else {
                    inputItemAmountList.Add(new ItemAmount(itemObject.GetItemSO(), 1));
                }

                // If we reach the weight limit, we're done adding items
                if (totaltWeight >= weightPerBox) {
                    return true;
                }
            }
        }


        return false;
    }

    private void CreateNewBox() {
        if (mode == Mode.Unpackaging) return;
        if (TryCreateNewBox(out List<ItemAmount> inputItemAmountList, out List<ItemObject> inputItemObjects)) {

            Transform prefabToSpawn = Resources.Load<Transform>("DefaultBox");
            Transform boxTransform = Instantiate(prefabToSpawn, transform);
            boxTransform.gameObject.SetActive(false);

            Box box = boxTransform.GetComponent<Box>();
            box.itemAmountList = inputItemAmountList;
            box.itemObjects = inputItemObjects;

            ItemObject boxItemObject = boxTransform.GetComponent<ItemObject>();

            if (outputInventory.TryAddItemObject(boxItemObject)) {
                foreach (ItemObject inputItemObject in inputItemObjects) {
                    inputInventory.TryRemoveItemObject(inputItemObject);
                }
            } else {
                Destroy(boxTransform.gameObject);
            }


        }
        OutputFirstItemObject();
    }

    private void OutputFirstItemObject() {
        if (isOutputting) return;  // Exit if an output operation is already in progress

        Vector3 outputPosition = outputRaycastPointTransform.position + outputRaycastPointTransform.forward;
        PlacedObject_Done placedObject = PlacedBuildingManager.Instance.GetPlacedObjectInCell(outputPosition);

        if (PlacedBuildingManager.Instance.IsNextObjectConveyorBelt(placedObject)) {
            ConveyerBelt conveyerBelt = placedObject.transform.GetComponent<ConveyerBelt>();

            bool objectInfront = Physics.SphereCast(outputRaycastPointTransform.position, 0.5f, outputRaycastPointTransform.forward, out _, 0.8f, itemLayer);

            if (!objectInfront && conveyerBelt.GetItems().Count < 2) {
                isOutputting = true;  

                ItemObject itemObject = outputInventory.GetNextItemToOutput();
                if (itemObject == null) {
                    isOutputting = false;  
                    return;
                }

                if (conveyerBelt.TryPutDownObject(itemObject)) {
                    outputInventory.TryRemoveItemObject(itemObject);
                }

                isOutputting = false;  
            }
        }
    }

    public bool TryAddItemToInputInventory(ItemObject itemObject) {

        ItemSO itemSO = itemObject.GetItemSO();

        if (inputInventory.TryAddItemObject(itemObject)) {
            itemObject.gameObject.SetActive(false);
            itemObject.transform.parent = transform;
            //OnInventoryChanged?.Invoke();
        }

        return true;
    }

    private void TryUnpackageInputBoxes() {
        if (inputInventory.GetActiveInventorySlots().Count == 0) return;

        List<ItemObject> boxesToUnpackage = new List<ItemObject>();

        // Collect all boxes to unpackage
        foreach (Inventory.InventorySlot inventorySlot in inputInventory.GetActiveInventorySlots()) {
            foreach (ItemObject boxItemObject in inventorySlot.itemObjectList) {
                Box box = boxItemObject.GetComponent<Box>();
                if (box != null) {
                    boxesToUnpackage.Add(boxItemObject);
                }
            }
        }

        // Process the collected boxes
        foreach (ItemObject boxItemObject in boxesToUnpackage) {
            Box box = boxItemObject.GetComponent<Box>();

            if (box != null && outputInventory.IsInventoryAvailableForItemList(box.itemAmountList)) {
                if (inputInventory.TryRemoveItemObject(box.GetComponent<ItemObject>())) {
                    foreach (ItemObject itemObjectInBox in box.itemObjects) {
                        outputInventory.TryAddItemObject(itemObjectInBox);
                    }
                }
            }
        }
    }

    public List<ItemAmount> GetInventoryItems() {
        return inputInventory.GetInventorySlotItemAmounts();
    }

    public Mode GetMode() {
        return mode;
    }

    public List<Inventory> GetInventories() {
        return new List<Inventory> { inputInventory, outputInventory };
    }

    public Inventory GetOutputInventory() {
        return outputInventory;
    }

    public Inventory GetInputInventory() {
        return inputInventory;
    }

    public void OnClick() {
        PackagerUI.Instance.Show(this);
    }

    public void ToggleMode() {
        if (mode == Mode.Packaging) {
            mode = Mode.Unpackaging;
            TryUnpackageInputBoxes();
        } else {
            mode = Mode.Packaging;
            CreateNewBox();
        }
    }
}
