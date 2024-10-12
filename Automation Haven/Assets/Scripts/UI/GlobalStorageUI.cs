using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageUI : MonoBehaviour {

    [SerializeField] private Transform storedItemsContainer;
    [SerializeField] private Transform itemTemplate;

    private void Awake() {
        itemTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        StorageManager.Instance.OnGlobalStorageUpdated += StorageUI_OnGlobalStorageUpdated;
    }

    private void StorageUI_OnGlobalStorageUpdated(object sender, System.EventArgs e) {
        RefreshStorageItems();
    }

    private void RefreshStorageItems() {
           foreach (Transform child in storedItemsContainer) {
            if (child == itemTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (ItemAmount itemAmount in StorageManager.Instance.GetAllItemAmounts()) {
            Transform itemTransform = Instantiate(itemTemplate, storedItemsContainer);
            itemTransform.gameObject.SetActive(true);

            itemTransform.Find("Image").GetComponent<Image>().sprite = itemAmount.itemSO.sprite;
            itemTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = itemAmount.amount.ToString();
        }
    }

}
