using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CraftingMachine : MonoBehaviour, IHasProgress, ICanBeGrabbedFrom, ICanBePutDownIn, ICanBeClicked, IHasInventory {


    public event Action OnInputInventoryChanged;
    public event Action OnOutputInventoryChanged;
    public event Action OnProgressChanged;
    public event EventHandler OnActiveRecipeNull;
    public event EventHandler OnActiveRecipeChanged;

    
    public enum Type {
        CraftingMachine,
        Furnace,
        Mixer,
        Refinery,
        WeaponAssembler,
        FurnitureAssembler,
    }

    private Inventory inputInventory => inventoryMonoBehaviour.inputInventory;
    private Inventory outputInventory => inventoryMonoBehaviour.outputInventory;

    private RecipeSO activeRecipeSO;
    private bool isCrafting;
    public PowerReciever powerReciever;
    private InventoryMonoBehaviour inventoryMonoBehaviour;

    private float craftingTimer;
    private float maxCraftingTime;
    private float craftingTimeMultiplier;

    private void Awake() {
        inventoryMonoBehaviour = transform.GetComponent<InventoryMonoBehaviour>();
    }

    private void Start() {

        if (powerReciever == null) {
            powerReciever = GetComponent<PowerReciever>();
        }
        if (activeRecipeSO == null) {
            SetActiveRecipe(null);
        }

        inputInventory.OnInventoryChanged += () => OnInputInventoryChanged?.Invoke();
        outputInventory.OnInventoryChanged += () => OnOutputInventoryChanged?.Invoke();
    }

    private void Update() {
        if (IsStorageAvailableForActiveRecipe() && activeRecipeSO != null) {
            TryCrafting();
        }

        if (isCrafting && IsStorageAvailableForActiveRecipe()) {
            craftingTimer += Time.deltaTime;
            OnProgressChanged?.Invoke();
            if (craftingTimer >= maxCraftingTime && IsStorageAvailableForActiveRecipe()) {
                craftingTimer = 0;
                OnProgressChanged?.Invoke();
                isCrafting = false;
                CraftRecipe();
            }
        }
    }

    public void CheckActiveRecipeSOStatus() {
        if (activeRecipeSO == null) {
            OnActiveRecipeNull?.Invoke(this, EventArgs.Empty);
            Debug.Log("Active recipe is null");
        } 
    }

    private bool TryCrafting() {
        if (!powerReciever.IsPowerAvailable()) { return false; }

        bool allItemsMatch = true;
        foreach (ItemAmount recipeItemAmount in activeRecipeSO.input) {
            bool itemFound = false;

            foreach (Inventory.InventorySlot inputInventorySlot in inputInventory.GetActiveInventorySlots()) {
                
                 if (inputInventorySlot.itemSO == recipeItemAmount.itemSO && inputInventorySlot.amount >= recipeItemAmount.amount) {
                     itemFound = true;
                     break;
                 }
                

            }
            if (!itemFound) {
                allItemsMatch = false;
                break;
            }
        }

        if (allItemsMatch && !isCrafting) {
            isCrafting = true;
            return true;
        } 
        return false;
    }


    private void CraftRecipe() {
        if (!outputInventory.IsInventoryAvailableForItemList(activeRecipeSO.output)) return;
        if (!TryCrafting() ) { return; }
        isCrafting = false;

        foreach (ItemAmount recipeItem in activeRecipeSO.input) {
            inputInventory.TryRemoveItem(recipeItem);
        }

        foreach (ItemAmount outputItem in activeRecipeSO.output) {
            List<ItemObject> itemObjects = new List<ItemObject>();

            for (int i = 0; i < outputItem.amount; i++) {
                ItemObject itemObject = Instantiate(outputItem.itemSO.prefab, transform).GetComponent<ItemObject>();
                itemObject.gameObject.SetActive(false);
                itemObjects.Add(itemObject);
            }

            //Debug.Log("Adding item: " + outputItem.itemSO.nameString + " amount: " + outputItem.amount + " with " + itemObjects.Count + " itemObjects");
            foreach (ItemObject itemObject in itemObjects) {
                outputInventory.TryAddItemObject(itemObject);
            }
        }
        powerReciever.ConsumePower();
    }

    public bool TryAddItemToInputInventory(ItemObject itemObject) {

        ItemSO itemSO = itemObject.GetItemSO();
        if (!inputInventory.IsSpaceAvailableForItemSO(itemSO)) return false;

        if (inputInventory.TryAddItemObject(itemObject)) {
            itemObject.gameObject.SetActive(false);
            itemObject.transform.parent = transform;
            //OnInventoryChanged?.Invoke();
        }

        return true;
    }

    public bool IsItemMatchingRecipeItem(ItemSO itemSO) {
        if (activeRecipeSO == null) { return false; }

        foreach (ItemAmount itemAmount in activeRecipeSO.input) {
            if (itemAmount.itemSO == itemSO) {
                return true;
            }
        }
        return false;
    }

    public void SetActiveRecipe(RecipeSO recipeSO) {
        
        activeRecipeSO = recipeSO;

        if (activeRecipeSO == null) {
            OnActiveRecipeNull?.Invoke(this, EventArgs.Empty);
            return;
        }

        List<ItemAmount> inputInventoryItemAmounts = inputInventory.GetInventorySlotItemAmounts();

        foreach (ItemAmount inputItemAmount in inputInventoryItemAmounts) {
            if (outputInventory.TryAddItem(inputItemAmount)) {
                inputInventory.TryRemoveItem(inputItemAmount);
            }
        }
        
        CalculateCraftingTime();
        OnActiveRecipeChanged?.Invoke(this, EventArgs.Empty);

        Debug.Log("Active Recipe Changed to: " + activeRecipeSO.name);

    }

    public bool IsStorageAvailableForActiveRecipe() {
        if (activeRecipeSO == null) return false;
        bool storageAvailable = true;

        foreach (ItemAmount itemAmount in activeRecipeSO.input) {
            if (!outputInventory.IsInventoryAvailableForItemAmount(itemAmount)) {
                storageAvailable = false;
                break;
            }
        }
        return storageAvailable;
    }

    public void AddCraftingTimeMultiplier(float multiplier) {
        craftingTimeMultiplier += multiplier;
        CalculateCraftingTime();
    }

    private float CalculateCraftingTime() {
        if (activeRecipeSO == null) return 0;

        maxCraftingTime = activeRecipeSO.craftingTime * (1 - craftingTimeMultiplier);
        OnProgressChanged?.Invoke();
        return maxCraftingTime;
    }

    public float GetProgressNormalized() {
        return craftingTimer / maxCraftingTime;
    }

    public float GetMaxProgress() {
        return maxCraftingTime;
    }

    public float GetRemainingTime() {
        return activeRecipeSO != null ? maxCraftingTime - craftingTimer : 0;
    }

    public RecipeSO GetActiveRecipeSO() {
        return activeRecipeSO;
    }

    public Inventory GetInputInventory() {
        return inputInventory;
    }

    public Inventory GetOutputInventory() {
        return outputInventory;
    }

    public List<Inventory> GetInventories() {
        return new List<Inventory> { inputInventory, outputInventory };
    }

    public void GrabObject(ItemObject itemObject) {
        outputInventory.TryRemoveItemObject(itemObject);
    }

    public ItemObject GetPotentialObject() {
        if (outputInventory == null) return null;

        ItemObject itemObject = outputInventory.GetNextItemToOutput();

        if (itemObject == null) return null;

        return itemObject;
    }

    public bool TryPutDownObject(ItemObject itemObject) {
        if (!inputInventory.IsSpaceAvailableForItemSO(itemObject.GetItemSO())) return false;

        if (!IsItemMatchingRecipeItem(itemObject.GetItemSO())) return false;

        return TryAddItemToInputInventory(itemObject);
    }

    public void OnClick() {
        CraftingMachineUI.Instance.Show(transform);
    }
}
