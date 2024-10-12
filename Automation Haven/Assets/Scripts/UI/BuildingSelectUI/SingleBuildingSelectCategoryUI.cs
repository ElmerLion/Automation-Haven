using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleBuildingSelectCategoryUI : MonoBehaviour {

    public event EventHandler OnNewBuildingSelected;

    [SerializeField] private Image categoryIcon;
    [SerializeField] private TextMeshProUGUI categoryName;
    [SerializeField] private Transform buildingsContainer;
    [SerializeField] private Transform buildingTemplate;

    private BuildingCategoryData.Category category;
    private List<PlacedObjectTypeSO> categoryBuildingTypes = new List<PlacedObjectTypeSO>();
    private List<SingleBuildingTypeSelectUI> buildingTypeSelectUIs = new List<SingleBuildingTypeSelectUI>();
    private List<PlacedObjectTypeSO> addedBuildingTypes = new List<PlacedObjectTypeSO>();
    private PlacedObjectTypeSO selectedBuildingType;

    private void Start() {
        Hide();
    }

    public void Initialize(BuildingCategoryData.Category category) {
        this.category = category;

        categoryIcon.sprite = BuildingCategoryData.GetCategoryIcon(category);
        categoryName.text = category.ToString();
        transform.GetComponent<Button>().onClick.RemoveAllListeners();
        transform.GetComponent<Button>().onClick.AddListener(() => ToggleCategoryDisplay());
        gameObject.SetActive(true);
    }

    public void AddBuildingType(PlacedObjectTypeSO buildingType) {
        if (categoryBuildingTypes.Contains(buildingType)) return;

        categoryBuildingTypes.Add(buildingType);
    }

    public void InitializeBuildingForCategory() {
        foreach (PlacedObjectTypeSO buildingType in categoryBuildingTypes) {
            if (addedBuildingTypes.Contains(buildingType)) continue;

            Transform buildingTypeUI = Instantiate(buildingTemplate, buildingsContainer);
            buildingTypeUI.gameObject.SetActive(true);

            SingleBuildingTypeSelectUI singleBuildingTypeSelectUI = buildingTypeUI.GetComponent<SingleBuildingTypeSelectUI>();
            singleBuildingTypeSelectUI.Initialize(buildingType, this);
            buildingTypeSelectUIs.Add(singleBuildingTypeSelectUI);
            addedBuildingTypes.Add(buildingType);
        }
    }

    private void ToggleCategoryDisplay() {
        BuildingSelectUI.Instance.ClearSelection();
        if (BuildingSelectUI.Instance.GetSelectedCategory() != this) {
            BuildingSelectUI.Instance.HideAllBuildingsInCategories();
        }
        buildingsContainer.gameObject.SetActive(!buildingsContainer.gameObject.activeSelf);

        if (buildingsContainer.gameObject.activeSelf) {
            BuildingSelectUI.Instance.SelectCategory(this);
        } 
    }

    public void SetSelectedBuildingType(PlacedObjectTypeSO selectedBuildingType) {
        this.selectedBuildingType = selectedBuildingType;
        OnNewBuildingSelected?.Invoke(this, EventArgs.Empty);
    }

    public void SetClickedBuildingType(PlacedObjectTypeSO clickedBuildingType) {
        BuildingSelectUI.Instance.SelectBuildingType(clickedBuildingType);
    }

    public PlacedObjectTypeSO GetSelectedBuildingType() {
        return selectedBuildingType;
    }

    public void Show() {
        buildingsContainer.gameObject.SetActive(true);
    }
    public void Hide() {
        buildingsContainer.gameObject.SetActive(false);
    }   

}
