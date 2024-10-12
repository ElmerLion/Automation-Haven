using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour {

    /// <summary>
    /// Det finns storages, när items åker in i den storagen så sparas det i ens inventory, man kan sedan också pumpa ut ur den storagen
    /// </summary>

    private Dictionary<ItemSO, int> currentItemAmountDictionary = new Dictionary<ItemSO, int>();
    private void Start() {

        LogCurrentItems();
    }

    public void AddItem(ItemSO itemSO, int amount) {
        if (currentItemAmountDictionary.ContainsKey(itemSO)) {
            currentItemAmountDictionary[itemSO] += amount;
        } else {
            Debug.Log("That item does not exist in current inventory, adding!");
            currentItemAmountDictionary.Add(itemSO, amount);
        }
        LogCurrentItems();
    }

    private void LogCurrentItems() {
        foreach (ItemSO itemSO in currentItemAmountDictionary.Keys) {
            Debug.Log(itemSO.nameString + ": " + currentItemAmountDictionary[itemSO]);
        }
    }

}
