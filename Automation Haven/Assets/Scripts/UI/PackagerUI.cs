using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PackagerUI : BaseUI {

    public static PackagerUI Instance { get; private set; }

    [Header("Inventory")]
    [SerializeField] private Transform inputInventoryContainer;
    [SerializeField] private Transform outputInventoryContainer;
    [SerializeField] private Transform inventoryItemTemplate;

    [Header("Extras")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform statusBackground;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI currentModeText;
    [SerializeField] private Button currentModeButton;

    private List<InventoryItemUI> inputInventoryItemUIList;
    private List<InventoryItemUI> outputInventoryItemUIList;
    private BoxPackager packager;
    private IHasInventory hasInventory;

    private void Awake() {
        Instance = this;
    }

    private void Start() {

        currentModeButton.onClick.AddListener(() => {
            Debug.Log("Clicked button");
            if (packager == null) return;

            packager.ToggleMode();
            currentModeText.text = packager.GetMode().ToString().ToUpper();
        });

        closeButton.onClick.AddListener(Hide);

        

        inventoryItemTemplate.gameObject.SetActive(false);
        inputInventoryItemUIList = new List<InventoryItemUI>();
        outputInventoryItemUIList = new List<InventoryItemUI>();

        Hide();
    }

    private void UpdateInputInventory() {
        Inventory inputInventory = packager.GetInputInventory();

        InventoryItemUI.UpdateInventoryUI(hasInventory, inputInventory, inputInventoryContainer, inventoryItemTemplate);
    }

    private void UpdateOutputInventory() {
        Inventory outputInventory = packager.GetOutputInventory();

        InventoryItemUI.UpdateInventoryUI(hasInventory, outputInventory, outputInventoryContainer, inventoryItemTemplate);
    }

    public void Show(BoxPackager packager) {
        Hide();
        //AutomationGameManager.Instance.CloseOtherUIs(this);

        this.packager = packager;
        hasInventory = packager.transform.GetComponent<IHasInventory>();
        UpdateInputInventory();
        UpdateOutputInventory();

        currentModeText.text = packager.GetMode().ToString().ToUpper();

        packager.OnInputInventoryChanged += UpdateInputInventory;
        packager.OnOutputInventoryChanged += UpdateOutputInventory;

        base.Show();

    }

    public override void Hide() {
        base.Hide();

        if (packager != null) {
            packager.OnInputInventoryChanged -= UpdateInputInventory;
            packager.OnOutputInventoryChanged -= UpdateOutputInventory;
        }

        packager = null;
        hasInventory = null;

    }
}
