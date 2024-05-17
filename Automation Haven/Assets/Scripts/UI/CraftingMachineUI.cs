using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMachineUI : BaseUI {

    public static CraftingMachineUI Instance { get; private set; }

    [Header("Recipes & Categories")]
    [SerializeField] private GameObject recipeItemTemplate;
    [SerializeField] private Transform recipeContainer;
    [SerializeField] private Transform categoryContainer;
    [SerializeField] private Transform categoryTemplate;

    [Header("Active Recipe & Inventory")]
    [SerializeField] private Image currentRecipeImage;
    [SerializeField] private Transform inputInventoryContainer;
    [SerializeField] private Transform outputInventoryContainer;
    [SerializeField] private Transform inventoryItemTemplate;

    [Header("Extras")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform progressBar;
    [SerializeField] private MachineUpgradesUI machineUpgradesUI;

    private List<RecipeSO> unlockedRecipeList;
    private ItemManager.Category firstValidCategory;

    private Dictionary<ItemManager.Category, Transform> categoryTransforms = new Dictionary<ItemManager.Category, Transform>();
    
    private List<InventoryItemUI> inputInventoryItemUIList;
    private List<InventoryItemUI> outputInventoryItemUIList;
    private CraftingMachine craftingMachine;
    private IHasInventory hasInventory;
    private PlacedObjectTypeSO craftingMachineTypeSO;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        inputInventoryItemUIList = new List<InventoryItemUI>();
        outputInventoryItemUIList = new List<InventoryItemUI>();

        unlockedRecipeList = RecipeManager.Instance.GetUnlockedRecipes();
        recipeItemTemplate.gameObject.SetActive(false);
        categoryTemplate.gameObject.SetActive(false);
        inventoryItemTemplate.gameObject.SetActive(false);

        closeButton.onClick.AddListener(Hide);

        SetupCategoryButtons();
        Hide();
        
    }

    private void SetupCategoryButtons() {
        foreach (ItemManager.Category category in ItemManager.Instance.GetCategories()) {
            Transform categoryTransform = Instantiate(categoryTemplate, categoryContainer);

            categoryTransform.Find("Icon").GetComponent<Image>().sprite = category.icon;
            categoryTransform.GetComponent<ItemCategoryButton>().Initialize(category);
            categoryTransform.GetComponent<Button>().onClick.AddListener(() => UpdateCategoryRecipes(category.category));
            categoryTransform.gameObject.SetActive(true);

            categoryTransforms.Add(category, categoryTransform);

        }
    }

    private void UpdateVisibleCategories() {
        bool firstCategory = true;
        foreach (ItemManager.Category category in ItemManager.Instance.GetCategories()) {

            List<RecipeSO> recipesInCategory = new List<RecipeSO>();
            foreach (RecipeSO recipeSO in unlockedRecipeList) {
                if (recipeSO.craftingMachineType != craftingMachineTypeSO.craftingMachineType) { continue; }

                if (recipeSO.output[0].itemSO.itemCategory == category.category) {
                    recipesInCategory.Add(recipeSO);
                }
            }

            if (recipesInCategory.Count > 0) {
                categoryTransforms[category].gameObject.SetActive(true);
                if (firstCategory) {
                    firstValidCategory = category;
                    firstCategory = false;
                }
            } else {
                categoryTransforms[category].gameObject.SetActive(false);
            }
        }
    }

    public void SetActiveRecipeUI(RecipeSO recipeSO) {
        if (recipeSO == null) {
            currentRecipeImage.gameObject.SetActive(false);
            return;
        }

        currentRecipeImage.gameObject.SetActive(true);
        currentRecipeImage.sprite = recipeSO.output[0].itemSO.sprite;

        currentRecipeImage.transform.parent.GetComponent<RecipeButton>().Initialize(recipeSO, craftingMachine);
    }

    private void UpdateCategoryRecipes(ItemSO.ItemCategory itemCategory) {
        foreach (Transform child in recipeContainer) {
            if (child.transform != recipeItemTemplate.transform) {
                Destroy(child.gameObject);
            }
        }

        foreach (RecipeSO recipeSO in unlockedRecipeList) {
            if (recipeSO.craftingMachineType != craftingMachineTypeSO.craftingMachineType) { continue; }

            ItemSO testItemSO = recipeSO.output[0].itemSO;
            if (testItemSO.itemCategory == itemCategory) {
                GameObject recipeTransform = Instantiate(recipeItemTemplate, recipeContainer);

                recipeTransform.GetComponent<RecipeButton>().Initialize(recipeSO, craftingMachine);
                recipeTransform.transform.Find("ItemIcon").GetComponent<Image>().sprite = testItemSO.sprite;
                recipeTransform.gameObject.SetActive(true);
            }
        }
    }

    private void SetCategoryToFirst() {
        UpdateCategoryRecipes(firstValidCategory.category);
        
    }

    private void CraftingMachine_OnProgressChanged() {
        if (craftingMachine == null) return;

        ProgressBarUI progressBarUI = progressBar.GetComponent<ProgressBarUI>();
        progressBarUI.UpdateVisual(craftingMachine.GetProgressNormalized());
        progressBarUI.UpdateRemainingTime(craftingMachine.GetRemainingTime());

    }


    public void Show(Transform sender) {
        Hide();
        //AutomationGameManager.Instance.CloseOtherUIs(this);
        
        craftingMachine = sender.GetComponent<CraftingMachine>();
        hasInventory = craftingMachine.GetComponent<IHasInventory>();
        craftingMachineTypeSO = craftingMachine.GetComponent<BuildingTypeHolder>().buildingType;

        machineUpgradesUI.Setup(craftingMachine.transform.GetComponent<UpgradeableMachine>());

        SetActiveRecipeUI(craftingMachine.GetActiveRecipeSO());
        unlockedRecipeList = RecipeManager.Instance.GetUnlockedRecipes();

        UpdateVisibleCategories();
        SetCategoryToFirst();
        UpdateInputInventory();
        UpdateOutputInventory();
        CraftingMachine_OnProgressChanged();

        craftingMachine.OnProgressChanged += CraftingMachine_OnProgressChanged;
        craftingMachine.OnActiveRecipeChanged += CraftingMachine_OnActiveRecipeChanged;

        craftingMachine.OnInputInventoryChanged += UpdateInputInventory;
        craftingMachine.OnOutputInventoryChanged += UpdateOutputInventory;

        base.Show();
    }


    private void CraftingMachine_OnActiveRecipeChanged(object sender, EventArgs e) {
        progressBar.GetComponent<ProgressBarUI>().UpdateRemainingTime(craftingMachine.GetRemainingTime());
    }

    public override void Hide() {
        if (craftingMachine != null) {
            craftingMachine.OnProgressChanged -= CraftingMachine_OnProgressChanged;
            craftingMachine.OnActiveRecipeChanged -= CraftingMachine_OnActiveRecipeChanged;
            craftingMachine.OnInputInventoryChanged -= UpdateInputInventory;
            craftingMachine.OnOutputInventoryChanged -= UpdateOutputInventory;

        }

        DisableInventoryUIList();

        craftingMachine = null;
        hasInventory = null;
        craftingMachineTypeSO = null;

        base.Hide();
    }    

    private void UpdateInputInventory() {
        InventoryItemUI.UpdateInventoryUI(hasInventory, craftingMachine.GetInputInventory(), inputInventoryContainer, inventoryItemTemplate);
    }

    private void UpdateOutputInventory() {
        InventoryItemUI.UpdateInventoryUI(hasInventory, craftingMachine.GetOutputInventory(), outputInventoryContainer, inventoryItemTemplate);
    }

    private void DisableInventoryUIList() {
        List<InventoryItemUI> toDisableList = new List<InventoryItemUI>();
        foreach (InventoryItemUI inventoryItemUI in inputInventoryItemUIList) {
            toDisableList.Add(inventoryItemUI);
        }

        foreach (InventoryItemUI inventoryItemUI in outputInventoryItemUIList) {
            toDisableList.Add(inventoryItemUI);
        }

        foreach (InventoryItemUI inventoryItemUI in toDisableList) {
            inventoryItemUI.gameObject.SetActive(false);
        }
    }

    public CraftingMachine GetSelectedCraftingMachine() {
        return craftingMachine;
    }
}
