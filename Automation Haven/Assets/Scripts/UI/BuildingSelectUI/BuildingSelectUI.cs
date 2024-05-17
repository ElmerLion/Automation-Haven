using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BuildingSelectUI : MonoBehaviour {

    public static BuildingSelectUI Instance { get; private set; }

    
    [SerializeField] private Transform categoryTemplate;
    [SerializeField] private Transform buildingCategoriesContainer;
    [SerializeField] private Transform buildingTemplate;
    [SerializeField] private Button arrowButton;
    [SerializeField] private Transform costItemTemplate;

    private Dictionary<BuildingCategoryData.Category?, BuildingCategoryUI> categoryUIs = new Dictionary<BuildingCategoryData.Category?, BuildingCategoryUI>();
    private Dictionary<BuildingCategoryData.Category?, SingleBuildingSelectCategoryUI> categorySingleDic = new Dictionary<BuildingCategoryData.Category?, SingleBuildingSelectCategoryUI>();


    private List<PlacedObjectTypeSO> buildingTypes;
    private SingleBuildingSelectCategoryUI selectedCategory = null;
    private PlacedObjectTypeSO selectedBuildingType = null;

    private void Awake() {
        Instance = this;
        buildingTypes = PlacedBuildingManager.Instance.GetUnlockedBuildingTypeList();

        categoryTemplate.gameObject.SetActive(false);
        buildingTemplate.gameObject.SetActive(false);
        costItemTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        InitCategories();
        //InitBuildings();
        ClearSelection();

        PlacedBuildingManager.Instance.OnNewBuildingUnlocked += (sender, e) => InitCategories();
        //arrowButton.onClick.AddListener(() => ClearSelection());
    }

    private void InitCategories() {
        foreach (PlacedObjectTypeSO buildingType in buildingTypes) {
            BuildingCategoryData.Category category = buildingType.categoryData.category;
            if (!categoryUIs.ContainsKey(category)) {
                Transform categoryUI = Instantiate(categoryTemplate, buildingCategoriesContainer);
                categoryUIs[category] = new BuildingCategoryUI(categoryUI, categoryUI.Find("BuildingsContainer"));

                SingleBuildingSelectCategoryUI singleBuildingSelectCategoryUI = categoryUI.GetComponent<SingleBuildingSelectCategoryUI>();
                singleBuildingSelectCategoryUI.Initialize(category);
                categorySingleDic.Add(category, singleBuildingSelectCategoryUI);
            }
            categorySingleDic[category].AddBuildingType(buildingType);
        }

        foreach (SingleBuildingSelectCategoryUI singleBuildingSelectCategory in categorySingleDic.Values) {
            singleBuildingSelectCategory.InitializeBuildingForCategory();
        }
    }

    /*private void InitBuildings() {
        foreach (var categoryBuildingTypeUIs in categoryUIs) {
            Transform buildingsContainer = categoryBuildingTypeUIs.Value.ContainerTransform;
            foreach (PlacedObjectTypeSO buildingType in categoryBuildingTypeUIs.Value.categoryBuildingTypes) {
                
                Transform buildingUI = Instantiate(buildingTemplate, buildingsContainer);
                Transform buttonTransform = buildingUI.Find("Button");

                buttonTransform.GetComponent<Image>().sprite = buildingType.buildingSprite;
                buttonTransform.GetComponent<SingleBuildingTypeSelectUI>().Initialize(buildingType);
                buildingUI.gameObject.SetActive(true);


                buildingUI.Find("Button").GetComponent<Button>().onClick.AddListener(() => SelectBuildingType(buildingType));
                buildingUI.Find("Name").gameObject.SetActive(false);

                categoryBuildingTypeUIs.Value.AddBuildingTypeUI(buildingType, buildingUI);


            }
            buildingsContainer.gameObject.SetActive(false);
        }

    }*/

    public void SelectCategory(SingleBuildingSelectCategoryUI category) {
        selectedCategory = category;
    }

    public void SelectBuildingType(PlacedObjectTypeSO buildingType) {
        HideAllBuildingsInCategories();
        selectedBuildingType = buildingType;
        GridBuildingSystem.Instance.SelectObjectType(buildingType);
    }

   /* public void ShowBuildingCosts(PlacedObjectTypeSO buildingTypeSO) {

        if (buildingTypeSO != selectedBuildingType) {
            ClearDisplayCosts(selectedBuildingType);
            selectedBuildingType = buildingTypeSO;

            BuildingTypeUI buildingUI = categoryUIs[selectedCategory.Value].BuildingTypeUIs[buildingTypeSO];
            buildingUI.Transform.Find("Name").GetComponent<TextMeshProUGUI>().text = buildingTypeSO.nameString;
            buildingUI.Transform.Find("Name").gameObject.SetActive(true);

            buildingUI.CostTransforms.ForEach(t => Destroy(t.gameObject));
            buildingUI.CostTransforms.Clear();

            foreach (ItemAmount itemAmount in buildingTypeSO.buildingCostList.requiredResources) {
                Transform costItemTransform = Instantiate(costItemTemplate, buildingUI.Transform.Find("CostContainer"));
                costItemTransform.Find("Image").GetComponent<Image>().sprite = itemAmount.itemSO.sprite;
                costItemTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = itemAmount.amount.ToString();
                costItemTransform.gameObject.SetActive(true);
                buildingUI.CostTransforms.Add(costItemTransform);
            }
        }
    }

    public void ClearDisplayCosts(PlacedObjectTypeSO buildingTypeSO) {
        if (buildingTypeSO != null && selectedCategory.HasValue && categoryUIs[selectedCategory.Value].BuildingTypeUIs.TryGetValue(buildingTypeSO, out BuildingTypeUI buildingUI)) {
            buildingUI.Transform.Find("Name").gameObject.SetActive(false);
            buildingUI.CostTransforms.ForEach(t => t.gameObject.SetActive(false));
            selectedBuildingType = null;
        }
    }*/

    public void HideAllBuildingsInCategories() {
        foreach (SingleBuildingSelectCategoryUI singleBuildingSelect in categorySingleDic.Values) {
            singleBuildingSelect.Hide();
        }
    }

    public void ClearSelection() {
        if (selectedCategory == null) return;
        
        selectedCategory.Hide();
        selectedCategory = null;
        selectedBuildingType = null;
        GridBuildingSystem.Instance.DeselectObjectType();
    }

    public SingleBuildingSelectCategoryUI GetSelectedCategory() {
        return selectedCategory;
    }
    public List<PlacedObjectTypeSO> GetBuildingTypeList() {
        return buildingTypes;
    }

    private class BuildingCategoryUI {
        public Transform CategoryTransform { get; private set; }
        public Transform ContainerTransform { get; private set; }
        public Dictionary<PlacedObjectTypeSO, BuildingTypeUI> BuildingTypeUIs { get; private set; }
        public List<PlacedObjectTypeSO> categoryBuildingTypes;

        public BuildingCategoryUI(Transform categoryTransform, Transform containerTransform) {
            CategoryTransform = categoryTransform;
            BuildingTypeUIs = new Dictionary<PlacedObjectTypeSO, BuildingTypeUI>();
            ContainerTransform = containerTransform;
            categoryBuildingTypes = new List<PlacedObjectTypeSO>();
        }

        public void AddBuildingTypeUI(PlacedObjectTypeSO buildingType, Transform buildingTransform) {
            BuildingTypeUIs.Add(buildingType, new BuildingTypeUI(buildingTransform));
        }
    }

    private class BuildingTypeUI {
        public Transform Transform { get; set; }
        public List<Transform> CostTransforms { get; private set; }

        public BuildingTypeUI(Transform transform) {
            Transform = transform;
            CostTransforms = new List<Transform>();
        }
    }
}
