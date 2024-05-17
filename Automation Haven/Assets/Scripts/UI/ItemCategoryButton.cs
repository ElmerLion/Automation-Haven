using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemCategoryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private ItemManager.Category category;

    public void Initialize(ItemManager.Category category) {
        this.category = category;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.ShowCategoryToolTip(category);
    }

    public void OnPointerExit(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.Hide();
    }
}
