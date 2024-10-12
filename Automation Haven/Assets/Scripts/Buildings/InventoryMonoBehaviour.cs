using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMonoBehaviour : MonoBehaviour {

    [SerializeField] private List<InventoryData> inventorySettings;

    [HideInInspector] public List<Inventory> inventories { get; private set; }

    public Inventory inputInventory { get; private set; }
    public Inventory outputInventory { get; private set; }
    public Inventory storageInventory { get; private set; }

    private void Start() {
        if (inventories == null) {
            inventories = new List<Inventory>();
        }

        if (inventorySettings.Count == inventories.Count)  return; 

        foreach (InventoryData inventoryData in inventorySettings) {
            Inventory inventory = new Inventory(inventoryData.inventorySlots, inventoryData.type, inventoryData.slotsPerItem);

            if (inventoryData.type == Inventory.InventoryType.Input) {
                inputInventory = inventory;
            } else if (inventoryData.type == Inventory.InventoryType.Output) {
                outputInventory = inventory;
            } else if (inventoryData.type == Inventory.InventoryType.Storage) {
                storageInventory = inventory;
            }

            inventories.Add(inventory);
        }
    }

    public Inventory GetInventory(Inventory.InventoryType type) {
        foreach (Inventory inventory in inventories) {
            if (inventory.type == type) {
                return inventory;
            }
        }
        return null;
    }

    [System.Serializable]
    public class InventoryData {
        public Inventory.InventoryType type;
        public int inventorySlots;
        public int slotsPerItem;

    }
    
}
