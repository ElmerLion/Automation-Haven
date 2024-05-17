
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleContractSelectionUI : MonoBehaviour {
    [SerializeField] private Transform neededItemsContainer;
    [SerializeField] private Transform neededItemPrefab;

    [SerializeField] private TextMeshProUGUI companyTitle;
    [SerializeField] private Image companyIcon;

    [SerializeField] private Button acceptButton;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private TextMeshProUGUI reputationRewardText;
    [SerializeField] private TextMeshProUGUI reputationPenaltyText;

    private ContractManager.Contract contract;

    public void Initialize(ContractManager.Contract contract) {
        this.contract = contract;

        companyTitle.text = contract.companySO.companyName;
        companyIcon.sprite = contract.companySO.companyLogo;

        SetTimeText(contract.time);
        SetRewardText(contract.reward);
        SetReputationRewardText(contract.reputationReward);
        SetReputationPenaltyText(contract.reputationPenalty);

        acceptButton.onClick.AddListener(AcceptContract);

        gameObject.SetActive(true);
        neededItemPrefab.gameObject.SetActive(false);
    }

    public void AddItemToItemContainer(ItemAmount itemAmount) {
        Transform itemTransform = Instantiate(neededItemPrefab, neededItemsContainer);

        itemTransform.GetComponent<InventoryItemUI>().InitializeItem(itemAmount.itemSO, itemAmount.amount, false);

        itemTransform.gameObject.SetActive(true);
    }

    private void AcceptContract() {
        ContractManager.Instance.AcceptContract(contract);
        ContractSelectionUI.Instance.HandleAcceptButtonClick(contract);
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
