using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static ResearchManager;

public class ResearchTreeUI : BaseUI {

    public static ResearchTreeUI Instance { get; private set; }

    [Header("Categories")]
    [SerializeField] private Transform categoryContainer;
    [SerializeField] private Transform categoryTemplate;
    [SerializeField] private List<SingleResearchCategoryUI> singleResearchCategoryUIList;

    [Header("Research Nodes and Sections")]
    [SerializeField] private Transform sectionContainer;
    [SerializeField] private Transform sectionTemplate;
    [SerializeField] private Transform researchNodeTemplate;

    [Header("Side Bar")]
    [SerializeField] private Transform newUnlocksContainer;
    [SerializeField] private Transform newUnlockTemplate;
    [SerializeField] private Transform prerequisitesContainer;
    [SerializeField] private Transform prerequisiteTemplate;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI researchProgressText;

    [Space]
    [SerializeField] private Button researchButton;
    [SerializeField] private TextMeshProUGUI researchButtonText;
    [SerializeField] private Image researchButtonBar;

    [Header("Other")]
    [SerializeField] private Button closeButton;

    private Dictionary<ResearchNodeSO, SingleResearchNodeUI> researchNodeUIDic = new Dictionary<ResearchNodeSO, SingleResearchNodeUI>();
    private ResearchNode selectedResearchNode;


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        sectionTemplate.gameObject.SetActive(false);
        researchNodeTemplate.gameObject.SetActive(false);
        newUnlockTemplate.gameObject.SetActive(false);
        prerequisiteTemplate.gameObject.SetActive(false);

