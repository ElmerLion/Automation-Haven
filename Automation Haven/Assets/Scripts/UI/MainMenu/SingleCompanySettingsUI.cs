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
    [SerializeField] private Image companyLogo;
    [SerializeField] private Button removeButton;
    [SerializeField] private Button selectButton;

    private Company company;
    
    public void Initialize(Company company) {
        possibleRequestTemplate.SetActive(false);

        this.company = company;
        companyNameText.text = company.companyName;
        startingReputationText.text = company.companySO.companyWorldReputation.ToString();
        companyLogo.sprite = company.companySO.companyLogo;

        foreach (ItemSO request in company.contractItems) {
            GameObject requestItem = Instantiate(possibleRequestTemplate, possibleRequestsContainer);
            requestItem.transform.Find("Image").GetComponent<Image>().sprite = request.sprite;
            requestItem.SetActive(true);
        }

        removeButton.onClick.AddListener(() => {
            NewWorldManager.Instance.RemoveCompanyFromStarting(company);
            Destroy(gameObject);
        });

        if (selectButton != null) {
            selectButton.onClick.AddListener(() => {
                NewWorldManager.Instance.AddCompanyToStarting(company);
                CreateNewWorldUI.Instance.HideAddCompanies();
                Destroy(gameObject);
            });
        }

        gameObject.SetActive(true);
    }
}
