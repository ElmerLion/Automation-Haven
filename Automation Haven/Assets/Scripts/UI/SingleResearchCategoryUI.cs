using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleResearchCategoryUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI categoryName;
    [SerializeField] private ResearchManager.ResearchCategory category;

    private Transform researchNodeTemplate;
    private Transform sectionTemplate;
    private Transform sectionContainer;

    private RectTransform categoryBackground;
    private List<SingleResearchNodeUI> researchNodeUIList = new List<SingleResearchNodeUI>();
    private List<ResearchManager.ResearchSection> researchSectionList = new List<ResearchManager.ResearchSection>(); 

    public void Setup() {
        categoryBackground = transform.Find("Background").GetComponent<RectTransform>();

        categoryName.text = category.ToString();

        transform.GetComponent<Button>().onClick.AddListener(() => {
            ResearchTreeUI.Instance.ShowResearchSlots(this);
        });

        researchNodeTemplate = ResearchTreeUI.Instance.GetResourceNodeTemplate();
        sectionTemplate = ResearchTreeUI.Instance.GetSectionTemplate();
        sectionContainer = ResearchTreeUI.Instance.GetSectionContainer();

        SetupResearchSections();
    }

    private void SetupResearchSections() {
        researchSectionList = ResearchManager.Instance.GetResearchSectionsInCategory(category);

        foreach (ResearchManager.ResearchSection researchSection in researchSectionList) {
            Transform sectionTransform = Instantiate(sectionTemplate, sectionContainer);
            researchSection.transform = sectionTransform;

            foreach (ResearchNodeSO researchNodeSO in researchSection.researchSectionSO.researchNodes) {
                Transform researchNodeTransform = Instantiate(researchNodeTemplate, sectionTransform);
                researchNodeTransform.gameObject.SetActive(true);

                SingleResearchNodeUI singleResearchNodeUI = researchNodeTransform.GetComponent<SingleResearchNodeUI>();

                ResearchTreeUI.Instance.AddResearchNodeToUI(researchNodeSO, singleResearchNodeUI);
                researchNodeUIList.Add(singleResearchNodeUI);

                singleResearchNodeUI.Setup(researchNodeSO);
            }
        }

        ResearchTreeUI.Instance.ShowResearchSlots(this);

        if (category == ResearchManager.ResearchCategory.Factory) {
            //ResearchTreeUI.Instance.ShowResearchSlots(this);
            ResearchTreeUI.Instance.ShowSideBarInfo(researchNodeUIList[0].GetResearchNode());
        }
    }

    public TextMeshProUGUI GetCategoryText() {
        return categoryName;
    }

    public RectTransform GetCategoryBackground() {
        return categoryBackground;
    }

    public List<SingleResearchNodeUI> GetResearchNodeUIList() {
        return researchNodeUIList;
    }

    public List<ResearchManager.ResearchSection> GetResearchSectionList() {
        return researchSectionList;
    }
}
