using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SingleNewUnlockUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public enum UnlockType {
        Recipe,
        Item,
        PlacedObjectType,
    }

    private UnlockType unlockType;
    private RecipeSO recipeSO;
    private ItemSO contractItemSO;
    private PlacedObjectTypeSO placedObjectTypeSO;

    public void SetRecipe(RecipeSO recipeSO) {
        unlockType = UnlockType.Recipe;
        this.recipeSO = recipeSO;
    }

    public void SetItem(ItemSO contractItemSO) {
        unlockType = UnlockType.Item;
        this.contractItemSO = contractItemSO;
    }

    public void SetPlacedObjectType(PlacedObjectTypeSO placedObjectTypeSO) {
        unlockType = UnlockType.PlacedObjectType;
        this.placedObjectTypeSO = placedObjectTypeSO;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (unlockType == UnlockType.Recipe) {
            InterfaceToolTipUI.Instance.ShowRecipeToolTip(recipeSO, true);
        } else if (unlockType == UnlockType.Item) {
            InterfaceToolTipUI.Instance.ShowInventoryItemToolTip(contractItemSO, false);
        } else if (unlockType == UnlockType.PlacedObjectType) {
            //InterfaceToolTipUI.Instance.ShowPlacedObjectTypeToolTip(placedObjectTypeSO);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.Hide();
    }
}
