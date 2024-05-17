using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectFilterUI : BaseUI {

    public static SelectFilterUI Instance { get; private set; }

    [SerializeField] private Transform categoryContainer;
    [SerializeField] private Transform categoryTemplate;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private Transform itemTemplate;
    [SerializeField] private Transform closeButton;

    private Dictionary<ItemSO, Transform> itemTransformsInContainer;
    private List<ItemSO> itemList;
    private Grabber grabber;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        itemTransformsInContainer = new Dictionary<ItemSO, Transform>();
        itemList = ItemManager.Instance.GetUnlockedItems();
        itemTemplate.gameObject.SetActive(false);
        categoryTemplate.gameObject.SetActive(false);

        closeButton.GetComponent<Button>().onClick.AddListener(Hide);

        SetupCategoryButtons();

        Hide();
    }

    private void SetupCategoryButtons() {
        foreach (ItemManager.Category category in ItemManager.Instance.GetCategories()) {
            Transform categoryTransform = Instantiate(categoryTemplate, categoryContainer);

            categoryTransform.Find("Icon").GetComponent<Image>().sprite = category.icon;
            categoryTransform.GetComponent<ItemCategoryButton>().Initialize(category);
            categoryTransform.GetComponent<Button>().onClick.AddListener(() => UpdateCategoryItemFilters(category.category));
            categoryTransform.gameObject.SetActive(true);

        }
    }

    private void SetCategoryToFirst() {
        UpdateCategoryItemFilters(ItemManager.Instance.GetCategories()[0].category);
    }

    private void UpdateCategoryItemFilters(ItemSO.ItemCategory itemCategory) {
        foreach (Transform child in itemContainer) {
            if (child.transform != itemTemplate.transform) {
                child.gameObject.SetActive(false);
            }
        }

        foreach (ItemSO itemSO in itemList) {

            if (itemSO.itemCategory == itemCategory) {

                if (itemTransformsInContainer.ContainsKey(itemSO)) {
                    itemTransformsInContainer[itemSO].Find("Outline").GetComponent<Image>().color = grabber.GetFilteredItems().Contains(itemSO) ? Color.green : Color.red;
                    itemTransformsInContainer[itemSO].Find("Button").GetComponent<FilterItemButtonUI>().Initialize(itemSO, grabber);
                    itemTransformsInContainer[itemSO].gameObject.SetActive(true);
                    continue;
                }


                GameObject itemTransform = Instantiate(itemTemplate.gameObject, itemContainer);
                Transform buttonTransform = itemTransform.transform.Find("Button");
                buttonTransform.GetComponent<Image>().sprite = itemSO.sprite;
                buttonTransform.GetComponent<FilterItemButtonUI>().Initialize(itemSO, grabber);
                itemTransform.SetActive(true);

                itemTransformsInContainer.Add(itemSO, itemTransform.transform);
            }
        }
    }

    public void Show(Transform sender) {
        Hide();

        //AutomationGameManager.Instance.CloseOtherUIs(this);

        grabber = sender.GetComponent<Grabber>();

        SetCategoryToFirst();

        base.Show();

    }

}
