using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class ItemAmount {

    public ItemSO itemSO;
    public int amount;

    public ItemAmount(ItemSO itemSO, int amount) {
        this.itemSO = itemSO;
        this.amount = amount;
    }

    public static ItemAmount GetItemSOInItemAmountList(ItemSO itemSO, List<ItemAmount> itemAmountList) {
        foreach (ItemAmount itemAmount in itemAmountList) {
            if (itemAmount.itemSO == itemSO) return itemAmount;
        }
        return null;
    }

}
