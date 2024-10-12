using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour, IShowWorldTooltip {

    
    public List<ItemAmount> itemAmountList;
    public List<ItemObject> itemObjects;

        public Box(List<ItemAmount> itemSOList) {
            this.itemAmountList = itemSOList;
        }

    public string GetTooltipInfo() {
        string tooltipText = "";

        foreach (ItemAmount itemAmount in itemAmountList) {
            tooltipText += itemAmount.itemSO.nameString + " x" + itemAmount.amount + "\n";
        }

        return tooltipText;
    }

    public void RemoveItemsFromBox(ItemSO itemSO, int amount = 1) {
        foreach (ItemAmount boxItemAmount in itemAmountList) {
            if (boxItemAmount.itemSO == itemSO) {
                boxItemAmount.amount -= amount;
                if (boxItemAmount.amount <= 0) {
                    itemAmountList.Remove(boxItemAmount);
                }
                break;
            }
        }

    }
    public void RemoveItemsFromBox(ItemAmount itemAmount) {
        foreach (ItemAmount boxItemAmount in itemAmountList) {
            if (boxItemAmount.itemSO == itemAmount.itemSO) {

                int amountToRemove = itemAmount.amount;
                if (itemAmount.amount > boxItemAmount.amount) {
                    amountToRemove = itemAmount.amount;
                }

                Debug.Log("Amount to remove: " + amountToRemove);
                boxItemAmount.amount -= amountToRemove;
                if (boxItemAmount.amount <= 0) {
                    itemAmountList.Remove(boxItemAmount);
                    if (itemAmountList.Count == 0) {
                        Destroy(gameObject);
                    }
                }
                break;
            }
        }

    }
    

}
