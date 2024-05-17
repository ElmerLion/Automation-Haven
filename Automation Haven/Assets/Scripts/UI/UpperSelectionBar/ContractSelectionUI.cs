using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContractSelectionUI : BaseUI {

    public static ContractSelectionUI Instance { get; private set; }

    [SerializeField] private Transform contractButtonPrefab;
    [SerializeField] private Transform contractButtonContainer;

    private int addedContracts = 0;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        ContractManager.Instance.OnNewContractCreated += AddContract;
        ContractManager.Instance.OnShowContractSelection += Show;

        contractButtonPrefab.gameObject.SetActive(false);

        Hide();
    }

    public void AddContract(ContractManager.Contract contract) {
        if (addedContracts >= ContractManager.Instance.GetMaxGeneratedContractsAmount()) return;
        Transform contractButtonTransform = Instantiate(contractButtonPrefab, contractButtonContainer);
        SingleContractSelectionUI contractButton = contractButtonTransform.GetComponent<SingleContractSelectionUI>();

        contractButton.Initialize(contract);
        addedContracts++;

        foreach (ItemAmount itemAmount in contract.neededItemAmount) {
            contractButton.AddItemToItemContainer(itemAmount);
        }

    }

    public void HandleAcceptButtonClick(ContractManager.Contract contract) {
        addedContracts = 0;
        Hide();
        ClearContractButtons();
    }

    private void ClearContractButtons() {
        foreach (Transform child in contractButtonContainer) {
            if (child == contractButtonPrefab) continue;
            Destroy(child.gameObject);
        }
    }
}
