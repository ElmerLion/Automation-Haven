using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Inventory {

    public event Action OnInventoryChanged;

    private List<InventorySlot> activeInventorySlots = new List<InventorySlot>();
    private List<InventorySlot> inactiveInventorySlots = new List<InventorySlot>();

    private List<InventoryItemUI> inventoryItemUIs = new List<InventoryItemUI>();

    private int maxInventorySlots;
    private int slotsPerItem;
    private int currentId;

    public Inventory(int maxInventorySlots, int slotsPerItem = 0) {
        this.maxInventorySlots = maxInventorySlots;
        this.slotsPerItem = slotsPerItem;
    }

    public bool TryAddItemObject(ItemObject itemObject) {
        ItemSO itemSO = itemObject.GetItemSO();
        int amount = 1;

        InventorySlot inventorySlot = GetInventorySlotWithSpaceLeft(itemSO, amount);

        if (inventorySlot != null && activeInventorySlots.Contains(inventorySlot)) {
            inventorySlot.amount++;
            inventorySlot.itemObjectList.Add(itemObject);
            OnInventoryChanged?.Invoke();
            return true;
        }

        if (activeInventorySlots.Count < maxInventorySlots) {

            if (inactiveInventorySlots.Contains(inventorySlot)) {
                inactiveInventorySlots.Remove(inventorySlot);
                activeInventorySlots.Add(inventorySlot);

                inventorySlot.amount = amount;
                inventorySlot.itemObjectList.Add(itemObject);
            } else {
                if (slotsPerItem > 0 && GetInventorySlotsForItem(itemSO).Count >= slotsPerItem) return false;

                InventorySlot newInventorySlot = new InventorySlot(itemSO, 1, itemSO.stackSize, currentId++);
                newInventorySlot.itemObjectList.Add(itemObject);
                activeInventorySlots.Add(newInventorySlot);
            }
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool TryAddItem(ItemSO itemSO, int amount = 1, List<ItemObject> itemObjects = null) {
        InventorySlot inventorySlot = GetInventorySlotWithSpaceLeft(itemSO, amount);

        if (itemObjects == null) {
            itemObjects = new List<ItemObject>();
        }

        if (inventorySlot != null && activeInventorySlots.Contains(inventorySlot)) {
            if (inventorySlot.amount + amount <= inventorySlot.maxAmount) {
                inventorySlot.amount += amount;

                foreach (ItemObject itemObject in itemObjects) {
                    if (itemObject == null) continue;
                    inventorySlot.itemObjectList.Add(itemObject);
                }
                OnInventoryChanged?.Invoke();
                return true;
            } 
        }

        if (activeInventorySlots.Count < maxInventorySlots) {

            if (inactiveInventorySlots.Contains(inventorySlot)) {
                inactiveInventorySlots.Remove(inventorySlot);
                activeInventorySlots.Add(inventorySlot);

                inventorySlot.amount = amount;

                foreach (ItemObject itemObject in itemObjects) {
                    if (itemObject == null) continue;
                    inventorySlot.itemObjectList.Add(itemObject);
                }
            } else {
                if (slotsPerItem > 0 && GetInventorySlotsForItem(itemSO).Count >= slotsPerItem) return false;

                InventorySlot newInventorySlot = new InventorySlot(itemSO, amount, itemSO.stackSize, currentId++);

                foreach (ItemObject itemObject in itemObjects) {
                    if (itemObject == null) continue;
                    newInventorySlot.itemObjectList.Add(itemObject);
                }

                activeInventorySlots.Add(newInventorySlot);
            }
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    private void LogCurrentInventory() {
        foreach (InventorySlot inventorySlot in activeInventorySlots) {
            Debug.Log(inventorySlot.itemSO.name + " " + inventorySlot.amount);
        }
        Debug.Log("InventoryItemUI Count: " + inventoryItemUIs.Count);
        Debug.Log("Active Inventory Slot Count: " + activeInventorySlots.Count);
        Debug.Log("Inactive Inventory Slot Count: " + inactiveInventorySlots.Count);
    }

    public bool TryAddItem(ItemAmount itemAmount, List<ItemObject> itemObjects = null) {
        ItemSO itemSO = itemAmount.itemSO;
        int amount = itemAmount.amount;

        InventorySlot inventorySlot = GetInventorySlotWithSpaceLeft(itemSO, amount);

        if (inventorySlot != null && activeInventorySlots.Contains(inventorySlot)) {
            if (inventorySlot.amount + amount <= inventorySlot.maxAmount) {
                inventorySlot.amount += amount;

                foreach (ItemObject itemObject in itemObjects) {
                    if (itemObject == null) continue;
                    inventorySlot.itemObjectList.Add(itemObject);
                }
                OnInventoryChanged?.Invoke();
                return true;
            }
        } 

        if (activeInventorySlots.Count < maxInventorySlots) {

            if (inactiveInventorySlots.Contains(inventorySlot)) {
                inactiveInventorySlots.Remove(inventorySlot);
                activeInventorySlots.Add(inventorySlot);

                inventorySlot.amount = amount;

                foreach (ItemObject itemObject in itemObjects) {
                    if (itemObject == null) continue;
                    inventorySlot.itemObjectList.Add(itemObject);
                }
            } else {
                if (slotsPerItem > 0 && GetInventorySlotsForItem(itemSO).Count >= slotsPerItem) return false;

                InventorySlot newInventorySlot = new InventorySlot(itemSO, amount, itemSO.stackSize, currentId++);

                foreach (ItemObject itemObject in itemObjects) {
                    if (itemObject == null) continue;
                    newInventorySlot.itemObjectList.Add(itemObject);
                }

                activeInventorySlots.Add(newInventorySlot);
            }
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public bool TryRemoveItem(ItemSO itemSO, int amount, ItemObject itemObject = null) {
        int leftToRemove = amount;
        List<InventorySlot> inventorySlotsCopy = GetInventorySlotsForItem(itemSO);

        foreach (InventorySlot inventorySlot in inventorySlotsCopy) {
            if (inventorySlot != null) {
                int toRemove = Math.Min(leftToRemove, inventorySlot.amount); // Determine the actual amount to remove from this slot
                inventorySlot.amount -= toRemove;
                leftToRemove -= toRemove;

                if (itemObject != null && inventorySlot.itemObjectList.Contains(itemObject)) {
                    inventorySlot.itemObjectList.Remove(itemObject);
                }

                if (itemObject == null) {
                    for (int i = 0; i < amount; i++) {
                        if (inventorySlot.itemObjectList.Count <= 0) break;
                        inventorySlot.itemObjectList.RemoveAt(0);
                    }
                }

                if (inventorySlot.amount <= 0) {
                    activeInventorySlots.Remove(inventorySlot);
                    inactiveInventorySlots.Add(inventorySlot);
                }

                if (leftToRemove <= 0) break;
                
            }
        }

        OnInventoryChanged?.Invoke();
        return leftToRemove == 0;
    }

    public bool TryRemoveItem(ItemAmount itemAmount, ItemObject itemObject = null) {

        ItemSO itemSO = itemAmount.itemSO;
        int amount = itemAmount.amount;

        int leftToRemove = amount;
        List<InventorySlot> inventorySlotsCopy = GetInventorySlotsForItem(itemSO);

        foreach (InventorySlot inventorySlot in inventorySlotsCopy) {
            if (inventorySlot != null) {
                int toRemove = Math.Min(leftToRemove, inventorySlot.amount); 
                inventorySlot.amount -= toRemove;
                leftToRemove -= toRemove;

                if (itemObject != null && inventorySlot.itemObjectList.Contains(itemObject)) {
                    inventorySlot.itemObjectList.Remove(itemObject);
                }

                if (itemObject == null) {
                    for (int i = 0; i < amount; i++) {
                        if (inventorySlot.itemObjectList.Count <= 0) break;
                        inventorySlot.itemObjectList.RemoveAt(0);
                    }
                }

                if (inventorySlot.amount <= 0) {
                    activeInventorySlots.Remove(inventorySlot);
                    inactiveInventorySlots.Add(inventorySlot);
                }
            }
        }
        OnInventoryChanged?.Invoke();
        return leftToRemove == 0;
    }

    public bool TryRemoveItemObject(ItemObject itemObject) {
        ItemSO itemSO = itemObject.GetItemSO();

        foreach (InventorySlot inventorySlot in GetInventorySlotsForItem(itemSO)) {
            if (inventorySlot.itemObjectList.Contains(itemObject)) {
                inventorySlot.itemObjectList.Remove(itemObject);
                inventorySlot.amount--;
                if (inventorySlot.amount <= 0) {
                    activeInventorySlots.Remove(inventorySlot);
                    inactiveInventorySlots.Add(inventorySlot);
                }
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public bool TryMoveItemToOtherInventory(ItemAmount itemAmount, Inventory otherInventory) {
        ItemSO itemSO = itemAmount.itemSO;
        int amount = itemAmount.amount;

        if (TryRemoveItem(itemSO, amount)) {
            otherInventory.TryAddItem(itemSO, amount);
            return true;
        }
        return false;
    }

    public ItemObject GetNextItemToOutput() {
        /*foreach (InventorySlot inventorySlot in activeInventorySlots) {
            if (inventorySlot.itemObjectList.Count > 0) {
                ItemObject itemObject = inventorySlot.itemObjectList[0];
                return itemObject;
            }
        }*/

        if (activeInventorySlots.Count == 0) return null;
        if (activeInventorySlots[activeInventorySlots.Count - 1].itemObjectList.Count == 0) return null;

        return activeInventorySlots[activeInventorySlots.Count - 1].itemObjectList[0];
    }

    public List<ItemAmount> GetInventorySlotItemAmounts() {
        List<ItemAmount> itemAmounts = new List<ItemAmount>();
        foreach (InventorySlot inventorySlot in activeInventorySlots) {
            itemAmounts.Add(new ItemAmount(inventorySlot.itemSO, inventorySlot.amount));
        }
        return itemAmounts;
    }

    public bool IsSpaceAvailableForItemSO(ItemSO itemSO) {
        foreach (InventorySlot inventorySlot in GetInventorySlotsForItem(itemSO)) {
            if (inventorySlot == null) continue;

            if (inventorySlot.amount < inventorySlot.maxAmount) return true;
        }

        return !IsMaxInventorySlotsReached();
    }

    public bool IsInventoryAvailableForItemAmount(ItemAmount itemAmount) {
        ItemSO itemSO = itemAmount.itemSO;
        int amount = itemAmount.amount;

        foreach (InventorySlot inventorySlot in GetInventorySlotsForItem(itemSO)) {
            if (inventorySlot == null) continue;

            if (inventorySlot.amount + amount < inventorySlot.maxAmount) return true;
        }
        return !IsMaxInventorySlotsReached();
    }

    public bool IsInventoryFull() {
        foreach (InventorySlot inventorySlot in activeInventorySlots) {
            if (inventorySlot.amount < inventorySlot.maxAmount) return false;
        }
        return true;
    }

    public List<InventorySlot> GetInventorySlotsForItem(ItemSO itemSO) {
        List<InventorySlot> itemInventorySlots = new List<InventorySlot>();
        foreach (InventorySlot activeInventorySlot in activeInventorySlots) {
            if (activeInventorySlot.itemSO == itemSO) {
                itemInventorySlots.Add(activeInventorySlot);
            }
        }

        foreach (InventorySlot inactiveInventorySlot in inactiveInventorySlots) {
            if (inactiveInventorySlot.itemSO == itemSO) {
                itemInventorySlots.Add(inactiveInventorySlot);
            }
        }

        return itemInventorySlots;
    }

    public bool IsInventoryAvailableForItemList(List<ItemAmount> itemAmounts) {
        foreach (ItemAmount itemAmount in itemAmounts) {
            if (IsInventoryAvailableForItemAmount(itemAmount)) return true;
        }
        return false;
    }


    public bool IsMaxInventorySlotsReached() {
        return activeInventorySlots.Count >= maxInventorySlots;
    }

    public List<InventorySlot> GetActiveInventorySlots() {
        return activeInventorySlots;
    }

    public List<InventorySlot> GetInactiveInventorySlots() {
        return inactiveInventorySlots;
    }

    public List<InventoryItemUI> GetInventoryItemsUI() {
        return inventoryItemUIs;
    }

    public int GetMaxInventorySlots() {
        return maxInventorySlots;
    }



    public List<InventorySlot> GetInventorySlots() {
        List<InventorySlot> inventorySlots = new List<InventorySlot>();
        inventorySlots.AddRange(activeInventorySlots);
        inventorySlots.AddRange(inactiveInventorySlots);
        return inventorySlots;
    }

    public void ActivateInventorySlot(InventorySlot inventorySlot) {
        if (inactiveInventorySlots.Contains(inventorySlot)) {
            inactiveInventorySlots.Remove(inventorySlot);
            activeInventorySlots.Add(inventorySlot);
        }
        OnInventoryChanged?.Invoke();
    }

    public void AddInventoryItemUI(InventoryItemUI inventoryItemUI) {
        inventoryItemUIs.Add(inventoryItemUI);
    }

    public InventorySlot GetInventorySlotWithSpaceLeft(ItemSO itemSO, int amountToAdd) {
        foreach (InventorySlot activeInventorySlot in activeInventorySlots) {
            if (activeInventorySlot.itemSO == itemSO && activeInventorySlot.amount < activeInventorySlot.maxAmount - amountToAdd) {
                return activeInventorySlot;
            }
        }
        
        foreach (InventorySlot inactiveInventorySlot in inactiveInventorySlots) {
            if (inactiveInventorySlot.itemSO == itemSO && inactiveInventorySlot.amount < inactiveInventorySlot.maxAmount - amountToAdd) {
                return inactiveInventorySlot;
            }
        }
        
        return null;
    }

    public class InventorySlot {
        public int id;
        public ItemSO itemSO;
        public int amount;
        public int maxAmount;
        public List<ItemObject> itemObjectList;
        public InventoryItemUI inventoryItemUI;

        public InventorySlot(ItemSO itemSO, int amount, int maxAmount, int id) {
            this.itemSO = itemSO;
            this.amount = amount;
            this.maxAmount = maxAmount;
            this.id = id;
            itemObjectList = new List<ItemObject>();
        }

       
    }



}
