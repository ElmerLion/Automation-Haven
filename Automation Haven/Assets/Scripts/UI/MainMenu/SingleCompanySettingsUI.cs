using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleCompanySettingsUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI startingReputationText;
    [SerializeField] private Transform possibleRequestsContainer;
    [SerializeField] private GameObject possibleRequestTemplate;
    [SerializeField] private Button removeButton;
    [SerializeField] private Button selectButton;

    private CompanySO companySO;
    
    public void Initialize(CompanySO company) {
        companySO = company;
        companyNameText.text = companySO.companyName;
        startingReputationText.text = companySO.companyWorldReputation.ToString();

        foreach (ItemSO request in companySO.possibleNeededItems) {
            GameObject requestItem = Instantiate(possibleRequestTemplate, possibleRequestsContainer);
            requestItem.transform.GetChild(0).GetComponent<Image>().sprite = request.sprite;
        }

        removeButton.onClick.AddListener(() => {
            NewWorldManager.Instance.RemoveCompanyFromStarting(companySO);
            Destroy(gameObject);
        });

        if (selectButton != null) {
            selectButton.onClick.AddListener(() => {
                NewWorldManager.Instance.AddCompanyToStarting(companySO);
                CreateNewWorldUI.Instance.HideAddCompanies();
                Destroy(gameObject);
            });
        }

        gameObject.SetActive(true);
    }
}