        ResearchManager.Instance.OnResearchLoaded += ResearchManager_OnResearchLoaded;

    }

    private void ResearchManager_OnResearchLoaded(object sender, System.EventArgs e) {
        SetupCategories();

        ResearchManager.Instance.OnResearchQueueChanged += ResearchManager_OnResearchQueueChanged;
        ResearchManager.Instance.OnNodeResearched += ResearchManager_OnNodeResearched;
        ResearchManager.Instance.OnActiveResearchProgressChanged += ResearchManager_OnActiveResearchProgressChanged;

        researchButton.onClick.AddListener(AddSelectedResearchNodeToResearch);
        closeButton.onClick.AddListener(Hide);

        Hide();
    }

    private void ResearchManager_OnActiveResearchProgressChanged(object sender, ResearchManager.ResearchNode e) {
        UpdateResearchProgress(e);
    }

    public override void Show() {
        base.Show();

        if (ResearchManager.Instance.GetActiveResearchNode() != null) {
            ShowSideBarInfo(ResearchManager.Instance.GetActiveResearchNode());
        }
    }

    private void ResearchManager_OnResearchQueueChanged(object sender, System.EventArgs e) {
        foreach (ResearchManager.ResearchNode researchNode in ResearchManager.Instance.GetResearchQueue()) {
            researchNodeUIDic[researchNode.researchNodeSO].ShowQueueNumber(ResearchManager.Instance.GetResearchQueue().IndexOf(researchNode) + 1);
        }

        UpdateResearchButton(selectedResearchNode);
    }

    private void ResearchManager_OnNodeResearched(object sender, ResearchManager.ResearchNode e) {
        researchNodeUIDic[e.researchNodeSO].MarkAsCompleted();

        UpdateResearchButton(e);
    }

    private void SetupCategories() {
        foreach (SingleResearchCategoryUI singleResearchCategoryUI in singleResearchCategoryUIList) {
            singleResearchCategoryUI.Setup();
        }
    }

    private void AddSelectedResearchNodeToResearch() {
        if (ResearchManager.Instance.TryAddNewResearchToQueue(selectedResearchNode)) {
            researchNodeUIDic[selectedResearchNode.researchNodeSO].ShowQueueNumber(ResearchManager.Instance.GetResearchQueueCount());
            return;
        }

        if (ResearchManager.Instance.DoesResearchQueueContainNode(selectedResearchNode)) {
            ResearchManager.Instance.StopResearchNode(selectedResearchNode);
            researchNodeUIDic[selectedResearchNode.researchNodeSO].RemoveFromQueue();
        }
    }

    public void ShowResearchSlots(SingleResearchCategoryUI senderCategory) {
        foreach (SingleResearchCategoryUI category in singleResearchCategoryUIList) {
            if (category != senderCategory) {
                foreach (ResearchManager.ResearchSection researchSection in category.GetResearchSectionList()) {
                    researchSection.transform.gameObject.SetActive(false);
                }
            }
        }

        foreach (ResearchManager.ResearchSection researchSection in senderCategory.GetResearchSectionList()) {
            researchSection.transform.gameObject.SetActive(true);
        }
    }

    public void ShowSideBarInfo(ResearchManager.ResearchNode researchNode) {
        selectedResearchNode = researchNode;

        title.text = researchNode.researchNodeSO.nameString;
        description.text = researchNode.researchNodeSO.description;

        foreach (Transform child in newUnlocksContainer) {
            if (child == newUnlockTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (PlacedObjectTypeSO buildingType in researchNode.researchNodeSO.unlockedBuildings) {
            Transform newUnlockTransform = Instantiate(newUnlockTemplate, newUnlocksContainer);
            newUnlockTransform.gameObject.SetActive(true);
            newUnlockTransform.Find("Name").GetComponent<TextMeshProUGUI>().text = buildingType.nameString;
            newUnlockTransform.Find("Icon").GetComponent<Image>().sprite = buildingType.buildingSprite;

            SingleNewUnlockUI singleNewUnlockUI = newUnlockTransform.GetComponent<SingleNewUnlockUI>();
            singleNewUnlockUI.SetPlacedObjectType(buildingType);
        }

        foreach (ItemSO itemSO in researchNode.researchNodeSO.unlockedItems) {
            Transform newUnlockTransform = Instantiate(newUnlockTemplate, newUnlocksContainer);
            newUnlockTransform.gameObject.SetActive(true);
            newUnlockTransform.Find("Name").GetComponent<TextMeshProUGUI>().text = itemSO.nameString;
            newUnlockTransform.Find("Icon").GetComponent<Image>().sprite = itemSO.sprite;

            SingleNewUnlockUI singleNewUnlockUI = newUnlockTransform.GetComponent<SingleNewUnlockUI>();
            singleNewUnlockUI.SetItem(itemSO);
        }

        foreach (Transform child in prerequisitesContainer) {
            if (child == prerequisiteTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (ResearchNodeSO researchNodeSO in researchNode.researchNodeSO.prerequisiteResearchList) {
            Transform prerequisiteTransform = Instantiate(prerequisiteTemplate, prerequisitesContainer);
            prerequisiteTransform.gameObject.SetActive(true);

            TextMeshProUGUI nameText = prerequisiteTransform.Find("Name").GetComponent<TextMeshProUGUI>();

            nameText.text = researchNodeSO.nameString;
            prerequisiteTransform.Find("Icon").GetComponent<Image>().sprite = researchNodeSO.icon;

            if (ResearchManager.Instance.GetResearchNode(researchNodeSO).isResearched) {
                nameText.color = Color.green;
            } 
        }

        UpdateResearchButton(researchNode);
        UpdateResearchProgress(researchNode);
    }

    private void UpdateResearchButton(ResearchNode researchNode) {
        if (selectedResearchNode != researchNode) return;

        if (researchNode.isResearched) {
            researchButton.interactable = false;
            researchButtonText.text = "Researched";
        } else if (ResearchManager.Instance.GetActiveResearchNode() == researchNode) {
            researchButton.interactable = true;
            researchButtonText.text = "Stop Research";
        } else if (ResearchManager.Instance.DoesResearchQueueContainNode(researchNode)) {
            researchButton.interactable = true;
            researchButtonText.text = "In Queue";
        } else {
            researchButton.interactable = true;
            researchButtonText.text = "Research";
        }
    }

    public Transform GetResourceNodeTemplate() {
        return researchNodeTemplate;
    }

    public Transform GetSectionTemplate() {
        return sectionTemplate;
    }

    public Transform GetSectionContainer() {
        return sectionContainer;
    }

    public void AddResearchNodeToUI(ResearchNodeSO researchNodeSO, SingleResearchNodeUI singleResearchNodeUI) {
        researchNodeUIDic.Add(researchNodeSO, singleResearchNodeUI);
    }

    private void UpdateResearchProgress(ResearchManager.ResearchNode testResearchNode) {
       researchButtonBar.fillAmount = selectedResearchNode.GetResearchProgressNormalized();
       researchProgressText.text = selectedResearchNode.researchProgress + "/" + selectedResearchNode.researchNodeSO.researchCost;
    }

}
