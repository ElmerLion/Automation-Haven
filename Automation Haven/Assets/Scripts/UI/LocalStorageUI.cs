using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LocalStorageUI : BaseUI {

    public static LocalStorageUI Instance { get; private set; }

    [Header("Inventory")]
    [SerializeField] private Transform emptySlotsContainer;
    [SerializeField] private Transform emptySlotTemplate;
    [SerializeField] private Transform inventoryItemContainer;
    [SerializeField] private Transform inventoryItemTemplate;

    [Header("Extras")]
    [SerializeField] private Button closeButton;

    private List<Transform> emptyInventorySlots;
    private Storage storage;
    private IHasInventory hasInventory;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        emptySlotTemplate.gameObject.SetActive(false);
        inventoryItemTemplate.gameObject.SetActive(false);

        emptyInventorySlots = new List<Transform>();

        closeButton.onClick.AddListener(Hide);

        Hide();
    }

    public void Show(Storage storage) {
        Hide();
        //AutomationGameManager.Instance.CloseOtherUIs(this);

        this.storage = storage;
        hasInventory = storage.transform.GetComponent<IHasInventory>();

        UpdateInventoryUI();

        storage.OnInventoryChanged += UpdateInventoryUI;

        base.Show();
    }

    public override void Hide() {
        base.Hide();

        if (storage != null) {
            storage.OnInventoryChanged -= UpdateInventoryUI;
        }

    }

    private void UpdateInventoryUI() {
        UpdateEmptySlots();

        InventoryItemUI.UpdateInventoryUI(hasInventory, storage.GetInventory(), inventoryItemContainer, inventoryItemTemplate);
    }

    public void UpdateEmptySlots() {
        int maxInventorySlots = storage.GetInventory().GetMaxInventorySlots();

        for (int i = 0; i < maxInventorySlots; i++) {
            if (i < emptyInventorySlots.Count) {
                emptyInventorySlots[i].gameObject.SetActive(true);
            } else {
                Transform emptySlotTransform = Instantiate(emptySlotTemplate, emptySlotsContainer);
                emptySlotTransform.gameObject.SetActive(true);
                emptyInventorySlots.Add(emptySlotTransform);
            }
        }

        for (int i = maxInventorySlots; i < emptyInventorySlots.Count; i++) {
            emptyInventorySlots[i].gameObject.SetActive(false);
        }
    }


    
}
