using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class StorageManager : MonoBehaviour {

    public static StorageManager Instance { get; private set; }

    public event EventHandler OnGlobalStorageUpdated;

    [SerializeField] private List<ItemAmount> startingItems;
    private List<ItemAmount> allItemAmounts;
    private List<Inventory> connectedInventories;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
    }

    public void AddItemAmountToGlobalInventory(ItemAmount itemAmountInput) {
        ItemAmount existingItem = allItemAmounts.Find(item => item.itemSO == itemAmountInput.itemSO);

        if (existingItem != null) {
            existingItem.amount += itemAmountInput.amount;
        } else {
            allItemAmounts.Add(new ItemAmount(itemAmountInput.itemSO, itemAmountInput.amount));
        }

        OnGlobalStorageUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void AddItemSOToGlobalInventory(ItemSO itemSOInput, int amount = 1) {
        ItemAmount existingItem = allItemAmounts.Find(item => item.itemSO == itemSOInput);

        if (existingItem != null) {
            existingItem.amount += amount;
        } else {
            allItemAmounts.Add(new ItemAmount(itemSOInput, amount));
        }
        OnGlobalStorageUpdated?.Invoke(this, EventArgs.Empty);
        
    }

    public void RemoveItemSOFromGlobalInventory(ItemSO itemSOInput, int amount = 1) {
        ItemAmount existingItem = allItemAmounts.Find(item => item.itemSO == itemSOInput);

        if (existingItem != null) {
            existingItem.amount -= amount;
            if (existingItem.amount <= 0) {
                allItemAmounts.Remove(existingItem);
            }
        } else {
            Debug.LogError("Trying to remove item that doesn't exist: " + itemSOInput.name);
        }
        OnGlobalStorageUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItemAmountFromGlobalInventory(ItemAmount itemAmountInput) {
        for (int i = 0; i < itemAmountInput.amount; i++) {
            RemoveItemSOFromGlobalInventory(itemAmountInput.itemSO);
        }
    }

    public List<ItemAmount> GetAllItemAmounts() {
        return allItemAmounts;
    }

    public bool CanAffordItems(List<ItemAmount> inputItemAmounts) {
        foreach (ItemAmount neededItemAmount in inputItemAmounts) {
            if (GetTotalItemAmount(neededItemAmount.itemSO) < neededItemAmount.amount) {
                return false;
            }
        }
        return true;
    }

    public int GetTotalItemAmount(ItemSO itemSO) {
        ItemAmount existingItem = allItemAmounts.Find(item => item.itemSO == itemSO);
        if (existingItem != null) {
            return existingItem.amount;
        }
        return 0;
    }

    public void ConnectInventory(Inventory inventory) {
        if (connectedInventories == null) {
            connectedInventories = new List<Inventory>();
        }

        if (connectedInventories.Contains(inventory)) return;

        connectedInventories.Add(inventory);

        foreach (ItemAmount itemAmount in inventory.GetInventorySlotItemAmounts()) {
            AddItemAmountToGlobalInventory(itemAmount);
        }

        inventory.OnItemAdded += Inventory_OnItemAdded;
        inventory.OnItemRemoved += Inventory_OnItemRemoved;


    }

    private void Inventory_OnItemRemoved(ItemObject itemObject) {
        RemoveItemSOFromGlobalInventory(itemObject.GetItemSO());
    }

    private void Inventory_OnItemAdded(ItemObject itemObject) {
        AddItemSOToGlobalInventory(itemObject.GetItemSO());
    }

    public void DisconnectInventory(Inventory inventory) {
        if (connectedInventories.Contains(inventory) == false) return;

        connectedInventories.Remove(inventory);

        foreach (ItemAmount itemAmount in inventory.GetInventorySlotItemAmounts()) {
            RemoveItemAmountFromGlobalInventory(itemAmount);
        }

        inventory.OnItemAdded -= Inventory_OnItemAdded;
        inventory.OnItemRemoved -= Inventory_OnItemRemoved;
    }

    private void SaveManager_OnGameSaved(string filePath) {
        ES3.Save("globalStorage", allItemAmounts, filePath);
        ES3.Save("globalConnectedInventories", connectedInventories, filePath);
    }

    private void SaveManager_OnGameLoaded(string filePath) {
        allItemAmounts = ES3.Load("globalStorage", filePath, new List<ItemAmount>());
        connectedInventories = ES3.Load("globalConnectedInventories", filePath, new List<Inventory>());

        if (allItemAmounts.Count <= 0) {
            foreach (ItemAmount itemAmount in startingItems) {
                AddItemAmountToGlobalInventory(itemAmount);
            }
        }

        OnGlobalStorageUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded -= SaveManager_OnGameLoaded;
    }

}

