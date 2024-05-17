using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceToolTipUI : MonoBehaviour {

    public static InterfaceToolTipUI Instance { get; private set; }

    [Header("Main")]
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private Transform itemTemplate;
    [SerializeField] private RectTransform backgroundRectTransform;

    [Header("Resources")]
    [SerializeField] private Transform imageWithTextTemplate;
    [SerializeField] private Transform textTemplate;
    [SerializeField] private Sprite craftingTimeSprite;
    [SerializeField] private Sprite priceSprite;

    private List<Transform> textTemplateList;
    private List<Transform> imageWithTextTemplateList;
    private List<Transform> itemContainerList;

    private bool isShowing;
    private Vector3 startingOffset = new Vector3(-105, 0, 0);
    private Vector3 multiplierPerItemOffset = new Vector3(35, 0, 0);
    private Vector3 itemIconOffsetOnHigherNumbers = new Vector3(15, 0, 0);

    private void Awake() {
        Instance = this;

        textTemplateList = new List<Transform>();
        imageWithTextTemplateList = new List<Transform>();

        itemTemplate.gameObject.SetActive(false);
        textTemplate.gameObject.SetActive(false);
        imageWithTextTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        Hide();
    }

    private void Update() {
        if (isShowing) {
            UpdateMousePosition();
        }
    }

    public void ShowCategoryToolTip(ItemManager.Category category) {
        if (category == null) return;

        HideAllObjects();

        nameText.text = category.name.ToUpper();

        float totalWidth = 0;
        float totalHeight = 0;

        totalWidth += nameText.preferredWidth;
        totalHeight += nameText.preferredHeight;

        UpdateBackgroundSize(totalHeight, totalWidth, 20, 25);

        isShowing = true;
        gameObject.SetActive(true);
    }

    public void ShowInventoryItemToolTip(ItemSO item, bool isOutputItem, ItemObject itemObject = null) {
        if (item == null) return;

        HideAllObjects();

        nameText.text = item.nameString.ToUpper();

        float totalWidth = nameText.preferredWidth;
        float totalHeight = 0;
        float paddingHeight = 10;
        float paddingWidth = 25;


        totalHeight += nameText.preferredHeight;

        if (isOutputItem) {
            string outputString = "CLICK TO OUTPUT";
            TextMeshProUGUI outputText = AddText(outputString);
            totalHeight += outputText.preferredHeight;
            totalWidth = outputText.preferredWidth;
            paddingHeight = 23;
        }

        if (itemObject != null && itemObject.TryGetComponent(out Box box)) {
            paddingHeight = 0;

            RectTransform itemTemplateRectTransform = itemTemplate.GetComponent<RectTransform>();
            if (nameText.preferredWidth > box.itemAmountList.Count * itemTemplateRectTransform.sizeDelta.x) {
                //totalWidth += recipeNameText.preferredWidth;
            } else {
                totalWidth = box.itemAmountList.Count * itemTemplateRectTransform.sizeDelta.x;
            }

            totalHeight += itemTemplateRectTransform.sizeDelta.y;

            float totalExtraWidthItems = AddItemAmountList(box.itemAmountList);

            float itemContainerWidth = box.itemAmountList.Count * itemTemplateRectTransform.sizeDelta.x;
            if (itemContainerWidth > nameText.preferredWidth) {
                totalWidth = itemContainerWidth + totalExtraWidthItems;
            }

        }

            UpdateBackgroundSize(totalHeight, totalWidth, paddingHeight, paddingWidth);

        isShowing = true;
        UpdateMousePosition();
        gameObject.SetActive(true);
    }

    public void ShowRecipeToolTip(RecipeSO recipe, bool showCraftingMachineType = false) {
        if (recipe == null) return;

        HideAllObjects();

        nameText.text = recipe.output[0].itemSO.nameString.ToUpper();
        string craftingTimeString = recipe.craftingTime + "s";
        Transform craftingTimeTransform = AddImageWithText(craftingTimeSprite, craftingTimeString, out TextMeshProUGUI createdTimeText);


        foreach (Transform child in itemsContainer) {
            if (child == itemTemplate) {
                continue;
            }
            Destroy(child.gameObject);
        }

        float totalWidth = 0;
        float totalHeight = 0;
        totalWidth += nameText.preferredWidth;
        totalHeight += nameText.preferredHeight + createdTimeText.preferredHeight;

        float totalExtraWidthItems = AddItemAmountList(recipe.input);

        RectTransform itemTemplateRectTransform = itemTemplate.GetComponent<RectTransform>();
        
        float itemContainerWidth = recipe.input.Count * itemTemplateRectTransform.sizeDelta.x;
        if (itemContainerWidth > nameText.preferredWidth) {
            totalWidth = itemContainerWidth;
        }

        if (showCraftingMachineType) {
            string craftingMachineTypeString = "CRAFTED IN: " + recipe.craftingMachineType.ToString().ToUpper();
            TextMeshProUGUI craftingMachineTypeText = AddText(craftingMachineTypeString);
            totalHeight += craftingMachineTypeText.preferredHeight;

            if (craftingMachineTypeText.preferredWidth > recipe.input.Count * itemTemplateRectTransform.sizeDelta.x + totalExtraWidthItems && craftingMachineTypeText.preferredWidth > nameText.preferredWidth && showCraftingMachineType) {
                totalWidth = craftingMachineTypeText.preferredWidth;
            } 
        }

        totalHeight += itemTemplateRectTransform.sizeDelta.y;

        float paddingX = 30;

        UpdateBackgroundSize(totalHeight, totalWidth, 10, paddingX);

        UpdateMousePosition();
        itemsContainer.gameObject.SetActive(true);
        isShowing = true;
        gameObject.SetActive(true);
    }

    public void ShowFilteredItemToolTip(ItemSO itemSO, bool isFiltered) {
        if (itemSO == null) return;

        HideAllObjects();

        nameText.text = itemSO.nameString.ToUpper();

        itemsContainer.gameObject.SetActive(false);

        string text = isFiltered ? "CLICK TO REMOVE FROM FILTER" : "CLICK TO ADD TO FILTER";
        TextMeshProUGUI isFilteredText = AddText(text);

        float totalWidth = 0;
        float totalHeight = 0;

        totalWidth += isFilteredText.preferredWidth;
        totalHeight += nameText.preferredHeight + isFilteredText.preferredHeight;

        UpdateBackgroundSize(totalHeight, totalWidth, 27, 25);

        isShowing = true;
        UpdateMousePosition();
        gameObject.SetActive(true);
    }

    public void ShowMachineUpgradeToolTip(UpgradeableMachine.MachineUpgrade machineUpgrade) {
        if (machineUpgrade == null) return;

        HideAllObjects();

        nameText.text = machineUpgrade.upgradeSO.nameString.ToUpper();

        string upgradeValueString = machineUpgrade.GetUpgradeValuePercent() + "%";
        Transform upgradeValueTransform = AddImageWithText(craftingTimeSprite, upgradeValueString, out TextMeshProUGUI createdUpgradeValueText);
        TextMeshProUGUI currentLevelText = AddText("LEVEL: " + machineUpgrade.GetCurrentLevel() + "/" + machineUpgrade.GetMaxLevel());
        Transform costTransform = AddImageWithText(priceSprite, machineUpgrade.currentPrice.ToString(), out TextMeshProUGUI createdCostText);

        float totalWidth = 0;
        float totalHeight = 0;

        totalWidth += nameText.preferredWidth;
        totalHeight += nameText.preferredHeight + createdCostText.preferredHeight + currentLevelText.preferredHeight + createdUpgradeValueText.preferredHeight;

        UpdateBackgroundSize(totalHeight, totalWidth, 25, 25);

        Show();
    }

    public void ShowResearchNodeToolTip(ResearchManager.ResearchNode researchNode) {
        HideAllObjects();

        nameText.text = researchNode.researchNodeSO.nameString.ToUpper();

        string description = researchNode.researchNodeSO.description;

        TextMeshProUGUI descriptionText = AddText(description);

        Transform researchCostTransform = AddImageWithText(priceSprite, researchNode.researchProgress.ToString(), out TextMeshProUGUI createdResearchCostText);


        float totalWidth = 0;
        float totalHeight = 0;

        totalWidth += descriptionText.preferredWidth;
        totalHeight += nameText.preferredHeight + descriptionText.preferredHeight + createdResearchCostText.preferredHeight;

        UpdateBackgroundSize(totalHeight, totalWidth, 25, 0);

        Show();

    }

    private void Show() {
        isShowing = true;
        UpdateMousePosition();
        gameObject.SetActive(true);
    }

    private void UpdateBackgroundSize(float totalHeight, float totalWidth, float paddingHeight, float paddingWidth) {
        totalHeight += paddingHeight;
        totalWidth += paddingWidth;

        Vector2 backgroundSize = backgroundRectTransform.sizeDelta;
        backgroundSize.x = totalWidth;
        backgroundSize.y = totalHeight;
        backgroundRectTransform.sizeDelta = backgroundSize;
    }

    private void UpdateMousePosition() {
        Vector3 mousePosition = Input.mousePosition;

        float offsetX = 15;
        float offsetY = 50;
        mousePosition.x += offsetX;
        mousePosition.y += offsetY;

        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, mousePosition, null, out Vector2 localPoint);
        transform.localPosition = localPoint;
    }

    private void HideAllObjects() {
        foreach (Transform textTransform in textTemplateList) {
            if (textTransform.gameObject.activeSelf) {
                textTransform.gameObject.SetActive(false);
            }
        }

        foreach (Transform imageWithTextTransform in imageWithTextTemplateList) {
            if (imageWithTextTransform.gameObject.activeSelf) {
                imageWithTextTransform.gameObject.SetActive(false);
            }
        }

        itemsContainer.gameObject.SetActive(false);
    }

    private TextMeshProUGUI AddText(string text, int siblingIndex = -1) {
        foreach (Transform textTransform in textTemplateList) {
            if (!textTransform.gameObject.activeSelf) {
                textTransform.gameObject.SetActive(true);

                if (siblingIndex == -1) {
                    textTransform.SetAsLastSibling();
                } else {
                    textTransform.SetSiblingIndex(siblingIndex);
                }
                TextMeshProUGUI existingTextMeshProUGUI = textTransform.GetComponent<TextMeshProUGUI>();
                existingTextMeshProUGUI.text = text;
                return existingTextMeshProUGUI;
            }
        }

        Transform newTextTransform = Instantiate(textTemplate, container);
        newTextTransform.gameObject.SetActive(true);
        newTextTransform.SetSiblingIndex(siblingIndex);

        TextMeshProUGUI newTextMeshProUGUI = newTextTransform.GetComponent<TextMeshProUGUI>();
        newTextMeshProUGUI.text = text;

        textTemplateList.Add(newTextTransform);

        return newTextMeshProUGUI;
    }

    private Transform AddImageWithText(Sprite sprite, string text, out TextMeshProUGUI createdText, int siblingIndex = -1) {
        foreach (Transform imageWithTextTransform in imageWithTextTemplateList) {
            if (!imageWithTextTransform.gameObject.activeSelf) {
                imageWithTextTransform.gameObject.SetActive(true);

                if (siblingIndex == -1) {
                    imageWithTextTransform.SetAsLastSibling();
                } else {
                    imageWithTextTransform.SetSiblingIndex(siblingIndex);
                }

                Image image = imageWithTextTransform.Find("Image").GetComponent<Image>();
                image.sprite = sprite;

                TextMeshProUGUI existingTextMeshProUGUI = imageWithTextTransform.Find("Text").GetComponent<TextMeshProUGUI>();
                existingTextMeshProUGUI.text = text;
                createdText = existingTextMeshProUGUI;
                return imageWithTextTransform;
            }
        }

        Transform newImageWithTextTransform = Instantiate(imageWithTextTemplate, container);
        newImageWithTextTransform.gameObject.SetActive(true);
        newImageWithTextTransform.SetSiblingIndex(siblingIndex);

        Image newImage = newImageWithTextTransform.Find("Image").GetComponent<Image>();
        newImage.sprite = sprite;

        TextMeshProUGUI newTextMeshProUGUI = newImageWithTextTransform.Find("Text").GetComponent<TextMeshProUGUI>();
        newTextMeshProUGUI.text = text;
        createdText = newTextMeshProUGUI;

        imageWithTextTemplateList.Add(newImageWithTextTransform);

        return newImageWithTextTransform;
    }

    private float AddItemAmountList(List<ItemAmount> itemAmountList) {

        int amountOfItems = 0;

        float totalWidth = 0;

        Vector3 extraOffset = Vector3.zero;
        for (int i = 0; i < itemAmountList.Count; i++) {

            ItemSO itemSO = itemAmountList[i].itemSO;
            int amount = itemAmountList[i].amount;

            if (itemsContainer.childCount <= i) {
                Transform childItemTransform = itemsContainer.GetChild(i);
                childItemTransform.Find("AmountText").GetComponent<TextMeshProUGUI>().text = amount.ToString();
                childItemTransform.Find("ItemIcon").GetComponent<Image>().sprite = itemSO.sprite;

                childItemTransform.gameObject.SetActive(true);
                continue;
            }

            Transform itemTransform = Instantiate(itemTemplate, itemsContainer);
            itemTransform.gameObject.SetActive(true);

            itemTransform.localPosition = Vector3.zero;
            itemTransform.localScale = Vector3.one;

            RectTransform rectTransform = itemTransform.GetComponent<RectTransform>();

            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            Vector2 newPosition = startingOffset + (multiplierPerItemOffset * amountOfItems) + extraOffset;

            rectTransform.anchoredPosition = newPosition;
            amountOfItems++;
            

            TextMeshProUGUI amountText = itemTransform.Find("AmountText").GetComponent<TextMeshProUGUI>();
            Image itemIcon = itemTransform.Find("ItemIcon").GetComponent<Image>();

            if (amount > 9) {
                itemIcon.transform.position += itemIconOffsetOnHigherNumbers;
                extraOffset = new Vector3(15, 0, 0);
            } else {
                extraOffset = Vector3.zero;
            }

            amountText.text = amount.ToString();
            itemIcon.sprite = itemSO.sprite;

            itemTransform.gameObject.SetActive(true);

            if (extraOffset != Vector3.zero) {
                totalWidth += extraOffset.x;
            }

        }

        itemsContainer.gameObject.SetActive(true);

        return totalWidth;
    }

    public void Hide() {
        isShowing = false;
        gameObject.SetActive(false);
    }

}
