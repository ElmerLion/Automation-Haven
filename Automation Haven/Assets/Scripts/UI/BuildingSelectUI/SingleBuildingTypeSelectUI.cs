using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleBuildingTypeSelectUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Transform costContainer;
    [SerializeField] private Transform costItemTemplate;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;

    private PlacedObjectTypeSO buildingType;
    private Dictionary<ItemSO, Transform> costTransformList;
    private SingleBuildingSelectCategoryUI parentCategory;

    private void Start() {
        ClearDisplayCost();
    }

    public void Initialize(PlacedObjectTypeSO buildingType, SingleBuildingSelectCategoryUI singleBuildingSelectCategoryUI) {
        this.buildingType = buildingType;
        parentCategory = singleBuildingSelectCategoryUI;

        image.sprite = buildingType.buildingSprite;
        nameText.text = buildingType.nameString;
        costTransformList = new Dictionary<ItemSO, Transform>();
        transform.GetComponent<Button>().onClick.AddListener(() => {
            parentCategory.SetClickedBuildingType(buildingType);
            ClearDisplayCost();
        });
        ClearDisplayCost();

        parentCategory.OnNewBuildingSelected += ParentCategory_OnNewBuildingSelected;
    }

    private void ParentCategory_OnNewBuildingSelected(object sender, System.EventArgs e) {
        ClearDisplayCost();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        ShowBuildingCost();
    }

    public void OnPointerExit(PointerEventData eventData) {
        ClearDisplayCost();
        parentCategory.SetSelectedBuildingType(null);
    }

    private void ShowBuildingCost() {
        if (buildingType != parentCategory.GetSelectedBuildingType()) {

            parentCategory.SetSelectedBuildingType(buildingType);

            nameText.text = buildingType.nameString;
            nameText.gameObject.SetActive(true);

            foreach (ItemAmount itemAmount in buildingType.buildingCostList.requiredResources) {
                if (costTransformList.ContainsKey(itemAmount.itemSO)) {
                    costTransformList[itemAmount.itemSO].Find("Amount").GetComponent<TextMeshProUGUI>().text = itemAmount.amount.ToString();
                    costTransformList[itemAmount.itemSO].gameObject.SetActive(true);
                } else {
                    Transform costItemTransform = Instantiate(costItemTemplate, costContainer);
                    costItemTransform.Find("Image").GetComponent<Image>().sprite = itemAmount.itemSO.sprite;
                    costItemTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = itemAmount.amount.ToString();
                    costItemTransform.gameObject.SetActive(true);
                    costTransformList[itemAmount.itemSO] = costItemTransform;
                }
            }
        }
    }

    private void ClearDisplayCost() {
        nameText.gameObject.SetActive(false);
        foreach (Transform costTransform in costTransformList.Values) {
            costTransform.gameObject.SetActive(false);
        }
    }

}
