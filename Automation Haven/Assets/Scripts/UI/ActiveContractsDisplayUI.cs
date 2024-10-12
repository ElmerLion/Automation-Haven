using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveContractsDisplayUI : BaseUI {

    public static ActiveContractsDisplayUI Instance { get; private set; }

    [SerializeField] private Transform expandedActiveContractHolder;
    [SerializeField] private Transform minimizedActiveContractsContainer;
    [SerializeField] private Transform expandedActiveContractsContainer;
    [SerializeField] private Transform minimizedActiveContractPrefab;
    [SerializeField] private Transform expandedActiveContractPrefab;
    [SerializeField] private Button showActiveContractsButton;
    [SerializeField] private Button closeButton;

    private List<ActiveContract> activeContractList = new List<ActiveContract>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        minimizedActiveContractPrefab.gameObject.SetActive(false);
        expandedActiveContractPrefab.gameObject.SetActive(false);

        ContractManager.Instance.OnContractCompleted += RemoveActiveContract;
        ContractManager.Instance.OnContractAccepted += AddActiveContract;
        ContractManager.Instance.OnContractFailed += RemoveActiveContract;

        HideMinimizedContracts();

        showActiveContractsButton.onClick.AddListener(() => {
            if (isOpen) return;
            minimizedActiveContractsContainer.gameObject.SetActive(!minimizedActiveContractsContainer.gameObject.activeSelf);
        });

        closeButton.onClick.AddListener(() => {
            Hide();
        });
    }


    public void AddActiveContract(ContractManager.Contract contract) {
        Transform minimzedActiveContract = Instantiate(minimizedActiveContractPrefab, minimizedActiveContractsContainer);
        
        SingleActiveContractDisplay minimizedActiveContractDisplay = minimzedActiveContract.GetComponent<SingleActiveContractDisplay>();
        minimizedActiveContractDisplay.Initialize(contract, false);

        minimzedActiveContract.gameObject.SetActive(true);

        Transform expandedActiveContract = Instantiate(expandedActiveContractPrefab, expandedActiveContractsContainer);

        SingleActiveContractDisplay expandedActiveContractDisplay = expandedActiveContract.GetComponent<SingleActiveContractDisplay>();

        expandedActiveContractDisplay.Initialize(contract, true);
        expandedActiveContract.gameObject.SetActive(true);

        foreach (ItemAmount itemAmount in contract.neededItemAmount) {
            expandedActiveContractDisplay.AddItemToItemContainer(itemAmount);
        }

        ActiveContract activeContract = new ActiveContract(contract, expandedActiveContract, minimzedActiveContract, expandedActiveContractDisplay, minimizedActiveContractDisplay);
        activeContractList.Add(activeContract);

        contract.InvokeProgressChangedEvent(contract);
    }

    public void RemoveActiveContract(ContractManager.Contract contract) {
        foreach (ActiveContract activeContract in activeContractList) {
            if (activeContract.contract == contract) {
                Destroy(activeContract.expandedTransform.gameObject);
                Destroy(activeContract.minimizedTransform.gameObject);
                activeContractList.Remove(activeContract);
                
                break;
            }
        }
    }

    public void ShowDeliveryPointUI(DeliveryPoint deliveryPoint) {
        if (ContractManager.Instance.GetActiveContracts().Count == 0) return;

        ShowExpandedContracts();

        foreach (ActiveContract activeContract in activeContractList) {
            activeContract.expandedDisplay.SetButtonToAddToDeliveryPoint(deliveryPoint);
        }
    }

    public void ShowExpandedContracts() {
        expandedActiveContractHolder.gameObject.SetActive(true);

        List<ContractManager.Contract> activeContractsUI = new List<ContractManager.Contract>();

        foreach (ActiveContract activeContract in activeContractList) {
            activeContractsUI.Add(activeContract.contract);
        }

        foreach (ContractManager.Contract contract in ContractManager.Instance.GetActiveContracts()) {
            if (!activeContractsUI.Contains(contract)) {
                AddActiveContract(contract);
            }
        }

        base.Show();
    }

    public override void Hide() {
        HideExpandedContracts();

        base.Hide();
    }

    public void HideExpandedContracts() {
        expandedActiveContractHolder.gameObject.SetActive(false);

        foreach (ActiveContract activeContract in activeContractList) {
            activeContract.expandedDisplay.ResetButtonListeners();
        }
    }

    private void HideMinimizedContracts() {
        minimizedActiveContractsContainer.gameObject.SetActive(false);
        expandedActiveContractHolder.gameObject.SetActive(false);
    }

    public class ActiveContract {
        public ContractManager.Contract contract;

        public Transform expandedTransform;
        public SingleActiveContractDisplay expandedDisplay;
        public Transform minimizedTransform;
        public SingleActiveContractDisplay minimizedDisplay;

        public ActiveContract(ContractManager.Contract contract, Transform expandedTransform, Transform minimizedTransform, SingleActiveContractDisplay expandedDisplay, SingleActiveContractDisplay minimizedDisplay) {
            this.contract = contract;
            this.expandedTransform = expandedTransform;
            this.minimizedTransform = minimizedTransform;
            this.expandedDisplay = expandedDisplay;
            this.minimizedDisplay = minimizedDisplay;
        }
    }


}
