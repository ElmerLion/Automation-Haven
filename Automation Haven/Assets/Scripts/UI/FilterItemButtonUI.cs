using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FilterItemButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private ItemSO itemSO;
    private Grabber grabber;

    public void Initialize(ItemSO itemSO, Grabber grabber) {
        this.grabber = grabber;
        this.itemSO = itemSO;
        Button button = GetComponent<Button>();
        if (button != null) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ToggleFilter);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.ShowFilteredItemToolTip(itemSO, grabber.GetFilteredItems().Contains(itemSO));
    }

    public void OnPointerExit(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.Hide();
    }

    private void ToggleFilter() {
        Transform outlineTransform = transform.parent.Find("Outline");
        if (grabber.GetFilteredItems().Contains(itemSO)) {
            grabber.RemoveFilteredItem(itemSO);
            outlineTransform.GetComponent<Image>().color = Color.red;
        } else {
            grabber.AddNewFilteredItem(itemSO);
            outlineTransform.GetComponent<Image>().color = Color.green;
        }
        InterfaceToolTipUI.Instance.ShowFilteredItemToolTip(itemSO, grabber.GetFilteredItems().Contains(itemSO));

    }

}
