using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemObject : MonoBehaviour, IShowWorldTooltip {

    [SerializeField] private ItemSO itemSO;
    [SerializeField] private Transform raycastPoint;

    private IItemParent parent;
    public bool hasReachedMiddlePoint = false;

    public void SetParent(Transform newParentTransform) {
        this.parent = newParentTransform.GetComponent<IItemParent>();
        transform.parent = newParentTransform;
    }

    public ItemSO.ItemCategory GetItemCategory() {
        return itemSO.itemCategory;
    }

    public ItemSO GetItemSO() {
        return itemSO;
    }
    public Transform GetRaycastPoint() {
        return raycastPoint;
    }

    public string GetTooltipInfo() {
        string tooltipText = itemSO.nameString;

        if (itemSO.chemicalSymbol != "") {
            tooltipText += " ( " + itemSO.chemicalSymbol + " )";
        }

        return tooltipText;
    }
}
