using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public static IHasInventory previousSender;

    public static void ResetStaticData() {
        previousSender = null;
    }

    public static bool IsInventorySlotInList(Inventory.InventorySlot inventorySlot, List<InventoryItemUI> inventoryItemUIList) {
        foreach (InventoryItemUI inventoryItemUI in inventoryItemUIList) {
            if (inventoryItemUI == inventorySlot.inventoryItemUI) return true;
        }
        return false;
    }
    
    public static InventoryItemUI GetInventoryItemUI(ItemSO itemSO, List<InventoryItemUI> inventoryItemUIList) {
        foreach (InventoryItemUI inventoryItemUI in inventoryItemUIList) {
            if (inventoryItemUI.itemAmount.itemSO == itemSO) return inventoryItemUI;
        }
        return null;
    }

    public static void UpdateInventoryItem(ItemAmount itemAmount, Transform parent, Transform inventoryItemTemplate, List<InventoryItemUI> inventoryItemUIList, bool isOutputItem) {

        /*if (!IsInventoryItemInList(itemAmount.itemSO, inventoryItemUIList)) {
            Transform inventoryItemTransform = Instantiate(inventoryItemTemplate, parent);

            InventoryItemUI inventoryItem = inventoryItemTransform.GetComponent<InventoryItemUI>();
            inventoryItem.InitializeItem(itemAmount.itemSO, itemAmount.amount, isOutputItem);


            inventoryItemUIList.Add(inventoryItem);
            inventoryItem.UpdateAmount(itemAmount.amount);
        }*/

        //InventoryItemUI inventoryItemUI = GetInventoryItemUI(inventorySlot, inventoryItemUIList);

        //if (inventoryItemUI == null) return;

        //inventoryItemUI.ChangeIsOutputItem(isOutputItem);

        //inventoryItemUI.UpdateAmount(itemAmount.amount);
    }

    public static void UpdateInventoryUI(IHasInventory sender, Inventory inventory, Transform parent, Transform inventoryItemTemplate) {
        if (previousSender != null && previousSender != sender) {
            foreach (Inventory previousInventory in previousSender.GetInventories()) {
                foreach (InventoryItemUI inventoryItemUI in previousInventory.GetInventoryItemsUI()) {
                    inventoryItemUI.gameObject.SetActive(false);
                }
            }
        }

        foreach (Inventory.InventorySlot inventorySlot in inventory.GetActiveInventorySlots()) {
            InventoryItemUI inventoryItemUI = inventorySlot.inventoryItemUI;

            if (inventoryItemUI != null && inventoryItemUI.GetId() == inventorySlot.id) {
                inventoryItemUI.UpdateAmount(inventorySlot.amount);
                continue;
            }


            Transform inventoryItemTransform = Instantiate(inventoryItemTemplate, parent);
            inventoryItemUI = inventoryItemTransform.GetComponent<InventoryItemUI>();
            inventorySlot.inventoryItemUI = inventoryItemUI;
            

            if (inventorySlot.itemObjectList.Count > 0) {
                inventoryItemUI.InitializeItem(inventorySlot.itemSO, inventorySlot.amount, false, inventorySlot.itemObjectList[0]);
            } else {
                inventoryItemUI.InitializeItem(inventorySlot.itemSO, inventorySlot.amount, false);
            }

            inventory.AddInventoryItemUI(inventoryItemUI);
            inventoryItemUI.SetId(inventorySlot.id);
            inventoryItemUI.UpdateAmount(inventorySlot.amount);
        }

        if (inventory.GetActiveInventorySlots().Count == 0) {
            foreach (InventoryItemUI inventoryItemUI in inventory.GetInventoryItemsUI()) {
                inventoryItemUI.gameObject.SetActive(false);
            }
        }

        previousSender = sender;
    }

    public ItemSO itemSO;
    public int amount;
    public ItemAmount itemAmount;
    private TextMeshProUGUI amountText;
    private bool isOutputItem;
    private ItemObject itemObject;
    private int inventorySlotId;

    public void InitializeItem(ItemSO itemSO, int amount, bool isOutputItem = false, ItemObject itemObject = null) {
        this.itemSO = itemSO;
        this.amount = amount;
        this.isOutputItem = isOutputItem;
        this.itemObject = itemObject;
        itemAmount = new ItemAmount(itemSO, amount);

        amountText = transform.Find("AmountText").GetComponent<TextMeshProUGUI>();

        transform.Find("ItemIcon").GetComponent<Image>().sprite = itemSO.sprite;

        if (isOutputItem) {
            transform.GetComponent<Button>().onClick.AddListener(() => {
                CraftingMachine selectedCraftingMachine = CraftingMachineUI.Instance.GetSelectedCraftingMachine();
                //if (selectedCraftingMachine.IsOutputStorageAvailable(itemAmount)) {
                    //selectedCraftingMachine.MoveItemFromInputToOutput(itemAmount);

                    UpdateAmount(amount - itemAmount.amount);
                //}
            });
        }

        UpdateAmount(amount);
    }

    public void UpdateAmount(int amount) {
        this.amount = amount;
        amountText.text = amount.ToString();

        if (amount <= 0) {
            Debug.Log("Disabling " + itemSO.name);
            gameObject.SetActive(false);
        } else {
            //Debug.Log("Enabling " + itemSO.name);
            gameObject.SetActive(true);
        }
    }

    public void SetId(int id) {
        inventorySlotId = id;
    }

    public int GetId() {
        return inventorySlotId;
    }

    public void ChangeIsOutputItem(bool isOutputItem) {
        this.isOutputItem = isOutputItem;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.ShowInventoryItemToolTip(itemSO, isOutputItem, itemObject);
        
    }

    public void OnPointerExit(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.Hide();
    }

    private void OnDisable() {
        if (isOutputItem) { return; }
        InterfaceToolTipUI.Instance.Hide();
    }
}
