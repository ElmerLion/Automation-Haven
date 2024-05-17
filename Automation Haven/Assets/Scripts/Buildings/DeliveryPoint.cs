using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryPoint : MonoBehaviour, ICanBePutDownIn, ICanBeGrabbedFrom, ICanBeClicked {
    
    private ContractManager.Contract currentContract;
    [SerializeField] private List<ItemObject> outputInventory;

    private int currentContractIndex = 0;

    private void Start() {
        outputInventory = new List<ItemObject>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (currentContractIndex >= ContractManager.Instance.GetActiveContracts().Count) currentContractIndex = 0;

            currentContract = ContractManager.Instance.GetActiveContracts()[currentContractIndex];
            currentContractIndex++;
            Debug.Log("Current contract: " + currentContract.companySO.companyName);
        }
    }

    public void DeliverItemsForContract(ItemSO itemSO, int amount = 1) {
        if (currentContract == null) return;

        ItemAmount exisitingItem = ItemAmount.GetItemSOInItemAmountList(itemSO, currentContract.neededItemAmount);
        if (exisitingItem == null) return;

        ContractManager.Instance.DeliverItemsToContract(currentContract, itemSO, amount);
    }

    public bool TryDeliverBoxForContract(Box box) {
        if (currentContract == null || box == null || box.itemAmountList.Count <= 0) return false;

        List<ItemAmount> toRemoveItemAmountList = new List<ItemAmount>();
        foreach (ItemAmount itemAmount in box.itemAmountList) {
            ItemAmount excessItemAmount;
            ContractManager.Instance.DeliverItemsToContract(currentContract, itemAmount, out excessItemAmount);
            if (excessItemAmount != null && excessItemAmount.amount > 0) {
                // Handle excess items here
                ItemObject newItemObject = Instantiate(excessItemAmount.itemSO.prefab, transform).GetComponent<ItemObject>();
                outputInventory.Add(newItemObject);
            }
            // Mark all items for removal from the box since we've dealt with them
            toRemoveItemAmountList.Add(itemAmount);
        }

        foreach (ItemAmount itemAmount in toRemoveItemAmountList) {
            box.RemoveItemsFromBox(itemAmount);
        }
        return true;
    }

    public bool TryDeliverBoxToMarket(Box box) {
        if (box == null || box.itemAmountList.Count <= 0) return false;

        List<ItemObject> itemObjectListCopy = new List<ItemObject>(box.itemObjects);

        foreach (ItemObject itemObject in itemObjectListCopy) {
            PlayerEconomyManager.Instance.AddMoneyForItem(itemObject);
            box.itemObjects.Remove(itemObject);
            Destroy(itemObject.gameObject);

            if (box.itemObjects.Count == 0) {
                Destroy(box.gameObject);
                return true;
            }
        }

        return false;
    }

    public bool IsContractCompleted() {
        if (currentContract == null) return false;
        return currentContract.isCompleted;
    }

    public ContractManager.Contract GetCurrentContract() {
        return currentContract;
    }

    public bool TryPutDownObject(ItemObject itemObject) {
        if (currentContract != null) {
            foreach (ItemAmount itemAmount in itemObject.GetComponent<Box>().itemAmountList) {
                if (!ContractManager.Instance.DoesContractNeedItemSO(currentContract, itemAmount.itemSO)) {
                    return false;
                }
            }
        }

        if (itemObject.TryGetComponent(out Box box)) {
            if (TryDeliverBoxForContract(box)) return true;

            TryDeliverBoxToMarket(box);

            return true;
        }
        return false;
    }

    public void GrabObject(ItemObject itemObject) {
        if (outputInventory.Contains(itemObject)) {
            outputInventory.Remove(itemObject);
        }
    }

    public ItemObject GetPotentialObject() {
        if (outputInventory.Count == 0) return null;

        return outputInventory[0];
    }

    public void SetContract(ContractManager.Contract contract) {
        currentContract = contract;
    }

    public void OnClick() {
        ActiveContractsDisplayUI.Instance.ShowDeliveryPointUI(this);
    }
}
