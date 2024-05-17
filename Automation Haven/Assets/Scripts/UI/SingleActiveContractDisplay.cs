using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Heat;

public class SingleActiveContractDisplay : MonoBehaviour {

    private bool isExpanded;
    [SerializeField] private Transform neededItemsContainer;
    [SerializeField] private Transform neededItemPrefab;

    [SerializeField] private TextMeshProUGUI companyTitle;
    [SerializeField] private Image companyIcon;

    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private TextMeshProUGUI reputationRewardText;
    [SerializeField] private TextMeshProUGUI reputationPenaltyText;

    private ContractManager.Contract contract;
    private List<InventoryItemUI> neededItemInventoryItemUI = new List<InventoryItemUI>();

    public void Initialize(ContractManager.Contract contract, bool isExpanded) {
        this.contract = contract;
        this.isExpanded = isExpanded;

        companyTitle.text = contract.companySO.companyName;
        companyIcon.sprite = contract.companySO.companyLogo;

        SetTimeText(contract.time);
        SetRewardText(contract.reward);

        if (isExpanded) {
            SetReputationRewardText(contract.reputationReward);
            SetReputationPenaltyText(contract.reputationPenalty);
            neededItemPrefab.gameObject.SetActive(false);
        }

        if (!isExpanded) {
            Button button = transform.GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => ShowMoreContractInfo());
        }

        gameObject.SetActive(true);


        contract.OnContractProgressChanged += Contract_OnContractProgressChanged;
    }

    public void SetButtonToAddToDeliveryPoint(DeliveryPoint delivertyPoint) {
        if (!isExpanded) return;

        Button button = transform.GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() => {
            delivertyPoint.SetContract(contract);
            if (isExpanded) {
                button.onClick.RemoveAllListeners();

                button.onClick.AddListener(() => ShowMoreContractInfo());
            }

            ActiveContractsDisplayUI.Instance.Hide();
        });
    }

    private void Contract_OnContractProgressChanged(ContractManager.Contract obj) {
        UpdateItemProgress();
    }

    public void AddItemToItemContainer(ItemAmount itemAmount) {
        Transform itemTransform = Instantiate(neededItemPrefab, neededItemsContainer);

        InventoryItemUI inventoryItemUI = itemTransform.GetComponent<InventoryItemUI>();
        inventoryItemUI.InitializeItem(itemAmount.itemSO, itemAmount.amount, false);
        neededItemInventoryItemUI.Add(inventoryItemUI);

        itemTransform.gameObject.SetActive(true);
    }

    public void ResetButtonListeners() {
        transform.GetComponent<Button>().onClick.RemoveAllListeners();
    }

    private void UpdateItemProgress() {
        if (!isExpanded) return; 

        foreach (ItemAmount itemAmount in contract.neededItemAmount) {
            InventoryItemUI inventoryItemUI = InventoryItemUI.GetInventoryItemUI(itemAmount.itemSO, neededItemInventoryItemUI);
            if (inventoryItemUI == null) continue;

            inventoryItemUI.UpdateAmount(itemAmount.amount);
        }
    }

    private void ShowMoreContractInfo() {
        if (isExpanded) return;

        ActiveContractsDisplayUI.Instance.ShowExpandedContracts();
    }

    private void SetTimeText(float timeAmount) {
        timeText.text = timeAmount.ToString() + "D";
    }

    private void SetRewardText(float rewardAmount) {
        rewardText.text = rewardAmount.ToString() + "C";
    }

    private void SetReputationRewardText(float reputationRewardAmount) {
        reputationRewardText.text = reputationRewardAmount.ToString();
    }

    private void SetReputationPenaltyText(float reputationPenaltyAmount) {
        reputationPenaltyText.text = reputationPenaltyAmount.ToString();
    }
}
